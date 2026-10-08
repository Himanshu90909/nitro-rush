package com.nitrorush.leaderboard;

import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import com.nitrorush.racing.RaceResultRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.dao.DataAccessException;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.data.redis.core.ZSetOperations;
import org.springframework.stereotype.Service;

import java.util.*;
import java.util.stream.Collectors;

/**
 * Redis sorted-set leaderboard (ZADD nitrorush:leaderboard:global).
 *
 * Scores can ONLY be submitted through RaceService — there is deliberately no
 * public score endpoint, so a client can never post an arbitrary number.
 * If Redis is unavailable the service falls back to a Postgres aggregate
 * (SUM of race scores) so the API stays honest rather than empty.
 */
@Service
public class LeaderboardService {

    private static final Logger log = LoggerFactory.getLogger(LeaderboardService.class);
    static final String KEY_GLOBAL = "nitrorush:leaderboard:global";

    private final StringRedisTemplate redis;
    private final RaceResultRepository raceResultRepository;
    private final PlayerProfileRepository profileRepository;

    public LeaderboardService(StringRedisTemplate redis,
                              RaceResultRepository raceResultRepository,
                              PlayerProfileRepository profileRepository) {
        this.redis = redis;
        this.raceResultRepository = raceResultRepository;
        this.profileRepository = profileRepository;
    }

    /** Adds score to the player's total. Called ONLY by RaceService. */
    public void submitScore(UUID playerId, long score) {
        try {
            redis.opsForZSet().incrementScore(KEY_GLOBAL, playerId.toString(), score);
        } catch (DataAccessException e) {
            log.warn("Redis unavailable, score for {} will still count via Postgres fallback: {}", playerId, e.getMessage());
        }
    }

    public List<LeaderboardDtos.LeaderboardEntry> top(int limit) {
        try {
            return redisTop(limit);
        } catch (DataAccessException | IllegalStateException e) {
            log.warn("Redis unavailable, using Postgres leaderboard fallback: {}", e.getMessage());
            return fromPostgres(limit);
        }
    }

    private List<LeaderboardDtos.LeaderboardEntry> redisTop(int limit) {
        {
            Set<ZSetOperations.TypedTuple<String>> tuples =
                    redis.opsForZSet().reverseRangeWithScores(KEY_GLOBAL, 0, Math.max(0, limit - 1));
            if (tuples == null || tuples.isEmpty()) {
                return fromPostgres(limit);
            }
            List<LeaderboardDtos.LeaderboardEntry> entries = new ArrayList<>();
            long rank = 1;
            for (ZSetOperations.TypedTuple<String> tuple : tuples) {
                UUID playerId = UUID.fromString(Objects.requireNonNull(tuple.getValue()));
                entries.add(new LeaderboardDtos.LeaderboardEntry(
                        playerId, usernameOf(playerId),
                        tuple.getScore() == null ? 0L : tuple.getScore().longValue(), rank++));
            }
            return entries;
        }
    }

    public LeaderboardDtos.LeaderboardResponse leaderboard(UUID requesterId, int limit) {
        List<LeaderboardDtos.LeaderboardEntry> entries = top(limit);
        LeaderboardDtos.LeaderboardEntry self = null;
        for (LeaderboardDtos.LeaderboardEntry entry : entries) {
            if (entry.playerId().equals(requesterId)) {
                self = entry;
                break;
            }
        }
        if (self == null) {
            self = playerEntry(requesterId);
        }
        return new LeaderboardDtos.LeaderboardResponse(entries, self);
    }

    public LeaderboardDtos.LeaderboardEntry playerEntry(UUID playerId) {
        try {
            return redisPlayerEntry(playerId);
        } catch (DataAccessException | IllegalStateException e) {
            log.warn("Redis unavailable, using Postgres leaderboard fallback: {}", e.getMessage());
            return fromPostgresFor(playerId);
        }
    }

    private LeaderboardDtos.LeaderboardEntry redisPlayerEntry(UUID playerId) {
        {
            Long rank = redis.opsForZSet().reverseRank(KEY_GLOBAL, playerId.toString());
            Double score = redis.opsForZSet().score(KEY_GLOBAL, playerId.toString());
            if (score == null) {
                return fromPostgresFor(playerId);
            }
            return new LeaderboardDtos.LeaderboardEntry(playerId, usernameOf(playerId),
                    score == null ? 0 : score.longValue(), rank == null ? 0 : rank + 1);
        }
    }

    public List<LeaderboardDtos.LeaderboardEntry> nearby(UUID playerId, int range) {
        LeaderboardDtos.LeaderboardEntry self = playerEntry(playerId);
        if (self == null || self.rank() <= 0) {
            return top(2 * range + 1);
        }
        long start = Math.max(0, self.rank() - 1 - range);
        long end = self.rank() - 1 + range;
        try {
            Set<ZSetOperations.TypedTuple<String>> tuples =
                    redis.opsForZSet().reverseRangeWithScores(KEY_GLOBAL, start, end);
            if (tuples == null || tuples.isEmpty()) {
                return fromPostgres((int) (end - start + 1));
            }
            List<LeaderboardDtos.LeaderboardEntry> entries = new ArrayList<>();
            long rank = start + 1;
            for (ZSetOperations.TypedTuple<String> tuple : tuples) {
                UUID id = UUID.fromString(Objects.requireNonNull(tuple.getValue()));
                entries.add(new LeaderboardDtos.LeaderboardEntry(
                        id, usernameOf(id), tuple.getScore() == null ? 0L : tuple.getScore().longValue(), rank++));
            }
            return entries;
        } catch (DataAccessException | IllegalStateException e) {
            log.warn("Redis unavailable, using Postgres leaderboard fallback: {}", e.getMessage());
            return fromPostgres((int) (end - start + 1));
        }
    }

    private List<LeaderboardDtos.LeaderboardEntry> fromPostgres(int limit) {
        List<Object[]> rows = raceResultRepository.aggregateScoresByPlayer();
        List<LeaderboardDtos.LeaderboardEntry> entries = new ArrayList<>();
        long rank = 1;
        for (Object[] row : rows) {
            if (entries.size() >= limit) break;
            UUID playerId = (UUID) row[0];
            long total = ((Number) row[1]).longValue();
            entries.add(new LeaderboardDtos.LeaderboardEntry(playerId, usernameOf(playerId), total, rank++));
        }
        return entries;
    }

    private LeaderboardDtos.LeaderboardEntry fromPostgresFor(UUID playerId) {
        long total = raceResultRepository.totalScoreForPlayer(playerId);
        if (total == 0) {
            return new LeaderboardDtos.LeaderboardEntry(playerId, usernameOf(playerId), 0, 0);
        }
        return new LeaderboardDtos.LeaderboardEntry(playerId, usernameOf(playerId), total, 0);
    }

    private String usernameOf(UUID playerId) {
        return profileRepository.findAllByIdIn(List.of(playerId)).stream()
                .map(PlayerProfile::getUsername)
                .findFirst()
                .orElse("player-" + playerId.toString().substring(0, 8));
    }
}
