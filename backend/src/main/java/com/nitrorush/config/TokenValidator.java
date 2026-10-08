package com.nitrorush.config;

import java.util.UUID;

public interface TokenValidator {
    UUID validate(String token);
}
