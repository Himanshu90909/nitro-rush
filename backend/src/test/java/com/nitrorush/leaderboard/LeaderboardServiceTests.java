package com.nitrorush.leaderboard;

import com.nitrorush.player.PlayerProfileRepository;
import com.nitrorush.racing.RaceResultRepository;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.redis.RedisConnectionFailureException;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.data.redis.core.ZSetOperations;

import java.util.LinkedHashSet;
import java.util.Set;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.anyLong;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class LeaderboardServiceTests {

    @Mock
    private StringRedisTemplate redisTemplate;
    @Mock
    private ZSetOperations<String, String> zSetOperations;
    @Mock
    private RaceResultRepository raceResultRepository;
    @Mock
    private PlayerProfileRepository profileRepository;

    private LeaderboardService service;

    @BeforeEach
    void setUp() {
        service = new LeaderboardService(redisTemplate, raceResultRepository, profileRepository);
    }

    @Test
    void submitScoreWritesToRedisSortedSet() {
        when(redisTemplate.opsForZSet()).thenReturn(zSetOperations);
        UUID playerId = UUID.randomUUID();

        service.submitScore(playerId, 1100);

        verify(zSetOperations).incrementScore(LeaderboardService.KEY_GLOBAL, playerId.toString(), 1100.0);
    }

    @Test
    void submitScoreDegradesGracefullyWhenRedisIsDown() {
        when(redisTemplate.opsForZSet()).thenThrow(new RedisConnectionFailureException("down"));
        UUID playerId = UUID.randomUUID();

        assertDoesNotThrow(() -> service.submitScore(playerId, 1100));
    }

    @Test
    void topReadsZRevRange() {
        when(redisTemplate.opsForZSet()).thenReturn(zSetOperations);
        UUID first = UUID.randomUUID();
        UUID second = UUID.randomUUID();
        Set<ZSetOperations.TypedTuple<String>> tuples = new LinkedHashSet<>();
        tuples.add(tuple(first, 3000));
        tuples.add(tuple(second, 2000));
        when(zSetOperations.reverseRangeWithScores(LeaderboardService.KEY_GLOBAL, 0, 9)).thenReturn(tuples);
        when(profileRepository.findAllByIdIn(any())).thenReturn(java.util.List.of());

        var entries = service.top(10);

        assertEquals(2, entries.size());
        assertEquals(1, entries.get(0).rank());
        assertEquals(3000, entries.get(0).score());
        assertEquals(2, entries.get(1).rank());
    }

    @Test
    void topFallsBackToPostgresWhenRedisFails() {
        when(redisTemplate.opsForZSet()).thenThrow(new RedisConnectionFailureException("down"));
        UUID p1 = UUID.randomUUID();
        when(raceResultRepository.aggregateScoresByPlayer()).thenReturn(java.util.List.<Object[]>of(
                new Object[]{p1, 4200L}));
        when(profileRepository.findAllByIdIn(any())).thenReturn(java.util.List.of());

        var entries = service.top(10);

        assertEquals(1, entries.size());
        assertEquals(4200, entries.get(0).score());
        verify(raceResultRepository).aggregateScoresByPlayer();
    }

    private ZSetOperations.TypedTuple<String> tuple(UUID playerId, double score) {
        return new ZSetOperations.TypedTuple<>() {
            @Override
            public String getValue() { return playerId.toString(); }
            @Override
            public Double getScore() { return score; }
            @Override
            public int compareTo(ZSetOperations.TypedTuple<String> other) { return Double.compare(score, other.getScore()); }
        };
    }
}
