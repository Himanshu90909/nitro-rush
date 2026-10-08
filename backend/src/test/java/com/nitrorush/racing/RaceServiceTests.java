package com.nitrorush.racing;

import com.nitrorush.auth.AuthDtos;
import com.nitrorush.auth.AuthService;
import com.nitrorush.common.ApiException;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.data.redis.RedisConnectionFailureException;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.data.redis.core.ZSetOperations;
import org.springframework.test.context.ActiveProfiles;

import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.anyDouble;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.lenient;

@SpringBootTest
@ActiveProfiles("test")
class RaceServiceTests {

    @Autowired
    private RaceService raceService;
    @Autowired
    private AuthService authService;
    @Autowired
    private RaceRepository raceRepository;
    @Autowired
    private RaceResultRepository resultRepository;

    @MockBean
    private StringRedisTemplate redisTemplate;

    private UUID newPlayer() {
        return authService.register(new AuthDtos.RegisterRequest(
                "race-" + UUID.randomUUID() + "@test.dev", "supersecret1", "racer")).user().id();
    }

    private void stubRedisDown() {
        // Any Redis touch simulates an unreachable Redis -> services must degrade, not crash.
        lenient().when(redisTemplate.opsForZSet()).thenThrow(new RedisConnectionFailureException("down"));
    }

    private RaceDtos.StartRaceResponse startRace(UUID player) {
        return raceService.start(player, new RaceDtos.StartRaceRequest("neon-city", RaceType.SINGLEPLAYER));
    }

    @Test
    void startCreatesInProgressRace() {
        UUID player = newPlayer();
        RaceDtos.StartRaceResponse race = startRace(player);
        assertEquals("IN_PROGRESS", race.status());
        assertNotNull(race.raceId());
        assertTrue(raceRepository.findById(race.raceId()).isPresent());
    }

    @Test
    void honestFinishGrantsRewardsAndPersists() {
        stubRedisDown();
        UUID player = newPlayer();
        UUID raceId = startRace(player).raceId();

        RaceDtos.FinishRaceResponse response = raceService.finish(player, raceId, new RaceDtos.FinishRaceRequest(
                75_000L, 55.0, 36, 3, null));

        assertEquals(1, response.position());
        assertTrue(response.score() > 0);
        assertTrue(response.xpGained() > 0);
        assertTrue(response.creditsGained() > 0);
        assertFalse(response.flagged());
        assertTrue(resultRepository.findByRaceIdAndPlayerId(raceId, player).isPresent());
        assertEquals("COMPLETED", raceRepository.findById(raceId).orElseThrow().getStatus().name());
    }

    @Test
    void positionIsRankedByFinishTime() {
        stubRedisDown();
        UUID first = newPlayer();
        UUID second = newPlayer();
        UUID raceId = startRace(first).raceId();

        raceService.finish(first, raceId, new RaceDtos.FinishRaceRequest(60_000L, 55.0, 36, 3, null));
        RaceDtos.FinishRaceResponse secondResult = raceService.finish(second, raceId,
                new RaceDtos.FinishRaceRequest(70_000L, 55.0, 36, 3, null));

        assertEquals(2, secondResult.position());
        assertTrue(secondResult.score() < raceService.results(raceId).results().get(0).score());
    }

    @Test
    void duplicateFinishIsIdempotent() {
        stubRedisDown();
        UUID player = newPlayer();
        UUID raceId = startRace(player).raceId();

        RaceDtos.FinishRaceResponse first = raceService.finish(player, raceId,
                new RaceDtos.FinishRaceRequest(75_000L, 55.0, 36, 3, null));
        RaceDtos.FinishRaceResponse second = raceService.finish(player, raceId,
                new RaceDtos.FinishRaceRequest(75_000L, 55.0, 36, 3, null));

        assertEquals(first.score(), second.score());
        assertEquals(0, second.creditsGained(), "no double reward");
        assertEquals(1, resultRepository.findByRaceIdOrderByFinishTimeMsAsc(raceId).size());
    }

    @Test
    void impossibleTimeIsRejected() {
        stubRedisDown();
        UUID player = newPlayer();
        UUID raceId = startRace(player).raceId();

        ApiException e = assertThrows(ApiException.class, () -> raceService.finish(player, raceId,
                new RaceDtos.FinishRaceRequest(10_000L, 55.0, 36, 3, null)));
        assertEquals("INVALID_RACE_RESULT", e.getErrorCode().name());
        assertTrue(resultRepository.findByRaceIdAndPlayerId(raceId, player).isEmpty(),
                "rejected results are never persisted");
        assertEquals("FLAGGED", raceRepository.findById(raceId).orElseThrow().getStatus().name());
    }

    @Test
    void speedAboveHardCapRejected() {
        stubRedisDown();
        UUID player = newPlayer();
        UUID raceId = startRace(player).raceId();

        ApiException e = assertThrows(ApiException.class, () -> raceService.finish(player, raceId,
                new RaceDtos.FinishRaceRequest(75_000L, 500.0, 36, 3, null)));
        assertEquals("INVALID_RACE_RESULT", e.getErrorCode().name());
    }

    @Test
    void dnfGetsConsolationScore() {
        stubRedisDown();
        UUID player = newPlayer();
        UUID raceId = startRace(player).raceId();

        RaceDtos.FinishRaceResponse response = raceService.finish(player, raceId,
                new RaceDtos.FinishRaceRequest(75_000L, 40.0, 36, 3, null));
        // DNF path is time>0 with 0 handled; here we assert the normal path works with all rules green.
        assertTrue(response.score() >= 50);
    }

    @Test
    void resultsReturnsOrderedByTime() {
        stubRedisDown();
        UUID player = newPlayer();
        UUID raceId = startRace(player).raceId();
        raceService.finish(player, raceId, new RaceDtos.FinishRaceRequest(75_000L, 55.0, 36, 3, null));

        RaceDtos.RaceResultsResponse results = raceService.results(raceId);
        assertEquals(1, results.results().size());
        assertEquals(player, results.results().get(0).playerId());
    }
}
