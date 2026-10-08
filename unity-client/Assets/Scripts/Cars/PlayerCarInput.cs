using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NitroRush.Cars
{
    /// <summary>
    /// Component reading player hardware inputs. Uses Unity Input System when available with legacy Input fallback.
    /// </summary>
    public class PlayerCarInput : MonoBehaviour, ICarInput
    {
        [Header("Legacy Input Axes (Fallback)")]
        [SerializeField] private string throttleAxis = "Vertical";
        [SerializeField] private string steerAxis = "Horizontal";
        [SerializeField] private KeyCode driftKey = KeyCode.Space;
        [SerializeField] private KeyCode nitroKey = KeyCode.LeftShift;

#if ENABLE_INPUT_SYSTEM
        [Header("Input System Actions")]
        [SerializeField] private InputAction throttleAction;
        [SerializeField] private InputAction steerAction;
        [SerializeField] private InputAction driftAction;
        [SerializeField] private InputAction nitroAction;

        private void OnEnable()
        {
            throttleAction.Enable();
            steerAction.Enable();
            driftAction.Enable();
            nitroAction.Enable();
        }

        private void OnDisable()
        {
            throttleAction.Disable();
            steerAction.Disable();
            driftAction.Disable();
            nitroAction.Disable();
        }
#endif

        public float Throttle
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (throttleAction != null && throttleAction.enabled)
                {
                    return throttleAction.ReadValue<float>();
                }
#endif
                return Input.GetAxis(throttleAxis);
            }
        }

        public float Steer
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (steerAction != null && steerAction.enabled)
                {
                    return steerAction.ReadValue<float>();
                }
#endif
                return Input.GetAxis(steerAxis);
            }
        }

        public bool DriftHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (driftAction != null && driftAction.enabled)
                {
                    return driftAction.IsPressed();
                }
#endif
                return Input.GetKey(driftKey);
            }
        }

        public bool NitroHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (nitroAction != null && nitroAction.enabled)
                {
                    return nitroAction.IsPressed();
                }
#endif
                return Input.GetKey(nitroKey);
            }
        }
    }
}
