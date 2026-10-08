package com.nitrorush.events;

import com.nitrorush.common.ApiResponse;
import com.nitrorush.common.SecurityUtils;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/events")
public class EventController {

    private final EventService eventService;

    public EventController(EventService eventService) {
        this.eventService = eventService;
    }

    @GetMapping
    public ApiResponse<List<EventDtos.EventResponse>> activeEvents() {
        return ApiResponse.ok(eventService.activeEvents());
    }

    @GetMapping("/{eventId}")
    public ApiResponse<EventDtos.EventResponse> event(@PathVariable UUID eventId) {
        return ApiResponse.ok(eventService.event(eventId));
    }

    @PostMapping("/{eventId}/join")
    public ApiResponse<EventDtos.EventProgressResponse> join(@PathVariable UUID eventId) {
        return ApiResponse.ok(eventService.join(SecurityUtils.currentUserId(), eventId));
    }

    @GetMapping("/{eventId}/progress")
    public ApiResponse<EventDtos.EventProgressResponse> progress(@PathVariable UUID eventId) {
        return ApiResponse.ok(eventService.progress(SecurityUtils.currentUserId(), eventId));
    }
}
