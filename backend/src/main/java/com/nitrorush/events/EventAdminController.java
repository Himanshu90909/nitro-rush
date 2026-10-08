package com.nitrorush.events;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.nitrorush.common.ApiException;
import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.ErrorCode;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.time.Instant;
import java.util.List;
import java.util.Map;
import java.util.UUID;

/** Admin-only event lifecycle. Protected by hasRole(ADMIN) in SecurityConfig. */
@RestController
@RequestMapping("/api/v1/admin/events")
public class EventAdminController {

    private final GameEventRepository eventRepository;
    private final ObjectMapper objectMapper;

    public EventAdminController(GameEventRepository eventRepository, ObjectMapper objectMapper) {
        this.eventRepository = eventRepository;
        this.objectMapper = objectMapper;
    }

    @PostMapping
    public ApiResponse<EventDtos.EventResponse> create(@Valid @RequestBody EventDtos.AdminEventRequest request) {
        if (!request.endTime().isAfter(request.startTime())) {
            throw ApiException.of(ErrorCode.VALIDATION_FAILED, "endTime must be after startTime");
        }
        try {
            String configuration = objectMapper.writeValueAsString(Map.of(
                    "objectives", request.objectives() == null ? List.of() : request.objectives(),
                    "rewards", request.rewards() == null ? Map.of() : request.rewards()));
            GameEvent event = eventRepository.save(new GameEvent(
                    request.name(), request.description(), request.startTime(), request.endTime(), configuration));
            return ApiResponse.ok(new EventDtos.EventResponse(event.getId(), event.getName(), event.getDescription(),
                    event.getStartTime(), event.getEndTime(), event.isActive(),
                    request.objectives() == null ? List.of() : request.objectives(),
                    request.rewards()));
        } catch (ApiException e) {
            throw e;
        } catch (Exception e) {
            throw ApiException.of(ErrorCode.VALIDATION_FAILED, "Invalid event payload: " + e.getMessage());
        }
    }

    @PutMapping("/{eventId}")
    public ApiResponse<EventDtos.EventResponse> update(@PathVariable UUID eventId,
                                                       @Valid @RequestBody EventDtos.AdminEventRequest request) {
        GameEvent event = eventRepository.findById(eventId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Event not found"));
        event.setName(request.name());
        if (request.description() != null) event.setDescription(request.description());
        event.setStartTime(request.startTime());
        event.setEndTime(request.endTime());
        eventRepository.save(event);
        return ApiResponse.ok(new EventDtos.EventResponse(event.getId(), event.getName(), event.getDescription(),
                event.getStartTime(), event.getEndTime(), event.isActive(), List.of(), null));
    }

    /** Soft delete — deactivates so historical progress and claims stay intact. */
    @DeleteMapping("/{eventId}")
    public ApiResponse<Void> deactivate(@PathVariable UUID eventId) {
        GameEvent event = eventRepository.findById(eventId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Event not found"));
        event.setActive(false);
        eventRepository.save(event);
        return ApiResponse.ok(null);
    }
}
