package com.nitrorush.common;

import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;

import java.util.UUID;

public class SecurityUtils {

    private SecurityUtils() {}

    public static CurrentUser getCurrentUser() {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        if (auth == null || !auth.isAuthenticated() || auth.getPrincipal() == null) {
            throw ApiException.of(ErrorCode.AUTH_REQUIRED, "Authentication required");
        }

        Object principal = auth.getPrincipal();
        UUID userId;
        if (principal instanceof UUID u) {
            userId = u;
        } else if (principal instanceof String s) {
            try {
                userId = UUID.fromString(s);
            } catch (IllegalArgumentException e) {
                throw ApiException.of(ErrorCode.AUTH_REQUIRED, "Invalid authentication principal format");
            }
        } else {
            throw ApiException.of(ErrorCode.AUTH_REQUIRED, "Invalid authentication principal");
        }

        String role = auth.getAuthorities().stream()
                .findFirst()
                .map(a -> a.getAuthority().replace("ROLE_", ""))
                .orElse("PLAYER");

        return new CurrentUser(userId, role);
    }

    public static UUID currentUserId() {
        return getCurrentUser().userId();
    }
}
