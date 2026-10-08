using System;
using UnityEngine;
using NitroRush.Cars;

namespace NitroRush.Physics
{
    /// <summary>
    /// Core Rigidbody arcade vehicle physics simulation engine.
    /// Handles forward/reverse acceleration, speed-sensitive steering torque,
    /// dynamic lateral tire friction (grip) reduction during drift, quadratic aerodynamic downforce,
    /// and non-allocating ground raycast checks.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarPhysics : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private LayerMask groundLayerMask = ~0;
        [SerializeField] private float rayDistance = 1.2f;
        [SerializeField] private float gripCoefficient = 15.0f;
        [SerializeField] private float downforceCoefficient = 2.5f;

        private Rigidbody _rb;
        private CarStats _stats;
        private ICarInput _input;
        private float _gripModifier = 1.0f;
        private float _topSpeedModifier = 1.0f;
        private bool _isGrounded;
        private readonly RaycastHit[] _groundHits = new RaycastHit[1];

        /// <summary>
        /// Current 3D linear velocity vector targeting Unity 6 API with pre-Unity 6 fallback.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
#if UNITY_6000_0_OR_NEWER
                return _rb.linearVelocity;
#else
                return _rb.velocity;
#endif
            }
            set
            {
#if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = value;
#else
                _rb.velocity = value;
#endif
            }
        }

        /// <summary>
        /// Scalar forward velocity = Vector3.Dot(Velocity, transform.forward).
        /// Positive indicates forward motion; negative indicates reverse motion.
        /// </summary>
        public float ForwardVelocity => Vector3.Dot(Velocity, transform.forward);

        /// <summary>
        /// Scalar lateral velocity = Vector3.Dot(Velocity, transform.right).
        /// Measures sideways tire slip velocity along local right direction vector.
        /// </summary>
        public float LateralVelocity => Vector3.Dot(Velocity, transform.right);

        /// <summary>
        /// Total scalar speed in meters per second = Velocity.magnitude.
        /// </summary>
        public float CurrentSpeed => Velocity.magnitude;

        /// <summary>
        /// True if raycast detects ground contact underneath vehicle wheels.
        /// </summary>
        public bool IsGrounded => _isGrounded;

        public CarStats Stats => _stats;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = new Vector3(0f, -0.3f, 0f); // Low center of mass for anti-roll stability
        }

        public void Initialize(CarStats stats, ICarInput input)
        {
            _stats = stats;
            _input = input;
        }

        public void SetGripModifier(float modifier)
        {
            _gripModifier = Mathf.Clamp(modifier, 0.1f, 2.0f);
        }

        public void SetTopSpeedModifier(float modifier)
        {
            _topSpeedModifier = Mathf.Clamp(modifier, 0.5f, 1.0f);
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            if (!_isGrounded) return;

            float throttle = _input?.Throttle ?? 0f;
            float steer = _input?.Steer ?? 0f;

            ApplyEngineForces(throttle);
            ApplySteeringTorque(steer);
            ApplyLateralFriction();
            ApplyDownforce();
        }

        private void CheckGrounded()
        {
            // Raycast down from local origin to check ground contact without GC allocation
            Ray ray = new Ray(transform.position + Vector3.up * 0.2f, -transform.up);
            int hitCount = UnityEngine.Physics.RaycastNonAlloc(ray, _groundHits, rayDistance, groundLayerMask);
            _isGrounded = hitCount > 0;
        }

        private void ApplyEngineForces(float throttle)
        {
            float effectiveMaxSpeed = _stats.TopSpeed * _topSpeedModifier;
            float currentForwardVel = ForwardVelocity;

            if (throttle > 0f)
            {
                // Forward Acceleration:
                // Acceleration force = transform.forward * (throttle * Acceleration * mass)
                // Scaled down as current forward velocity approaches max speed cap
                float speedRatio = Mathf.Clamp01(1.0f - (currentForwardVel / (effectiveMaxSpeed + 1e-5f)));
                Vector3 accelForce = transform.forward * (throttle * _stats.Acceleration * speedRatio * _rb.mass);
                _rb.AddForce(accelForce, ForceMode.Force);
            }
            else if (throttle < 0f)
            {
                if (currentForwardVel > 0.5f)
                {
                    // Braking Deceleration:
                    // Brake force opposes forward motion = -transform.forward * (Mathf.Abs(throttle) * Braking * mass)
                    Vector3 brakeForce = -transform.forward * (Mathf.Abs(throttle) * _stats.Braking * _rb.mass);
                    _rb.AddForce(brakeForce, ForceMode.Force);
                }
                else
                {
                    // Reverse Acceleration:
                    // Reverse force = -transform.forward * (Mathf.Abs(throttle) * Acceleration * 0.5 * mass)
                    Vector3 reverseForce = -transform.forward * (Mathf.Abs(throttle) * _stats.Acceleration * 0.5f * _rb.mass);
                    _rb.AddForce(reverseForce, ForceMode.Force);
                }
            }
        }

        private void ApplySteeringTorque(float steer)
        {
            if (Mathf.Abs(steer) < 0.01f) return;

            // Speed-sensitive steering curve: reduce steering sensitivity at high velocity to maintain stability
            // Speed factor tapers from 1.0 at rest down to 0.4 at max top speed
            float speedFactor = Mathf.Lerp(1.0f, 0.4f, CurrentSpeed / (_stats.TopSpeed + 1e-5f));

            // Steering torque = transform.up * (steer * Handling * speedFactor * mass)
            // Vector cross product rotation around local up axis
            Vector3 turnTorque = transform.up * (steer * _stats.Handling * speedFactor * _rb.mass);
            _rb.AddTorque(turnTorque, ForceMode.Force);
        }

        private void ApplyLateralFriction()
        {
            // Lateral tire grip force opposes sideways slip velocity along local right axis
            // Lateral velocity = Vector3.Dot(Velocity, transform.right)
            // Lateral force = -transform.right * (lateralVel * gripCoefficient * gripModifier * mass)
            float latVel = LateralVelocity;
            float effectiveGrip = gripCoefficient * _gripModifier;
            Vector3 lateralGripForce = -transform.right * (latVel * effectiveGrip * _rb.mass);

            _rb.AddForce(lateralGripForce, ForceMode.Force);
        }

        private void ApplyDownforce()
        {
            // Quadratic aerodynamic downforce = -transform.up * (downforceFactor * speed * speed)
            // Downforce magnitude increases quadratically with forward speed to press tires into track surface
            float speed = CurrentSpeed;
            Vector3 downforce = -transform.up * (downforceCoefficient * speed * speed);
            _rb.AddForce(downforce, ForceMode.Force);
        }
    }
}
