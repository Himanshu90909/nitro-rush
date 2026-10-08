package com.nitrorush.player;

import jakarta.validation.constraints.Size;

import java.util.UUID;

public final class PlayerDtos {
    private PlayerDtos() {}

    public record ProfileResponse(UUID id, UUID userId, String username, int level, long xp,
                                  long credits, int premiumTokens, String headline) {}

    public record UpdateProfileRequest(
            @Size(min = 3, max = 20, message = "Username must be 3-20 characters") String username,
            @Size(max = 120, message = "Headline must be at most 120 characters") String headline) {}
}
