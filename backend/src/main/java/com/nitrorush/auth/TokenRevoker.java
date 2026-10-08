package com.nitrorush.auth;

import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Propagation;
import org.springframework.transaction.annotation.Transactional;

/**
 * Runs in its OWN transaction so a rollback of the caller (which throws after
 * detecting reuse) can never resurrect the revoked tokens. Without REQUIRES_NEW
 * the deleteByUserId would be rolled back together with the exception.
 */
@Service
public class TokenRevoker {

    private final RefreshTokenRepository refreshTokenRepository;

    public TokenRevoker(RefreshTokenRepository refreshTokenRepository) {
        this.refreshTokenRepository = refreshTokenRepository;
    }

    @Transactional(propagation = Propagation.REQUIRES_NEW)
    public void revokeAllForUser(Object userId) {
        try {
            refreshTokenRepository.deleteByUserId((java.util.UUID) userId);
        } catch (ClassCastException ignored) {
            // defensive: wrong type never happens in practice
        }
    }
}
