package com.nitrorush.rewards;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

/**
 * Idempotency ledger: one row per (player, sourceType, sourceId).
 * The UNIQUE constraint makes double-claiming impossible at the DB level.
 */
@Entity
@Table(name = "reward_claims",
        uniqueConstraints = @UniqueConstraint(name = "uk_claim", columnNames = {"player_id", "source_type", "source_id"}))
public class RewardClaim {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "player_id", nullable = false)
    private UUID playerId;

    @Column(name = "source_type", nullable = false, length = 16)
    private String sourceType;

    @Column(name = "source_id", nullable = false, length = 64)
    private String sourceId;

    @Column(nullable = false, updatable = false)
    private Instant claimedAt;

    public RewardClaim() {
    }

    public RewardClaim(UUID playerId, String sourceType, String sourceId) {
        this.playerId = playerId;
        this.sourceType = sourceType;
        this.sourceId = sourceId;
        this.claimedAt = Instant.now();
    }

    public UUID getId() { return id; }
    public UUID getPlayerId() { return playerId; }
    public String getSourceType() { return sourceType; }
    public String getSourceId() { return sourceId; }
    public Instant getClaimedAt() { return claimedAt; }
}
