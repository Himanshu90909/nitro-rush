package com.nitrorush.leaderboard;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

/**
 * Read-only leaderboard API. There is NO score-submission endpoint by design —
 * scores are only written by RaceService after server-side validation.
 */
@RestController
@RequestMapping("/api/v1/leaderboards")
public class LeaderboardController {

    private final LeaderboardService leaderboardService;

    public LeaderboardController(LeaderboardService leaderboardService) {
        this.leaderboardService = leaderboardService;
    }

    @GetMapping("/global")
    public ApiResponse<List<LeaderboardDtos.LeaderboardEntry>> global(@RequestParam(defaultValue = "50") int limit) {
        return ApiResponse.ok(leaderboardService.top(Math.min(100, Math.max(1, limit))));
    }

    @GetMapping("/player/{playerId}")
    public ApiResponse<LeaderboardDtos.LeaderboardEntry> player(@PathVariable UUID playerId) {
        return ApiResponse.ok(leaderboardService.playerEntry(playerId));
    }

    @GetMapping("/nearby")
    public ApiResponse<List<LeaderboardDtos.LeaderboardEntry>> nearby(@RequestParam(defaultValue = "2") int range) {
        return ApiResponse.ok(leaderboardService.nearby(SecurityUtils.currentUserId(), Math.min(10, Math.max(1, range))));
    }
}
