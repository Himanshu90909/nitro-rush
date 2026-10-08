package com.nitrorush.config;

import io.micrometer.core.instrument.Counter;
import io.micrometer.core.instrument.Gauge;
import io.micrometer.core.instrument.MeterRegistry;
import io.micrometer.core.instrument.Timer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.event.EventListener;
import org.springframework.core.env.Environment;

import java.util.Arrays;

@Configuration
public class MetricsConfig {

    private static final Logger log = LoggerFactory.getLogger(MetricsConfig.class);

    private final MeterRegistry registry;
    private final Environment environment;

    public MetricsConfig(MeterRegistry registry, Environment environment, RaceWebSocketHandler raceWebSocketHandler) {
        this.registry = registry;
        this.environment = environment;

        Gauge.builder("websocket_active_connections", raceWebSocketHandler.getActiveConnections(), java.util.concurrent.atomic.AtomicInteger::get)
                .description("Number of active websocket connection sessions")
                .register(registry);
    }

    @Bean
    public Counter racesStartedCounter() {
        return Counter.builder("races_started")
                .description("Total number of races started")
                .register(registry);
    }

    @Bean
    public Counter leaderboardSubmissionsCounter() {
        return Counter.builder("leaderboard_submissions")
                .description("Total number of leaderboard score submissions")
                .register(registry);
    }

    @Bean
    public Timer apiRequestsTimer() {
        return Timer.builder("api_requests")
                .description("Timer for API requests processing")
                .register(registry);
    }

    @EventListener(ApplicationReadyEvent.class)
    public void onStartup() {
        String activeProfiles = Arrays.toString(environment.getActiveProfiles());
        log.info("=================================================");
        log.info(" NITRO RUSH BACKEND PART 2 STARTED SUCCESSFULLY   ");
        log.info(" Active Profiles: {}                             ", activeProfiles);
        log.info("=================================================");
    }
}
