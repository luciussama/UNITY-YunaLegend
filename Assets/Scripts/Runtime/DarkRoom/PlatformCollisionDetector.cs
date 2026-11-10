using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public class PlatformCollisionDetector : MonoBehaviour
    {
        [SerializeField] private DarkRoomPlatformMarker marker;
        [SerializeField] private DarkRoomProgressionManager progression;

        private void Awake()
        {
            if (marker == null)
            {
                marker = GetComponentInParent<DarkRoomPlatformMarker>();
            }
            if (progression == null)
            {
                progression = FindFirstObjectByType<DarkRoomProgressionManager>();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Detecta quando o jogador aterrissa sobre a plataforma por cima
            if (hit.normal.y > 0.6f && marker != null && progression != null)
            {
                progression.NotifyPlatformLanded(marker.spec.index);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Yuna25DPlayerController>() != null && marker != null && progression != null)
            {
                progression.NotifyPlatformLanded(marker.spec.index);
            }
        }
    }
}
