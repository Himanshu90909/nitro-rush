package com.nitrorush.config;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;

@Component
public class RaceWebSocketHandler extends TextWebSocketHandler {

    private static final Logger log = LoggerFactory.getLogger(RaceWebSocketHandler.class);

    private final ObjectMapper objectMapper;
    private final TokenValidator tokenValidator;

    private final Map<String, WebSocketSession> sessions = new ConcurrentHashMap<>();
    private final Map<String, PlayerState> playerStates = new ConcurrentHashMap<>();
    private final Map<String, String> sessionRaceMap = new ConcurrentHashMap<>();
    private final Map<String, UUID> sessionPlayerMap = new ConcurrentHashMap<>();

    private final AtomicInteger activeConnections = new AtomicInteger(0);

    public RaceWebSocketHandler(ObjectMapper objectMapper, TokenValidator tokenValidator) {
        this.objectMapper = objectMapper;
        this.tokenValidator = tokenValidator;
    }

    public AtomicInteger getActiveConnections() {
        return activeConnections;
    }

    @Override
    public void afterConnectionEstablished(WebSocketSession session) {
        activeConnections.incrementAndGet();
        sessions.put(session.getId(), session);

        String query = session.getUri() != null ? session.getUri().getQuery() : null;
        if (query != null && query.contains("token=")) {
            String token = extractQueryParam(query, "token");
            if (token != null) {
                UUID playerId = tokenValidator.validate(token);
                if (playerId != null) {
                    sessionPlayerMap.put(session.getId(), playerId);
                    session.getAttributes().put("userId", playerId);
                }
            }
        }
    }

    @Override
    protected void handleTextMessage(WebSocketSession session, TextMessage message) throws Exception {
        String payload = message.getPayload();
        Map<String, Object> msg = objectMapper.readValue(payload, Map.class);
        String type = (String) msg.get("type");

        if ("join".equalsIgnoreCase(type)) {
            String raceId = (String) msg.get("raceId");
            if (raceId != null) {
                sessionRaceMap.put(session.getId(), raceId);
            }
            UUID playerId = sessionPlayerMap.get(session.getId());
            if (playerId == null && msg.get("playerId") != null) {
                try {
                    playerId = UUID.fromString(msg.get("playerId").toString());
                    sessionPlayerMap.put(session.getId(), playerId);
                } catch (Exception ignored) {}
            }
            if (playerId != null) {
                PlayerState ps = playerStates.computeIfAbsent(playerId.toString(), id -> new PlayerState());
                ps.playerId = playerId.toString();
            }
            sendJson(session, Map.of("type", "joined", "raceId", raceId != null ? raceId : "", "status", "OK"));
        } else if ("input".equalsIgnoreCase(type)) {
            UUID playerId = sessionPlayerMap.get(session.getId());
            if (playerId == null && msg.get("playerId") != null) {
                try {
                    playerId = UUID.fromString(msg.get("playerId").toString());
                } catch (Exception ignored) {}
            }
            if (playerId != null) {
                PlayerState ps = playerStates.computeIfAbsent(playerId.toString(), id -> new PlayerState());
                ps.playerId = playerId.toString();

                double throttle = msg.get("throttle") instanceof Number n ? n.doubleValue() : 0.0;
                double steer = msg.get("steer") instanceof Number n ? n.doubleValue() : 0.0;
                boolean nitro = Boolean.TRUE.equals(msg.get("nitro"));

                double maxCap = nitro ? 280.0 : 220.0;
                ps.speed = Math.min(maxCap, Math.max(0.0, ps.speed + (throttle * 10.0) - 2.0));
                ps.x += ps.speed * 0.01 * Math.sin(ps.ry);
                ps.z += ps.speed * 0.01 * Math.cos(ps.ry);
                ps.ry += steer * 0.05;
            }
        }
    }

    @Scheduled(fixedRate = 100)
    public void broadcastRaceState() {
        if (sessions.isEmpty()) {
            return;
        }

        long now = System.currentTimeMillis();
        List<Map<String, Object>> posList = new ArrayList<>();
        for (PlayerState ps : playerStates.values()) {
            posList.add(Map.of(
                "playerId", ps.playerId,
                "x", ps.x,
                "y", ps.y,
                "z", ps.z,
                "ry", ps.ry,
                "speed", ps.speed,
                "lap", ps.lap,
                "checkpoint", ps.checkpoint
            ));
        }

        Map<String, Object> stateMsg = Map.of(
            "type", "state",
            "positions", posList,
            "raceState", "IN_PROGRESS",
            "serverTime", now
        );

        try {
            String json = objectMapper.writeValueAsString(stateMsg);
            TextMessage textMessage = new TextMessage(json);
            for (WebSocketSession session : sessions.values()) {
                if (session.isOpen()) {
                    session.sendMessage(textMessage);
                }
            }
        } catch (Exception e) {
            log.warn("Failed to broadcast race state: {}", e.getMessage());
        }
    }

    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus status) {
        activeConnections.decrementAndGet();
        sessions.remove(session.getId());
        sessionRaceMap.remove(session.getId());
        UUID playerId = sessionPlayerMap.remove(session.getId());
        if (playerId != null) {
            playerStates.remove(playerId.toString());
        }
    }

    private void sendJson(WebSocketSession session, Object obj) {
        try {
            if (session.isOpen()) {
                session.sendMessage(new TextMessage(objectMapper.writeValueAsString(obj)));
            }
        } catch (IOException e) {
            log.warn("Error sending message: {}", e.getMessage());
        }
    }

    private String extractQueryParam(String query, String paramName) {
        for (String pair : query.split("&")) {
            String[] kv = pair.split("=");
            if (kv.length == 2 && paramName.equalsIgnoreCase(kv[0])) {
                return kv[1];
            }
        }
        return null;
    }

    public static class PlayerState {
        public String playerId = "";
        public double x = 0.0;
        public double y = 0.0;
        public double z = 0.0;
        public double ry = 0.0;
        public double speed = 0.0;
        public int lap = 1;
        public int checkpoint = 0;
    }
}
