package com.nitrorush.auth;

import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.security.Keys;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import javax.crypto.SecretKey;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.Date;
import java.util.UUID;

/**
 * Issues and parses JWTs using jjwt 0.12.x.
 * Access tokens: short-lived (default 15 min), subject = userId, claim "role".
 * Refresh tokens: opaque random strings hashed with SHA-256 before storage
 * (see RefreshToken) — JWT is not used for refresh.
 */
@Service
public class JwtService {

    private final SecretKey secretKey;
    private final long expirationMinutes;

    public JwtService(@Value("${app.jwt.secret}") String secret,
                      @Value("${app.jwt.expiration-min:15}") long expirationMinutes) {
        byte[] keyBytes = secret.getBytes(StandardCharsets.UTF_8);
        if (keyBytes.length < 32) {
            throw new IllegalArgumentException("app.jwt.secret must be at least 32 bytes");
        }
        this.secretKey = Keys.hmacShaKeyFor(keyBytes);
        this.expirationMinutes = expirationMinutes;
    }

    /** Access token with sub = userId and role claim. */
    public String issueAccessToken(UUID userId, Role role) {
        Instant now = Instant.now();
        return Jwts.builder()
                .subject(userId.toString())
                .claim("role", role.name())
                .issuedAt(Date.from(now))
                .expiration(Date.from(now.plusSeconds(expirationMinutes * 60)))
                .signWith(secretKey)
                .compact();
    }

    /** Parsed access-token payload. Throws ApiException on invalid/expired tokens. */
    public TokenPayload parseAccessToken(String token) {
        try {
            Claims claims = Jwts.parser()
                    .verifyWith(secretKey)
                    .build()
                    .parseSignedClaims(token)
                    .getPayload();
            return new TokenPayload(
                    UUID.fromString(claims.getSubject()),
                    Role.valueOf(claims.get("role", String.class)),
                    claims.getExpiration().toInstant());
        } catch (ApiException e) {
            throw e;
        } catch (Exception e) {
            throw ApiException.of(ErrorCode.INVALID_TOKEN, "Invalid or expired token");
        }
    }

    public long getExpirationMinutes() {
        return expirationMinutes;
    }

    /** Immutable token payload. */
    public record TokenPayload(UUID userId, Role role, Instant expiry) {}
}
