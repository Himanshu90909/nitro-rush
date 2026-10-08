package com.nitrorush.garage;

import com.nitrorush.auth.AuthDtos;
import com.nitrorush.auth.AuthService;
import com.nitrorush.common.ApiException;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;

import java.util.Map;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

@SpringBootTest
@ActiveProfiles("test")
class GarageServiceTests {

    @Autowired
    private GarageService garageService;
    @Autowired
    private AuthService authService;
    @Autowired
    private CarRepository carRepository;
    @Autowired
    private PlayerCarRepository playerCarRepository;
    @Autowired
    private CarUpgradeRepository upgradeRepository;
    @Autowired
    private com.nitrorush.player.PlayerProfileRepository profileRepository;

    private UUID newPlayer() {
        return authService.register(new AuthDtos.RegisterRequest(
                "garage-" + UUID.randomUUID() + "@test.dev", "supersecret1", "garage")).user().id();
    }

    private Car seedCar(Rarity rarity, long price) {
        return carRepository.save(new Car("TestCar-" + UUID.randomUUID().toString().substring(0, 6),
                rarity, 55, 14, 12, 16, 60, price));
    }

    @Test
    void purchaseDeductsCreditsAndCreatesUpgrades() {
        UUID player = newPlayer();
        Car car = seedCar(Rarity.COMMON, 1000);

        GarageDtos.PurchaseResponse response = garageService.purchase(player, car.getId());

        assertEquals(car.getId(), response.carId());
        assertEquals(4000, response.newCredits(), "5000 start - 1000 price");
        assertTrue(playerCarRepository.findByPlayerIdAndCarId(player, car.getId()).isPresent());

        UUID playerCarId = playerCarRepository.findByPlayerIdAndCarId(player, car.getId()).orElseThrow().getId();
        assertEquals(5, upgradeRepository.findByPlayerCarId(playerCarId).size(),
                "all 5 upgrade categories start at level 1");
    }

    @Test
    void insufficientCreditsRejected() {
        UUID player = newPlayer();
        Car car = seedCar(Rarity.LEGENDARY, 1_000_000);

        ApiException e = assertThrows(ApiException.class, () -> garageService.purchase(player, car.getId()));
        assertEquals("INSUFFICIENT_CREDITS", e.getErrorCode().name());
        assertTrue(playerCarRepository.findByPlayerIdAndCarId(player, car.getId()).isEmpty());
    }

    @Test
    void doublePurchaseIsIdempotent() {
        UUID player = newPlayer();
        Car car = seedCar(Rarity.COMMON, 1000);
        garageService.purchase(player, car.getId());

        // Second purchase returns success without charging again.
        GarageDtos.PurchaseResponse second = garageService.purchase(player, car.getId());
        assertEquals(4000, second.newCredits(), "no double charge");
    }

    @Test
    void upgradeUnownedCarRejected() {
        UUID player = newPlayer();
        Car car = seedCar(Rarity.COMMON, 1000);
        ApiException e = assertThrows(ApiException.class,
                () -> garageService.upgrade(player, car.getId(), UpgradeType.ENGINE));
        assertEquals("CAR_NOT_OWNED", e.getErrorCode().name());
    }

    @Test
    void upgradeToLevelFiveThenCapped() {
        UUID player = newPlayer();
        Car car = seedCar(Rarity.COMMON, 1000);
        garageService.purchase(player, car.getId());

        // Full ENGINE ladder costs 1200+2400+3600+4800 = 12000 credits.
        var profile = profileRepository.findByUserId(player).orElseThrow();
        profile.setCredits(50_000);
        profileRepository.saveAndFlush(profile);

        // Levels 1->2 ... 4->5
        for (int expected = 2; expected <= 5; expected++) {
            GarageDtos.UpgradeResponse response = garageService.upgrade(player, car.getId(), UpgradeType.ENGINE);
            assertEquals(expected, response.newLevel());
        }

        ApiException e = assertThrows(ApiException.class,
                () -> garageService.upgrade(player, car.getId(), UpgradeType.ENGINE));
        assertEquals("INVALID_UPGRADE_LEVEL", e.getErrorCode().name());
    }

    @Test
    void garageReturnsCurrentStatsAndUpgrades() {
        UUID player = newPlayer();
        Car car = seedCar(Rarity.COMMON, 1000);
        garageService.purchase(player, car.getId());
        garageService.upgrade(player, car.getId(), UpgradeType.TURBO);

        GarageDtos.GarageResponse garage = garageService.garage(player);
        assertEquals(1, garage.owned().size());

        GarageDtos.OwnedCarResponse owned = garage.owned().get(0);
        assertEquals(2, owned.upgrades().get("TURBO"));
        assertEquals(1, owned.upgrades().get("ENGINE"));

        Map<String, Double> stats = owned.currentStats();
        assertEquals(car.getBaseSpeed() * 1.08, stats.get("speed"), 0.001, "TURBO level 2 = +8% speed");
    }
}
