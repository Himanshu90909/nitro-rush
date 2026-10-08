using UnityEngine;
using NitroRush.Core;
using NitroRush.Physics;

namespace NitroRush.Cars
{
    /// <summary>
    /// Manages vehicle nitro boost tank capacity, drain rate during burn, forward thrust impulse force,
    /// passive regeneration while inactive, and event firing.
    /// </summary>
    [RequireComponent(typeof(CarPhysics))]
    public class CarNitro : MonoBehaviour
    {
        [Header("Regeneration Settings")]
        [SerializeField] private float passiveRegenRate = 0.5f; // Seconds restored per second of driving

        private CarPhysics _carPhysics;
        private Rigidbody _rb;
        private ICarInput _input;
        private string _racerId = "player_local";

        public float MaxCapacity { get; private me; set; } = 8.0f;
        public float CurrentNitro { get; private set; }
        public bool IsNitroActive { get; private set; }

        private void Awake()
        {
            _carPhysics = GetComponent<CarPhysics>();
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(ICarInput input, string racerId)
        {
            _input = input;
            _racerId = racerId;
            MaxCapacity = _carPhysics.Stats.NitroCapacity;
            CurrentNitro = MaxCapacity;
        }

        private void FixedUpdate()
        {
            if (_input == null) return;

            bool nitroRequested = _input.NitroHeld && CurrentNitro > 0.05f;

            if (nitroRequested)
            {
                if (!IsNitroActive)
                {
                    StartNitro();
                }

                // Drain nitro tank over time
                CurrentNitro = Mathf.Max(0f, CurrentNitro - Time.fixedDeltaTime);

                // Apply forward nitro boost acceleration force
                // Boost force = transform.forward * (nitroAcceleration * mass)
                float nitroAccel = _carPhysics.Stats.NitroAcceleration;
                Vector3 boostForce = transform.forward * (nitroAccel * _rb.mass);
                _rb.AddForce(boostForce, ForceMode.Force);

                GameEvents.NitroStateChanged.Invoke(new NitroStateChangedEventArgs(_racerId, true, CurrentNitro));

                if (CurrentNitro <= 0f)
                {
                    StopNitro();
                }
            }
            else
            {
                if (IsNitroActive)
                {
                    StopNitro();
                }

                // Passive nitro tank regeneration
                if (CurrentNitro < MaxCapacity)
                {
                    CurrentNitro = Mathf.Min(MaxCapacity, CurrentNitro + (passiveRegenRate * Time.fixedDeltaTime));
                }
            }
        }

        private void StartNitro()
        {
            IsNitroActive = true;
            GameEvents.NitroStateChanged.Invoke(new NitroStateChangedEventArgs(_racerId, true, CurrentNitro));
        }

        private void StopNitro()
        {
            IsNitroActive = false;
            GameEvents.NitroStateChanged.Invoke(new NitroStateChangedEventArgs(_racerId, false, CurrentNitro));
        }

        public void Refill(float amount)
        {
            CurrentNitro = Mathf.Min(MaxCapacity, CurrentNitro + amount);
        }
    }
}
