using UnityEngine;

namespace NitroRush.AI
{
    /// <summary>
    /// Handles overtaking logic by inspecting left and right clearances when blocked ahead.
    /// </summary>
    public class OvertakeSystem
    {
        private float _brakingDistance = 12f;

        public float CalculateOvertakeSteer(SensorData sensorData)
        {
            if (!sensorData.carAhead || sensorData.distance > _brakingDistance)
            {
                return 0f;
            }

            // Pick side with more room
            if (sensorData.leftClearance > sensorData.rightClearance)
            {
                return -0.8f; // Steer left
            }
            else
            {
                return 0.8f; // Steer right
            }
        }
    }
}
