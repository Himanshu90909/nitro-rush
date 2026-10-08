package com.nitrorush.garage;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface PlayerCarRepository extends JpaRepository<PlayerCar, UUID> {
    List<PlayerCar> findByPlayerId(UUID playerId);
    Optional<PlayerCar> findByPlayerIdAndCarId(UUID playerId, UUID carId);
}
