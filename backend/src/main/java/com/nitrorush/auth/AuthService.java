package com.nitrorush.auth;

import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;
import java.time.Instant;
import java.util.Base64;
import java.util.HexFormat;
import java.util.UUID;

/**
 * Registration, login and refresh-token rotation.
 *
 * Security model:
 * - passwords hashed with BCrypt (never stored or logged raw)
 * - refresh tokens are random 256-bit values; only their SHA-256 hash is persisted
 * - refresh tokens are single-use: each refresh marks the old token used and
 *   issues a fresh one (rotation limits replay windows)
 */
@Service
public class AuthService {

    private static final Logger log = LoggerFactory.getLogger(AuthService.class);
    private static final SecureRandom RANDOM = new SecureRandom();
    private static final long REFRESH_DAYS = 7;

    private final UserRepository userRepository;
    private final RefreshTokenRepository refreshTokenRepository;
    private final TokenRevoker tokenRevoker;
    private final PlayerProfileRepository playerProfileRepository;
    private final JwtService jwtService;
    private final PasswordEncoder passwordEncoder;

    public AuthService(UserRepository userRepository,
                       RefreshTokenRepository refreshTokenRepository,
                       TokenRevoker tokenRevoker,
                       PlayerProfileRepository playerProfileRepository,
                       JwtService jwtService,
                       PasswordEncoder passwordEncoder) {
        this.userRepository = userRepository;
        this.refreshTokenRepository = refreshTokenRepository;
        this.tokenRevoker = tokenRevoker;
        this.playerProfileRepository = playerProfileRepository;
        this.jwtService = jwtService;
        this.passwordEncoder = passwordEncoder;
    }

    @Transactional
    public AuthDtos.AuthResponse register(AuthDtos.RegisterRequest request) {
        if (userRepository.existsByEmailIgnoreCase(request.email())) {
            throw ApiException.of(ErrorCode.EMAIL_TAKEN, "An account with this email already exists");
        }
        User user = new User(request.email().trim().toLowerCase(),
                passwordEncoder.encode(request.password()), Role.PLAYER);
        user = userRepository.save(user);

        PlayerProfile profile = new PlayerProfile(user, request.username().trim());
        playerProfileRepository.save(profile);

        log.info("Registered new player userId={}", user.getId());
        return buildAuthResponse(user);
    }

    @Transactional
    public AuthDtos.AuthResponse login(AuthDtos.LoginRequest request) {
        User user = userRepository.findByEmailIgnoreCase(request.email())
                .orElseThrow(() -> ApiException.of(ErrorCode.INVALID_CREDENTIALS, "Invalid email or password"));

        if (!passwordEncoder.matches(request.password(), user.getPasswordHash())) {
            throw ApiException.of(ErrorCode.INVALID_CREDENTIALS, "Invalid email or password");
        }

        user.setLastLogin(Instant.now());
        userRepository.save(user);
        log.info("Login userId={}", user.getId());
        return buildAuthResponse(user);
    }

    /** Rotates a refresh token: single-use, old token is invalidated. */
    @Transactional
    public AuthDtos.AuthResponse refresh(AuthDtos.RefreshRequest request) {
        String hash = sha256(request.refreshToken());
        RefreshToken stored = refreshTokenRepository.findByTokenHash(hash)
                .orElseThrow(() -> ApiException.of(ErrorCode.INVALID_TOKEN, "Invalid refresh token"));

        if (stored.isUsed()) {
            // Reuse of a rotated token is suspicious: revoke all of the user's
            // sessions. REQUIRES_NEW — the throw below must not roll this back.
            tokenRevoker.revokeAllForUser(stored.getUserId());
            throw ApiException.of(ErrorCode.INVALID_TOKEN, "Refresh token already used — sessions revoked");
        }
        if (stored.getExpiry().isBefore(Instant.now())) {
            throw ApiException.of(ErrorCode.INVALID_TOKEN, "Refresh token expired");
        }

        stored.setUsed(true);
        refreshTokenRepository.save(stored);

        User user = userRepository.findById(stored.getUserId())
                .orElseThrow(() -> ApiException.of(ErrorCode.INVALID_TOKEN, "User no longer exists"));
        return buildAuthResponse(user);
    }

    @Transactional
    public void logout(AuthDtos.RefreshRequest request) {
        refreshTokenRepository.findByTokenHash(sha256(request.refreshToken()))
                .ifPresent(token -> {
                    token.setUsed(true);
                    refreshTokenRepository.save(token);
                });
    }

    private AuthDtos.AuthResponse buildAuthResponse(User user) {
        String access = jwtService.issueAccessToken(user.getId(), user.getRole());
        String refresh = newRefreshToken(user.getId());
        return new AuthDtos.AuthResponse(access, refresh,
                new AuthDtos.UserDto(user.getId(), user.getEmail(), user.getRole().name()));
    }

    private String newRefreshToken(UUID userId) {
        byte[] bytes = new byte[32];
        RANDOM.nextBytes(bytes);
        String raw = Base64.getUrlEncoder().withoutPadding().encodeToString(bytes);
        refreshTokenRepository.save(new RefreshToken(userId, sha256(raw), Instant.now().plusSeconds(REFRESH_DAYS * 86400)));
        return raw;
    }

    static String sha256(String value) {
        try {
            MessageDigest digest = MessageDigest.getInstance("SHA-256");
            return HexFormat.of().formatHex(digest.digest(value.getBytes(StandardCharsets.UTF_8)));
        } catch (NoSuchAlgorithmException e) {
            throw new IllegalStateException("SHA-256 unavailable", e);
        }
    }
}
