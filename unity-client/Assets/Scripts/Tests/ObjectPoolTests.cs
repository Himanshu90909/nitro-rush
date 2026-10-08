using NUnit.Framework;
using UnityEngine;
using NitroRush.Core;

namespace NitroRush.Tests
{
    [TestFixture]
    public class ObjectPoolTests
    {
        private class TestComponent : Component
        {
        }

        private class TestClass
        {
            public int Value { get; set; }
        }

        [Test]
        public void TestPoolClassPrewarm()
        {
            int prewarmCount = 10;
            var pool = new Pool<TestClass>(prewarmCount: prewarmCount);

            Assert.AreEqual(prewarmCount, pool.InactiveCount);
            Assert.AreEqual(prewarmCount, pool.InstantiationCount);
            Assert.AreEqual(0, pool.HitCount);
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void TestPoolClassGetReleaseCycling()
        {
            var pool = new Pool<TestClass>(prewarmCount: 5);

            TestClass obj1 = pool.Get();
            Assert.NotNull(obj1);
            Assert.AreEqual(1, pool.HitCount);
            Assert.AreEqual(1, pool.ActiveCount);

            pool.Release(obj1);
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(5, pool.InactiveCount);
        }

        [Test]
        public void TestHitRateMath()
        {
            var pool = new Pool<TestClass>(prewarmCount: 0);

            // Get on empty pool forces creation
            TestClass obj1 = pool.Get();
            Assert.AreEqual(1, pool.InstantiationCount);
            Assert.AreEqual(0, pool.HitCount);
            // Hit rate = 0 / (0 + 1) = 0.0
            Assert.AreEqual(0f, pool.PoolHitRate, 0.001f);

            pool.Release(obj1);

            // Second get reuses pooled object = hit!
            TestClass obj2 = pool.Get();
            Assert.AreEqual(1, pool.InstantiationCount);
            Assert.AreEqual(1, pool.HitCount);
            // Hit rate = 1 / (1 + 1) = 0.5 (50%)
            Assert.AreEqual(0.5f, pool.PoolHitRate, 0.001f);
        }

        [Test]
        public void TestPoolClassClear()
        {
            var pool = new Pool<TestClass>(prewarmCount: 8);
            Assert.AreEqual(8, pool.InactiveCount);

            pool.Clear();
            Assert.AreEqual(0, pool.InactiveCount);
            Assert.AreEqual(8, pool.DestroyCount);
        }
    }
}
