package com.nitrorush.analytics;

import java.util.List;
import java.util.Map;
import java.util.UUID;

public final class AnalyticsDtos {
    private AnalyticsDtos() {}

    /** Allowed telemetry types — the enum in Unity TelemetryLogger matches this list. */
    public static final List<String> ALLOWED_TYPES = List.of(
            "race_started", "race_finished", "race_rejected", "car_selected", "upgrade_purchased",
            "event_joined", "reward_claimed", "player_login", "matchmaking_started",
            "matchmaking_completed", "nitro_used", "drift_performed");

    public record TelemetryEvent(String eventType, Map<String, Object> payload) {}

    public record TelemetryBatch(UUID playerId, List<TelemetryEvent> events) {}

    public record PlayerAnalyticsResponse(Map<String, Long> eventCounts) {}

    public record AnalyticsSummary(long totalEvents, Map<String, Long> byType,
                                    long eventsToday, long activePlayersToday) {}
}
