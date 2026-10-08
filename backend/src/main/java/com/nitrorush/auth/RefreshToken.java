package com.nitrorush.auth;

import jakarta.persistence.*;
import java.time.Instant;
import java.util.UUID;

/** Single-use refresh token. Only the SHA-256 hash of the token is stored. */
@Entity
@Table(name = "refresh_tokens", indexes = @Index(name = "idx_refresh_token_hash", columnList = "tokenHash"))
public class RefreshToken {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(nullable = false)
    private UUID userId;

    /** SHA-256 hex of the raw token — never store the raw token. */
    @Column(nullable = false, length = 64)
    private String tokenHash;

    @Column(nullable = false)
    private Instant expiry;

    @Column(nullable = false)
    private boolean used;

    @Column(nullable = false, updatable = false)
    private Instant createdAt;

    public RefreshToken() {
    }

    public RefreshToken(UUID userId, String tokenHash, Instant expiry) {
        this.userId = userId;
        this.tokenHash = tokenHash;
        this.expiry = expiry;
        this.used = false;
        this.createdAt = Instant.now();
    }

    public UUID getId() { return id; }
    public UUID getUserId() { return userId; }
    public String getTokenHash() { return tokenHash; }
    public Instant getExpiry() { return expiry; }
    public boolean isUsed() { return used; }
    public void setUsed(boolean used) { this.used = used; }
    public Instant getCreatedAt() { return createdAt; }
}
