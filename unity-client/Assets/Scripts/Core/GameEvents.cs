using System;
using System.Collections.Generic;
using UnityEngine;
using NitroRush.Racing;

namespace NitroRush.Core
{
    /// <summary>
    /// Generic, strongly-typed observer event container supporting multi-subscriber callbacks without garbage allocation on invoke.
    /// </summary>
    public class GameEvent<T>
    {
        private readonly List<Action<T>> _listeners = new List<Action<T>>();

        public void AddListener(Action<T> listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void RemoveListener(Action<T> listener)
        {
            if (listener != null)
            {
                _listeners.Remove(listener);
            }
        }

        public void Invoke(T args)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(args);
            }
        }

        public void Clear()
        {
            _listeners.Clear();
        }
    }

    #region Event Arguments Definitions

    public class RaceStartedEventArgs
    {
        public List<string> RacerIds { get; }
        public TrackData Track { get; }
        public float StartTimestamp { get; }

        public RaceStartedEventArgs(List<string> racerIds, TrackData track, float startTimestamp)
        {
            RacerIds = racerIds;
            Track = track;
            StartTimestamp = startTimestamp;
        }
    }

    public class RaceFinishedEventArgs
    {
        public List<RacerResult> Standings { get; }
        public float TotalRaceTime { get; }
        public string WinnerRacerId { get; }
        public TrackData Track { get; }

        public RaceFinishedEventArgs(List<RacerResult> standings, float totalRaceTime, string winnerRacerId, TrackData track)
        {
            Standings = standings;
            TotalRaceTime = totalRaceTime;
            WinnerRacerId = winnerRacerId;
            Track = track;
        }
    }

    public class CheckpointPassedEventArgs
    {
        public string RacerId { get; }
        public int CheckpointIndex { get; }
        public int LapNumber { get; }
        public float LapTime { get; }
        public bool IsValid { get; }

        public CheckpointPassedEventArgs(string racerId, int checkpointIndex, int lapNumber, float lapTime, bool isValid)
        {
            RacerId = racerId;
            CheckpointIndex = checkpointIndex;
            LapNumber = lapNumber;
            LapTime = lapTime;
            IsValid = isValid;
        }
    }

    public class DriftStateChangedEventArgs
    {
        public string RacerId { get; }
        public bool IsDrifting { get; }
        public float DriftScore { get; }
        public float DriftAngle { get; }

        public DriftStateChangedEventArgs(string racerId, bool isDrifting, float driftScore, float driftAngle)
        {
            RacerId = racerId;
            IsDrifting = isDrifting;
            DriftScore = driftScore;
            DriftAngle = driftAngle;
        }
    }

    public class NitroStateChangedEventArgs
    {
        public string RacerId { get; }
        public bool IsNitroActive { get; }
        public float RemainingCapacity { get; }

        public NitroStateChangedEventArgs(string racerId, bool isNitroActive, float remainingCapacity)
        {
            RacerId = racerId;
            IsNitroActive = isNitroActive;
            RemainingCapacity = remainingCapacity;
        }
    }

    public class CarDamagedEventArgs
    {
        public string RacerId { get; }
        public float DamageAmount { get; }
        public float CurrentHealth { get; }
        public Vector3 ImpactPoint { get; }

        public CarDamagedEventArgs(string racerId, float damageAmount, float currentHealth, Vector3 impactPoint)
        {
            RacerId = racerId;
            DamageAmount = damageAmount;
            CurrentHealth = currentHealth;
            ImpactPoint = impactPoint;
        }
    }

    public class GameStateChangedEventArgs
    {
        public GameStateType PreviousState { get; }
        public GameStateType NewState { get; }

        public GameStateChangedEventArgs(GameStateType previousState, GameStateType newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    #endregion

    /// <summary>
    /// Global centralized event hub providing static strongly-typed pub/sub events across all gameplay modules.
    /// </summary>
    public static class GameEvents
    {
        public static readonly GameEvent<RaceStartedEventArgs> RaceStarted = new GameEvent<RaceStartedEventArgs>();
        public static readonly GameEvent<RaceFinishedEventArgs> RaceFinished = new GameEvent<RaceFinishedEventArgs>();
        public static readonly GameEvent<CheckpointPassedEventArgs> CheckpointPassed = new GameEvent<CheckpointPassedEventArgs>();
        public static readonly GameEvent<DriftStateChangedEventArgs> DriftStateChanged = new GameEvent<DriftStateChangedEventArgs>();
        public static readonly GameEvent<NitroStateChangedEventArgs> NitroStateChanged = new GameEvent<NitroStateChangedEventArgs>();
        public static readonly GameEvent<CarDamagedEventArgs> CarDamaged = new GameEvent<CarDamagedEventArgs>();
        public static readonly GameEvent<GameStateChangedEventArgs> GameStateChanged = new GameEvent<GameStateChangedEventArgs>();

        /// <summary>
        /// Clears all event subscribers across all system events.
        /// </summary>
        public static void ResetAll()
        {
            RaceStarted.Clear();
            RaceFinished.Clear();
            CheckpointPassed.Clear();
            DriftStateChanged.Clear();
            NitroStateChanged.Clear();
            CarDamaged.Clear();
            GameStateChanged.Clear();
        }
    }
}
