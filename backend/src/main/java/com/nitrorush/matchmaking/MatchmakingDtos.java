package com.nitrorush.matchmaking;

import java.util.List;
import java.util.UUID;

public final class MatchmakingDtos {
    private MatchmakingDtos() {}

    public record QueueStatus(int position, long queueSize, long estimatedWaitMs) {}

    public record LobbyInfo(UUID lobbyId, List<UUID> playerIds, String trackId) {}
}
