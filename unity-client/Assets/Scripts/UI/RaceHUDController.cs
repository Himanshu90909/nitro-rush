using System;
using UnityEngine;
using UnityEngine.UI;

namespace NitroRush.UI
{
    /// <summary>
    /// Event-driven HUD controller updating speed, nitro bar, lap counter, position rank, and countdown.
    /// Binds directly to GameEvents without polling where events suffice.
    /// </summary>
    public class RaceHUDController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text speedText;
        [SerializeField] private Image nitroBarFill;
        [SerializeField] private Text positionText;
        [SerializeField] private Text lapText;
        [SerializeField] private Text countdownText;

        private int _totalLaps = 3;

        private void OnEnable()
        {
            GameEvents.OnRaceFinished += HandleRaceFinished;
            GameEvents.OnCheckpointPassed += HandleCheckpointPassed;
            GameEvents.OnNitroActivated += HandleNitroActivated;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnRaceFinished -= HandleRaceFinished;
            GameEvents.OnCheckpointPassed -= HandleCheckpointPassed;
            GameEvents.OnNitroActivated -= HandleNitroActivated;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        public void UpdateTelemetry(float speedMs, float nitroPercentage)
        {
            float speedKmh = speedMs * 3.6f;
            if (speedText != null) speedText.text = $"{Mathf.RoundToInt(speedKmh)} KM/H";
            if (nitroBarFill != null) nitroBarFill.fillAmount = Mathf.Clamp01(nitroPercentage);
        }

        public void UpdatePosition(int rank, int totalRacers)
        {
            if (positionText != null) positionText.text = $"{rank}/{totalRacers}";
        }

        public void SetCountdown(string text)
        {
            if (countdownText != null) countdownText.text = text;
        }

        private void HandleCheckpointPassed(int racerId, int checkpointIndex)
        {
            if (lapText != null)
            {
                lapText.text = $"CHECKPOINT {checkpointIndex}";
            }
        }

        private void HandleNitroActivated(int carId)
        {
            // Flash HUD Nitro indicator
        }

        private void HandleRaceFinished(RaceFinishedEventArgs args)
        {
            if (countdownText != null) countdownText.text = "FINISH!";
        }

        private void HandleGameStateChanged(string fromState, string toState)
        {
            if (toState == "Racing" && countdownText != null)
            {
                countdownText.text = "GO!";
            }
        }
    }
}
