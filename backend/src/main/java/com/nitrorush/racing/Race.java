package com.nitrorush.racing;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "races")
public class Race {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(nullable = false, length = 64)
    private String trackId;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private RaceType raceType;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private RaceStatus status = RaceStatus.IN_PROGRESS;

    @Column(nullable = false, updatable = false)
    private Instant startedAt;

    private Instant completedAt;

    public Race() {
    }

    public Race(String trackId, RaceType raceType) {
        this.trackId = trackId;
        this.raceType = raceType;
        this.startedAt = Instant.now();
    }

    public UUID getId() { return id; }
    public String getTrackId() { return trackId; }
    public RaceType getRaceType() { return raceType; }
    public RaceStatus getStatus() { return status; }
    public void setStatus(RaceStatus status) { this.status = status; }
    public Instant getStartedAt() { return startedAt; }
    public Instant getCompletedAt() { return completedAt; }
    public void setCompletedAt(Instant completedAt) { this.completedAt = completedAt; }
}
