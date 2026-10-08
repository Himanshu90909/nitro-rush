using System;
using System.Collections.Generic;

namespace NitroRush.Racing
{
    public struct RacerProgressData
    {
        public string RacerId;
        public int CurrentLap;
        public int LastPassedCheckpointIndex;
        public float DistanceToNextCheckpoint;
        public float TotalProgressScore;
        public float TotalRaceTime;
        public int CurrentRank;
    }

    /// <summary>
    /// Computes and updates live racer ranks based on calculated track progress.
    /// Time Complexity: O(N log N) sorting step per evaluation frame for N registered racers.
    /// Micro-optimized for low N (e.g. N = 8-12 racers in multiplayer lobby).
    /// Tie-breaking: when progress scores match within 1e-4 epsilon, earlier total race time breaks ties.
    /// </summary>
    public class RacePositionTracker
    {
        private readonly List<RacerProgressData> _racerProgressList = new List<RacerProgressData>();

        public void UpdateRacerProgress(RacerProgressData progressData)
        {
            int index = _racerProgressList.FindIndex(r => r.RacerId == progressData.RacerId);
            if (index >= 0)
            {
                _racerProgressList[index] = progressData;
            }
            else
            {
                _racerProgressList.Add(progressData);
            }
        }

        /// <summary>
        /// Recalculates standings ranks for all registered racers in O(N log N) time.
        /// </summary>
        public List<RacerProgressData> CalculateStandings()
        {
            // O(N log N) sorting using custom progress comparator with total race time tie-breaker
            _racerProgressList.Sort((a, b) =>
            {
                float diff = b.TotalProgressScore - a.TotalProgressScore;
                if (Math.Abs(diff) > 0.0001f)
                {
                    return diff > 0f ? 1 : -1;
                }
                // Tie-breaker: smaller total race time places ahead
                return a.TotalRaceTime.CompareTo(b.TotalRaceTime);
            });

            for (int i = 0; i < _racerProgressList.Count; i++)
            {
                var item = _racerProgressList[i];
                item.CurrentRank = i + 1;
                _racerProgressList[i] = item;
            }

            return _racerProgressList;
        }

        public void Clear()
        {
            _racerProgressList.Clear();
        }
    }
}
