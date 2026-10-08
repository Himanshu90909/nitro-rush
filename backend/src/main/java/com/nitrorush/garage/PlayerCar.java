package com.nitrorush.garage;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "player_cars", uniqueConstraints = @UniqueConstraint(name = "uk_player_car", columnNames = {"player_id", "car_id"}))
public class PlayerCar {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "player_id", nullable = false)
    private UUID playerId;

    @Column(name = "car_id", nullable = false)
    private UUID carId;

    @Column(nullable = false)
    private boolean owned;

    @Column(nullable = false, updatable = false)
    private Instant purchasedAt;

    public PlayerCar() {
    }

    public PlayerCar(UUID playerId, UUID carId) {
        this.playerId = playerId;
        this.carId = carId;
        this.owned = true;
        this.purchasedAt = Instant.now();
    }

    public UUID getId() { return id; }
    public UUID getPlayerId() { return playerId; }
    public UUID getCarId() { return carId; }
    public boolean isOwned() { return owned; }
    public Instant getPurchasedAt() { return purchasedAt; }
}
