package com.nitrorush.events;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface EventProgressRepository extends JpaRepository<EventProgress, UUID> {
    List<EventProgress> findByPlayerIdAndEventId(UUID playerId, UUID eventId);
    Optional<EventProgress> findByPlayerIdAndEventIdAndObjectiveType(UUID playerId, UUID eventId, ObjectiveType objectiveType);
    List<EventProgress> findByEventIdAndObjectiveTypeAndCompletedAtIsNull(UUID eventId, ObjectiveType objectiveType);
}
