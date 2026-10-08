package com.nitrorush.inventory;

import jakarta.persistence.*;

import java.util.UUID;

@Entity
@Table(name = "inventory_items", uniqueConstraints = @UniqueConstraint(name = "uk_inventory", columnNames = {"player_id", "item_id"}))
public class InventoryItem {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "player_id", nullable = false)
    private UUID playerId;

    @Column(name = "item_id", nullable = false, length = 64)
    private String itemId;

    @Column(nullable = false)
    private int quantity;

    public InventoryItem() {
    }

    public InventoryItem(UUID playerId, String itemId, int quantity) {
        this.playerId = playerId;
        this.itemId = itemId;
        this.quantity = quantity;
    }

    public UUID getId() { return id; }
    public UUID getPlayerId() { return playerId; }
    public String getItemId() { return itemId; }
    public int getQuantity() { return quantity; }
    public void setQuantity(int quantity) { this.quantity = quantity; }
}
