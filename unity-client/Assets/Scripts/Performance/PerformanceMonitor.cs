using System;
using System.Text;
using UnityEngine;
using NitroRush.Core;

namespace NitroRush.Performance
{
    /// <summary>
    /// Singleton Performance Monitor tracking rolling average FPS over 60 frames,
    /// frame time in milliseconds, per-frame GC memory allocation delta, and total system memory.
    /// Periodically logs summary metrics to Unity Console every 10 seconds.
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        public static PerformanceMonitor Instance { get; private set; }

        [Header("Monitor Configuration")]
        [SerializeField] private int fpsSampleBufferCount = 60;
        [SerializeField] private float autoLogIntervalSeconds = 10.0f;

        private float[] _frameTimeBuffer;
        private int _sampleIndex = 0;
        private int _samplesAccumulated = 0;
        private float _logTimer = 0.0f;
        private long _previousTotalMemory = 0;

        public float FPS { get; private set; }
        public float FrameTimeMs { get; private set; }
        public long GCAllocDeltaBytes { get; private set; }
        public long TotalMemoryBytes { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _frameTimeBuffer = new float[fpsSampleBufferCount];
            _previousTotalMemory = System.GC.GetTotalMemory(false);
            
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<PerformanceMonitor>();
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            
            // Record frame delta time in circular rolling buffer
            _frameTimeBuffer[_sampleIndex] = dt;
            _sampleIndex = (_sampleIndex + 1) % fpsSampleBufferCount;
            _samplesAccumulated = Mathf.Min(fpsSampleBufferCount, _samplesAccumulated + 1);

            // Compute rolling average FPS and Frame Time (ms)
            float totalDelta = 0f;
            for (int i = 0; i < _samplesAccumulated; i++)
            {
                totalDelta += _frameTimeBuffer[i];
            }

            float avgDelta = totalDelta / _samplesAccumulated;
            FrameTimeMs = avgDelta * 1000.0f;
            FPS = avgDelta > 0.0001f ? 1.0f / avgDelta : 0f;

            // Track GC memory allocation delta
            long currentMemory = System.GC.GetTotalMemory(false);
            GCAllocDeltaBytes = Math.Max(0, currentMemory - _previousTotalMemory);
            TotalMemoryBytes = currentMemory;
            _previousTotalMemory = currentMemory;

            // Periodic 10-second metric auto-log summary
            _logTimer += dt;
            if (_logTimer >= autoLogIntervalSeconds)
            {
                _logTimer = 0f;
                LogPerformanceSummary();
            }
        }

        /// <summary>
        /// Output performance metric summary log to Unity Console.
        /// </summary>
        public void LogPerformanceSummary()
        {
            float totalMb = TotalMemoryBytes / (1024.0f * 1024.0f);
            float gcKb = GCAllocDeltaBytes / 1024.0f;

            var sb = new StringBuilder();
            sb.AppendLine("=== NITRO RUSH PERFORMANCE SUMMARY ===");
            sb.AppendLine($"FPS: {FPS:F1} | Frame Time: {FrameTimeMs:F2} ms");
            sb.AppendLine($"Total Heap Memory: {totalMb:F2} MB | Frame GC Alloc: {gcKb:F2} KB");

            if (PoolManager.Instance != null)
            {
                var stats = PoolManager.Instance.GetAllStats();
                sb.AppendLine($"Active Pools Count: {stats.Count}");
                foreach (var pool in stats)
                {
                    sb.AppendLine($" -> Pool '{pool.PoolKey}': Active={pool.ActiveCount}, Inactive={pool.InactiveCount}, Hits={pool.HitCount}, HitRate={pool.HitRate * 100f:F1}%");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
