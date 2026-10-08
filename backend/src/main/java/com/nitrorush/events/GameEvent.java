package com.nitrorush.events;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

/**
 * Live event configured entirely on the backend. The Unity client fetches
 * /api/v1/events and renders whatever it finds — nothing is hard-coded client-side.
 */
@Entity
@Table(name = "events")
public class GameEvent {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(nullable = false, length = 64)
    private String name;

    @Column(length = 500)
    private String description;

    @Column(nullable = false)
    private Instant startTime;

    @Column(nullable = false)
    private Instant endTime;

    /**
     * JSON configuration:
     * {"objectives":[{"type":"WIN_RACES","count":5}],
     *  "rewards":{"credits":5000,"xp":1000,"tokens":10}}
     */
    @Column(nullable = false, columnDefinition = "text")
    private String configuration;

    @Column(nullable = false)
    private boolean active = true;

    public GameEvent() {
    }

    public GameEvent(String name, String description, Instant startTime, Instant endTime, String configuration) {
        this.name = name;
        this.description = description;
        this.startTime = startTime;
        this.endTime = endTime;
        this.configuration = configuration;
        this.active = true;
    }

    public UUID getId() { return id; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
    public Instant getStartTime() { return startTime; }
    public void setStartTime(Instant startTime) { this.startTime = startTime; }
    public Instant getEndTime() { return endTime; }
    public void setEndTime(Instant endTime) { this.endTime = endTime; }
    public String getConfiguration() { return configuration; }
    public void setConfiguration(String configuration) { this.configuration = configuration; }
    public boolean isActive() { return active; }
    public void setActive(boolean active) { this.active = active; }
}
