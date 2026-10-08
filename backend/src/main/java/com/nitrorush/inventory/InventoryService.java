package com.nitrorush.inventory;

import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

/**
 * In-memory HashMap is used for the response view; the DB is the source of truth.
 * Quantity is capped at 9999 per item.
 */
@Service
public class InventoryService {

    private static final int MAX_QUANTITY = 9999;

    private final InventoryItemRepository repository;

    public InventoryService(InventoryItemRepository repository) {
        this.repository = repository;
    }

    @Transactional
    public InventoryItem addItem(UUID playerId, String itemId, int amount) {
        if (amount <= 0) {
            throw ApiException.of(ErrorCode.VALIDATION_FAILED, "Amount must be positive");
        }
        InventoryItem item = repository.findByPlayerIdAndItemId(playerId, itemId)
                .orElseGet(() -> new InventoryItem(playerId, itemId, 0));
        int newQuantity = Math.min(MAX_QUANTITY, item.getQuantity() + amount);
        item.setQuantity(newQuantity);
        return repository.save(item);
    }

    @Transactional(readOnly = true)
    public Map<String, Integer> getInventory(UUID playerId) {
        Map<String, Integer> view = new HashMap<>();
        for (InventoryItem item : repository.findByPlayerId(playerId)) {
            view.put(item.getItemId(), item.getQuantity());
        }
        return view;
    }

    @Transactional(readOnly = true)
    public List<InventoryItem> itemsOf(UUID playerId) {
        return repository.findByPlayerId(playerId);
    }
}
