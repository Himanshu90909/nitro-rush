package com.nitrorush.leaderboard;

import java.util.List;
import java.util.UUID;

public final class LeaderboardDtos {
    private LeaderboardDtos() {}

    public record LeaderboardEntry(UUID playerId, String username, long score, long rank) {}

    public record LeaderboardResponse(List<LeaderboardEntry> entries, LeaderboardEntry self) {}
}
