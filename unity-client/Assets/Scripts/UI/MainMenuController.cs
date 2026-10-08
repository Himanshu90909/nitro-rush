using UnityEngine;
using UnityEngine.UI;

namespace NitroRush.UI
{
    /// <summary>
    /// Controller for Main Menu screen navigation and scene transitions via SceneLoader.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button garageButton;
        [SerializeField] private Button eventsButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button settingsButton;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (garageButton != null) garageButton.onClick.AddListener(OnGarageClicked);
            if (eventsButton != null) eventsButton.onClick.AddListener(OnEventsClicked);
            if (leaderboardButton != null) leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            if (profileButton != null) profileButton.onClick.AddListener(OnProfileClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnPlayClicked() => SceneLoader.LoadScene("Scene_Race");
        private void OnGarageClicked() => SceneLoader.LoadScene("Scene_Garage");
        private void OnEventsClicked() => SceneLoader.LoadScene("Scene_Events");
        private void OnLeaderboardClicked() => SceneLoader.LoadScene("Scene_Leaderboard");
        private void OnProfileClicked() => SceneLoader.LoadScene("Scene_Profile");
        private void OnSettingsClicked() => SceneLoader.LoadScene("Scene_Settings");
    }
}
