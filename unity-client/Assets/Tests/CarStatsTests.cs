using NUnit.Framework;
using UnityEngine;
using NitroRush.Cars;

namespace NitroRush.Tests
{
    [TestFixture]
    public class CarStatsTests
    {
        private CarData _sampleCarData;

        [SetUp]
        public void SetUp()
        {
            _sampleCarData = ScriptableObject.CreateInstance<CarData>();
            _sampleCarData.carId = "test_car";
            _sampleCarData.displayName = "Test Racer";
            _sampleCarData.topSpeed = 100f;        // 100 m/s
            _sampleCarData.acceleration = 20f;     // 20 m/s^2
            _sampleCarData.handling = 10f;
            _sampleCarData.braking = 30f;
            _sampleCarData.nitroCapacity = 10f;    // 10s
            _sampleCarData.nitroAcceleration = 15f;
            _sampleCarData.driftControl = 1.0f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_sampleCarData != null)
            {
                Object.DestroyImmediate(_sampleCarData);
            }
        }

        [Test]
        public void TestLevel1Upgrades()
        {
            // Level 1 upgrade across all parts should yield base values (0% increase)
            var stats = CarStats.FromData(_sampleCarData, 1, 1, 1, 1, 1);

            Assert.AreEqual(100f, stats.TopSpeed, 0.001f);
            Assert.AreEqual(20f, stats.Acceleration, 0.001f);
            Assert.AreEqual(10f, stats.Handling, 0.001f);
            Assert.AreEqual(30f, stats.Braking, 0.001f);
            Assert.AreEqual(10f, stats.NitroCapacity, 0.001f);
            Assert.AreEqual(15f, stats.NitroAcceleration, 0.001f);
            Assert.AreEqual(1.0f, stats.DriftControl, 0.001f);
        }

        [Test]
        public void TestLevel5Upgrades()
        {
            // Level 5 upgrades:
            // Engine topSpeed: 100 * (1 + 4 * 0.05) = 100 * 1.20 = 120
            // Turbo acceleration: 20 * (1 + 4 * 0.06) = 20 * 1.24 = 24.8
            // Tires handling: 10 * (1 + 4 * 0.04) = 10 * 1.16 = 11.6
            // Brakes braking: 30 * (1 + 4 * 0.05) = 30 * 1.20 = 36
            // Nitro capacity: 10 * (1 + 4 * 0.08) = 10 * 1.32 = 13.2
            // Nitro acceleration: 15 * (1 + 4 * 0.07) = 15 * 1.28 = 19.2
            var stats = CarStats.FromData(_sampleCarData, 5, 5, 5, 5, 5);

            Assert.AreEqual(120f, stats.TopSpeed, 0.001f);
            Assert.AreEqual(24.8f, stats.Acceleration, 0.001f);
            Assert.AreEqual(11.6f, stats.Handling, 0.001f);
            Assert.AreEqual(36f, stats.Braking, 0.001f);
            Assert.AreEqual(13.2f, stats.NitroCapacity, 0.001f);
            Assert.AreEqual(19.2f, stats.NitroAcceleration, 0.001f);
        }

        [Test]
        public void TestClampingUpgradeLevels()
        {
            // Values outside 1-5 must clamp cleanly to range
            var statsLow = CarStats.FromData(_sampleCarData, -2, 0, 0, 0, -5);
            Assert.AreEqual(100f, statsLow.TopSpeed, 0.001f);

            var statsHigh = CarStats.FromData(_sampleCarData, 99, 99, 99, 99, 99);
            Assert.AreEqual(120f, statsHigh.TopSpeed, 0.001f);
        }
    }
}
