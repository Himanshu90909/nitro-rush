using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroRush.Networking;
using NitroRush.Utilities;
using UnityEngine;

namespace NitroRush.Analytics
{
    public enum TelemetryEventType
    {
        race_started,
        race_finished,
        car_selected,
        upgrade_purchased,
        event_joined,
        reward_claimed,
        player_login,
        matchmaking_started,
        matchmaking_completed
    }

    [Serializable]
    public class TelemetryEvent
    {
        public string eventName;
        public string timestamp;
    }

    /// <summary>
    /// Queue-based telemetry logger batching analytics data every 30s or 20 events. Zero PII.
    /// </summary>
    public class TelemetryLogger
    {
        private readonly Queue<TelemetryEvent> _eventQueue = new Queue<TelemetryEvent>();
        private readonly ApiClient _apiClient;
        private float _timeSinceLastFlush = 0f;

        public int QueueCount => _eventQueue.Count;

        public TelemetryLogger(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public void LogEvent(TelemetryEventType eventType)
        {
            _eventQueue.Enqueue(new TelemetryEvent
            {
                eventName = eventType.ToString(),
                timestamp = RestUtilities.ToIso8601String(DateTime.UtcNow)
            });

            if (_eventQueue.Count >= 20)
            {
                _ = FlushAsync();
            }
        }

        public void UpdateTimer(float deltaTime)
        {
            _timeSinceLastFlush += deltaTime;
            if (_timeSinceLastFlush >= 30f)
            {
                _timeSinceLastFlush = 0f;
                if (_eventQueue.Count > 0)
                {
                    _ = FlushAsync();
                }
            }
        }

        public async Task FlushAsync()
        {
            if (_eventQueue.Count == 0 || _apiClient == null) return;

            var batch = new List<TelemetryEvent>(_eventQueue);
            _eventQueue.Clear();

            try
            {
                var wrapper = new JsonArrayWrapper<TelemetryEvent> { items = batch.ToArray() };
                await _apiClient.PostAsync<string>(ApiEndpoints.AnalyticsPlayer, wrapper);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Telemetry flush failed: {ex.Message}");
            }
        }
    }
}
