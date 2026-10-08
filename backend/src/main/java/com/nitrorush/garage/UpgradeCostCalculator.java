package com.nitrorush.garage;

/** Pure, testable upgrade pricing — no Spring, no DB. */
public final class UpgradeCostCalculator {

    private static final long BASE_ENGINE = 1200;
    private static final long BASE_TURBO = 1500;
    private static final long BASE_TIRES = 800;
    private static final long BASE_BRAKES = 800;
    private static final long BASE_NITRO = 1000;

    private UpgradeCostCalculator() {}

    /** Cost of upgrading FROM currentLevel to currentLevel+1. */
    public static long cost(UpgradeType type, int currentLevel, Rarity rarity) {
        if (currentLevel < 1 || currentLevel >= 5) {
            throw new IllegalArgumentException("Current level must be between 1 and 4");
        }
        long base = switch (type) {
            case ENGINE -> BASE_ENGINE;
            case TURBO -> BASE_TURBO;
            case TIRES -> BASE_TIRES;
            case BRAKES -> BASE_BRAKES;
            case NITRO -> BASE_NITRO;
        };
        return Math.round(base * currentLevel * rarityMultiplier(rarity));
    }

    public static double rarityMultiplier(Rarity rarity) {
        return switch (rarity) {
            case COMMON -> 1.0;
            case RARE -> 1.5;
            case EPIC -> 2.0;
            case LEGENDARY -> 3.0;
        };
    }

    /** Each upgrade level above 1 adds +8% of the base stat. */
    public static double upgradedStat(double baseStat, int level) {
        return baseStat * (1.0 + 0.08 * (Math.max(1, Math.min(5, level)) - 1));
    }
}
