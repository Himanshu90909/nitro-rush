package com.nitrorush.rewards;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;
import java.util.UUID;

public interface RewardClaimRepository extends JpaRepository<RewardClaim, UUID> {
    Optional<RewardClaim> findByPlayerIdAndSourceTypeAndSourceId(UUID playerId, String sourceType, String sourceId);
    boolean existsByPlayerIdAndSourceTypeAndSourceId(UUID playerId, String sourceType, String sourceId);
}
