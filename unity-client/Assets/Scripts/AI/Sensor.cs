using UnityEngine;

namespace NitroRush.AI
{
    public struct SensorData
    {
        public bool carAhead;
        public float distance;
        public float leftClearance;
        public float rightClearance;
        public bool obstacleLeft;
        public bool obstacleRight;
    }

    /// <summary>
    /// Low-allocation obstacle detection component utilizing RaycastNonAlloc.
    /// </summary>
    public class Sensor : MonoBehaviour
    {
        private readonly RaycastHit[] _forwardHits = new RaycastHit[4];
        private readonly RaycastHit[] _leftHits = new RaycastHit[4];
        private readonly RaycastHit[] _rightHits = new RaycastHit[4];

        public SensorData DetectObstacles(Transform carTransform, float rayDistance, LayerMask obstacleMask)
        {
            SensorData data = new SensorData
            {
                carAhead = false,
                distance = rayDistance,
                leftClearance = rayDistance,
                rightClearance = rayDistance,
                obstacleLeft = false,
                obstacleRight = false
            };

            Vector3 origin = carTransform.position + Vector3.up * 0.5f;
            Vector3 forward = carTransform.forward;
            Vector3 right = carTransform.right;
            Vector3 left = -carTransform.right;

            // Forward Raycast
            int hitCount = Physics.RaycastNonAlloc(origin, forward, _forwardHits, rayDistance, obstacleMask);
            if (hitCount > 0)
            {
                float closest = rayDistance;
                for (int i = 0; i < hitCount; i++)
                {
                    if (_forwardHits[i].distance < closest && _forwardHits[i].collider.gameObject != carTransform.gameObject)
                    {
                        closest = _forwardHits[i].distance;
                    }
                }
                if (closest < rayDistance)
                {
                    data.carAhead = true;
                    data.distance = closest;
                }
            }

            // Left Diagonal Raycast
            Vector3 leftDiag = (forward + left * 0.5f).normalized;
            hitCount = Physics.RaycastNonAlloc(origin, leftDiag, _leftHits, rayDistance, obstacleMask);
            if (hitCount > 0)
            {
                float closest = rayDistance;
                for (int i = 0; i < hitCount; i++)
                {
                    if (_leftHits[i].distance < closest && _leftHits[i].collider.gameObject != carTransform.gameObject)
                    {
                        closest = _leftHits[i].distance;
                    }
                }
                data.leftClearance = closest;
                data.obstacleLeft = closest < rayDistance * 0.5f;
            }

            // Right Diagonal Raycast
            Vector3 rightDiag = (forward + right * 0.5f).normalized;
            hitCount = Physics.RaycastNonAlloc(origin, rightDiag, _rightHits, rayDistance, obstacleMask);
            if (hitCount > 0)
            {
                float closest = rayDistance;
                for (int i = 0; i < hitCount; i++)
                {
                    if (_rightHits[i].distance < closest && _rightHits[i].collider.gameObject != carTransform.gameObject)
                    {
                        closest = _rightHits[i].distance;
                    }
                }
                data.rightClearance = closest;
                data.obstacleRight = closest < rayDistance * 0.5f;
            }

            return data;
        }
    }
}
