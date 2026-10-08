using UnityEngine;
using NitroRush.Core;

namespace NitroRush.Cars
{
    /// <summary>
    /// Visual effects manager requesting pooled particle systems (nitro flames, tire smoke, collision sparks)
    /// via PoolManager without creating or instantiating objects during runtime driving.
    /// </summary>
    public class CarVFX : MonoBehaviour
    {
        [Header("Pooled VFX Keys")]
        [SerializeField] private string nitroVfxPoolKey = "vfx_nitro_flame";
        [SerializeField] private string tireSmokeVfxPoolKey = "vfx_tire_smoke";

        [Header("Emitter Anchor Transforms")]
        [SerializeField] private Transform[] exhaustAnchors;
        [SerializeField] private Transform[] rearWheelAnchors;

        private CarDrift _carDrift;
        private CarNitro _carNitro;

        private void Awake()
        {
            _carDrift = GetComponent<CarDrift>();
            _carNitro = GetComponent<CarNitro>();
        }

        private void Update()
        {
            // Spawn tire smoke from pool when drifting
            if (_carDrift != null && _carDrift.IsDrifting)
            {
                EmitTireSmoke();
            }

            // Spawn nitro exhaust flames from pool when nitro boost is active
            if (_carNitro != null && _carNitro.IsNitroActive)
            {
                EmitNitroFlames();
            }
        }

        private void EmitTireSmoke()
        {
            if (PoolManager.Instance == null || rearWheelAnchors == null) return;

            foreach (var anchor in rearWheelAnchors)
            {
                if (anchor == null) continue;
                var pfx = PoolManager.Instance.Get<ParticleSystem>(tireSmokeVfxPoolKey);
                if (pfx != null)
                {
                    pfx.transform.position = anchor.position;
                    pfx.transform.rotation = anchor.rotation;
                    pfx.Play();
                }
            }
        }

        private void EmitNitroFlames()
        {
            if (PoolManager.Instance == null || exhaustAnchors == null) return;

            foreach (var anchor in exhaustAnchors)
            {
                if (anchor == null) continue;
                var pfx = PoolManager.Instance.Get<ParticleSystem>(nitroVfxPoolKey);
                if (pfx != null)
                {
                    pfx.transform.position = anchor.position;
                    pfx.transform.rotation = anchor.rotation;
                    pfx.Play();
                }
            }
        }
    }
}
