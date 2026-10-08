using System;
using System.Collections;
using UnityEngine;
using NitroRush.Core;

namespace NitroRush.Racing
{
    public enum RaceStatePhase
    {
        Waiting,
        Countdown,
        Racing,
        Finished
    }

    /// <summary>
    /// Controls match state flow (Waiting -> Countdown 3-2-1-GO -> Active Racing -> Finish Podium).
    /// </summary>
    public class RaceStateMachine : MonoBehaviour
    {
        public RaceStatePhase CurrentPhase { get; private me; set; } = RaceStatePhase.Waiting;

        public event Action<RaceStatePhase, RaceStatePhase> OnPhaseChanged;
        public event Action<int> OnCountdownTick; // Fires 3, 2, 1, 0 (0 = GO!)

        public void TransitionTo(RaceStatePhase targetPhase)
        {
            if (CurrentPhase == targetPhase) return;

            // Validate transition rules
            if (!IsValidTransition(CurrentPhase, targetPhase))
            {
                Debug.LogWarning($"[RaceStateMachine] Invalid transition rejected: {CurrentPhase} -> {targetPhase}");
                return;
            }

            RaceStatePhase prev = CurrentPhase;
            CurrentPhase = targetPhase;

            Debug.Log($"[RaceStateMachine] Race phase transition: {prev} -> {targetPhase}");
            OnPhaseChanged?.Invoke(prev, targetPhase);

            if (targetPhase == RaceStatePhase.Countdown)
            {
                StartCoroutine(RunCountdownRoutine());
            }
        }

        public static bool IsValidTransition(RaceStatePhase current, RaceStatePhase next)
        {
            switch (current)
            {
                case RaceStatePhase.Waiting:
                    return next == RaceStatePhase.Countdown;
                case RaceStatePhase.Countdown:
                    return next == RaceStatePhase.Racing || next == RaceStatePhase.Waiting;
                case RaceStatePhase.Racing:
                    return next == RaceStatePhase.Finished;
                case RaceStatePhase.Finished:
                    return next == RaceStatePhase.Waiting;
                default:
                    return false;
            }
        }

        private IEnumerator RunCountdownRoutine()
        {
            for (int count = 3; count >= 0; count--)
            {
                OnCountdownTick?.Invoke(count);
                yield return new WaitForSeconds(1.0f);
            }

            TransitionTo(RaceStatePhase.Racing);
        }
    }
}
