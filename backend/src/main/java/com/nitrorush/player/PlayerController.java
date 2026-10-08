package com.nitrorush.player;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/v1/player")
public class PlayerController {

    private final PlayerService playerService;

    public PlayerController(PlayerService playerService) {
        this.playerService = playerService;
    }

    @GetMapping("/profile")
    public ApiResponse<PlayerDtos.ProfileResponse> getProfile() {
        return ApiResponse.ok(playerService.getProfile(SecurityUtils.currentUserId()));
    }

    @PutMapping("/profile")
    public ApiResponse<PlayerDtos.ProfileResponse> updateProfile(@Valid @RequestBody PlayerDtos.UpdateProfileRequest request) {
        return ApiResponse.ok(playerService.updateProfile(SecurityUtils.currentUserId(), request));
    }
}
