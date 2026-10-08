using UnityEngine;

namespace NitroRush.AI
{
    /// <summary>
    /// Stuck detection and recovery system for AI cars. Reverse + opposite steer, falling back to checkpoint teleport.
    /// </summary>
    public class RecoverySystem
    {
        private float _speedThreshold = 1.0f;
        private float _stuckTimeThreshold = 3.0f;
        private float _teleportThreshold = 6.0f;
        private float _teleportCooldown = 5.0f;

        private float _stuckTimer = 0f;
        private float _cooldownTimer = 0f;
        private Vector3 _lastCheckpointPosition = Vector3.zero;
        private Quaternion _lastCheckpointRotation = Quaternion.identity;

        public void SetLastCheckpoint(Vector3 position, Quaternion rotation)
        {
            _lastCheckpointPosition = position;
            _lastCheckpointRotation = rotation;
        }

        public bool UpdateRecovery(float deltaTime, float currentSpeed, Transform carTransform, out float throttle, out float steer)
        {
            throttle = 0f;
            steer = 0f;

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= deltaTime;
            }

            if (Mathf.Abs(currentSpeed) < _speedThreshold)
            {
                _stuckTimer += deltaTime;
            }
            else
            {
                _stuckTimer = 0f;
                return false;
            }

            if (_stuckTimer >= _teleportThreshold && _cooldownTimer <= 0f)
            {
                // Teleport to last checkpoint fallback
                if (_lastCheckpointPosition != Vector3.zero)
                {
                    carTransform.position = _lastCheckpointPosition + Vector3.up * 0.5f;
                    carTransform.rotation = _lastCheckpointRotation;
                }
                _stuckTimer = 0f;
                _cooldownTimer = _teleportCooldown;
                return false;
            }
            else if (_stuckTimer >= _stuckTimeThreshold)
            {
                // Reverse and steer opposite
                throttle = -1.0f;
                steer = 1.0f;
                return true;
            }

            return false;
        }
    }
}
