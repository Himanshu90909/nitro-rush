using UnityEngine;
using NitroRush.Cars;

namespace NitroRush.Racing
{
    /// <summary>
    /// Sequential track checkpoint with trigger collider reporting racer crossings to RaceManager.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Configuration")]
        [SerializeField] private int index;
        [SerializeField] private bool isFinishLine;
        [SerializeField] private Transform leftBoundary;
        [SerializeField] private Transform rightBoundary;

        public int Index => index;
        public bool IsFinishLine => isFinishLine;
        public Vector3 Position => transform.position;
        public Vector3 ForwardDirection => transform.forward;

        public Vector3 LeftBoundary => leftBoundary != null ? leftBoundary.position : transform.position - transform.right * 10f;
        public Vector3 RightBoundary => rightBoundary != null ? rightBoundary.position : transform.position + transform.right * 10f;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car != null && RaceManager.Instance != null)
            {
                RaceManager.Instance.OnRacerPassedCheckpoint(car.RacerId, this);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isFinishLine ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(20f, 5f, 2f));
            Gizmos.DrawRay(transform.position, transform.forward * 4f);
        }
    }
}
