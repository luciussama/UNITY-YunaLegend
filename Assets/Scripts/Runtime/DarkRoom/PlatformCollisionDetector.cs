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

        public void OnCharacterLanded(Yuna25DPlayerController player)
        {
            if (marker != null && progression != null)
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
