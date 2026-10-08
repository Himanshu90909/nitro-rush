package com.nitrorush.common;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;

public class InMemoryRateLimiter {

    public static class WindowBucket {
        private final long windowStartTimeMs;
        private final AtomicInteger count;

        public WindowBucket(long windowStartTimeMs) {
            this.windowStartTimeMs = windowStartTimeMs;
            this.count = new AtomicInteger(0);
        }

        public long getWindowStartTimeMs() {
            return windowStartTimeMs;
        }

        public AtomicInteger getCount() {
            return count;
        }
    }

    private final Map<String, WindowBucket> buckets = new ConcurrentHashMap<>();

    public boolean allowRequest(String key, int maxRequests, long windowSeconds, long nowMs) {
        long windowSizeMs = windowSeconds * 1000;
        buckets.compute(key, (k, bucket) -> {
            if (bucket == null || (nowMs - bucket.getWindowStartTimeMs()) >= windowSizeMs) {
                WindowBucket newBucket = new WindowBucket(nowMs);
                newBucket.getCount().incrementAndGet();
                return newBucket;
            } else {
                bucket.getCount().incrementAndGet();
                return bucket;
            }
        });
        WindowBucket currentBucket = buckets.get(key);
        return currentBucket != null && currentBucket.getCount().get() <= maxRequests;
    }

    public void clear() {
        buckets.clear();
    }
}
