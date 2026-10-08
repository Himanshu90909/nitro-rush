using UnityEngine;

namespace NitroRush.UI
{
    /// <summary>
    /// Static neon palette defining Nitro Rush aesthetic colors.
    /// </summary>
    public static class UITheme
    {
        public static readonly Color Background = HexToColor("#0B0F1A"); // Dark Blue / Black
        public static readonly Color Cyan = HexToColor("#00E5FF");       // Primary Accent
        public static readonly Color Magenta = HexToColor("#FF2E92");    // High Action / Nitro
        public static readonly Color Amber = HexToColor("#FFB300");      // Gold / Rewards

        public static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
