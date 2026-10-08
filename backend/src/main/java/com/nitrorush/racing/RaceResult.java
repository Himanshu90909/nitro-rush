package com.nitrorush.racing;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "race_results", uniqueConstraints = @UniqueConstraint(name = "uk_race_result", columnNames = {"race_id", "player_id"}))
public class RaceResult {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "race_id", nullable = false)
    private UUID raceId;

    @Column(name = "player_id", nullable = false)
    private UUID playerId;

    private int position;

    /** 0 = did not finish. */
    @Column(name = "finish_time_ms", nullable = false)
    private long finishTimeMs;

    private long score;
    private double maxSpeedObserved;

    @Column(nullable = false, updatable = false)
    private Instant createdAt;

    public RaceResult() {
    }

    public RaceResult(UUID raceId, UUID playerId, int position, long finishTimeMs, long score, double maxSpeedObserved) {
        this.raceId = raceId;
        this.playerId = playerId;
        this.position = position;
        this.finishTimeMs = finishTimeMs;
        this.score = score;
        this.maxSpeedObserved = maxSpeedObserved;
        this.createdAt = Instant.now();
    }

    public UUID getId() { return id; }
    public UUID getRaceId() { return raceId; }
    public UUID getPlayerId() { return playerId; }
    public int getPosition() { return position; }
    public long getFinishTimeMs() { return finishTimeMs; }
    public long getScore() { return score; }
    public double getMaxSpeedObserved() { return maxSpeedObserved; }
    public Instant getCreatedAt() { return createdAt; }
}
