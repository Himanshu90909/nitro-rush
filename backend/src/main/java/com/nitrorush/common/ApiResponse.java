package com.nitrorush.common;

import java.time.Instant;

public record ApiResponse<T>(
    boolean success,
    T data,
    ApiError error,
    String timestamp
) {
    public static <T> ApiResponse<T> ok(T data) {
        return new ApiResponse<>(true, data, null, Instant.now().toString());
    }

    public static <T> ApiResponse<T> error(ErrorCode code, String message) {
        return new ApiResponse<>(false, null, new ApiError(code.name(), message), Instant.now().toString());
    }

    public static <T> ApiResponse<T> error(ErrorCode code) {
        return new ApiResponse<>(false, null, new ApiError(code.name(), code.name()), Instant.now().toString());
    }
}
