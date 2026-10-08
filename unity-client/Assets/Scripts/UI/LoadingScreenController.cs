using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NitroRush.UI
{
    /// <summary>
    /// Displays loading screen progress and rotating gameplay tips.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image progressBarFill;
        [SerializeField] private Text tipText;

        [Header("Tips")]
        [SerializeField] private List<string> loadingTips = new List<string>
        {
            "Tip: Save nitro for long straightaways to maximize top speed!",
            "Tip: Feather the brake into corners to initiate a drift.",
            "Tip: Upgrading your turbo improves acceleration significantly.",
            "Tip: Draft behind opponents to gain slipstream speed boosts."
        };

        private void Start()
        {
            ShowRandomTip();
        }

        public void ShowRandomTip()
        {
            if (tipText != null && loadingTips.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, loadingTips.Count);
                tipText.text = loadingTips[randomIndex];
            }
        }

        public void SetProgress(float progress)
        {
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = Mathf.Clamp01(progress);
            }
        }
    }
}
