using UnityEngine;

namespace NitroRush.AI
{
    public enum AIIntent
    {
        FollowLine,
        Overtake,
        Avoid,
        Recover,
        UseNitro
    }

    /// <summary>
    /// Evaluates sensor inputs and vehicle state at configurable Hz tick rates (accumulator-driven).
    /// </summary>
    public class DecisionMaker
    {
        private float _tickRateHz = 10f; // 10 updates per second
        private float _accumulator = 0f;
        private AIIntent _currentIntent = AIIntent.FollowLine;

        public float TickRateHz
        {
            get => _tickRateHz;
            set => _tickRateHz = Mathf.Max(1f, value);
        }

        public AIIntent CurrentIntent => _currentIntent;

        public AIIntent Tick(float deltaTime, SensorData sensorData, Vector3 currentPosition)
        {
            _accumulator += deltaTime;
            float interval = 1f / _tickRateHz;

            if (_accumulator >= interval)
            {
                _accumulator -= interval;
                _currentIntent = EvaluateIntent(sensorData);
            }

            return _currentIntent;
        }

        private AIIntent EvaluateIntent(SensorData sensorData)
        {
            if (sensorData.carAhead)
            {
                if (sensorData.distance < 5f)
                {
                    return AIIntent.Avoid;
                }
                else if (sensorData.distance < 15f)
                {
                    return AIIntent.Overtake;
                }
            }

            if (!sensorData.carAhead && sensorData.leftClearance > 10f && sensorData.rightClearance > 10f)
            {
                return AIIntent.UseNitro;
            }

            return AIIntent.FollowLine;
        }
    }
}
