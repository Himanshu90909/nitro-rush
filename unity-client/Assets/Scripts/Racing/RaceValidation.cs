using System;
using UnityEngine;

namespace NitroRush.Racing
{
    /// <summary>
    /// Pure C# static validation routines shared between client-side simulation,
    /// NUnit unit tests, and server anti-cheat verification.
    /// </summary>
    public static class RaceValidation
    {
        /// <summary>
        /// Validates that a reported checkpoint index matches expected sequential progression order.
        /// Expected next index = (currentCheckpointIndex + 1) % totalCheckpoints.
        /// Rejects out-of-order skipping to prevent track shortcut exploit hacks.
        /// </summary>
        public static bool IsCheckpointOrderValid(int lastPassedCheckpoint, int reportedCheckpoint, int totalCheckpoints)
        {
            if (totalCheckpoints <= 0) return false;
            if (reportedCheckpoint < 0 || reportedCheckpoint >= totalCheckpoints) return false;

            int expectedNext = (lastPassedCheckpoint + 1) % totalCheckpoints;
            return reportedCheckpoint == expectedNext;
        }

        /// <summary>
        /// Validates lap progression step logic.
        /// </summary>
        public static bool IsLapProgressValid(int currentLap, int reportedLap, int totalLaps, bool allCheckpointsPassed)
        {
            if (reportedLap < currentLap) return false;
            if (reportedLap > totalLaps + 1) return false;

            if (reportedLap == currentLap + 1)
            {
                return allCheckpointsPassed;
            }

            return reportedLap == currentLap;
        }

        /// <summary>
        /// Validates whether a vehicle velocity reading is physically plausible given maximum top speed specs.
        /// </summary>
        public static bool IsPlausibleSpeed(float currentSpeed, float maxTheoreticalSpeed, float tolerance = 1.25f)
        {
            if (currentSpeed < 0f) return false;
            float upperLimit = maxTheoreticalSpeed * tolerance;
            return currentSpeed <= upperLimit;
        }

        /// <summary>
        /// Validates minimum elapsed time to cover a track segment distance.
        /// t_min = distance / (v_max * tolerance)
        /// </summary>
        public static bool IsPlausibleSegmentTime(float segmentDistanceMeters, float timeTakenSeconds, float maxPossibleSpeedMps)
        {
            if (timeTakenSeconds <= 0.001f) return false;
            float minPossibleTime = segmentDistanceMeters / (maxPossibleSpeedMps * 1.30f);
            return timeTakenSeconds >= minPossibleTime;
        }
    }
}
