package com.nitrorush.auth;

import com.nitrorush.common.ApiException;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;

import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

@SpringBootTest
@ActiveProfiles("test")
class AuthServiceTests {

    @Autowired
    private AuthService authService;
    @Autowired
    private UserRepository userRepository;
    @Autowired
    private PlayerProfileRepository profileRepository;
    @Autowired
    private RefreshTokenRepository refreshTokenRepository;

    private AuthDtos.RegisterRequest registerRequest() {
        String email = "player-" + UUID.randomUUID() + "@test.dev";
        return new AuthDtos.RegisterRequest(email, "supersecret1", "tester");
    }

    @Test
    void registerCreatesUserAndProfile() {
        AuthDtos.RegisterRequest request = registerRequest();
        AuthDtos.AuthResponse response = authService.register(request);

        assertNotNull(response.accessToken());
        assertNotNull(response.refreshToken());
        assertEquals("PLAYER", response.user().role());
        assertTrue(profileRepository.findByUserId(response.user().id()).isPresent());
    }

    @Test
    void duplicateEmailIsRejected() {
        AuthDtos.RegisterRequest request = registerRequest();
        authService.register(request);
        ApiException e = assertThrows(ApiException.class,
                () -> authService.register(new AuthDtos.RegisterRequest(request.email(), "otherpass1", "other")));
        assertEquals("EMAIL_TAKEN", e.getErrorCode().name());
    }

    @Test
    void loginWithWrongPasswordFails() {
        AuthDtos.RegisterRequest request = registerRequest();
        authService.register(request);
        ApiException e = assertThrows(ApiException.class,
                () -> authService.login(new AuthDtos.LoginRequest(request.email(), "wrongpass1")));
        assertEquals("INVALID_CREDENTIALS", e.getErrorCode().name());
    }

    @Test
    void loginWithCorrectPasswordReturnsTokens() {
        AuthDtos.RegisterRequest request = registerRequest();
        authService.register(request);
        AuthDtos.AuthResponse response = authService.login(new AuthDtos.LoginRequest(request.email(), "supersecret1"));
        assertNotNull(response.accessToken());
        assertEquals(request.email(), response.user().email());
    }

    @Test
    void refreshRotatesOldToken() {
        AuthDtos.AuthResponse first = authService.register(registerRequest());
        AuthDtos.AuthResponse second = authService.refresh(new AuthDtos.RefreshRequest(first.refreshToken()));

        assertNotNull(second.refreshToken());
        assertNotEquals(first.refreshToken(), second.refreshToken(), "rotation must issue a new refresh token");

        // Old token is single-use: reuse revokes everything.
        ApiException e = assertThrows(ApiException.class,
                () -> authService.refresh(new AuthDtos.RefreshRequest(first.refreshToken())));
        assertEquals("INVALID_TOKEN", e.getErrorCode().name());

        // Reuse also revoked the new family token.
        assertThrows(ApiException.class,
                () -> authService.refresh(new AuthDtos.RefreshRequest(second.refreshToken())));
    }

    @Test
    void logoutInvalidatesRefreshToken() {
        AuthDtos.AuthResponse response = authService.register(registerRequest());
        authService.logout(new AuthDtos.RefreshRequest(response.refreshToken()));
        ApiException e = assertThrows(ApiException.class,
                () -> authService.refresh(new AuthDtos.RefreshRequest(response.refreshToken())));
        assertEquals("INVALID_TOKEN", e.getErrorCode().name());
    }

    @Test
    void storedRefreshTokenIsHashedNotRaw() {
        AuthDtos.AuthResponse response = authService.register(registerRequest());
        String storedHash = refreshTokenRepository
                .findByTokenHash(AuthService.sha256(response.refreshToken()))
                .orElseThrow().getTokenHash();
        assertNotEquals(response.refreshToken(), storedHash);
        assertEquals(64, storedHash.length(), "SHA-256 hex digest");
    }
}
