using UnityEngine;
using NitroRush.Core;
using NitroRush.Physics;

namespace NitroRush.Cars
{
    /// <summary>
    /// Top-level coordinator component wiring together CarPhysics, CarDrift, CarNitro, CarDamage,
    /// CarMotor, CarSteering, CarAudio, and CarVFX sub-systems.
    /// Requires Rigidbody and caches all component references in Awake to eliminate runtime GetComponent calls.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CarPhysics))]
    [RequireComponent(typeof(CarDrift))]
    [RequireComponent(typeof(CarNitro))]
    [RequireComponent(typeof(CarDamage))]
    public class CarController : MonoBehaviour
    {
        [Header("Vehicle Configuration")]
        [SerializeField] private CarData carData;

        // Cached sub-component references
        private Rigidbody _rigidbody;
        private CarPhysics _carPhysics;
        private CarDrift _carDrift;
        private CarNitro _carNitro;
        private CarDamage _carDamage;
        private CarMotor _carMotor;
        private CarSteering _carSteering;
        private CarAudio _carAudio;
        private CarVFX _carVFX;
        private ICarInput _carInput;

        public string RacerId { get; private set; } = "player_local";
        public CarData Data => carData;
        public CarStats Stats { get; private set; }

        public float CurrentSpeed => _carPhysics != null ? _carPhysics.CurrentSpeed : 0f;
        public bool IsDrifting => _carDrift != null && _carDrift.IsDrifting;
        public bool NitroActive => _carNitro != null && _carNitro.IsNitroActive;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            // Cache all sub-components up front to guarantee zero GetComponent calls in Update/FixedUpdate
            _rigidbody = GetComponent<Rigidbody>();
            _carPhysics = GetComponent<CarPhysics>();
            _carDrift = GetComponent<CarDrift>();
            _carNitro = GetComponent<CarNitro>();
            _carDamage = GetComponent<CarDamage>();
            _carMotor = GetComponent<CarMotor>();
            _carSteering = GetComponent<CarSteering>();
            _carAudio = GetComponent<CarAudio>();
            _carVFX = GetComponent<CarVFX>();

            // Auto-detect player input component if attached locally
            _carInput = GetComponent<ICarInput>() ?? GetComponent<PlayerCarInput>();
        }

        private void Start()
        {
            if (carData != null && Stats.TopSpeed <= 0f)
            {
                Initialize(carData, 1, 1, 1, 1, 1, RacerId);
            }
        }

        /// <summary>
        /// Fully initializes car specs, upgrade levels, driver input source, and racer identity.
        /// </summary>
        public void Initialize(CarData data, int engineLvl, int turboLvl, int tiresLvl, int brakesLvl, int nitroLvl, string racerId, ICarInput input = null)
        {
            carData = data;
            RacerId = racerId;

            Stats = CarStats.FromData(carData, engineLvl, turboLvl, tiresLvl, brakesLvl, nitroLvl);

            if (input != null)
            {
                _carInput = input;
            }

            _carPhysics.Initialize(Stats, _carInput);
            _carDrift.Initialize(_carInput, RacerId);
            _carNitro.Initialize(_carInput, RacerId);
            _carDamage.Initialize(RacerId);
        }

        /// <summary>
        /// Explicitly updates driver input provider (e.g., swapping between PlayerCarInput and AI input).
        /// </summary>
        public void SetInput(ICarInput input)
        {
            _carInput = input;
            _carPhysics.Initialize(Stats, _carInput);
            _carDrift.Initialize(_carInput, RacerId);
            _carNitro.Initialize(_carInput, RacerId);
        }

        private void Update()
        {
            float throttle = _carInput?.Throttle ?? 0f;
            float steer = _carInput?.Steer ?? 0f;

            if (_carMotor != null)
            {
                _carMotor.UpdateMotor(CurrentSpeed, Stats.TopSpeed, throttle);
            }

            if (_carSteering != null)
            {
                _carSteering.UpdateSteering(steer, CurrentSpeed, Stats.TopSpeed);
            }
        }
    }
}
