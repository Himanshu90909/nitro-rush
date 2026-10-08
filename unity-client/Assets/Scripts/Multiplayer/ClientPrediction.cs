using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.Multiplayer
{
    [Serializable]
    public struct LocalInputCmd
    {
        public uint sequenceNumber;
        public float throttle;
        public float steer;
        public bool nitro;
        public bool drift;
    }

    public struct StateSnapshot
    {
        public uint sequenceNumber;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
    }

    /// <summary>
    /// Client-side prediction and reconciliation buffer for local player vehicle.
    /// Re-applies unacknowledged inputs upon server state corrections.
    /// </summary>
    public class ClientPrediction
    {
        private readonly List<LocalInputCmd> _pendingInputs = new List<LocalInputCmd>();

        public IReadOnlyList<LocalInputCmd> PendingInputs => _pendingInputs;

        public void AddInput(LocalInputCmd cmd)
        {
            _pendingInputs.Add(cmd);
        }

        public Vector3 ReplayInputs(StateSnapshot serverState, float speedPerInput, float steerPerInput)
        {
            // Remove acknowledged inputs
            _pendingInputs.RemoveAll(cmd => cmd.sequenceNumber <= serverState.sequenceNumber);

            Vector3 simulatedPos = serverState.position;
            Quaternion simulatedRot = serverState.rotation;

            // Replay unacknowledged inputs
            for (int i = 0; i < _pendingInputs.Count; i++)
            {
                var cmd = _pendingInputs[i];
                simulatedRot *= Quaternion.Euler(0, cmd.steer * steerPerInput, 0);
                simulatedPos += simulatedRot * Vector3.forward * (cmd.throttle * speedPerInput);
            }

            return simulatedPos;
        }
    }
}
