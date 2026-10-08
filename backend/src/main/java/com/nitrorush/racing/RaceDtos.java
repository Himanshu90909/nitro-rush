package com.nitrorush.racing;

import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.util.List;
import java.util.UUID;

public final class RaceDtos {
    private RaceDtos() {}

    public record StartRaceRequest(@NotBlank String trackId, @NotNull RaceType raceType) {}

    public record StartRaceResponse(UUID raceId, String trackId, String status) {}

    /**
     * Client REPORTS its observed run; the SERVER decides position, score and
     * rewards. No client-sent currency, score or position is ever trusted.
     */
    public record FinishRaceRequest(@NotNull Long finishTimeMs,
                                    @NotNull @Min(0) Double maxSpeed,
                                    @NotNull @Min(0) Integer checkpointsPassed,
                                    @NotNull @Min(1) @Max(20) Integer laps,
                                    @Min(0) Double parTimeMs) {}

    public record FinishRaceResponse(UUID raceId, int position, long score, long xpGained,
                                    long creditsGained, boolean flagged, String flagReason) {}

    public record RaceResultDto(UUID playerId, int position, long finishTimeMs, long score) {}

    public record RaceResultsResponse(UUID raceId, List<RaceResultDto> results) {}
}
