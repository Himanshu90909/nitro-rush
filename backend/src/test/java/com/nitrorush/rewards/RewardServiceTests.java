package com.nitrorush.rewards;

import com.nitrorush.auth.AuthDtos;
import com.nitrorush.auth.AuthService;
import com.nitrorush.common.ApiException;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;

import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

@SpringBootTest
@ActiveProfiles("test")
class RewardServiceTests {

    @Autowired
    private RewardService rewardService;
    @Autowired
    private AuthService authService;

    private UUID newPlayer() {
        return authService.register(new AuthDtos.RegisterRequest(
                "rewards-" + UUID.randomUUID() + "@test.dev", "supersecret1", "claim")).user().id();
    }

    @Test
    void firstRaceClaimGrantsRewards() {
        UUID player = newPlayer();
        String raceId = UUID.randomUUID().toString();

        RewardDtos.ClaimResponse response = rewardService.claim(player, new RewardDtos.ClaimRequest("RACE", raceId));

        assertEquals(500, response.grantedCredits());
        assertEquals(100, response.grantedXp());
        assertEquals(5500, response.newCredits());
        assertFalse(response.alreadyClaimed());
    }

    @Test
    void duplicateClaimIsIdempotent() {
        UUID player = newPlayer();
        String raceId = UUID.randomUUID().toString();
        rewardService.claim(player, new RewardDtos.ClaimRequest("RACE", raceId));

        RewardDtos.ClaimResponse second = rewardService.claim(player, new RewardDtos.ClaimRequest("RACE", raceId));

        assertEquals(0, second.grantedCredits(), "no double grant");
        assertEquals(5500, second.newCredits(), "balance unchanged");
        assertTrue(second.alreadyClaimed());
    }

    @Test
    void unknownSourceTypeRejected() {
        UUID player = newPlayer();
        ApiException e = assertThrows(ApiException.class,
                () -> rewardService.claim(player, new RewardDtos.ClaimRequest("FREE_MONEY", "x")));
        assertEquals("VALIDATION_FAILED", e.getErrorCode().name());
    }

    @Test
    void raceClaimRequiresUuidSource() {
        UUID player = newPlayer();
        assertThrows(ApiException.class,
                () -> rewardService.claim(player, new RewardDtos.ClaimRequest("RACE", "not-a-uuid")));
    }
}
