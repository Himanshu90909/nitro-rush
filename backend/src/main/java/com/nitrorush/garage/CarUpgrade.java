package com.nitrorush.garage;

import jakarta.persistence.*;

import java.util.UUID;

@Entity
@Table(name = "car_upgrades", uniqueConstraints = @UniqueConstraint(name = "uk_upgrade", columnNames = {"player_car_id", "upgrade_type"}))
public class CarUpgrade {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "player_car_id", nullable = false)
    private UUID playerCarId;

    @Enumerated(EnumType.STRING)
    @Column(name = "upgrade_type", nullable = false)
    private UpgradeType upgradeType;

    /** Upgrade levels run 1..5; each level above 1 adds +8% of the base stat. */
    @Column(nullable = false)
    private int level = 1;

    public CarUpgrade() {
    }

    public CarUpgrade(UUID playerCarId, UpgradeType upgradeType, int level) {
        this.playerCarId = playerCarId;
        this.upgradeType = upgradeType;
        this.level = level;
    }

    public UUID getId() { return id; }
    public UUID getPlayerCarId() { return playerCarId; }
    public UpgradeType getUpgradeType() { return upgradeType; }
    public int getLevel() { return level; }
    public void setLevel(int level) { this.level = level; }
}
