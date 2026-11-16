using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Camera isometrica 3D proxima que acompanha Yuna pela sala ampla,
    /// incentivando a exploracao e mantendo os respawns fora do enquadramento.
    /// </summary>
    public sealed class IsometricToyRoomCamera : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 offset = new Vector3(0f, 7.2f, -7.8f);
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0.7f, 0.2f);
        [SerializeField] private float smoothTime = 0.22f;

        private Transform overrideTarget;
        private Vector3 velocity;

        public void Configure(Transform target)
        {
            followTarget = target;
            SnapToTarget();
        }

        public void SetOverrideTarget(Transform target)
        {
            overrideTarget = target;
        }

        public void SnapToTarget()
        {
            Transform active = overrideTarget != null ? overrideTarget : followTarget;
            if (active == null)
            {
                return;
            }

            Vector3 desiredFocus = ClampFocus(active.position + lookAtOffset);
            transform.position = desiredFocus + offset;
            transform.LookAt(desiredFocus);
        }

        private void LateUpdate()
        {
            Transform active = overrideTarget != null ? overrideTarget : followTarget;
            if (active == null)
            {
                return;
            }

            Vector3 desiredFocus = ClampFocus(active.position + lookAtOffset);
            Vector3 desiredPos = desiredFocus + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
            transform.LookAt(transform.position - offset);
        }

        private static Vector3 ClampFocus(Vector3 focus)
        {
            float maxX = ToyRoomCatalog.HalfWidth - 3.2f;
            float maxZ = ToyRoomCatalog.HalfDepth - 2.6f;
            focus.x = Mathf.Clamp(focus.x, -maxX, maxX);
            focus.z = Mathf.Clamp(focus.z, -maxZ, maxZ);
            return focus;
        }
    }
}
