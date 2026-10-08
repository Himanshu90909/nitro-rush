using System;
using UnityEngine.Networking;

namespace NitroRush.Utilities
{
    /// <summary>
    /// REST utility functions for URL encoding and ISO 8601 formatting.
    /// </summary>
    public static class RestUtilities
    {
        public static string UrlEncode(string value)
        {
            return UnityWebRequest.EscapeURL(value ?? "");
        }

        public static string ToIso8601String(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public static DateTime ParseIso8601(string isoString)
        {
            if (DateTime.TryParse(isoString, out DateTime result))
            {
                return result.ToUniversalTime();
            }
            return DateTime.UtcNow;
        }
    }
}
