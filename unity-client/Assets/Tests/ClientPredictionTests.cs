using NUnit.Framework;
using NitroRush.Multiplayer;
using UnityEngine;

namespace NitroRush.Tests
{
    public class ClientPredictionTests
    {
        [Test]
        public void ReplayInputs_MatchesSequentialSimulation()
        {
            var prediction = new ClientPrediction();
            prediction.AddInput(new LocalInputCmd { sequenceNumber = 1, throttle = 1.0f, steer = 0f });
            prediction.AddInput(new LocalInputCmd { sequenceNumber = 2, throttle = 1.0f, steer = 0f });

            var state = new StateSnapshot
            {
                sequenceNumber = 0,
                position = Vector3.zero,
                rotation = Quaternion.identity
            };

            Vector3 finalPos = prediction.ReplayInputs(state, speedPerInput: 10f, steerPerInput: 0f);

            Assert.AreEqual(new Vector3(0, 0, 20f), finalPos);
        }
    }
}
