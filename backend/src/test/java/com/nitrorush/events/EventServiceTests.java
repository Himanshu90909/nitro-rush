package com.nitrorush.events;

import com.nitrorush.auth.AuthDtos;
import com.nitrorush.auth.AuthService;
import com.nitrorush.rewards.RewardClaimRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

@SpringBootTest
@ActiveProfiles("test")
class EventServiceTests {

    @Autowired
    private EventService eventService;
    @Autowired
    private GameEventRepository eventRepository;
    @Autowired
    private AuthService authService;
    @Autowired
    private RewardClaimRepository claimRepository;

    private UUID newPlayer() {
        return authService.register(new AuthDtos.RegisterRequest(
                "events-" + UUID.randomUUID() + "@test.dev", "supersecret1", "event")).user().id();
    }

    private GameEvent seedEvent(int winRacesCount, int finishCount, long credits) {
        String config = String.format(
                "{\"objectives\":[{\"type\":\"WIN_RACES\",\"count\":%d},{\"type\":\"FINISH_RACES\",\"count\":%d}],"
                        + "\"rewards\":{\"credits\":%d,\"xp\":100,\"tokens\":2}}",
                winRacesCount, finishCount, credits);
        return eventRepository.save(new GameEvent("Nitro Weekend", "Test event",
                Instant.now().minusSeconds(3600), Instant.now().plusSeconds(3600), config));
    }

    @Test
    void activeEventsParsesConfiguration() {
        GameEvent event = seedEvent(5, 3, 5000);
        List<EventDtos.EventResponse> active = eventService.activeEvents();

        assertTrue(active.stream().anyMatch(e -> e.id().equals(event.getId())));
        EventDtos.EventResponse dto = active.stream().filter(e -> e.id().equals(event.getId())).findFirst().orElseThrow();
        assertEquals(2, dto.objectives().size());
        assertEquals(5000L, dto.rewards().credits());
    }

    @Test
    void joinIsIdempotent() {
        UUID player = newPlayer();
        GameEvent event = seedEvent(5, 3, 5000);

        EventDtos.EventProgressResponse first = eventService.join(player, event.getId());
        EventDtos.EventProgressResponse second = eventService.join(player, event.getId());

        assertEquals(first, second);
        assertEquals(2, second.objectives().size());
    }

    @Test
    void recordObjectiveIncrementsProgress() {
        UUID player = newPlayer();
        GameEvent event = seedEvent(2, 2, 5000);
        eventService.join(player, event.getId());

        eventService.recordObjective(player, ObjectiveType.FINISH_RACES);
        EventDtos.EventProgressResponse progress = eventService.progress(player, event.getId());

        assertEquals(1, progress.objectives().stream()
                .filter(o -> "FINISH_RACES".equals(o.type()))
                .findFirst().orElseThrow().progress());
        assertFalse(progress.eventCompleted());
    }

    @Test
    void completingAllObjectivesGrantsRewardsOnce() {
        UUID player = newPlayer();
        GameEvent event = seedEvent(1, 1, 5000);
        eventService.join(player, event.getId());

        eventService.recordObjective(player, ObjectiveType.WIN_RACES);
        eventService.recordObjective(player, ObjectiveType.FINISH_RACES);

        EventDtos.EventProgressResponse progress = eventService.progress(player, event.getId());
        assertTrue(progress.eventCompleted());
        assertTrue(claimRepository.existsByPlayerIdAndSourceTypeAndSourceId(player, "EVENT", event.getId().toString()),
                "claim recorded in idempotency ledger");

        // Extra objective ticks after completion must not double-grant.
        eventService.recordObjective(player, ObjectiveType.WIN_RACES);
        long claimCount = claimRepository.findAll().stream()
                .filter(c -> c.getPlayerId().equals(player) && c.getSourceType().equals("EVENT"))
                .count();
        assertEquals(1, claimCount);
    }

    @Test
    void unknownEventRejected() {
        UUID player = newPlayer();
        assertThrows(com.nitrorush.common.ApiException.class,
                () -> eventService.join(player, UUID.randomUUID()));
    }
}
