package com.nitrorush.garage;

import jakarta.persistence.*;

import java.util.UUID;

/** Catalog car. All stats are base values in SI units (speed in m/s); upgrades scale them. */
@Entity
@Table(name = "cars")
public class Car {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(nullable = false, length = 64)
    private String name;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private Rarity rarity;

    private double baseSpeed;
    private double baseAcceleration;
    private double baseHandling;
    private double baseBraking;
    private double baseNitro;

    @Column(nullable = false)
    private long price;

    public Car() {
    }

    public Car(String name, Rarity rarity, double baseSpeed, double baseAcceleration,
               double baseHandling, double baseBraking, double baseNitro, long price) {
        this.name = name;
        this.rarity = rarity;
        this.baseSpeed = baseSpeed;
        this.baseAcceleration = baseAcceleration;
        this.baseHandling = baseHandling;
        this.baseBraking = baseBraking;
        this.baseNitro = baseNitro;
        this.price = price;
    }

    public UUID getId() { return id; }
    public String getName() { return name; }
    public Rarity getRarity() { return rarity; }
    public double getBaseSpeed() { return baseSpeed; }
    public double getBaseAcceleration() { return baseAcceleration; }
    public double getBaseHandling() { return baseHandling; }
    public double getBaseBraking() { return baseBraking; }
    public double getBaseNitro() { return baseNitro; }
    public long getPrice() { return price; }
}
