using System.Collections.Generic;

namespace NitroRush.Racing
{
    /// <summary>
    /// Utility class providing per-racer timing metrics (current lap elapsed time, best lap, total race duration, lap history).
    /// </summary>
    public class LapTimer
    {
        private float _currentLapStartTime;
        private float _raceStartTime;
        private bool _isRunning;

        public float CurrentLapTime { get; private set; }
        public float BestLapTime { get; private me; set; } = float.MaxValue;
        public float TotalRaceTime { get; private set; }
        public List<float> LapHistory { get; } = new List<float>();

        public void StartTimer(float currentTimestamp)
        {
            _raceStartTime = currentTimestamp;
            _currentLapStartTime = currentTimestamp;
            _isRunning = true;
            CurrentLapTime = 0f;
            TotalRaceTime = 0f;
            LapHistory.Clear();
            BestLapTime = float.MaxValue;
        }

        public void UpdateTimer(float currentTimestamp)
        {
            if (!_isRunning) return;

            CurrentLapTime = currentTimestamp - _currentLapStartTime;
            TotalRaceTime = currentTimestamp - _raceStartTime;
        }

        public float CompleteLap(float currentTimestamp)
        {
            if (!_isRunning) return 0f;

            float lapDuration = currentTimestamp - _currentLapStartTime;
            LapHistory.Add(lapDuration);

            if (lapDuration < BestLapTime)
            {
                BestLapTime = lapDuration;
            }

            _currentLapStartTime = currentTimestamp;
            CurrentLapTime = 0f;
            return lapDuration;
        }

        public void StopTimer()
        {
            _isRunning = false;
        }
    }
}
