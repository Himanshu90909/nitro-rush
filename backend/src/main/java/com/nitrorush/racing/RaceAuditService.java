package com.nitrorush.racing;

import com.nitrorush.analytics.AnalyticsService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Propagation;
import org.springframework.transaction.annotation.Transactional;

import java.util.Map;
import java.util.UUID;

/**
 * Persists cheat-detection outcomes in its OWN transaction. RaceService throws
 * after calling this — REQUIRES_NEW guarantees the FLAGGED status and the
 * telemetry event survive the caller's rollback.
 */
@Service
public class RaceAuditService {

    private static final Logger log = LoggerFactory.getLogger(RaceAuditService.class);

    private final RaceRepository raceRepository;
    private final AnalyticsService analyticsService;

    public RaceAuditService(RaceRepository raceRepository, AnalyticsService analyticsService) {
        this.raceRepository = raceRepository;
        this.analyticsService = analyticsService;
    }

    @Transactional(propagation = Propagation.REQUIRES_NEW)
    public void markFlagged(UUID raceId, UUID playerId, String reason) {
        raceRepository.findById(raceId).ifPresent(race -> {
            race.setStatus(RaceStatus.FLAGGED);
            raceRepository.save(race);
        });
        analyticsService.record(playerId, "race_rejected", Map.of("reason", String.valueOf(reason)));
        log.warn("REJECTED race result from player {} on race {}: {}", playerId, raceId, reason);
    }
}
