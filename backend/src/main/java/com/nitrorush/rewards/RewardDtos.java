package com.nitrorush.rewards;

import jakarta.validation.constraints.NotBlank;

public final class RewardDtos {
    private RewardDtos() {}

    public record ClaimRequest(@NotBlank String sourceType, @NotBlank String sourceId) {}

    public record ClaimResponse(long grantedCredits, long grantedXp, long newCredits, long newXp, boolean alreadyClaimed) {}
}
