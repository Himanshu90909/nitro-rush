package com.nitrorush.analytics;

import org.springframework.data.jpa.repository.JpaRepository;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

public interface AnalyticsEventRepository extends JpaRepository<AnalyticsEvent, UUID> {

    long countByEventType(String eventType);

    long countByCreatedAtAfter(Instant after);

    List<AnalyticsEvent> findByPlayerIdOrderByCreatedAtDesc(UUID playerId);

    List<AnalyticsEvent> findByEventTypeAndCreatedAtAfter(String eventType, Instant after);
}
