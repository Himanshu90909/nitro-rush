using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using NitroRush.Core;

namespace NitroRush.Tests
{
    [TestFixture]
    public class PerformanceBenchmarkRunner
    {
        private class BenchmarkSample
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void BenchmarkPooledCyclePerformance()
        {
            const int iterations = 10000;
            var pool = new Pool<BenchmarkSample>(prewarmCount: 500);

            // Record baseline memory before benchmark execution
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            long initialMemory = System.GC.GetTotalMemory(true);

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                BenchmarkSample item = pool.Get();
                item.Id = i;
                item.Name = "RacerSample";
                pool.Release(item);
            }

            sw.Stop();

            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryDelta = finalMemory - initialMemory;

            UnityEngine.Debug.Log($"=== POOL BENCHMARK BENCHMARK REPORT ===");
            UnityEngine.Debug.Log($"Iterations: {iterations}");
            UnityEngine.Debug.Log($"Execution Time: {sw.ElapsedMilliseconds} ms ({sw.Elapsed.TotalMicroseconds / iterations:F2} us/op)");
            UnityEngine.Debug.Log($"Hit Count: {pool.HitCount} | Instantiations: {pool.InstantiationCount}");
            UnityEngine.Debug.Log($"Hit Rate: {pool.PoolHitRate * 100f:F1}%");
            UnityEngine.Debug.Log($"Memory Delta: {memoryDelta / 1024f:F2} KB");

            Assert.Greater(pool.HitCount, 0);
            Assert.Less(sw.ElapsedMilliseconds, 500, "10k pool operations should complete under 500ms");
        }
    }
}
