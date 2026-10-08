package com.nitrorush.matchmaking;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Component;

/**
 * Periodically pulls players out of the queue into lobbies.
 * Disabled in the test profile via nitrorush.matchmaking.enabled=false.
 */
@Component
@ConditionalOnProperty(name = "nitrorush.matchmaking.enabled", havingValue = "true", matchIfMissing = true)
public class MatchmakingRefresher {

    private static final Logger log = LoggerFactory.getLogger(MatchmakingRefresher.class);

    private final MatchmakingService matchmakingService;

    public MatchmakingRefresher(MatchmakingService matchmakingService) {
        this.matchmakingService = matchmakingService;
    }

    @Scheduled(fixedDelay = 3000)
    public void tick() {
        matchmakingService.tryMatch().ifPresent(lobby ->
                log.debug("Lobby {} created by refresher", lobby.lobbyId()));
    }
}
