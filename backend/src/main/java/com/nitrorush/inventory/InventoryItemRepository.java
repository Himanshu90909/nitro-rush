package com.nitrorush.inventory;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface InventoryItemRepository extends JpaRepository<InventoryItem, UUID> {
    List<InventoryItem> findByPlayerId(UUID playerId);
    Optional<InventoryItem> findByPlayerIdAndItemId(UUID playerId, String itemId);
}
