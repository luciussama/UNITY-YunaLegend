using UnityEngine;

namespace YunaLegend.DarkRoom
{
    /// <summary>
    /// Neutral sprite orientation for the isolated 2.5D proof. Attach below Body_Visual,
    /// leaving that parent available for poses. The physical controller keeps its existing yaw.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class IllustratedCharacterFacing : MonoBehaviour
    {
        private Yuna25DPlayerController player;
        private SpriteRenderer painting;

        private void Awake()
        {
            player = GetComponentInParent<Yuna25DPlayerController>();
            painting = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (player == null) return;
            transform.localRotation = Quaternion.AngleAxis(-player.transform.eulerAngles.y, Vector3.up);
            painting.flipX = player.RunDirection < 0f;
        }
    }
}
