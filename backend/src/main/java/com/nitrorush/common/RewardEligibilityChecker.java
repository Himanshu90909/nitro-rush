package com.nitrorush.common;

import java.util.UUID;

public interface RewardEligibilityChecker {
    boolean isEligible(UUID playerId, String sourceType, String sourceId);
}
