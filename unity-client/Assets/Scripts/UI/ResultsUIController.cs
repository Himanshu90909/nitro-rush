using System;
using System.Threading.Tasks;
using NitroRush.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace NitroRush.UI
{
    [Serializable]
    public class ClaimRewardResponse
    {
        public bool success;
        public int creditsAwarded;
        public int xpAwarded;
    }

    /// <summary>
    /// Post-race summary UI displaying position, race duration, XP earned, and async reward claiming.
    /// </summary>
    public class ResultsUIController : MonoBehaviour
    {
        [Header("UI Text Fields")]
        [SerializeField] private Text positionText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text xpText;
        [SerializeField] private Text creditsText;
        [SerializeField] private Button claimButton;

        private ApiClient _apiClient;
        private string _raceId;

        public void DisplayResults(int position, float finishTimeSeconds, int xpEarned, int creditsEarned, string raceId, ApiClient apiClient)
        {
            _apiClient = apiClient;
            _raceId = raceId;

            if (positionText != null) positionText.text = $"POSITION: #{position}";
            if (timeText != null) timeText.text = $"TIME: {TimeSpan.FromSeconds(finishTimeSeconds):mm\:ss\.ff}";
            if (xpText != null) xpText.text = $"+{xpEarned} XP";
            if (creditsText != null) creditsText.text = $"+{creditsEarned} CREDITS";

            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(() => _ = ClaimRewardAsync());
            }
        }

        private async Task ClaimRewardAsync()
        {
            if (_apiClient == null || string.IsNullOrEmpty(_raceId)) return;

            if (claimButton != null) claimButton.interactable = false;

            try
            {
                var response = await _apiClient.PostAsync<ClaimRewardResponse>($"/api/v1/rewards/claim?raceId={_raceId}", null);
                if (response != null && response.success)
                {
                    if (creditsText != null) creditsText.text = "CLAIMED!";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to claim reward: {ex.Message}");
                if (claimButton != null) claimButton.interactable = true;
            }
        }
    }
}
