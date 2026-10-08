package com.nitrorush.events;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.time.Instant;
import java.util.List;
import java.util.Map;
import java.util.UUID;

public final class EventDtos {
    private EventDtos() {}

    public record ObjectiveSpec(String type, int count) {}
    public record RewardsSpec(Long credits, Long xp, Integer tokens) {}

    public record ObjectiveProgress(String type, int progress, int target, boolean completed) {}

    public record EventResponse(UUID id, String name, String description, Instant startTime,
                                Instant endTime, boolean active, List<ObjectiveSpec> objectives,
                                RewardsSpec rewards) {}

    public record EventProgressResponse(List<ObjectiveProgress> objectives, boolean eventCompleted) {}

    public record AdminEventRequest(@NotBlank String name, String description,
                                    @NotNull Instant startTime, @NotNull Instant endTime,
                                    List<ObjectiveSpec> objectives, RewardsSpec rewards) {}
}
