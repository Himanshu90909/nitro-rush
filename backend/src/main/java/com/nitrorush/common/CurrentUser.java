package com.nitrorush.common;

import java.util.UUID;

public record CurrentUser(
    UUID userId,
    String role
) {}
