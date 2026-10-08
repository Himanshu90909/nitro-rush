package com.nitrorush.garage;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
public class GarageController {

    private final GarageService garageService;

    public GarageController(GarageService garageService) {
        this.garageService = garageService;
    }

    @GetMapping("/api/v1/cars")
    public ApiResponse<List<GarageDtos.CarResponse>> allCars() {
        return ApiResponse.ok(garageService.allCars());
    }

    @GetMapping("/api/v1/garage")
    public ApiResponse<GarageDtos.GarageResponse> garage() {
        return ApiResponse.ok(garageService.garage(SecurityUtils.currentUserId()));
    }

    @PostMapping("/api/v1/garage/cars/{carId}/purchase")
    public ApiResponse<GarageDtos.PurchaseResponse> purchase(@PathVariable UUID carId) {
        return ApiResponse.ok(garageService.purchase(SecurityUtils.currentUserId(), carId));
    }

    @PostMapping("/api/v1/garage/cars/{carId}/upgrade")
    public ApiResponse<GarageDtos.UpgradeResponse> upgrade(@PathVariable UUID carId,
                                                          @Valid @RequestBody GarageDtos.UpgradeRequest request) {
        return ApiResponse.ok(garageService.upgrade(SecurityUtils.currentUserId(), carId, request.upgradeType()));
    }
}
