using UnityEngine;
using NitroRush.Core;
using NitroRush.Physics;

namespace NitroRush.Cars
{
    /// <summary>
    /// Collision impulse-based vehicle damage simulation.
    /// Accumulates physical damage from impacts exceeding velocity threshold,
    /// reduces vehicle top speed cap by up to 20%, and processes repair pickups.
    /// </summary>
    [RequireComponent(typeof(CarPhysics))]
    public class CarDamage : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private float minImpactThreshold = 6.0f; // Velocity threshold in m/s before damage registers
        [SerializeField] private float damageMultiplier = 2.0f;
        [SerializeField] private float maxHealth = 100.0f;

        private CarPhysics _carPhysics;
        private string _racerId = "player_local";

        public float CurrentHealth { get; private set; }
        public float HealthPercent => CurrentHealth / maxHealth;
        public float DamagePercent => 1.0f - HealthPercent;

        private void Awake()
        {
            _carPhysics = GetComponent<CarPhysics>();
            CurrentHealth = maxHealth;
        }

        public void Initialize(string racerId)
        {
            _racerId = racerId;
            CurrentHealth = maxHealth;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Relative impact speed magnitude = collision.relativeVelocity.magnitude
            float relativeImpactSpeed = collision.relativeVelocity.magnitude;

            if (relativeImpactSpeed > minImpactThreshold)
            {
                float excessSpeed = relativeImpactSpeed - minImpactThreshold;
                float damageAmount = excessSpeed * damageMultiplier;

                ApplyDamage(damageAmount, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
            }
        }

        public void ApplyDamage(float amount, Vector3 impactPoint)
        {
            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

            // Calculate top speed penalty: max health = 100% top speed; 0% health = 80% top speed (20% reduction cap)
            float topSpeedPenaltyModifier = 1.0f - (DamagePercent * 0.20f);
            _carPhysics.SetTopSpeedModifier(topSpeedPenaltyModifier);

            GameEvents.CarDamaged.Invoke(new CarDamagedEventArgs(_racerId, amount, CurrentHealth, impactPoint));
        }

        public void Repair(float amount)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);

            float topSpeedPenaltyModifier = 1.0f - (DamagePercent * 0.20f);
            _carPhysics.SetTopSpeedModifier(topSpeedPenaltyModifier);
        }
    }
}
