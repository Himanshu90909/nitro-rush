package com.nitrorush.events;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import com.nitrorush.player.PlayerProfile;
import com.nitrorush.player.PlayerProfileRepository;
import com.nitrorush.player.PlayerService;
import com.nitrorush.rewards.RewardClaim;
import com.nitrorush.rewards.RewardClaimRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.dao.DataIntegrityViolationException;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.util.*;

/**
 * Live events: objectives are tracked server-side only. Objective progress is
 * recorded from validated gameplay (RaceService calls recordObjective).
 * Rewards are granted once automatically when ALL objectives complete, and
 * recorded in the reward-claims ledger for idempotency.
 */
@Service
public class EventService {

    private static final Logger log = LoggerFactory.getLogger(EventService.class);

    private final GameEventRepository eventRepository;
    private final EventProgressRepository progressRepository;
    private final ObjectMapper objectMapper;
    private final PlayerProfileRepository profileRepository;
    private final PlayerService playerService;
    private final RewardClaimRepository claimRepository;

    public EventService(GameEventRepository eventRepository,
                        EventProgressRepository progressRepository,
                        ObjectMapper objectMapper,
                        PlayerProfileRepository profileRepository,
                        PlayerService playerService,
                        RewardClaimRepository claimRepository) {
        this.eventRepository = eventRepository;
        this.progressRepository = progressRepository;
        this.objectMapper = objectMapper;
        this.profileRepository = profileRepository;
        this.playerService = playerService;
        this.claimRepository = claimRepository;
    }

    @Transactional(readOnly = true)
    public List<EventDtos.EventResponse> activeEvents() {
        Instant now = Instant.now();
        return eventRepository.findByActiveTrueOrderByStartTimeDesc().stream()
                .filter(event -> event.getEndTime().isAfter(now))
                .map(event -> toDto(event, true))
                .toList();
    }

