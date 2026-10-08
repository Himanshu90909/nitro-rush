using UnityEngine;

namespace NitroRush.Cars
{
    /// <summary>
    /// Focused helper sub-component managing simulated engine RPM math and power band delivery.
    /// </summary>
    public class CarMotor : MonoBehaviour
    {
        [Header("Engine Powerband")]
        [SerializeField] private float idleRpm = 1000.0f;
        [SerializeField] private float maxRpm = 8000.0f;

        public float CurrentRpm { get; private set; }

        public void UpdateMotor(float currentSpeed, float topSpeed, float throttle)
        {
            float speedPercent = Mathf.Clamp01(currentSpeed / (topSpeed + 1e-5f));
            
            // Simulated RPM pitch curve based on speed ratio and throttle load
            float targetRpm = Mathf.Lerp(idleRpm, maxRpm, speedPercent);
            if (throttle > 0f)
            {
                targetRpm += 500f * throttle;
            }

            CurrentRpm = Mathf.Lerp(CurrentRpm, Mathf.Min(maxRpm, targetRpm), Time.deltaTime * 5.0f);
        }
    }
}
