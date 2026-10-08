package com.nitrorush.auth;

import com.nitrorush.common.ApiException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

class JwtServiceTests {

    private JwtService jwtService;

    @BeforeEach
    void setUp() {
        jwtService = new JwtService("test-secret-key-that-is-long-enough-32-bytes!", 15);
    }

    @Test
    void accessTokenRoundTrip() {
        UUID userId = UUID.randomUUID();
        String token = jwtService.issueAccessToken(userId, Role.PLAYER);

        JwtService.TokenPayload payload = jwtService.parseAccessToken(token);
        assertEquals(userId, payload.userId());
        assertEquals(Role.PLAYER, payload.role());
        assertNotNull(payload.expiry());
    }

    @Test
    void tamperedTokenIsRejected() {
        String token = jwtService.issueAccessToken(UUID.randomUUID(), Role.PLAYER);
        String tampered = token.substring(0, token.length() - 2) + "zz";
        assertThrows(ApiException.class, () -> jwtService.parseAccessToken(tampered));
    }

    @Test
    void garbageTokenIsRejected() {
        assertThrows(ApiException.class, () -> jwtService.parseAccessToken("not-a-jwt"));
    }

    @Test
    void expiredTokenIsRejected() {
        JwtService shortLived = new JwtService("test-secret-key-that-is-long-enough-32-bytes!", 0);
        String token = shortLived.issueAccessToken(UUID.randomUUID(), Role.ADMIN);
        assertThrows(ApiException.class, () -> jwtService.parseAccessToken(token));
    }

    @Test
    void secretTooShortIsRejected() {
        assertThrows(IllegalArgumentException.class, () -> new JwtService("short", 15));
    }
}
