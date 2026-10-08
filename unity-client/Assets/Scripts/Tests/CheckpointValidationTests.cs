using NUnit.Framework;
using NitroRush.Racing;

namespace NitroRush.Tests
{
    [TestFixture]
    public class CheckpointValidationTests
    {
        [Test]
        public void TestSequentialCheckpointValid()
        {
            int totalCheckpoints = 5;

            Assert.IsTrue(RaceValidation.IsCheckpointOrderValid(0, 1, totalCheckpoints));
            Assert.IsTrue(RaceValidation.IsCheckpointOrderValid(1, 2, totalCheckpoints));
            Assert.IsTrue(RaceValidation.IsCheckpointOrderValid(2, 3, totalCheckpoints));
            Assert.IsTrue(RaceValidation.IsCheckpointOrderValid(3, 4, totalCheckpoints));
            Assert.IsTrue(RaceValidation.IsCheckpointOrderValid(4, 0, totalCheckpoints)); // Wrap around lap finish
        }

        [Test]
        public void TestOutOfOrderCheckpointRejected()
        {
            int totalCheckpoints = 5;

            // Skipping checkpoint 1 to jump to 2 is an anti-cheat violation
            Assert.IsFalse(RaceValidation.IsCheckpointOrderValid(0, 2, totalCheckpoints));

            // Backwards pass
            Assert.IsFalse(RaceValidation.IsCheckpointOrderValid(3, 1, totalCheckpoints));

            // Out-of-bounds checkpoint index
            Assert.IsFalse(RaceValidation.IsCheckpointOrderValid(0, 99, totalCheckpoints));
            Assert.IsFalse(RaceValidation.IsCheckpointOrderValid(0, -1, totalCheckpoints));
        }

        [Test]
        public void TestPlausibleSpeedValidation()
        {
            float maxSpeed = 80f; // 80 m/s (~288 km/h)

            Assert.IsTrue(RaceValidation.IsPlausibleSpeed(0f, maxSpeed));
            Assert.IsTrue(RaceValidation.IsPlausibleSpeed(75f, maxSpeed));
            Assert.IsTrue(RaceValidation.IsPlausibleSpeed(95f, maxSpeed, 1.25f)); // Within 25% tolerance

            // Teleportation / speed hack violation
            Assert.IsFalse(RaceValidation.IsPlausibleSpeed(300f, maxSpeed, 1.25f));
            Assert.IsFalse(RaceValidation.IsPlausibleSpeed(-10f, maxSpeed));
        }

        [Test]
        public void TestPlausibleSegmentTimeValidation()
        {
            float segmentDistance = 100f; // 100 meters
            float maxSpeed = 50f;         // 50 m/s

            // Min theoretical time = 100 / (50 * 1.3) = 1.538s
            Assert.IsTrue(RaceValidation.IsPlausibleSegmentTime(segmentDistance, 3.0f, maxSpeed));
            Assert.IsTrue(RaceValidation.IsPlausibleSegmentTime(segmentDistance, 1.6f, maxSpeed));

            // Instant teleport pass (0.01s for 100m) rejected
            Assert.IsFalse(RaceValidation.IsPlausibleSegmentTime(segmentDistance, 0.01f, maxSpeed));
        }
    }
}
