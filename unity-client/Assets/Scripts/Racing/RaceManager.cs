using System.Collections.Generic;
using UnityEngine;
using NitroRush.Core;
using NitroRush.Cars;

namespace NitroRush.Racing
{
    public struct RacerResult
    {
        public string RacerId;
        public int Rank;
        public float TotalTime;
        public float BestLapTime;
    }

    /// <summary>
    /// Central manager orchestrating a race match: racer registration, lap counting with checkpoint validation,
    /// live progress evaluation formula, anti-cheat validation, and finish podium events.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("Race Configuration")]
        [SerializeField] private TrackData trackData;
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

        private readonly Dictionary<string, CarController> _racers = new Dictionary<string, CarController>();
        private readonly Dictionary<string, RacerProgressData> _progressMap = new Dictionary<string, RacerProgressData>();
        private readonly Dictionary<string, LapTimer> _timers = new Dictionary<string, LapTimer>();
        private readonly RacePositionTracker _positionTracker = new RacePositionTracker();
        private readonly List<RacerResult> _finishedResults = new List<RacerResult>();

        public TrackData Track => trackData;
        public bool IsRaceActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<RaceManager>();
            }
        }

        public void InitializeRace(TrackData track, List<Checkpoint> trackCheckpoints)
        {
            trackData = track;
            checkpoints = trackCheckpoints;
            _racers.Clear();
            _progressMap.Clear();
            _timers.Clear();
            _positionTracker.Clear();
            _finishedResults.Clear();
            IsRaceActive = false;
        }

        public void RegisterRacer(string racerId, CarController car)
        {
            if (string.IsNullOrEmpty(racerId) || car == null) return;

            _racers[racerId] = car;
            _timers[racerId] = new LapTimer();

            var initialProgress = new RacerProgressData
            {
                RacerId = racerId,
                CurrentLap = 1,
                LastPassedCheckpointIndex = 0,
                DistanceToNextCheckpoint = 0f,
                TotalProgressScore = 0f,
                TotalRaceTime = 0f,
                CurrentRank = _racers.Count
            };

            _progressMap[racerId] = initialProgress;
            _positionTracker.UpdateRacerProgress(initialProgress);
        }

        public void StartRace()
        {
            IsRaceActive = true;
            float timestamp = Time.time;

            foreach (var timer in _timers.Values)
            {
                timer.StartTimer(timestamp);
            }

            GameEvents.RaceStarted.Invoke(new RaceStartedEventArgs(new List<string>(_racers.Keys), trackData, timestamp));
        }

        private void Update()
        {
            if (!IsRaceActive) return;

            float currentTimestamp = Time.time;

            foreach (var kvp in _racers)
            {
                string racerId = kvp.Key;
                CarController car = kvp.Value;

                if (_timers.TryGetValue(racerId, out var timer))
                {
                    timer.UpdateTimer(currentTimestamp);
                }

                UpdateProgressScore(racerId, car, timer != null ? timer.TotalRaceTime : 0f);
            }

            List<RacerProgressData> standings = _positionTracker.CalculateStandings();
            for (int i = 0; i < standings.Count; i++)
            {
                _progressMap[standings[i].RacerId] = standings[i];
            }
        }

        public void OnRacerPassedCheckpoint(string racerId, Checkpoint checkpoint)
        {
            if (!IsRaceActive || !_progressMap.TryGetValue(racerId, out RacerProgressData progress))
                return;

            int totalCheckpoints = checkpoints.Count > 0 ? checkpoints.Count : 1;
            int lastPassed = progress.LastPassedCheckpointIndex;
            int reported = checkpoint.Index;

            // Enforce checkpoint order validation anti-cheat check
            bool isValidOrder = RaceValidation.IsCheckpointOrderValid(lastPassed, reported, totalCheckpoints);
            if (!isValidOrder && !(lastPassed == 0 && reported == 0)) // Allow initial finish line placement
            {
                Debug.LogWarning($"[RaceManager] Out-of-order checkpoint pass rejected for racer {racerId}. Expected {(lastPassed + 1) % totalCheckpoints}, reported {reported}");
                GameEvents.CheckpointPassed.Invoke(new CheckpointPassedEventArgs(racerId, reported, progress.CurrentLap, 0f, false));
                return;
            }

            progress.LastPassedCheckpointIndex = reported;
            float lapTime = 0f;

            // Handle lap completion on passing finish line (checkpoint 0)
            if (checkpoint.IsFinishLine && reported == 0)
            {
                if (_timers.TryGetValue(racerId, out var timer))
                {
                    lapTime = timer.CompleteLap(Time.time);
                }

                progress.CurrentLap++;

                if (progress.CurrentLap > trackData.totalLaps)
                {
                    HandleRacerFinished(racerId, timer);
                }
            }

            _progressMap[racerId] = progress;
            GameEvents.CheckpointPassed.Invoke(new CheckpointPassedEventArgs(racerId, reported, progress.CurrentLap, lapTime, true));
        }

        private void UpdateProgressScore(string racerId, CarController car, float totalRaceTime)
        {
            if (!_progressMap.TryGetValue(racerId, out RacerProgressData progress) || checkpoints.Count == 0)
                return;

            int totalCheckpoints = checkpoints.Count;
            int currentCkptIdx = progress.LastPassedCheckpointIndex;
            int nextCkptIdx = (currentCkptIdx + 1) % totalCheckpoints;

            Checkpoint currentCkpt = checkpoints[currentCkptIdx];
            Checkpoint nextCkpt = checkpoints[nextCkptIdx];

            // Progress formula: progress = (lap * totalCheckpoints) + currentCheckpointIndex + (distanceToNext / segmentLength)
            Vector3 segment = nextCkpt.Position - currentCkpt.Position;
            float segmentLength = Mathf.Max(0.001f, segment.magnitude);
            Vector3 racerOffset = car.Position - currentCkpt.Position;
            float projection = Vector3.Dot(racerOffset, segment.normalized);
            float normalizedSegmentProgress = Mathf.Clamp01(projection / segmentLength);

            float totalProgress = (progress.CurrentLap * totalCheckpoints) + currentCkptIdx + normalizedSegmentProgress;

            progress.DistanceToNextCheckpoint = segmentLength - projection;
            progress.TotalProgressScore = totalProgress;
            progress.TotalRaceTime = totalRaceTime;

            _progressMap[racerId] = progress;
            _positionTracker.UpdateRacerProgress(progress);
        }

        private void HandleRacerFinished(string racerId, LapTimer timer)
        {
            if (timer != null)
            {
                timer.StopTimer();
            }

            int rank = _finishedResults.Count + 1;
            var result = new RacerResult
            {
                RacerId = racerId,
                Rank = rank,
                TotalTime = timer != null ? timer.TotalRaceTime : 0f,
                BestLapTime = timer != null ? timer.BestLapTime : 0f
            };

            _finishedResults.Add(result);

            if (_finishedResults.Count >= _racers.Count)
            {
                IsRaceActive = false;
                string winnerId = _finishedResults.Count > 0 ? _finishedResults[0].RacerId : "";
                GameEvents.RaceFinished.Invoke(new RaceFinishedEventArgs(_finishedResults, result.TotalTime, winnerId, trackData));
            }
        }
    }
}
