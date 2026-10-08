using UnityEngine;
using NitroRush.Core;
using NitroRush.Physics;

namespace NitroRush.Cars
{
    /// <summary>
    /// Manages pitch-modulated engine sound audio source and pooled audio playback for drift/nitro/collisions.
    /// </summary>
    [RequireComponent(typeof(CarPhysics))]
    public class CarAudio : MonoBehaviour
    {
        [Header("Engine Audio Settings")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private float minPitch = 0.8f;
        [SerializeField] private float maxPitch = 2.5f;

        private CarPhysics _carPhysics;

        private void Awake()
        {
            _carPhysics = GetComponent<CarPhysics>();
        }

        private void Update()
        {
            if (engineAudioSource == null || _carPhysics == null) return;

            // Pitch modulation based on current speed percentage = CurrentSpeed / TopSpeed
            float speedPercent = Mathf.Clamp01(_carPhysics.CurrentSpeed / (_carPhysics.Stats.TopSpeed + 1e-5f));
            engineAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);
        }
    }
}
