package com.nitrorush.garage;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.UUID;

public interface CarUpgradeRepository extends JpaRepository<CarUpgrade, UUID> {
    List<CarUpgrade> findByPlayerCarId(UUID playerCarId);
}
