package com.nitrorush.racing;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

/** The most important tests in the project: server-side race validation. */
class AntiCheatValidatorTests {

    // Track assumptions: par 90s, car top speed 55 m/s, 12 checkpoints/lap, 3 laps.
    private static final long PAR = 90_000;
    private static final double TOP = 55.0;
    private static final int CPS = 12;
    private static final int LAPS = 3;

    @Test
    void honestRaceIsValid() {
        var v = AntiCheatValidator.validate(75_000, PAR, 55.0, TOP, CPS * LAPS, CPS, LAPS, LAPS);
        assertTrue(v.valid());
        assertFalse(v.flagged());
        assertNull(v.reason());
    }

    @Test
    void nitroSpeedWithinToleranceIsAccepted() {
        var v = AntiCheatValidator.validate(80_000, PAR, TOP * 1.30, TOP, CPS * LAPS, CPS, LAPS, LAPS);
        assertTrue(v.valid());
    }

    @Test
    void slightlySuspiciousSpeedIsFlaggedNotRejected() {
        var v = AntiCheatValidator.validate(80_000, PAR, TOP * 1.12, TOP, CPS * LAPS, CPS, LAPS, LAPS);
        assertTrue(v.valid());
        assertTrue(v.flagged(), "1.12x speed is suspicious but plausible (nitro) -> FLAGGED, not rejected");
        assertNotNull(v.reason());
    }

    @Test
    void speedOverHardCapIsRejected() {
        var v = AntiCheatValidator.validate(80_000, PAR, TOP * 1.40, TOP, CPS * LAPS, CPS, LAPS, LAPS);
        assertFalse(v.valid());
    }

    @Test
    void impossiblyFastTimeIsRejected() {
        // 35% of par = 31.5s floor; anything below is physically impossible.
        var v = AntiCheatValidator.validate(20_000, PAR, 55.0, TOP, CPS * LAPS, CPS, LAPS, LAPS);
        assertFalse(v.valid());
    }

    @Test
    void veryFastButAboveFloorIsFlagged() {
        // 44s is above the floor but below 50% of par -> flagged for review.
        var v = AntiCheatValidator.validate(44_000, PAR, 55.0, TOP, CPS * LAPS, CPS, LAPS, LAPS);
        assertTrue(v.valid());
        assertTrue(v.flagged());
    }

    @Test
    void skippedCheckpointsAreRejected() {
        var v = AntiCheatValidator.validate(75_000, PAR, 55.0, TOP, CPS * LAPS - 3, CPS, LAPS, LAPS);
        assertFalse(v.valid(), "Checkpoint count mismatch means skipping -> reject");
    }

    @Test
    void tooManyCheckpointsAlsoRejected() {
        var v = AntiCheatValidator.validate(75_000, PAR, 55.0, TOP, CPS * LAPS + 1, CPS, LAPS, LAPS);
        assertFalse(v.valid());
    }

    @Test
    void zeroLapsRejected() {
        var v = AntiCheatValidator.validate(75_000, PAR, 55.0, TOP, CPS, CPS, 0, LAPS);
        assertFalse(v.valid());
    }

    @Test
    void excessiveLapsRejected() {
        var v = AntiCheatValidator.validate(75_000, PAR, 55.0, TOP, CPS * 10, CPS, 10, LAPS);
        assertFalse(v.valid());
    }

    @Test
    void zeroFinishTimeRejected() {
        var v = AntiCheatValidator.validate(0, PAR, 55.0, TOP, CPS * LAPS, CPS, LAPS, LAPS);
        assertFalse(v.valid());
    }
}
