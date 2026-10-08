using UnityEngine;
using NitroRush.Core;
using NitroRush.Physics;

namespace NitroRush.Cars
{
    /// <summary>
    /// Detects vehicle drift condition (steering opposite lateral velocity while drift button is held),
    /// weakens tire grip coefficient via CarPhysics to sustain controlled slides, and calculates drift score.
    /// </summary>
    [RequireComponent(typeof(CarPhysics))]
    public class CarDrift : MonoBehaviour
    {
        [Header("Drift Configuration")]
        [SerializeField] private float minSpeedForDrift = 12.0f;
        [SerializeField] private float minLateralVelForDrift = 2.5f;
        [SerializeField] private float driftGripModifier = 0.35f;
        [SerializeField] private float scoreMultiplier = 10.0f;

        private CarPhysics _carPhysics;
        private ICarInput _input;
        private string _racerId = "player_local";

        public bool IsDrifting { get; private set; }
        public float CurrentDriftScore { get; private set; }
        public float CurrentDriftAngle { get; private set; }

        private void Awake()
        {
            _carPhysics = GetComponent<CarPhysics>();
        }

        public void Initialize(ICarInput input, string racerId)
        {
            _input = input;
            _racerId = racerId;
        }

        private void FixedUpdate()
        {
            if (_input == null || !_carPhysics.IsGrounded)
            {
                EndDrift();
                return;
            }

            float currentSpeed = _carPhysics.CurrentSpeed;
            float latVel = Mathf.Abs(_carPhysics.LateralVelocity);
            bool driftButton = _input.DriftHeld;
            float steer = _input.Steer;

            // Drift condition check: speed exceeds threshold, drift button held, and sideways slip present
            bool evaluateDrift = driftButton && currentSpeed >= minSpeedForDrift && (latVel >= minLateralVelForDrift || Mathf.Abs(steer) > 0.3f);

            if (evaluateDrift)
            {
                if (!IsDrifting)
                {
                    StartDrift();
                }

                UpdateDrift(currentSpeed);
            }
            else if (IsDrifting)
            {
                EndDrift();
            }
        }

        private void StartDrift()
        {
            IsDrifting = true;
            _carPhysics.SetGripModifier(driftGripModifier);
            GameEvents.DriftStateChanged.Invoke(new DriftStateChangedEventArgs(_racerId, true, CurrentDriftScore, CurrentDriftAngle));
        }

        private void UpdateDrift(float speed)
        {
            // Calculate drift angle between velocity vector direction and car forward direction
            // Drift angle = Vector3.Angle(Velocity.normalized, transform.forward)
            Vector3 velDir = _carPhysics.Velocity.normalized;
            CurrentDriftAngle = Vector3.Angle(velDir, transform.forward);

            // Accumulate score points based on drift angle, vehicle speed, and frame delta time
            float scoreIncrement = CurrentDriftAngle * speed * scoreMultiplier * Time.fixedDeltaTime;
            CurrentDriftScore += scoreIncrement;

            GameEvents.DriftStateChanged.Invoke(new DriftStateChangedEventArgs(_racerId, true, CurrentDriftScore, CurrentDriftAngle));
        }

        private void EndDrift()
        {
            if (!IsDrifting) return;

            IsDrifting = false;
            _carPhysics.SetGripModifier(1.0f); // Restore normal tire grip
            GameEvents.DriftStateChanged.Invoke(new DriftStateChangedEventArgs(_racerId, false, CurrentDriftScore, CurrentDriftAngle));
        }

        public void ResetScore()
        {
            CurrentDriftScore = 0f;
            CurrentDriftAngle = 0f;
        }
    }
}
