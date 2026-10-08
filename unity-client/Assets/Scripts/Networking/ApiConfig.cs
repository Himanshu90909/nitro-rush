using UnityEngine;

namespace NitroRush.Networking
{
    /// <summary>
    /// Server-driven API configuration ScriptableObject.
    /// Stores server URLs, timeout parameters, and retry parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "ApiConfig", menuName = "NitroRush/Networking/ApiConfig")]
    public class ApiConfig : ScriptableObject
    {
        [SerializeField] private string apiBaseUrl = "https://api.nitrorushgame.com";
        [SerializeField] private string websocketUrl = "wss://ws.nitrorushgame.com";
        [SerializeField] private float requestTimeoutSec = 10f;
        [SerializeField] private int maxRetries = 3;

        public string ApiBaseUrl => apiBaseUrl;
        public string WebsocketUrl => websocketUrl;
        public float RequestTimeoutSec => requestTimeoutSec;
        public int MaxRetries => maxRetries;
    }
}
