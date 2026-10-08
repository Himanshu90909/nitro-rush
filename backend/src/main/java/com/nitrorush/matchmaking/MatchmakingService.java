package com.nitrorush.matchmaking;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.dao.DataAccessException;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.data.redis.core.ZSetOperations;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.*;

/**
 * Redis sorted-set matchmaking queue.
 *
 * - queue key: nitrorush:matchmaking:queue, score = enqueue epoch millis,
 *   member = playerId -> ZRANGE returns the longest-waiting players first
 * - rating = level * 100 (extensible skill model; region/latency on roadmap)
 * - tryMatch groups the oldest player with everyone within a 200-rating window,
 *   min 2 / max 8 players, into a lobby with a 5-minute TTL
 */
@Service
public class MatchmakingService {

    private static final Logger log = LoggerFactory.getLogger(MatchmakingService.class);
    static final String QUEUE_KEY = "nitrorush:matchmaking:queue";
    static final String LOBBY_PREFIX = "nitrorush:lobby:";
    static final int RATING_WINDOW = 200;
    static final int MIN_PLAYERS = 2;
    static final int MAX_PLAYERS = 8;
    static final Duration LOBBY_TTL = Duration.ofMinutes(5);

    private final StringRedisTemplate redis;
    private final ObjectMapper objectMapper;
    private final PlayerProfileRepository profileRepository;

    public MatchmakingService(StringRedisTemplate redis,
                              ObjectMapper objectMapper,
                              PlayerProfileRepository profileRepository) {
        this.redis = redis;
        this.objectMapper = objectMapper;
        this.profileRepository = profileRepository;
    }

    /** Adds the player to the queue (idempotent — ZADD NX semantics). */
    public void join(UUID playerId) {
        try {
            ZSetOperations<String, String> zSet = redis.opsForZSet();
            Double existing = zSet.score(QUEUE_KEY, playerId.toString());
            if (existing == null) {
                zSet.add(QUEUE_KEY, playerId.toString(), (double) System.currentTimeMillis());
            }
        } catch (DataAccessException e) {
            log.warn("Matchmaking queue unavailable: {}", e.getMessage());
        }
    }

    public MatchmakingDtos.QueueStatus status(UUID playerId) {
        try {
            Long rank = redis.opsForZSet().rank(QUEUE_KEY, playerId.toString());
            Long size = redis.opsForZSet().zCard(QUEUE_KEY);
            boolean queued = rank != null;
            long queueSize = size == null ? 0 : size;
            // Rough estimate: positions ahead * 3s tick interval of the refresher.
            long estimate = queued ? rank * 3000L : 0;
            return new MatchmakingDtos.QueueStatus(queued ? rank.intValue() + 1 : 0, queueSize, estimate);
        } catch (DataAccessException e) {
            return new MatchmakingDtos.QueueStatus(0, 0, 0);
        }
    }

    public void leave(UUID playerId) {
        try {
            redis.opsForZSet().remove(QUEUE_KEY, playerId.toString());
        } catch (DataAccessException e) {
            log.warn("Matchmaking queue unavailable: {}", e.getMessage());
        }
    }

    /** Attempts to form a lobby from the longest-waiting players. Returns empty when nobody matches. */
    public Optional<MatchmakingDtos.LobbyInfo> tryMatch() {
        try {
            Set<String> queued = redis.opsForZSet().range(QUEUE_KEY, 0, MAX_PLAYERS - 1);
            if (queued == null || queued.size() < MIN_PLAYERS) {
                return Optional.empty();
            }

            List<UUID> candidates = queued.stream().map(UUID::fromString).toList();
            UUID anchor = candidates.get(0);
            int anchorRating = ratingOf(anchor);

            List<UUID> matched = new ArrayList<>();
            for (UUID candidate : candidates) {
                if (Math.abs(ratingOf(candidate) - anchorRating) <= RATING_WINDOW) {
                    matched.add(candidate);
                }
                if (matched.size() >= MAX_PLAYERS) break;
            }
            if (matched.size() < MIN_PLAYERS) {
                return Optional.empty();
            }

            UUID lobbyId = UUID.randomUUID();
            Map<String, Object> lobby = new LinkedHashMap<>();
            lobby.put("playerIds", matched);
            lobby.put("trackId", "neon-city");
            lobby.put("createdAt", System.currentTimeMillis());
            redis.opsForValue().set(LOBBY_PREFIX + lobbyId, objectMapper.writeValueAsString(lobby), LOBBY_TTL);

            for (UUID playerId : matched) {
                redis.opsForZSet().remove(QUEUE_KEY, playerId.toString());
            }

            log.info("Matchmade lobby {} with {} players", lobbyId, matched.size());
            return Optional.of(new MatchmakingDtos.LobbyInfo(lobbyId, matched, "neon-city"));
        } catch (DataAccessException e) {
            log.warn("Matchmaking unavailable: {}", e.getMessage());
            return Optional.empty();
        } catch (Exception e) {
            log.warn("Matchmaking error: {}", e.getMessage());
            return Optional.empty();
        }
    }

    public Optional<MatchmakingDtos.LobbyInfo> lobby(UUID lobbyId) {
        try {
            String json = redis.opsForValue().get(LOBBY_PREFIX + lobbyId);
            if (json == null) {
                return Optional.empty();
            }
            Map<?, ?> raw = objectMapper.readValue(json, Map.class);
            List<UUID> playerIds = new ArrayList<>();
            for (Object id : (List<?>) raw.get("playerIds")) {
                playerIds.add(UUID.fromString(id.toString()));
            }
            Object track = raw.get("trackId");
            return Optional.of(new MatchmakingDtos.LobbyInfo(
                    lobbyId, playerIds, track == null ? "neon-city" : track.toString()));
        } catch (Exception e) {
            return Optional.empty();
        }
    }

    private int ratingOf(UUID playerId) {
        return profileRepository.findByUserId(playerId)
                .map(PlayerProfile::getLevel)
                .orElse(1) * 100;
    }
}
