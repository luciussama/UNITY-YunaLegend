using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D;

namespace YunaLegend.YunaIllustration
{
    /// <summary>T03.1 isolated candidate. Evaluates only the visual, never the physical root.</summary>
    public sealed class YunaIdleIllustration : MonoBehaviour
    {
        public PaintedMeshDeformer skin;
        public Yuna25DPlayerController player;
        public float period = 4f;
        public bool previewClock = true;
        public float Blink { get; private set; }
        float clock;
        MaterialPropertyBlock block;
        bool cinematicOverride;
        float overrideBlink, overrideLean, overrideHeadTilt, overrideBreathe;
        float overrideSquashX = 1f, overrideSquashY = 1f;

        public void SetCinematicOverride(
            bool active,
            float blink,
            float lean,
            float headTilt,
            float breathe,
            float squashX = 1f,
            float squashY = 1f)
        {
            cinematicOverride = active;
            overrideBlink = blink;
            overrideLean = lean;
            overrideHeadTilt = headTilt;
            overrideBreathe = breathe;
            overrideSquashX = squashX;
            overrideSquashY = squashY;
        }

        void Awake()
        {
            if (player == null) player = GetComponentInParent<Yuna25DPlayerController>();
        }

        void LateUpdate()
        {
            if (!previewClock) return;
            clock += Time.deltaTime;
            Evaluate(clock);
        }

        public void Evaluate(float seconds)
        {
            if (skin == null) return;
            float t = Mathf.Repeat(seconds, period);
            float phase = t / period * Mathf.PI * 2f;
            if (cinematicOverride)
            {
                skin.lean = overrideLean;
                skin.headTilt = overrideHeadTilt;
                skin.breathe = overrideBreathe;
                skin.bodyCompression = skin.sideBandSwing = 0f;
                skin.squashX = overrideSquashX;
                skin.squashY = overrideSquashY;
            }
            else
            {
                skin.lean = .4f * Mathf.Sin(phase);
                skin.headTilt = -.5f * Mathf.Sin(phase);
                skin.breathe = (1f - Mathf.Cos(phase)) * .009f;
                skin.bodyCompression = skin.sideBandSwing = 0f;
                skin.squashX = skin.squashY = 1f;
            }
            if (player != null)
            {
                skin.flipX = player.RunDirection < 0f;
                skin.transform.localRotation = Quaternion.AngleAxis(-player.transform.eulerAngles.y, Vector3.up);
            }
            skin.Refresh();
            // 40 ms close, 80 ms closed, 60 ms open. Four-second seamless cycle.
            Blink = cinematicOverride
                ? Mathf.Clamp01(overrideBlink)
                : (t < 2.24f || t >= 2.42f ? 0f
                    : t < 2.28f ? (t - 2.24f) / .04f
                    : t < 2.36f ? 1f : (2.42f - t) / .06f);
            var renderer = skin.GetComponent<MeshRenderer>();
            if (block == null) block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetFloat("_Blink", Blink);
            renderer.SetPropertyBlock(block);
        }
    }
}
