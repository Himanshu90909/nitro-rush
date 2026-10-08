package com.nitrorush.common;

import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;
import java.time.Duration;

@Component
public class RateLimitFilter extends OncePerRequestFilter {

    private static final Logger log = LoggerFactory.getLogger(RateLimitFilter.class);

    private final StringRedisTemplate redisTemplate;
    private final ObjectMapper objectMapper;
    private final InMemoryRateLimiter inMemoryRateLimiter = new InMemoryRateLimiter();

    @Autowired
    public RateLimitFilter(@Autowired(required = false) StringRedisTemplate redisTemplate, ObjectMapper objectMapper) {
        this.redisTemplate = redisTemplate;
        this.objectMapper = objectMapper;
    }

    @Override
    protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain filterChain)
            throws ServletException, IOException {

        String path = request.getRequestURI();
        String clientIp = getClientIp(request);

        boolean isAuth = path.startsWith("/api/v1/auth");
        int maxRequests = isAuth ? 10 : 100;
        long windowSeconds = isAuth ? 60 : 300;
        String bucketType = isAuth ? "auth" : "general";

        boolean allowed = true;

        if (redisTemplate != null) {
            try {
                long currentWindow = System.currentTimeMillis() / (windowSeconds * 1000);
                String redisKey = "ratelimit:" + bucketType + ":" + clientIp + ":" + currentWindow;
                Long count = redisTemplate.opsForValue().increment(redisKey);
                if (count != null && count == 1) {
                    redisTemplate.expire(redisKey, Duration.ofSeconds(windowSeconds));
                }
                if (count != null && count > maxRequests) {
                    allowed = false;
                }
            } catch (Exception ex) {
                log.warn("Redis rate limiter failed, falling back to in-memory: {}", ex.getMessage());
                allowed = inMemoryRateLimiter.allowRequest(bucketType + ":" + clientIp, maxRequests, windowSeconds, System.currentTimeMillis());
            }
        } else {
            allowed = inMemoryRateLimiter.allowRequest(bucketType + ":" + clientIp, maxRequests, windowSeconds, System.currentTimeMillis());
        }

        if (!allowed) {
            response.setStatus(HttpStatus.TOO_MANY_REQUESTS.value());
            response.setContentType(MediaType.APPLICATION_JSON_VALUE);
            response.setHeader("Retry-After", String.valueOf(windowSeconds));
            ApiResponse<Void> err = ApiResponse.error(ErrorCode.RATE_LIMITED, "Rate limit exceeded. Try again later.");
            response.getWriter().write(objectMapper.writeValueAsString(err));
            return;
        }

        filterChain.doFilter(request, response);
    }

    private String getClientIp(HttpServletRequest request) {
        String xForwardedFor = request.getHeader("X-Forwarded-For");
        if (xForwardedFor != null && !xForwardedFor.isBlank()) {
            return xForwardedFor.split(",")[0].trim();
        }
        return request.getRemoteAddr();
    }
}
