package com.nitrorush.events;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "event_progress", uniqueConstraints = @UniqueConstraint(name = "uk_event_progress", columnNames = {"player_id", "event_id", "objective_type"}))
public class EventProgress {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "player_id", nullable = false)
    private UUID playerId;

    @Column(name = "event_id", nullable = false)
    private UUID eventId;

    @Enumerated(EnumType.STRING)
    @Column(name = "objective_type", nullable = false)
    private ObjectiveType objectiveType;

    @Column(nullable = false)
    private int progress;

    private Instant completedAt;

    public EventProgress() {
    }

    public EventProgress(UUID playerId, UUID eventId, ObjectiveType objectiveType) {
        this.playerId = playerId;
        this.eventId = eventId;
        this.objectiveType = objectiveType;
        this.progress = 0;
    }

    public UUID getId() { return id; }
    public UUID getPlayerId() { return playerId; }
    public UUID getEventId() { return eventId; }
    public ObjectiveType getObjectiveType() { return objectiveType; }
    public int getProgress() { return progress; }
    public void setProgress(int progress) { this.progress = progress; }
    public Instant getCompletedAt() { return completedAt; }
    public void setCompletedAt(Instant completedAt) { this.completedAt = completedAt; }
}
