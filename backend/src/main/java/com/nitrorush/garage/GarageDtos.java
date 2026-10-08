package com.nitrorush.garage;

import jakarta.validation.constraints.NotNull;

import java.util.List;
import java.util.Map;
import java.util.UUID;

public final class GarageDtos {
    private GarageDtos() {}

    public record CarResponse(UUID id, String name, String rarity, double baseSpeed,
                              double baseAcceleration, double baseHandling,
                              double baseBraking, double baseNitro, long price) {}

    public record OwnedCarResponse(CarResponse car, Map<String, Integer> upgrades, Map<String, Double> currentStats) {}

    public record GarageResponse(List<OwnedCarResponse> owned, long credits, int premiumTokens) {}

    public record PurchaseResponse(UUID carId, long newCredits) {}

    public record UpgradeRequest(@NotNull UpgradeType upgradeType) {}

    public record UpgradeResponse(String upgradeType, int newLevel, long cost, long newCredits) {}
}
