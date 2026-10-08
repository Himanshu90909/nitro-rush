using UnityEngine;

namespace NitroRush.Cars
{
    public enum CarRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// ScriptableObject defining immutable base specifications and pricing tier for a vehicle model.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCarData", menuName = "NitroRush/Car Data")]
    public class CarData : ScriptableObject
    {
        [Header("Vehicle Identity")]
        public string carId = "car_falcon_gt";
        public string displayName = "Falcon GT";
        public CarRarity rarity = CarRarity.Rare;
        public int price = 25000;

        [Header("Base Performance Stats")]
        [Tooltip("Top speed in meters per second (e.g. 70 m/s = ~252 km/h)")]
        public float topSpeed = 70.0f;

        [Tooltip("Forward acceleration in m/s^2")]
        public float acceleration = 18.0f;

        [Tooltip("Steering torque responsiveness multiplier")]
        public float handling = 12.0f;

        [Tooltip("Braking deceleration rate in m/s^2")]
        public float braking = 25.0f;

        [Tooltip("Total nitro tank capacity in seconds of continuous burn")]
        public float nitroCapacity = 8.0f;

        [Tooltip("Additional forward acceleration added while burning nitro in m/s^2")]
        public float nitroAcceleration = 12.0f;

        [Tooltip("Lateral grip recovery modifier while in drift state")]
        public float driftControl = 1.0f;

        /// <summary>
        /// Calculates upgraded stat value based on upgrade level and per-level percentage scaling formula.
        /// Formula: stat = baseValue * (1 + (level - 1) * (perLevelPercent / 100))
        /// Level 1 returns baseValue (0% increase). Level 5 returns baseValue * (1 + 4 * percent).
        /// </summary>
        /// <param name="baseValue">Base stat value from ScriptableObject.</param>
        /// <param name="upgradeLevel">Upgrade level clamped between 1 and 5.</param>
        /// <param name="perLevelPercent">Percentage increase added per level beyond level 1.</param>
        public float GetUpgradedStat(float baseValue, int upgradeLevel, float perLevelPercent)
        {
            int clampedLevel = Mathf.Clamp(upgradeLevel, 1, 5);
            float multiplier = 1.0f + ((clampedLevel - 1) * (perLevelPercent / 100.0f));
            return baseValue * multiplier;
        }
    }
}
