package com.nitrorush.racing;

import com.nitrorush.analytics.AnalyticsService;
import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import com.nitrorush.garage.Car;
import com.nitrorush.garage.CarRepository;
import com.nitrorush.garage.PlayerCarRepository;
import com.nitrorush.leaderboard.LeaderboardService;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import com.nitrorush.player.PlayerService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.UUID;

/**
 * Server-authoritative race lifecycle.
 *
 * finish() pipeline: anti-cheat validation -> position computed from stored
 * results -> score derived from position -> rewards granted under a
 * pessimistic profile lock -> leaderboard submission -> telemetry event.
 */
@Service
public class RaceService {

    private static final Logger log = LoggerFactory.getLogger(RaceService.class);

    /** Position -> base score. DNF gets a small consolation. */
    private static final long[] POSITION_SCORE = {1000, 800, 600, 500, 400, 300};
    private static final long DNF_SCORE = 50;
    private static final long CLEAN_RACE_BONUS = 100;
    private static final long DEFAULT_PAR_TIME_MS = 90_000;
    private static final double DEFAULT_TOP_SPEED = 70.0;

    private final RaceRepository raceRepository;
    private final RaceResultRepository resultRepository;
    private final PlayerProfileRepository profileRepository;
    private final PlayerService playerService;
    private final CarRepository carRepository;
    private final PlayerCarRepository playerCarRepository;
    private final LeaderboardService leaderboardService;
    private final AnalyticsService analyticsService;
    private final RaceAuditService raceAuditService;

    public RaceService(RaceRepository raceRepository,
                       RaceResultRepository resultRepository,
                       PlayerProfileRepository profileRepository,
                       PlayerService playerService,
                       CarRepository carRepository,
                       PlayerCarRepository playerCarRepository,
                       LeaderboardService leaderboardService,
                       AnalyticsService analyticsService,
                       RaceAuditService raceAuditService) {
        this.raceRepository = raceRepository;
        this.resultRepository = resultRepository;
        this.profileRepository = profileRepository;
        this.playerService = playerService;
        this.carRepository = carRepository;
        this.playerCarRepository = playerCarRepository;
        this.leaderboardService = leaderboardService;
        this.analyticsService = analyticsService;
        this.raceAuditService = raceAuditService;
    }

    @Transactional
    public RaceDtos.StartRaceResponse start(UUID userId, RaceDtos.StartRaceRequest request) {
        Race race = raceRepository.save(new Race(request.trackId(), request.raceType()));
        analyticsService.record(userId, "race_started", java.util.Map.of("trackId", request.trackId(), "raceType", request.raceType().name()));
        log.info("Player {} started race {} on track {}", userId, race.getId(), request.trackId());
        return new RaceDtos.StartRaceResponse(race.getId(), race.getTrackId(), race.getStatus().name());
    }

    @Transactional
    public RaceDtos.FinishRaceResponse finish(UUID userId, UUID raceId, RaceDtos.FinishRaceRequest request) {
        Race race = raceRepository.findById(raceId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Race not found"));

        // Idempotent: duplicate finish returns the recorded result.
        var existing = resultRepository.findByRaceIdAndPlayerId(raceId, userId);
        if (existing.isPresent()) {
            RaceResult result = existing.get();
            return new RaceDtos.FinishRaceResponse(raceId, result.getPosition(), result.getScore(), 0, 0, false, "already finished");
        }

        long parTimeMs = request.parTimeMs() != null && request.parTimeMs() > 0 ? request.parTimeMs().longValue() : DEFAULT_PAR_TIME_MS;
        double carTopSpeed = bestOwnedCarTopSpeed(userId);

        AntiCheatValidator.Validity validity = AntiCheatValidator.validate(
                request.finishTimeMs(), parTimeMs,
                request.maxSpeed(), carTopSpeed,
                request.checkpointsPassed(), 12, // standard track: 12 checkpoints per lap
                request.laps(), 3);

        if (!validity.valid()) {
            // REQUIRES_NEW: FLAGGED status + telemetry must survive our own rollback.
            raceAuditService.markFlagged(raceId, userId, validity.reason());
            throw ApiException.of(ErrorCode.INVALID_RACE_RESULT, "Race result rejected: " + validity.reason());
        }

        boolean dnf = request.finishTimeMs() == 0;
        int position = dnf ? 99 : 1 + (int) resultRepository.findByRaceIdOrderByFinishTimeMsAsc(raceId).stream()
                .filter(r -> r.getFinishTimeMs() > 0 && r.getFinishTimeMs() < request.finishTimeMs())
                .count();

        long score = dnf ? DNF_SCORE
                : (position - 1 < POSITION_SCORE.length ? POSITION_SCORE[position - 1] : 250) + CLEAN_RACE_BONUS;
        long xp = score / 10;
        long credits = score / 2;

        PlayerProfile profile = playerService.getProfileEntity(userId);
        PlayerProfile locked = profileRepository.findByIdForUpdate(profile.getId())
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Player profile not found"));
        locked.setXp(locked.getXp() + xp);
        locked.setCredits(locked.getCredits() + credits);
        locked.setLevel(PlayerService.levelForXp(locked.getXp()));
        profileRepository.save(locked);

        resultRepository.save(new RaceResult(raceId, userId, position, request.finishTimeMs(), score, request.maxSpeed()));

        if (validity.flagged()) {
            race.setStatus(RaceStatus.FLAGGED);
        } else if (dnf) {
            race.setStatus(RaceStatus.COMPLETED);
        } else {
            race.setStatus(RaceStatus.COMPLETED);
        }
        race.setCompletedAt(java.time.Instant.now());
        raceRepository.save(race);

        leaderboardService.submitScore(userId, score);
        analyticsService.record(userId, "race_finished", java.util.Map.of(
                "position", position, "score", score, "trackId", race.getTrackId(), "flagged", validity.flagged()));

        log.info("Player {} finished race {} pos={} score={} flagged={}", userId, raceId, position, score, validity.flagged());
        return new RaceDtos.FinishRaceResponse(raceId, position, score, xp, credits, validity.flagged(), validity.reason());
    }

    @Transactional(readOnly = true)
    public RaceDtos.RaceResultsResponse results(UUID raceId) {
        List<RaceResult> results = resultRepository.findByRaceIdOrderByFinishTimeMsAsc(raceId);
        return new RaceDtos.RaceResultsResponse(raceId, results.stream()
                .map(r -> new RaceDtos.RaceResultDto(r.getPlayerId(), r.getPosition(), r.getFinishTimeMs(), r.getScore()))
                .toList());
    }

    /** Best base top speed among owned cars — used for the anti-cheat speed cap. */
    private double bestOwnedCarTopSpeed(UUID userId) {
        double best = DEFAULT_TOP_SPEED;
        for (var playerCar : playerCarRepository.findByPlayerId(userId)) {
            Car car = carRepository.findById(playerCar.getCarId()).orElse(null);
            if (car != null && car.getBaseSpeed() > best) {
                best = car.getBaseSpeed();
            }
        }
        return best;
    }
}
