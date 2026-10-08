using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroRush.Networking;

namespace NitroRush.LiveService
{
    [Serializable]
    public class LiveEventDto
    {
        public string id;
        public string title;
        public string description;
        public long endTimeUnix;
        public int targetObjectiveCount;
        public int currentObjectiveProgress;
    }

    /// <summary>
    /// Live Event service fetching events dynamically from backend.
    /// Server-driven (no hardcoded event definitions in client).
    /// </summary>
    public class LiveEventService
    {
        private readonly ApiClient _apiClient;
        public List<LiveEventDto> ActiveEvents { get; private set; } = new List<LiveEventDto>();

        public LiveEventService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task FetchLiveEventsAsync()
        {
            var eventsResponse = await _apiClient.GetAsync<JsonArrayWrapper<LiveEventDto>>(ApiEndpoints.Events);
            if (eventsResponse != null && eventsResponse.items != null)
            {
                ActiveEvents = new List<LiveEventDto>(eventsResponse.items);
            }
        }

        public TimeSpan GetTimeRemaining(LiveEventDto liveEvent)
        {
            long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long remainingSec = liveEvent.endTimeUnix - nowUnix;
            return remainingSec > 0 ? TimeSpan.FromSeconds(remainingSec) : TimeSpan.Zero;
        }
    }
}
