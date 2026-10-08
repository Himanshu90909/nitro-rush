package com.nitrorush.matchmaking;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.nitrorush.auth.AuthDtos;
import com.nitrorush.auth.AuthService;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.data.redis.RedisConnectionFailureException;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.data.redis.core.ZSetOperations;
import org.springframework.test.context.ActiveProfiles;

import java.util.LinkedHashSet;
import java.util.Set;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.*;

@SpringBootTest
@ActiveProfiles("test")
class MatchmakingServiceTests {

    @Autowired
    private MatchmakingService matchmakingService;
    @Autowired
    private AuthService authService;

    @MockBean
    private StringRedisTemplate redisTemplate;

    @SuppressWarnings("unchecked")
    private ZSetOperations<String, String> zSet() {
        ZSetOperations<String, String> z = mock(ZSetOperations.class);
        when(redisTemplate.opsForZSet()).thenReturn(z);
        return z;
    }

    private UUID newPlayer() {
        return authService.register(new AuthDtos.RegisterRequest(
                "mm-" + UUID.randomUUID() + "@test.dev", "supersecret1", "queued")).user().id();
    }

    @Test
    void joinAddsToQueueByTimestamp() {
        ZSetOperations<String, String> z = zSet();
        when(z.score(anyString(), anyString())).thenReturn(null);

        UUID player = newPlayer();
        matchmakingService.join(player);

        verify(z).add(eq(MatchmakingService.QUEUE_KEY), eq(player.toString()), anyDouble());
    }

    @Test
    void joinIsIdempotent() {
        ZSetOperations<String, String> z = zSet();
        UUID player = newPlayer();
        when(z.score(eq(MatchmakingService.QUEUE_KEY), eq(player.toString()))).thenReturn(1000.0);

        matchmakingService.join(player);

        verify(z, never()).add(anyString(), anyString(), anyDouble());
    }

    @Test
    void tryMatchGroupsSimilarRatingsAndRemovesFromQueue() {
        UUID a = newPlayer();
        UUID b = newPlayer();
        ZSetOperations<String, String> z = zSet();
        org.springframework.data.redis.core.ValueOperations<String, String> values =
                mock(org.springframework.data.redis.core.ValueOperations.class);
        when(redisTemplate.opsForValue()).thenReturn(values);

        when(z.range(eq(MatchmakingService.QUEUE_KEY), anyLong(), anyLong()))
                .thenReturn(new LinkedHashSet<>(Set.of(a.toString(), b.toString())));

        var lobby = matchmakingService.tryMatch();

        assertTrue(lobby.isPresent());
        assertEquals(2, lobby.get().playerIds().size());
        verify(z).remove(eq(MatchmakingService.QUEUE_KEY), eq(a.toString()));
        verify(z).remove(eq(MatchmakingService.QUEUE_KEY), eq(b.toString()));
    }

    @Test
    void singlePlayerDoesNotMatch() {
        UUID a = newPlayer();
        ZSetOperations<String, String> z = zSet();
        when(z.range(eq(MatchmakingService.QUEUE_KEY), anyLong(), anyLong()))
                .thenReturn(new LinkedHashSet<>(Set.of(a.toString())));

        assertTrue(matchmakingService.tryMatch().isEmpty());
    }

    @Test
    void redisDownStatusIsSafe() {
        when(redisTemplate.opsForZSet()).thenThrow(new RedisConnectionFailureException("down"));

        MatchmakingDtos.QueueStatus status = assertDoesNotThrow(
                () -> matchmakingService.status(UUID.randomUUID()));
        assertEquals(0, status.queueSize());
    }
}
