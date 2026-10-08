package com.nitrorush.analytics;

import org.junit.jupiter.api.Test;

import java.time.Instant;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

class AnalyticsServiceTests {

    @Test
    void sanitizeDropsPiiLookingKeys() {
        Map<String, Object> payload = new LinkedHashMap<>();
        payload.put("trackId", "neon-city");
        payload.put("email", "player@example.com");
        payload.put("password", "hunter2");
        payload.put("accessToken", "jwt...");
        payload.put("position", 1);

        Map<String, Object> clean = AnalyticsService.sanitize(payload);

        assertEquals("neon-city", clean.get("trackId"));
        assertEquals(1, clean.get("position"));
        assertFalse(clean.containsKey("email"));
        assertFalse(clean.containsKey("password"));
        assertFalse(clean.containsKey("accessToken"));
        assertEquals(2, clean.size());
    }

    @Test
    void sanitizeHandlesNullAndEmpty() {
        assertTrue(AnalyticsService.sanitize(null).isEmpty());
        assertTrue(AnalyticsService.sanitize(Map.of()).isEmpty());
    }

    @Test
    void allowedTypesCoverTheGameplayLoop() {
        assertTrue(AnalyticsDtos.ALLOWED_TYPES.contains("race_started"));
        assertTrue(AnalyticsDtos.ALLOWED_TYPES.contains("matchmaking_completed"));
        assertFalse(AnalyticsDtos.ALLOWED_TYPES.contains("password_hashed"));
    }
}
