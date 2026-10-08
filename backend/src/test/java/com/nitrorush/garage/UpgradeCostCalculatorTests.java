package com.nitrorush.garage;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class UpgradeCostCalculatorTests {

    @Test
    void engineUpgradeCommonLevel1To2() {
        assertEquals(1200, UpgradeCostCalculator.cost(UpgradeType.ENGINE, 1, Rarity.COMMON));
    }

    @Test
    void engineUpgradeCommonLevel4To5() {
        assertEquals(4800, UpgradeCostCalculator.cost(UpgradeType.ENGINE, 4, Rarity.COMMON));
    }

    @Test
    void turboRareCostsMoreThanCommon() {
        long common = UpgradeCostCalculator.cost(UpgradeType.TURBO, 2, Rarity.COMMON);
        long rare = UpgradeCostCalculator.cost(UpgradeType.TURBO, 2, Rarity.RARE);
        assertEquals(3000, common);
        assertEquals(4500, rare);
    }

    @Test
    void legendaryMultiplierIs3x() {
        assertEquals(9000, UpgradeCostCalculator.cost(UpgradeType.TURBO, 2, Rarity.LEGENDARY));
        assertEquals(7200, UpgradeCostCalculator.cost(UpgradeType.ENGINE, 2, Rarity.LEGENDARY));
    }

    @Test
    void maxLevelCannotBeUpgradedFurther() {
        assertThrows(IllegalArgumentException.class, () -> UpgradeCostCalculator.cost(UpgradeType.NITRO, 5, Rarity.COMMON));
        assertThrows(IllegalArgumentException.class, () -> UpgradeCostCalculator.cost(UpgradeType.NITRO, 0, Rarity.COMMON));
    }

    @Test
    void upgradedStatGrows8PercentPerLevel() {
        assertEquals(100.0, UpgradeCostCalculator.upgradedStat(100.0, 1), 0.0001);
        assertEquals(108.0, UpgradeCostCalculator.upgradedStat(100.0, 2), 0.0001);
        assertEquals(132.0, UpgradeCostCalculator.upgradedStat(100.0, 5), 0.0001);
        // Out-of-range levels clamp into 1..5.
        assertEquals(100.0, UpgradeCostCalculator.upgradedStat(100.0, 0), 0.0001);
        assertEquals(132.0, UpgradeCostCalculator.upgradedStat(100.0, 99), 0.0001);
    }
}
