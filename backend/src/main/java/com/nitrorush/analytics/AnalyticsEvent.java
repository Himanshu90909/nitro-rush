package com.nitrorush.analytics;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

/** Telemetry row. Payloads must never contain PII (sanitized in AnalyticsService). */
@Entity
@Table(name = "analytics_events", indexes = {
        @Index(name = "idx_analytics_type", columnList = "event_type"),
        @Index(name = "idx_analytics_created", columnList = "created_at")
})
public class AnalyticsEvent {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    private UUID playerId;

    @Column(name = "event_type", nullable = false, length = 48)
    private String eventType;

    @Column(columnDefinition = "text")
    private String payload;

    @Column(name = "created_at", nullable = false, updatable = false)
    private Instant createdAt;

    public AnalyticsEvent() {
    }

    public AnalyticsEvent(UUID playerId, String eventType, String payload) {
        this.playerId = playerId;
        this.eventType = eventType;
        this.payload = payload;
        this.createdAt = Instant.now();
    }

    public UUID getId() { return id; }
    public UUID getPlayerId() { return playerId; }
    public String getEventType() { return eventType; }
    public String getPayload() { return payload; }
    public Instant getCreatedAt() { return createdAt; }
}
