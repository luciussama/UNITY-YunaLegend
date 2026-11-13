using UnityEngine;

namespace YunaLegend.YunaIllustration
{
    /// <summary>
    /// Joint visibility authority for illustrated Yuna. The approved action components
    /// each own a renderer and can re-enable the run painting, so this resolver runs
    /// after all of them and leaves exactly one source visible.
    /// Priority: Stumble > Jump > Run > Idle.
    /// Set showIdle from the integration state; false falls back to Run.
    /// </summary>
    [DefaultExecutionOrder(240)]
    public sealed class YunaIllustrationVisibilityResolver : MonoBehaviour
    {
        public YunaIdleIllustration idle;
        public Renderer idleRenderer;
        public YunaRunIllustration run;
        public YunaJumpIllustration jump;
        public YunaStumbleIllustration stumble;
        [Tooltip("When true, Idle is the fallback source. When false, Run is the fallback.")]
        public bool showIdle;

        public string VisibleSource { get; private set; } = "None";
        public int VisibleCount { get; private set; }
        public int ComponentRequests { get; private set; }
        public int ComponentOverlapFrames { get; private set; }
        public int EmptyComponentFrames { get; private set; }
        public int ConflictingFrames { get; private set; }
        public int ResolveCalls { get; private set; }

        int componentFrame = -1;
        int conflictFrame = -1;

        void LateUpdate() { Resolve(); }

        /// <summary>
        /// Applies the joint priority. Safe to call after the action components refreshed.
        /// The method is idempotent; calling it again in the same frame only reasserts the result.
        /// </summary>
        public void Resolve()
        {
            if (run == null || jump == null || stumble == null || run.painting == null
                || jump.painting == null || stumble.painting == null) return;

            ResolveCalls++;
            ComponentRequests = Count(idleRenderer != null && idleRenderer.enabled,
                run.painting.enabled, jump.painting.enabled, stumble.painting.enabled);
            if (componentFrame != Time.frameCount)
            {
                componentFrame = Time.frameCount;
                if (ComponentRequests > 1) ComponentOverlapFrames++;
                if (ComponentRequests == 0) EmptyComponentFrames++;
            }

            bool stumbleOn = stumble.Pose >= 0;
            bool jumpOn = !stumbleOn && jump.Pose >= 0;
            bool runOn = !stumbleOn && !jumpOn && !showIdle;
            bool idleOn = !stumbleOn && !jumpOn && showIdle;

            stumble.painting.enabled = stumbleOn;
            jump.painting.enabled = jumpOn;
            run.painting.enabled = runOn;
            if (idleRenderer != null) idleRenderer.enabled = idleOn;

            VisibleSource = stumbleOn ? "Stumble" : jumpOn ? "Jump" : runOn ? "Run" : "Idle";
            VisibleCount = Count(idleOn, runOn, jumpOn, stumbleOn);
            if (VisibleCount != 1 && conflictFrame != Time.frameCount)
            {
                conflictFrame = Time.frameCount;
                ConflictingFrames++;
            }
        }

        static int Count(bool idleOn, bool runOn, bool jumpOn, bool stumbleOn)
        {
            return (idleOn ? 1 : 0) + (runOn ? 1 : 0) + (jumpOn ? 1 : 0) + (stumbleOn ? 1 : 0);
        }
    }
}
