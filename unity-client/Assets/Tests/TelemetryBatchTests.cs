using NUnit.Framework;
using NitroRush.Analytics;

namespace NitroRush.Tests
{
    public class TelemetryBatchTests
    {
        [Test]
        public void LogEvent_BatchThresholdFlushesAtTwenty()
        {
            var logger = new TelemetryLogger(null);

            for (int i = 0; i < 19; i++)
            {
                logger.LogEvent(TelemetryEventType.race_started);
            }

            Assert.AreEqual(19, logger.QueueCount);

            // 20th event triggers flush attempt
            logger.LogEvent(TelemetryEventType.race_finished);
            Assert.AreEqual(0, logger.QueueCount);
        }
    }
}
