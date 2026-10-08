namespace NitroRush.Networking
{
    /// <summary>
    /// Constants for backend REST API routes.
    /// </summary>
    public static class ApiEndpoints
    {
        public const string AuthRegister = "/api/v1/auth/register";
        public const string AuthLogin = "/api/v1/auth/login";
        public const string AuthRefresh = "/api/v1/auth/refresh";
        public const string PlayerProfile = "/api/v1/player/profile";
        public const string Cars = "/api/v1/cars";
        public const string Garage = "/api/v1/garage";
        public const string GaragePurchase = "/api/v1/garage/cars/{id}/purchase";
        public const string GarageUpgrade = "/api/v1/garage/cars/{id}/upgrade";
        public const string Events = "/api/v1/events";
        public const string ClaimReward = "/api/v1/rewards/claim";
        public const string LeaderboardGlobal = "/api/v1/leaderboards/global";
        public const string LeaderboardPlayer = "/api/v1/leaderboards/player/{id}";
        public const string RacesStart = "/api/v1/races/start";
        public const string RacesFinish = "/api/v1/races/finish";
        public const string AnalyticsPlayer = "/api/v1/analytics/player";
        public const string MatchmakingJoin = "/api/v1/matchmaking/join";
        public const string MatchmakingStatus = "/api/v1/matchmaking/status";
    }
}
