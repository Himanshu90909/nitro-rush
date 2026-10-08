using NUnit.Framework;
using NitroRush.Racing;

namespace NitroRush.Tests
{
    [TestFixture]
    public class RaceStateMachineTests
    {
        [Test]
        public void TestValidTransitions()
        {
            Assert.IsTrue(RaceStateMachine.IsValidTransition(RaceStatePhase.Waiting, RaceStatePhase.Countdown));
            Assert.IsTrue(RaceStateMachine.IsValidTransition(RaceStatePhase.Countdown, RaceStatePhase.Racing));
            Assert.IsTrue(RaceStateMachine.IsValidTransition(RaceStatePhase.Racing, RaceStatePhase.Finished));
            Assert.IsTrue(RaceStateMachine.IsValidTransition(RaceStatePhase.Finished, RaceStatePhase.Waiting));
        }

        [Test]
        public void TestInvalidTransitionsRejected()
        {
            // Cannot jump directly from Waiting to Racing without Countdown sequence
            Assert.IsFalse(RaceStateMachine.IsValidTransition(RaceStatePhase.Waiting, RaceStatePhase.Racing));

            // Cannot jump from Waiting to Finished
            Assert.IsFalse(RaceStateMachine.IsValidTransition(RaceStatePhase.Waiting, RaceStatePhase.Finished));

            // Cannot jump from Finished directly to Racing
            Assert.IsFalse(RaceStateMachine.IsValidTransition(RaceStatePhase.Finished, RaceStatePhase.Racing));
        }
    }
}
