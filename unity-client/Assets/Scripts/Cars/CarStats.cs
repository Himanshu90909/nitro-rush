using System;
using UnityEngine;

namespace NitroRush.Cars
{
    /// <summary>
    /// Runtime evaluated stats container calculated from base CarData ScriptableObject
    /// and current component upgrade levels (1 through 5).
    /// </summary>
    [Serializable]
    public struct CarStats
    {
        public int EngineLevel;
        public int TurboLevel;
        public int TiresLevel;
        public int BrakesLevel;
        public int NitroLevel;

        public float TopSpeed;
        public float Acceleration;
        public float Handling;
        public float Braking;
        public float NitroCapacity;
        public float NitroAcceleration;
        public float DriftControl;

        /// <summary>
        /// Computes all active runtime statistics using per-system upgrade formulas.
        /// </summary>
        public static CarStats FromData(CarData data, int engineLevel = 1, int turboLevel = 1, int tiresLevel = 1, int brakesLevel = 1, int nitroLevel = 1)
        {
            if (data == null)
            {
                return new CarStats();
            }

            var stats = new CarStats
            {
                EngineLevel = Mathf.Clamp(engineLevel, 1, 5),
                TurboLevel = Mathf.Clamp(turboLevel, 1, 5),
                TiresLevel = Mathf.Clamp(tiresLevel, 1, 5),
                BrakesLevel = Mathf.Clamp(brakesLevel, 1, 5),
                NitroLevel = Mathf.Clamp(nitroLevel, 1, 5)
            };

            // Calculate runtime values with percentage boosts per level
            // Engine increases top speed by 5% per level above 1
            stats.TopSpeed = data.GetUpgradedStat(data.topSpeed, stats.EngineLevel, 5.0f);

            // Turbo increases forward acceleration by 6% per level above 1
            stats.Acceleration = data.GetUpgradedStat(data.acceleration, stats.TurboLevel, 6.0f);

            // Tires increase handling turn responsiveness by 4% per level above 1
            stats.Handling = data.GetUpgradedStat(data.handling, stats.TiresLevel, 4.0f);

            // Brakes increase braking deceleration rate by 5% per level above 1
            stats.Braking = data.GetUpgradedStat(data.braking, stats.BrakesLevel, 5.0f);

            // Nitro upgrades increase nitro tank capacity by 8% per level above 1
            stats.NitroCapacity = data.GetUpgradedStat(data.nitroCapacity, stats.NitroLevel, 8.0f);

            // Nitro upgrades increase nitro acceleration boost force by 7% per level above 1
            stats.NitroAcceleration = data.GetUpgradedStat(data.nitroAcceleration, stats.NitroLevel, 7.0f);

            // Tires also increase drift control grip recovery by 5% per level above 1
            stats.DriftControl = data.GetUpgradedStat(data.driftControl, stats.TiresLevel, 5.0f);

            return stats;
        }
    }
}
