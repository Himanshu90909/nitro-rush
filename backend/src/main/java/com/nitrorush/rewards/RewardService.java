package com.nitrorush.rewards;

import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.UUID;

/**
 * Idempotent reward claiming. Claims are recorded in a ledger table with a
 * UNIQUE (player, sourceType, sourceId) constraint — the DB enforces single
 * claims even under races. The granted amount is decided SERVER-SIDE; the
 * client only names the source it claims for.
 */
@Service
public class RewardService {

    private static final Logger log = LoggerFactory.getLogger(RewardService.class);
    private static final long RACE_CREDITS = 500;
    private static final long RACE_XP = 100;

    private final RewardClaimRepository claimRepository;
    private final PlayerProfileRepository profileRepository;
    private final com.nitrorush.player.PlayerService playerService;

    public RewardService(RewardClaimRepository claimRepository,
                         PlayerProfileRepository profileRepository,
                         com.nitrorush.player.PlayerService playerService) {
        this.claimRepository = claimRepository;
        this.profileRepository = profileRepository;
        this.playerService = playerService;
    }

    @Transactional
    public RewardDtos.ClaimResponse claim(UUID userId, RewardDtos.ClaimRequest request) {
        PlayerProfile profile = playerService.getProfileEntity(userId);

        // Already claimed? Return the recorded state without granting again.
        if (claimRepository.existsByPlayerIdAndSourceTypeAndSourceId(userId, request.sourceType(), request.sourceId())) {
            return new RewardDtos.ClaimResponse(0, 0, profile.getCredits(), profile.getXp(), true);
        }

        long credits;
        long xp;
        switch (request.sourceType().toUpperCase()) {
            case "RACE" -> {
                validateRaceSource(request.sourceId());
                credits = RACE_CREDITS;
                xp = RACE_XP;
            }
            case "DAILY" -> {
                credits = 250;
                xp = 50;
            }
            default -> throw ApiException.of(ErrorCode.VALIDATION_FAILED,
                    "Unknown sourceType: " + request.sourceType());
        }

        PlayerProfile locked = profileRepository.findByIdForUpdate(profile.getId())
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Player profile not found"));
        locked.setCredits(locked.getCredits() + credits);
        locked.setXp(locked.getXp() + xp);
        locked.setLevel(com.nitrorush.player.PlayerService.levelForXp(locked.getXp()));
        profileRepository.save(locked);

        claimRepository.save(new RewardClaim(userId, request.sourceType(), request.sourceId()));
        log.info("Player {} claimed {} reward {} (+{} credits, +{} xp)",
                userId, request.sourceType(), request.sourceId(), credits, xp);

        return new RewardDtos.ClaimResponse(credits, xp, locked.getCredits(), locked.getXp(), false);
    }

    private static void validateRaceSource(String sourceId) {
        try {
            UUID.fromString(sourceId);
        } catch (IllegalArgumentException e) {
            throw ApiException.of(ErrorCode.VALIDATION_FAILED, "Race sourceId must be a race UUID");
        }
    }
}
