using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>Visual-only capsule-skin compensation on one flat illustrated support. Earlier platforms are untouched.</summary>
    [DefaultExecutionOrder(400)]
    public sealed class DresserFootAlignment : MonoBehaviour
    {
        public Yuna25DPlayerController player;
        public BoxCollider surface;
        public int supportIndex = 3;
        /// <summary>Pin the drawn sole (renderer bounds) instead of the root; used where the paint sits below the pivot.</summary>
        public bool pinVisibleSole;
        public float AnchorGap { get; private set; }
        Transform shifted;
        float applied;
        void Update() => Undo();
        void Undo()
        {
            if (shifted != null) shifted.position -= Vector3.up * applied;
            shifted = null; applied = 0;
        }
        void LateUpdate()
        {
            AnchorGap = 0;
            if (player == null || surface == null || !player.IsGrounded || player.CurrentSupportIndex != supportIndex) return;
            RaycastHit hit;
            if (!surface.Raycast(new Ray(player.transform.position + Vector3.up*.4f, Vector3.down), out hit, .8f)) return;
            var r = player.GetComponent<YunaIllustrationVisibilityResolver>();
            // The baseline remains the root in run-flight poses: do not pin the lifted shoe to the top.
            shifted = r.VisibleSource == "Idle" ? r.idle.transform : r.VisibleSource == "Jump" ? r.jump.transform
                : r.VisibleSource == "Stumble" ? r.stumble.transform : r.run.transform;
            if (pinVisibleSole)
            {
                var painting = r.VisibleSource == "Idle" ? r.idleRenderer
                    : r.VisibleSource == "Jump" ? r.jump.painting
                    : r.VisibleSource == "Stumble" ? r.stumble.painting : r.run.painting;
                if (painting == null) return;
                applied = Mathf.Clamp(hit.point.y - painting.bounds.min.y, -.2f, .2f);
                shifted.position += Vector3.up*applied;
                AnchorGap = painting.bounds.min.y - hit.point.y;
                return;
            }
            applied = -Mathf.Clamp(player.transform.position.y-hit.point.y, 0, .2f);
            shifted.position += Vector3.up*applied;
            AnchorGap = player.transform.position.y + applied-hit.point.y;
        }
        void OnDisable() => Undo();
    }
}
