package com.nitrorush.analytics;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/v1")
public class AnalyticsController {

    private final AnalyticsService analyticsService;

    public AnalyticsController(AnalyticsService analyticsService) {
        this.analyticsService = analyticsService;
    }

    /** Batched telemetry ingest. Events are sanitized and type-checked server-side. */
    @PostMapping("/events")
    public ApiResponse<Void> ingest(@RequestBody AnalyticsDtos.TelemetryBatch batch) {
        UUID playerId = null;
        try {
            playerId = SecurityUtils.currentUserId();
        } catch (Exception ignored) {
            // Anonymous telemetry is allowed but scoped to nothing identifiable.
        }
        if (batch != null && batch.events() != null) {
            for (AnalyticsDtos.TelemetryEvent event : batch.events()) {
                analyticsService.record(playerId, event.eventType(), event.payload());
            }
        }
        return ApiResponse.ok(null);
    }

    @GetMapping("/player")
    public ApiResponse<AnalyticsDtos.PlayerAnalyticsResponse> player() {
        return ApiResponse.ok(analyticsService.playerSummary(SecurityUtils.currentUserId()));
    }

    @GetMapping("/admin/analytics/summary")
    public ApiResponse<AnalyticsDtos.AnalyticsSummary> adminSummary() {
        return ApiResponse.ok(analyticsService.adminSummary());
    }
}
