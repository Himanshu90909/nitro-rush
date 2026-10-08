package com.nitrorush.config;

import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.security.Keys;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import javax.crypto.SecretKey;
import java.nio.charset.StandardCharsets;
import java.util.UUID;

@Component
public class JwtTokenValidator implements TokenValidator {

    private final SecretKey secretKey;

    public JwtTokenValidator(@Value("${jwt.secret:nitro-rush-default-jwt-secret-key-32bytes-long!}") String secret) {
        byte[] keyBytes = secret.getBytes(StandardCharsets.UTF_8);
        if (keyBytes.length < 32) {
            byte[] padded = new byte[32];
            System.arraycopy(keyBytes, 0, padded, 0, keyBytes.length);
            keyBytes = padded;
        }
        this.secretKey = Keys.hmacShaKeyFor(keyBytes);
    }

    @Override
    public UUID validate(String token) {
        if (token == null || token.trim().isEmpty()) {
            return null;
        }
        if (token.startsWith("Bearer ")) {
            token = token.substring(7).trim();
        }
        try {
            Claims claims = Jwts.parser()
                    .verifyWith(secretKey)
                    .build()
                    .parseSignedClaims(token)
                    .getPayload();

            String sub = claims.getSubject();
            if (sub != null) {
                return UUID.fromString(sub);
            }
            Object userIdClaim = claims.get("userId");
            if (userIdClaim != null) {
                return UUID.fromString(userIdClaim.toString());
            }
        } catch (Exception e) {
            // Token parsing or verification failed
        }
        return null;
    }
}
