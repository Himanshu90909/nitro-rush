using System.Collections.Generic;
using NUnit.Framework;
using NitroRush.Racing;

namespace NitroRush.Tests
{
    [TestFixture]
    public class LapProgressTests
    {
        [Test]
        public void TestLapProgressMath()
        {
            var tracker = new RacePositionTracker();

            // Racer A: Lap 2, checkpoint 1, progress = 11.5
            var racerA = new RacerProgressData
            {
                RacerId = "racer_A",
                CurrentLap = 2,
                LastPassedCheckpointIndex = 1,
                TotalProgressScore = 11.5f,
                TotalRaceTime = 45.0f
            };

            // Racer B: Lap 1, checkpoint 4, progress = 9.2
            var racerB = new RacerProgressData
            {
                RacerId = "racer_B",
                CurrentLap = 1,
                LastPassedCheckpointIndex = 4,
                TotalProgressScore = 9.2f,
                TotalRaceTime = 46.0f
            };

            tracker.UpdateRacerProgress(racerA);
            tracker.UpdateRacerProgress(racerB);

            List<RacerProgressData> standings = tracker.CalculateStandings();

            Assert.AreEqual("racer_A", standings[0].RacerId);
            Assert.AreEqual(1, standings[0].CurrentRank);

            Assert.AreEqual("racer_B", standings[1].RacerId);
            Assert.AreEqual(2, standings[1].CurrentRank);
        }

        [Test]
        public void TestTieBreakerHandling()
        {
            var tracker = new RacePositionTracker();

            // Identical progress score: Racer X finished in 50.0s, Racer Y in 50.5s
            var racerX = new RacerProgressData
            {
                RacerId = "racer_X",
                TotalProgressScore = 15.0000f,
                TotalRaceTime = 50.0f
            };

            var racerY = new RacerProgressData
            {
                RacerId = "racer_Y",
                TotalProgressScore = 15.0000f,
                TotalRaceTime = 50.5f
            };

            tracker.UpdateRacerProgress(racerY);
            tracker.UpdateRacerProgress(racerX);

            List<RacerProgressData> standings = tracker.CalculateStandings();

            // Racer X wins tie due to smaller total race time
            Assert.AreEqual("racer_X", standings[0].RacerId);
            Assert.AreEqual("racer_Y", standings[1].RacerId);
        }
    }
}
