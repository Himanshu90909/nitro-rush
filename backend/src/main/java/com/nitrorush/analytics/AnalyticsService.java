package com.nitrorush.analytics;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.time.temporal.ChronoUnit;
import java.util.*;
import java.util.stream.Collectors;

/**
 * Telemetry ingestion. Payloads are sanitized: any key containing
 * email/password/token is dropped server-side, and unknown event types are
 * rejected — the client enum is mirrored but the server never trusts it.
 */
@Service
public class AnalyticsService {

    private static final Logger log = LoggerFactory.getLogger(AnalyticsService.class);
    private static final Set<String> FORBIDDEN_KEY_FRAGMENTS = Set.of("email", "password", "token", "secret", "ssn");

    private final AnalyticsEventRepository repository;
    private final ObjectMapper objectMapper;

    public AnalyticsService(AnalyticsEventRepository repository, ObjectMapper objectMapper) {
        this.repository = repository;
        this.objectMapper = objectMapper;
    }

    @Transactional
    public void record(UUID playerId, String eventType, Map<String, Object> rawPayload) {
        if (!AnalyticsDtos.ALLOWED_TYPES.contains(eventType)) {
            log.debug("Dropped telemetry of unknown type {}", eventType);
            return;
        }
        try {
            Map<String, Object> clean = sanitize(rawPayload);
            repository.save(new AnalyticsEvent(playerId, eventType,
                    clean.isEmpty() ? "{}" : objectMapper.writeValueAsString(clean)));
        } catch (Exception e) {
            log.debug("Telemetry write failed: {}", e.getMessage());
        }
    }

    @Transactional(readOnly = true)
    public AnalyticsDtos.PlayerAnalyticsResponse playerSummary(UUID playerId) {
        Map<String, Long> counts = new LinkedHashMap<>();
        for (AnalyticsEvent event : repository.findByPlayerIdOrderByCreatedAtDesc(playerId)) {
            counts.merge(event.getEventType(), 1L, Long::sum);
        }
        return new AnalyticsDtos.PlayerAnalyticsResponse(counts);
    }

    @Transactional(readOnly = true)
    public AnalyticsDtos.AnalyticsSummary adminSummary() {
        Instant today = Instant.now().minus(24, ChronoUnit.HOURS);

        Map<String, Long> byType = new LinkedHashMap<>();
        long total = 0;
        for (String type : AnalyticsDtos.ALLOWED_TYPES) {
            long count = repository.countByEventType(type);
            if (count > 0) {
                byType.put(type, count);
                total += count;
            }
        }

        long eventsToday = repository.countByCreatedAtAfter(today);
        long activePlayersToday = repository.findByEventTypeAndCreatedAtAfter("player_login", today).stream()
                .map(AnalyticsEvent::getPlayerId)
                .filter(Objects::nonNull)
                .collect(Collectors.toSet())
                .size();

        return new AnalyticsDtos.AnalyticsSummary(total, byType, eventsToday, activePlayersToday);
    }

    static Map<String, Object> sanitize(Map<String, Object> payload) {
        if (payload == null) {
            return Map.of();
        }
        Map<String, Object> clean = new LinkedHashMap<>();
        for (Map.Entry<String, Object> entry : payload.entrySet()) {
            String key = entry.getKey() == null ? "" : entry.getKey().toLowerCase();
            if (FORBIDDEN_KEY_FRAGMENTS.stream().anyMatch(key::contains)) {
                continue; // PII / secret-looking keys are silently dropped
            }
            clean.put(entry.getKey(), entry.getValue());
        }
        return clean;
    }
}
