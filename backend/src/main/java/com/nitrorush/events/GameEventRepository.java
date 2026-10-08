package com.nitrorush.events;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.UUID;

public interface GameEventRepository extends JpaRepository<GameEvent, UUID> {
    List<GameEvent> findByActiveTrueOrderByStartTimeDesc();
}