    @Transactional(readOnly = true)
    public EventDtos.EventResponse event(UUID eventId) {
        GameEvent event = eventRepository.findById(eventId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Event not found"));
        return toDto(event, event.isActive() && event.getEndTime().isAfter(Instant.now()));
    }

    /** Join is idempotent — a UNIQUE constraint guards duplicate progress rows. */
    @Transactional
    public EventDtos.EventProgressResponse join(UUID userId, UUID eventId) {
        GameEvent event = eventRepository.findById(eventId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Event not found"));

        List<EventDtos.ObjectiveSpec> objectives = parseObjectives(event.getConfiguration());
        for (EventDtos.ObjectiveSpec spec : objectives) {
            if (progressRepository.findByPlayerIdAndEventIdAndObjectiveType(userId, eventId, ObjectiveType.valueOf(spec.type())).isEmpty()) {
                try {
                    progressRepository.saveAndFlush(new EventProgress(userId, eventId, ObjectiveType.valueOf(spec.type())));
                } catch (DataIntegrityViolationException race) {
                    // Another request inserted the same row — join is idempotent.
                    log.debug("Event join race for player {} event {}: ignored", userId, eventId);
                }
            }
        }
        return progress(userId, eventId);
    }

    @Transactional(readOnly = true)
    public EventDtos.EventProgressResponse progress(UUID userId, UUID eventId) {
        GameEvent event = eventRepository.findById(eventId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Event not found"));
        List<EventDtos.ObjectiveSpec> specs = parseObjectives(event.getConfiguration());

        Map<ObjectiveType, EventProgress> progressMap = new EnumMap<>(ObjectiveType.class);
        for (EventProgress p : progressRepository.findByPlayerIdAndEventId(userId, eventId)) {
            progressMap.put(p.getObjectiveType(), p);
        }

        boolean allComplete = !specs.isEmpty();
        List<EventDtos.ObjectiveProgress> result = new ArrayList<>();
        for (EventDtos.ObjectiveSpec spec : specs) {
            ObjectiveType type = ObjectiveType.valueOf(spec.type());
            EventProgress p = progressMap.get(type);
            int current = p == null ? 0 : p.getProgress();
            boolean completed = current >= spec.count();
            if (!completed) {
                allComplete = false;
            }
            result.add(new EventDtos.ObjectiveProgress(spec.type(), current, spec.count(), completed));
        }
        return new EventDtos.EventProgressResponse(result, allComplete);
    }

    /**
     * Called by validated gameplay flows (e.g. RaceService after a verified race).
     * Increments matching objectives on every ACTIVE event for the player.
     */
    @Transactional
    public void recordObjective(UUID userId, ObjectiveType type) {
        Instant now = Instant.now();
        for (GameEvent event : eventRepository.findByActiveTrueOrderByStartTimeDesc()) {
            if (!event.getEndTime().isAfter(now)) continue;
            List<EventDtos.ObjectiveSpec> specs = parseObjectives(event.getConfiguration());
            for (EventDtos.ObjectiveSpec spec : specs) {
                if (!spec.type().equals(type.name())) continue;

                EventProgress progress = progressRepository
                        .findByPlayerIdAndEventIdAndObjectiveType(userId, event.getId(), type)
                        .orElseGet(() -> progressRepository.save(new EventProgress(userId, event.getId(), type)));
                if (progress.getCompletedAt() != null) continue; // objective already done

                progress.setProgress(progress.getProgress() + 1);
                boolean objectiveDone = progress.getProgress() >= spec.count();
                if (objectiveDone) {
                    progress.setCompletedAt(Instant.now());
                }
                progressRepository.save(progress);
            }
            maybeCompleteEvent(userId, event, specs);
        }
    }

    /** Grants rewards once when every objective of an event is complete for this player. */
    private void maybeCompleteEvent(UUID userId, GameEvent event, List<EventDtos.ObjectiveSpec> specs) {
        String claimKey = event.getId().toString();
        if (claimRepository.existsByPlayerIdAndSourceTypeAndSourceId(userId, "EVENT", claimKey)) {
            return;
        }
        EventDtos.EventProgressResponse state = progress(userId, event.getId());
        if (!state.eventCompleted()) {
            return;
        }
        EventDtos.RewardsSpec rewards = parseRewards(event.getConfiguration());

        PlayerProfile profile = playerService.getProfileEntity(userId);
        PlayerProfile locked = profileRepository.findByIdForUpdate(profile.getId())
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Player profile not found"));
        if (rewards.credits() != null) locked.setCredits(locked.getCredits() + rewards.credits());
        if (rewards.xp() != null) {
            locked.setXp(locked.getXp() + rewards.xp());
            locked.setLevel(PlayerService.levelForXp(locked.getXp()));
        }
        if (rewards.tokens() != null) locked.setPremiumTokens(locked.getPremiumTokens() + rewards.tokens());
        profileRepository.save(locked);
        claimRepository.save(new RewardClaim(userId, "EVENT", claimKey));

        log.info("Player {} completed event {} (+{} credits, +{} xp, +{} tokens)", userId, event.getId(),
                rewards.credits(), rewards.xp(), rewards.tokens());
    }

    private EventDtos.EventResponse toDto(GameEvent event, boolean active) {
        return new EventDtos.EventResponse(event.getId(), event.getName(), event.getDescription(),
                event.getStartTime(), event.getEndTime(), active,
                parseObjectives(event.getConfiguration()), parseRewards(event.getConfiguration()));
    }

    List<EventDtos.ObjectiveSpec> parseObjectives(String configuration) {
        try {
            JsonNode root = objectMapper.readTree(configuration);
            List<EventDtos.ObjectiveSpec> specs = new ArrayList<>();
            for (JsonNode objective : root.path("objectives")) {
                specs.add(new EventDtos.ObjectiveSpec(
                        objective.path("type").asText("WIN_RACES"),
                        objective.path("count").asInt(1)));
            }
            if (specs.isEmpty()) {
                specs.add(new EventDtos.ObjectiveSpec("WIN_RACES", 1));
            }
            return specs;
        } catch (Exception e) {
            return List.of(new EventDtos.ObjectiveSpec("WIN_RACES", 1));
        }
    }

    EventDtos.RewardsSpec parseRewards(String configuration) {
        try {
            JsonNode rewards = objectMapper.readTree(configuration).path("rewards");
            return new EventDtos.RewardsSpec(
                    rewards.path("credits").asLong(0),
                    rewards.path("xp").asLong(0),
                    rewards.path("tokens").asInt(0));
        } catch (Exception e) {
            return new EventDtos.RewardsSpec(0L, 0L, 0);
        }
    }
}
