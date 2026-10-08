using UnityEngine;

namespace NitroRush.Cars
{
    /// <summary>
    /// Focused helper sub-component managing steering geometry, return force, and wheel turn angle math.
    /// </summary>
    public class CarSteering : MonoBehaviour
    {
        [Header("Geometry Settings")]
        [SerializeField] private float maxSteerAngle = 35.0f;
        [SerializeField] private float steerReturnSpeed = 8.0f;

        public float CurrentSteerAngle { get; private set; }

        public void UpdateSteering(float inputSteer, float currentSpeed, float topSpeed)
        {
            // Reduce maximum physical wheel angle at high speeds to prevent rollover
            float speedRatio = Mathf.Clamp01(currentSpeed / (topSpeed + 1e-5f));
            float effectiveMaxAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.4f, speedRatio);

            float targetAngle = inputSteer * effectiveMaxAngle;
            CurrentSteerAngle = Mathf.MoveTowards(CurrentSteerAngle, targetAngle, steerReturnSpeed * maxSteerAngle * Time.deltaTime);
        }
    }
}
