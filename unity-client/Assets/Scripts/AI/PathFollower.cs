using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.AI
{
    /// <summary>
    /// Follows waypoints and computes steering and throttle reductions based on track curvature.
    /// Curvature Math:
    /// - Vector dir1 = (Node B - Node A).normalized
    /// - Vector dir2 = (Node C - Node B).normalized
    /// - curvatureAngle = Vector3.Angle(dir1, dir2)
    /// - Speed reduction factor = 1.0 - Clamp01(curvatureAngle / 180.0) * curvatureFactor
    /// </summary>
    public class PathFollower
    {
        private List<WaypointNode> _currentPath = new List<WaypointNode>();
        private int _currentNodeIndex = 0;
        private float _waypointRadius = 4f;
        private float _steerSensitivity = 0.05f;

        public bool HasPath => _currentPath != null && _currentNodeIndex < _currentPath.Count;

        public void SetPath(List<WaypointNode> path)
        {
            _currentPath = path ?? new List<WaypointNode>();
            _currentNodeIndex = 0;
        }

        public void UpdateSteering(Transform transform, out float steer, out float throttle)
        {
            steer = 0f;
            throttle = 1.0f;

            if (!HasPath) return;

            WaypointNode targetNode = _currentPath[_currentNodeIndex];
            Vector3 toTarget = targetNode.Position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude < _waypointRadius)
            {
                _currentNodeIndex++;
                if (!HasPath) return;
                targetNode = _currentPath[_currentNodeIndex];
                toTarget = targetNode.Position - transform.position;
                toTarget.y = 0f;
            }

            float angle = Vector3.SignedAngle(transform.forward, toTarget.normalized, Vector3.up);
            steer = Mathf.Clamp(angle * _steerSensitivity, -1f, 1f);

            // Compute track curvature reduction
            float curvatureFactor = CalculateCurvature();
            throttle = Mathf.Clamp01(1.0f - (curvatureFactor / 90.0f));
        }

        private float CalculateCurvature()
        {
            if (_currentPath == null || _currentNodeIndex >= _currentPath.Count - 1)
                return 0f;

            Vector3 seg1 = (_currentPath[_currentNodeIndex].Position - _currentPath[Mathf.Max(0, _currentNodeIndex - 1)].Position).normalized;
            Vector3 seg2 = (_currentPath[_currentNodeIndex + 1].Position - _currentPath[_currentNodeIndex].Position).normalized;

            return Vector3.Angle(seg1, seg2);
        }
    }
}
