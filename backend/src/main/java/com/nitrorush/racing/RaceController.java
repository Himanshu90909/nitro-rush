package com.nitrorush.racing;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/v1/races")
public class RaceController {

    private final RaceService raceService;

    public RaceController(RaceService raceService) {
        this.raceService = raceService;
    }

    @PostMapping("/start")
    public ApiResponse<RaceDtos.StartRaceResponse> start(@Valid @RequestBody RaceDtos.StartRaceRequest request) {
        return ApiResponse.ok(raceService.start(SecurityUtils.currentUserId(), request));
    }

    @PostMapping("/{raceId}/finish")
    public ApiResponse<RaceDtos.FinishRaceResponse> finish(@PathVariable UUID raceId,
                                                           @Valid @RequestBody RaceDtos.FinishRaceRequest request) {
        return ApiResponse.ok(raceService.finish(SecurityUtils.currentUserId(), raceId, request));
    }

    @GetMapping("/{raceId}/results")
    public ApiResponse<RaceDtos.RaceResultsResponse> results(@PathVariable UUID raceId) {
        return ApiResponse.ok(raceService.results(raceId));
    }
}
