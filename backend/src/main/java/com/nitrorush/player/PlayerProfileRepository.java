package com.nitrorush.player;

import jakarta.persistence.LockModeType;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Lock;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface PlayerProfileRepository extends JpaRepository<PlayerProfile, UUID> {

    Optional<PlayerProfile> findByUserId(UUID userId);

    Optional<PlayerProfile> findByUsernameIgnoreCase(String username);

    List<PlayerProfile> findAllByIdIn(Iterable<UUID> ids);

    /**
     * Pessimistic write lock — the backbone of the server-authoritative
     * economy. Every currency mutation locks the row first so two
     * concurrent purchases can never double-spend.
     */
    @Lock(LockModeType.PESSIMISTIC_WRITE)
    @Query("select p from PlayerProfile p where p.id = :id")
    Optional<PlayerProfile> findByIdForUpdate(@Param("id") UUID id);
}
