using System;
using UnityEngine;

namespace NitroRush.AI
{
    public enum AIDifficulty { Easy, Medium, Hard }

    /// <summary>
    /// Input structure for AI vehicles, matching contract interface ICarInput.
    /// </summary>
    public class AICarInput : ICarInput
    {
        public float Throttle { get; set; }
        public float Steer { get; set; }
        public bool DriftHeld { get; set; }
        public bool NitroHeld { get; set; }
    }

    /// <summary>
    /// Main controller for AI-driven vehicles. Owns Sensors, DecisionMaker, PathFollower, OvertakeSystem, RecoverySystem.
    /// </summary>
    [RequireComponent(typeof(Sensor))]
    public class AIController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AIDifficulty difficulty = AIDifficulty.Medium;
        [SerializeField] private float rayDistance = 15f;
        [SerializeField] private LayerMask obstacleMask;

        public AICarInput InputState { get; private set; } = new AICarInput();
        public AIDifficulty Difficulty => difficulty;

        // Subsystems
        public Sensor SensorComponent { get; private set; }
        public DecisionMaker DecisionMakerComponent { get; private set; }
        public PathFollower PathFollowerComponent { get; private set; }
        public OvertakeSystem OvertakeSystemComponent { get; private set; }
        public RecoverySystem RecoverySystemComponent { get; private set; }

        private float _reactionDelayTimer = 0f;
        private float _reactionDelayDuration = 0.15f;
        private float _maxSteer = 0.85f;
        private float _nitroAggression = 0.6f;
        private float _mistakeRate = 0.08f;

        private void Awake()
        {
            SensorComponent = GetComponent<Sensor>();
            if (SensorComponent == null) SensorComponent = gameObject.AddComponent<Sensor>();

            DecisionMakerComponent = new DecisionMaker();
            PathFollowerComponent = new PathFollower();
            OvertakeSystemComponent = new OvertakeSystem();
            RecoverySystemComponent = new RecoverySystem();

            ApplyDifficultySettings();
        }

        public void SetDifficulty(AIDifficulty newDifficulty)
        {
            difficulty = newDifficulty;
            ApplyDifficultySettings();
        }

        private void ApplyDifficultySettings()
        {
            switch (difficulty)
            {
                case AIDifficulty.Easy:
                    _reactionDelayDuration = 0.30f;
                    _maxSteer = 0.70f;
                    _nitroAggression = 0.30f;
                    _mistakeRate = 0.20f;
                    break;
                case AIDifficulty.Medium:
                    _reactionDelayDuration = 0.15f;
                    _maxSteer = 0.85f;
                    _nitroAggression = 0.60f;
                    _mistakeRate = 0.08f;
                    break;
                case AIDifficulty.Hard:
                    _reactionDelayDuration = 0.05f;
                    _maxSteer = 1.00f;
                    _nitroAggression = 0.90f;
                    _mistakeRate = 0.00f;
                    break;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _reactionDelayTimer += dt;

            // Sensor tick
            SensorData sensorData = SensorComponent.DetectObstacles(transform, rayDistance, obstacleMask);

            // Decision maker tick accumulator
            AIIntent intent = DecisionMakerComponent.Tick(dt, sensorData, transform.position);

            if (_reactionDelayTimer >= _reactionDelayDuration)
            {
                _reactionDelayTimer = 0f;

                // Process Recovery System
                float currentSpeed = Vector3.Dot(transform.forward, GetComponent<Rigidbody>() != null ? GetComponent<Rigidbody>().velocity : Vector3.zero);
                if (RecoverySystemComponent.UpdateRecovery(dt, currentSpeed, transform, out float recoveryThrottle, out float recoverySteer))
                {
                    InputState.Throttle = recoveryThrottle;
                    InputState.Steer = recoverySteer;
                    InputState.NitroHeld = false;
                    InputState.DriftHeld = false;
                    return;
                }

                // Process Intent
                switch (intent)
                {
                    case AIIntent.Overtake:
                        float overtakeSteer = OvertakeSystemComponent.CalculateOvertakeSteer(sensorData);
                        InputState.Steer = Mathf.Clamp(overtakeSteer, -_maxSteer, _maxSteer);
                        InputState.Throttle = 1.0f;
                        break;

                    case AIIntent.Avoid:
                        float avoidSteer = sensorData.leftClearance > sensorData.rightClearance ? -1f : 1f;
                        InputState.Steer = Mathf.Clamp(avoidSteer, -_maxSteer, _maxSteer);
                        InputState.Throttle = 0.5f;
                        break;

                    case AIIntent.UseNitro:
                        InputState.NitroHeld = UnityEngine.Random.value < _nitroAggression;
                        InputState.Throttle = 1.0f;
                        break;

                    case AIIntent.FollowLine:
                    default:
                        if (PathFollowerComponent.HasPath)
                        {
                            PathFollowerComponent.UpdateSteering(transform, out float targetSteer, out float targetThrottle);
                            // Apply mistake factor
                            if (_mistakeRate > 0f && UnityEngine.Random.value < _mistakeRate * dt)
                            {
                                targetSteer += UnityEngine.Random.Range(-0.3f, 0.3f);
                            }
                            InputState.Steer = Mathf.Clamp(targetSteer, -_maxSteer, _maxSteer);
                            InputState.Throttle = targetThrottle;
                        }
                        else
                        {
                            InputState.Throttle = 1.0f;
                            InputState.Steer = 0f;
                        }
                        break;
                }
            }
        }
    }
}
