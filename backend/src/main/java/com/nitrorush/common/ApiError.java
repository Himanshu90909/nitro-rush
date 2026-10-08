package com.nitrorush.common;

public record ApiError(
    String code,
    String message
) {}
