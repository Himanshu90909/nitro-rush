package com.nitrorush.auth;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;

import java.util.UUID;

/** Auth request/response DTOs — entities are never exposed through the API. */
public final class AuthDtos {
    private AuthDtos() {}

    public record RegisterRequest(
            @Email @NotBlank String email,
            @NotBlank @Size(min = 8, max = 72) String password,
            @NotBlank @Size(min = 3, max = 20) String username) {}

    public record LoginRequest(@Email @NotBlank String email, @NotBlank String password) {}

    public record RefreshRequest(@NotBlank String refreshToken) {}

    public record UserDto(UUID id, String email, String role) {}

    public record AuthResponse(String accessToken, String refreshToken, UserDto user) {}
}
