using UnityEngine;

namespace YunaLegend.NandaIllustration
{
    /// <summary>
    /// T04.1 candidate. Animates only the painted companion. Navigation, light and
    /// trail remain with <c>NandaFairyGuide</c>; this component never moves the root.
    /// </summary>
    [DefaultExecutionOrder(210)]
    public sealed class NandaFlightIllustration : MonoBehaviour
    {
        public SpriteRenderer painting;
        public Transform visualRoot;
        public Sprite[] frames;
        public float cycleSeconds = .8f;
        public float hoverAmplitude = .035f;
        public float hoverPeriod = 2.4f;
        public bool previewClock = true;

        public int Pose { get; private set; } = -1;
        public int Cycle { get; private set; }
        public float CyclePhase { get; private set; }
        public float HoverOffset { get; private set; }
        public float Direction { get; private set; } = 1f;
        public float Clock => clock;
        public bool FlipX => painting != null && painting.flipX;
        public int FrameCount => frames == null ? 0 : frames.Length;

        float clock;
        Vector3 visualBase;

        void Awake()
        {
            if (painting == null) painting = GetComponentInChildren<SpriteRenderer>();
            if (visualRoot == null && painting != null) visualRoot = painting.transform;
            if (visualRoot != null) visualBase = visualRoot.localPosition;
        }

        void LateUpdate()
        {
            if (!previewClock) return;
            clock += Time.deltaTime;
            Evaluate(clock, Direction);
        }

        public void SetDirection(float facing)
        {
            Direction = facing < 0f ? -1f : 1f;
            if (painting != null) painting.flipX = Direction < 0f;
        }

        /// <summary>
        /// Deterministic evaluation. The same <paramref name="seconds"/> yields the same
        /// pose at any simulation rate, so frame cadence cannot drift with frame count.
        /// </summary>
        public void Evaluate(float seconds, float facing)
        {
            SetDirection(facing);
            if (painting == null || frames == null || frames.Length == 0) return;

            float safeSeconds = Mathf.Max(0f, seconds);
            float cycles = safeSeconds / cycleSeconds;
            Cycle = Mathf.FloorToInt(cycles);
            CyclePhase = cycles - Cycle;
            Pose = Mathf.Min(frames.Length - 1, Mathf.FloorToInt(CyclePhase * frames.Length + .0001f));
            painting.sprite = frames[Pose];

            HoverOffset = Mathf.Sin(safeSeconds * Mathf.PI * 2f / hoverPeriod) * hoverAmplitude;
            // The hover must live on a child so the guide keeps sole ownership of the root.
            if (visualRoot != null && visualRoot != transform)
                visualRoot.localPosition = visualBase + Vector3.up * HoverOffset;
        }
    }
}
