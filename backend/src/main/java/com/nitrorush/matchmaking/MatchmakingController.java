package com.nitrorush.matchmaking;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/v1/matchmaking")
public class MatchmakingController {

    private final MatchmakingService matchmakingService;

    public MatchmakingController(MatchmakingService matchmakingService) {
        this.matchmakingService = matchmakingService;
    }

    @PostMapping("/join")
    public ApiResponse<Void> join() {
        matchmakingService.join(SecurityUtils.currentUserId());
        return ApiResponse.ok(null);
    }

    @GetMapping("/status")
    public ApiResponse<MatchmakingDtos.QueueStatus> status() {
        return ApiResponse.ok(matchmakingService.status(SecurityUtils.currentUserId()));
    }

    @PostMapping("/leave")
    public ApiResponse<Void> leave() {
        matchmakingService.leave(SecurityUtils.currentUserId());
        return ApiResponse.ok(null);
    }

    @GetMapping("/lobby/{lobbyId}")
    public ApiResponse<MatchmakingDtos.LobbyInfo> lobby(@PathVariable UUID lobbyId) {
        return ApiResponse.ok(matchmakingService.lobby(lobbyId).orElse(null));
    }
}
