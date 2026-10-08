package com.nitrorush.inventory;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.Map;

@RestController
@RequestMapping("/api/v1/inventory")
public class InventoryController {

    private final InventoryService inventoryService;

    public InventoryController(InventoryService inventoryService) {
        this.inventoryService = inventoryService;
    }

    @GetMapping
    public ApiResponse<Map<String, Integer>> inventory() {
        return ApiResponse.ok(inventoryService.getInventory(SecurityUtils.currentUserId()));
    }
}
