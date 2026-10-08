using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.Multiplayer
{
    public struct TransformSnapshot
    {
        public float timestamp;
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary>
    /// Smooths remote opponent movement using snapshot interpolation buffer.
    /// Interpolation Math:
    /// - renderTime = currentTime - interpolationBackTime
    /// - ratio t = (renderTime - snap1.timestamp) / (snap2.timestamp - snap1.timestamp)
    /// - targetPosition = Vector3.Lerp(snap1.position, snap2.position, Mathf.Clamp01(t))
    /// - targetRotation = Quaternion.Slerp(snap1.rotation, snap2.rotation, Mathf.Clamp01(t))
    /// </summary>
    public class RemoteCarController : MonoBehaviour
    {
        [SerializeField] private float interpolationBackTime = 0.1f; // 100ms buffer
        private readonly List<TransformSnapshot> _snapshots = new List<TransformSnapshot>();

        public void AddSnapshot(float timestamp, Vector3 position, Quaternion rotation)
        {
            _snapshots.Add(new TransformSnapshot { timestamp = timestamp, position = position, rotation = rotation });
            if (_snapshots.Count > 30)
            {
                _snapshots.RemoveAt(0);
            }
        }

        private void Update()
        {
            if (_snapshots.Count < 2) return;

            float renderTime = Time.time - interpolationBackTime;

            for (int i = 0; i < _snapshots.Count - 1; i++)
            {
                TransformSnapshot s0 = _snapshots[i];
                TransformSnapshot s1 = _snapshots[i + 1];

                if (renderTime >= s0.timestamp && renderTime <= s1.timestamp)
                {
                    float duration = s1.timestamp - s0.timestamp;
                    float t = duration > 0f ? Mathf.Clamp01((renderTime - s0.timestamp) / duration) : 1f;

                    transform.position = Vector3.Lerp(s0.position, s1.position, t);
                    transform.rotation = Quaternion.Slerp(s0.rotation, s1.rotation, t);
                    return;
                }
            }

            // Fallback to latest snapshot if renderTime exceeds buffer
            transform.position = _snapshots[_snapshots.Count - 1].position;
            transform.rotation = _snapshots[_snapshots.Count - 1].rotation;
        }
    }
}
