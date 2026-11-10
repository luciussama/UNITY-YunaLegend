using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public class DarkRoomPlatformMarker : MonoBehaviour
    {
        public PlatformSpec spec;

        public Vector3 GetTopCenterWorld()
        {
            return transform.position + new Vector3(0f, spec.height * 0.5f, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = spec.tint;
            Gizmos.DrawWireCube(transform.position, new Vector3(spec.width, spec.height, spec.depth));
        }
    }
}
