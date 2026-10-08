package com.nitrorush.racing;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface RaceResultRepository extends JpaRepository<RaceResult, UUID> {

    List<RaceResult> findByRaceIdOrderByFinishTimeMsAsc(UUID raceId);

    Optional<RaceResult> findByRaceIdAndPlayerId(UUID raceId, UUID playerId);

    List<RaceResult> findTop20ByPlayerIdOrderByCreatedAtDesc(UUID playerId);

    /** Postgres fallback leaderboard: SUM(score) grouped by player, best first. */
    @Query("select r.playerId as playerId, sum(r.score) as total from RaceResult r group by r.playerId order by sum(r.score) desc")
    List<Object[]> aggregateScoresByPlayer();

    @Query("select coalesce(sum(r.score), 0) from RaceResult r where r.playerId = :playerId")
    long totalScoreForPlayer(@Param("playerId") UUID playerId);
}
