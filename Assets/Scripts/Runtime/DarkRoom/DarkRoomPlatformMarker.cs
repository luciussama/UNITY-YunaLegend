using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public class DarkRoomPlatformMarker : MonoBehaviour
    {
        public PlatformSpec spec;

        public Vector3 GetTopCenterWorld()
        {
            var contact = spec.contact;
            return transform.position + new Vector3(
                contact.offsetX,
                spec.height * 0.5f + contact.offsetY,
                0f);
        }

        /// <summary>Rotacao da superficie de pouso (pilhas diagonais inclinam o topo).</summary>
        public Quaternion GetContactRotation()
        {
            return Quaternion.Euler(0f, 0f, spec.contact.tiltDegrees);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = spec.tint;
            Gizmos.DrawWireCube(transform.position, new Vector3(spec.width, spec.height, spec.depth));
        }
    }
}
