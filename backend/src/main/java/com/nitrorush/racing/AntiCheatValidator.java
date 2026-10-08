package com.nitrorush.racing;

/**
 * Pure server-side race validation — fully unit-testable, no Spring.
 *
 * Rules (all tuned for the arcade model where nitro can exceed base top speed):
 * - finishTimeMs must be >= 35% of the track par time (physically impossible below that)
 * - maxSpeed is capped at 1.35x the car's top speed (nitro tolerance)
 * - between 1.10x and 1.35x is legal (nitro) but borderline -> race is FLAGGED for review
 * - checkpoint count must be exactly totalCheckpoints * laps (no skipping)
 * - laps must be within 1..totalLaps
 *
 * The same rules are mirrored client-side in RaceValidation.cs so honest clients
 * can fail fast without a round-trip — but the SERVER decision is authoritative.
 */
public final class AntiCheatValidator {

    private AntiCheatValidator() {}

    public record Validity(boolean valid, boolean flagged, String reason) {
        public static Validity ok() { return new Validity(true, false, null); }
        public static Validity flag(String reason) { return new Validity(true, true, reason); }
        public static Validity reject(String reason) { return new Validity(false, false, reason); }
    }

    public static Validity validate(long finishTimeMs, long parTimeMs,
                                     double maxSpeed, double carTopSpeed,
                                     int checkpointsPassed, int totalCheckpoints,
                                     int laps, int totalLaps) {

        if (parTimeMs <= 0) {
            return Validity.reject("Invalid par time configured for track");
        }
        if (laps < 1 || laps > totalLaps) {
            return Validity.reject("Lap count " + laps + " outside 1.." + totalLaps);
        }
        if (finishTimeMs <= 0) {
            return Validity.reject("Finish time must be positive");
        }
        if (checkpointsPassed != totalCheckpoints * laps) {
            return Validity.reject("Checkpoint mismatch: passed " + checkpointsPassed
                    + " expected " + (totalCheckpoints * laps));
        }
        if (finishTimeMs < parTimeMs * 0.35) {
            return Validity.reject("Finish time " + finishTimeMs + "ms is below the physical floor ("
                    + (long) (parTimeMs * 0.35) + "ms)");
        }
        if (maxSpeed > carTopSpeed * 1.35) {
            return Validity.reject("Max speed " + maxSpeed + " exceeds hard cap "
                    + String.format("%.1f", carTopSpeed * 1.35));
        }

        // Borderline but plausible -> accept and flag for review.
        if (maxSpeed > carTopSpeed * 1.10 || finishTimeMs < parTimeMs * 0.5) {
            return Validity.flag("Suspicious performance: maxSpeed=" + maxSpeed
                    + " topSpeed=" + carTopSpeed + " time=" + finishTimeMs + "ms");
        }
        return Validity.ok();
    }
}
