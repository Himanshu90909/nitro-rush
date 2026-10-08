package com.nitrorush.garage;

import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import com.nitrorush.player.PlayerService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

/**
 * Server-authoritative garage economy.
 *
 * Rules:
 * - the client NEVER sends a currency amount; the server reads and mutates balances
 * - every currency mutation locks the profile row (PESSIMISTIC_WRITE) first
 * - purchases are idempotent: buying an owned car returns success without charging
 * - upgrades are capped at level 5; cost = BASE(type) * currentLevel * rarity multiplier
 */
@Service
public class GarageService {

    private static final Logger log = LoggerFactory.getLogger(GarageService.class);

    private final CarRepository carRepository;
    private final PlayerCarRepository playerCarRepository;
    private final CarUpgradeRepository carUpgradeRepository;
    private final PlayerProfileRepository profileRepository;
    private final PlayerService playerService;

    public GarageService(CarRepository carRepository,
                         PlayerCarRepository playerCarRepository,
                         CarUpgradeRepository carUpgradeRepository,
                         PlayerProfileRepository profileRepository,
                         PlayerService playerService) {
        this.carRepository = carRepository;
        this.playerCarRepository = playerCarRepository;
        this.carUpgradeRepository = carUpgradeRepository;
        this.profileRepository = profileRepository;
        this.playerService = playerService;
    }

    @Transactional(readOnly = true)
    public List<GarageDtos.CarResponse> allCars() {
        return carRepository.findAllByOrderByNameAsc().stream().map(GarageService::toCarDto).toList();
    }

    @Transactional(readOnly = true)
    public GarageDtos.GarageResponse garage(UUID userId) {
        PlayerProfile profile = playerService.getProfileEntity(userId);
        List<GarageDtos.OwnedCarResponse> owned = new ArrayList<>();
        for (PlayerCar playerCar : playerCarRepository.findByPlayerId(userId)) {
            Car car = carRepository.findById(playerCar.getCarId())
                    .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Car not found"));
            Map<UpgradeType, Integer> upgrades = new HashMap<>();
            for (CarUpgrade upgrade : carUpgradeRepository.findByPlayerCarId(playerCar.getId())) {
                upgrades.put(upgrade.getUpgradeType(), upgrade.getLevel());
            }
            owned.add(new GarageDtos.OwnedCarResponse(
                    toCarDto(car),
                    upgrades.entrySet().stream().collect(java.util.stream.Collectors.toMap(e -> e.getKey().name(), Map.Entry::getValue)),
                    currentStats(car, upgrades)));
        }
        return new GarageDtos.GarageResponse(owned, profile.getCredits(), profile.getPremiumTokens());
    }

    @Transactional
    public GarageDtos.PurchaseResponse purchase(UUID userId, UUID carId) {
        Car car = carRepository.findById(carId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Car not found"));

        // Idempotent: already owned -> success without charging again.
        if (playerCarRepository.findByPlayerIdAndCarId(userId, carId).isPresent()) {
            return new GarageDtos.PurchaseResponse(carId, playerService.getProfileEntity(userId).getCredits());
        }

        PlayerProfile profile = lockProfile(userId);
        if (profile.getCredits() < car.getPrice()) {
            throw ApiException.of(ErrorCode.INSUFFICIENT_CREDITS,
                    "Not enough credits: need " + car.getPrice() + ", have " + profile.getCredits());
        }

        profile.setCredits(profile.getCredits() - car.getPrice());
        profileRepository.save(profile);

        PlayerCar playerCar = playerCarRepository.save(new PlayerCar(userId, carId));
        for (UpgradeType type : UpgradeType.values()) {
            carUpgradeRepository.save(new CarUpgrade(playerCar.getId(), type, 1));
        }

        log.info("Player {} purchased car {} for {} credits", userId, carId, car.getPrice());
        return new GarageDtos.PurchaseResponse(carId, profile.getCredits());
    }

    @Transactional
    public GarageDtos.UpgradeResponse upgrade(UUID userId, UUID carId, UpgradeType type) {
        PlayerCar playerCar = playerCarRepository.findByPlayerIdAndCarId(userId, carId)
                .orElseThrow(() -> ApiException.of(ErrorCode.CAR_NOT_OWNED, "You do not own this car"));

        CarUpgrade upgrade = carUpgradeRepository.findByPlayerCarId(playerCar.getId()).stream()
                .filter(u -> u.getUpgradeType() == type)
                .findFirst()
                .orElseGet(() -> carUpgradeRepository.save(new CarUpgrade(playerCar.getId(), type, 1)));

        if (upgrade.getLevel() >= 5) {
            throw ApiException.of(ErrorCode.INVALID_UPGRADE_LEVEL, "Upgrade already at max level (5)");
        }

        Car car = carRepository.findById(carId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Car not found"));
        long cost = UpgradeCostCalculator.cost(type, upgrade.getLevel(), car.getRarity());

        PlayerProfile profile = lockProfile(userId);
        if (profile.getCredits() < cost) {
            throw ApiException.of(ErrorCode.INSUFFICIENT_CREDITS,
                    "Not enough credits: need " + cost + ", have " + profile.getCredits());
        }

        profile.setCredits(profile.getCredits() - cost);
        profileRepository.save(profile);

        upgrade.setLevel(upgrade.getLevel() + 1);
        carUpgradeRepository.save(upgrade);

        log.info("Player {} upgraded {} on car {} to level {} for {} credits",
                userId, type, carId, upgrade.getLevel(), cost);
        return new GarageDtos.UpgradeResponse(type.name(), upgrade.getLevel(), cost, profile.getCredits());
    }

    /** Locked profile lookup — all currency mutations must go through this. */
    private PlayerProfile lockProfile(UUID userId) {
        PlayerProfile profile = playerService.getProfileEntity(userId);
        return profileRepository.findByIdForUpdate(profile.getId())
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Player profile not found"));
    }

    private static Map<String, Double> currentStats(Car car, Map<UpgradeType, Integer> upgrades) {
        Map<String, Double> stats = new HashMap<>();
        stats.put("speed", UpgradeCostCalculator.upgradedStat(car.getBaseSpeed(), upgrades.getOrDefault(UpgradeType.TURBO, 1)));
        stats.put("acceleration", UpgradeCostCalculator.upgradedStat(car.getBaseAcceleration(), upgrades.getOrDefault(UpgradeType.ENGINE, 1)));
        stats.put("handling", UpgradeCostCalculator.upgradedStat(car.getBaseHandling(), upgrades.getOrDefault(UpgradeType.TIRES, 1)));
        stats.put("braking", UpgradeCostCalculator.upgradedStat(car.getBaseBraking(), upgrades.getOrDefault(UpgradeType.BRAKES, 1)));
        stats.put("nitro", UpgradeCostCalculator.upgradedStat(car.getBaseNitro(), upgrades.getOrDefault(UpgradeType.NITRO, 1)));
        return stats;
    }

    private static GarageDtos.CarResponse toCarDto(Car car) {
        return new GarageDtos.CarResponse(car.getId(), car.getName(), car.getRarity().name(),
                car.getBaseSpeed(), car.getBaseAcceleration(), car.getBaseHandling(),
                car.getBaseBraking(), car.getBaseNitro(), car.getPrice());
    }
}
