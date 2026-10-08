using System;
using System.Threading.Tasks;

namespace NitroRush.Networking
{
    [Serializable]
    public class MatchmakingJoinRequest
    {
        public string gameMode;
        public string carId;
    }

    [Serializable]
    public class MatchmakingStatusResponse
    {
        public string ticketId;
        public string status; // "QUEUED", "MATCHED", "FAILED"
        public string matchId;
        public string lobbyServerUrl;
    }

    /// <summary>
    /// Handles joining matchmaking queues, status polling, and transition to race lobbies.
    /// </summary>
    public class MatchmakingClient
    {
        private readonly ApiClient _apiClient;

        public MatchmakingClient(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<MatchmakingStatusResponse> JoinQueueAndPollAsync(string gameMode, string carId, Action<string> onStatusUpdate)
        {
            var joinReq = new MatchmakingJoinRequest { gameMode = gameMode, carId = carId };
            var initialStatus = await _apiClient.PostAsync<MatchmakingStatusResponse>(ApiEndpoints.MatchmakingJoin, joinReq);

            if (initialStatus == null || string.IsNullOrEmpty(initialStatus.ticketId))
            {
                throw new ApiException(500, "Failed to join matchmaking queue.");
            }

            string ticketId = initialStatus.ticketId;
            int timeoutSeconds = 60;
            int elapsed = 0;

            while (elapsed < timeoutSeconds)
            {
                onStatusUpdate?.Invoke($"Searching for opponents... ({elapsed}s)");
                await Task.Delay(2000);
                elapsed += 2;

                var status = await _apiClient.GetAsync<MatchmakingStatusResponse>($"{ApiEndpoints.MatchmakingStatus}?ticketId={ticketId}");
                if (status != null && status.status == "MATCHED")
                {
                    onStatusUpdate?.Invoke("Match found! Loading lobby...");
                    return status;
                }
            }

            throw new TimeoutException("Matchmaking timed out after 60 seconds.");
        }
    }
}
