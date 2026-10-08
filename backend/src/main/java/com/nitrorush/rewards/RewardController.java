package com.nitrorush.rewards;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/v1/rewards")
public class RewardController {

    private final RewardService rewardService;

    public RewardController(RewardService rewardService) {
        this.rewardService = rewardService;
    }

    @PostMapping("/claim")
    public ApiResponse<RewardDtos.ClaimResponse> claim(@Valid @RequestBody RewardDtos.ClaimRequest request) {
        return ApiResponse.ok(rewardService.claim(SecurityUtils.currentUserId(), request));
    }
}
