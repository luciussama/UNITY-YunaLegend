using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D
{
    public enum PaintedPose { Idle, Run, Jump, Fall, Land, Stumble }

    /// <summary>
    /// T02.3 proof only: drives the painted skin from the existing controller state, without touching
    /// movement, physics or the neutral painting. Same pose selection the articulated 3D study used,
    /// so the comparison is about the rendering method, not about different gameplay.
    /// Legs do not articulate: this proof measures a deformation layer and the cost of that layer.
    /// </summary>
    [DefaultExecutionOrder(205)]
    public sealed class IllustratedPaintedAnimation : MonoBehaviour
    {
        public PaintedMeshDeformer skin;
        public PaintedPose CurrentPose { get; private set; }
        public float CurrentPhase { get; private set; }

        Yuna25DPlayerController player;
        float phase, landing, clock;
        bool grounded;

        void Awake() { Bind(); }

        void Bind()
        {
            if (player == null) player = GetComponentInParent<Yuna25DPlayerController>();
            if (skin == null) skin = GetComponentInChildren<PaintedMeshDeformer>();
        }

        void LateUpdate()
        {
            Bind();
            if (player == null || skin == null) return;
            float dt = Time.deltaTime;
            clock += dt;
            bool onGround = player.IsGrounded;
            if (onGround && !grounded) landing = .16f;
            grounded = onGround;
            landing = Mathf.Max(0f, landing - dt);

            bool running = player.AutoRunActive && !player.IsDead && Mathf.Abs(player.Velocity.x) > .01f;
            if (running && onGround) phase = player.GaitPhase;
            CurrentPhase = phase;

            var pose = player.IsStumbling ? PaintedPose.Stumble
                : !onGround && (running || player.IsCinematicFalling)
                    ? (player.Velocity.y > 0f ? PaintedPose.Jump : PaintedPose.Fall)
                : landing > 0f && running ? PaintedPose.Land
                : running ? PaintedPose.Run
                : PaintedPose.Idle;
            CurrentPose = pose;
            Apply(pose);
            // Face the camera like the static proof did; the physical controller keeps its own yaw.
            skin.transform.localRotation = Quaternion.AngleAxis(-player.transform.eulerAngles.y, Vector3.up);
            skin.flipX = player.RunDirection < 0f;
            skin.Refresh();
        }

        void Apply(PaintedPose pose)
        {
            float wave = Mathf.Sin(phase * Mathf.PI * 2f);
            skin.squashX = 1f;
            skin.squashY = 1f;
            skin.bodyCompression = 0f;
            skin.breathe = 0f;
            skin.sideBandSwing = 0f;
            switch (pose)
            {
                case PaintedPose.Idle:
                    skin.lean = Mathf.Sin(clock * 1.1f) * .8f;
                    skin.breathe = Mathf.Sin(clock * 1.7f) * .012f;
                    skin.headTilt = Mathf.Sin(clock * .8f) * 1.5f;
                    skin.bodyCompression = .006f;
                    break;
                case PaintedPose.Run:
                    skin.lean = 7f + Mathf.Sin(phase * 4f * Mathf.PI) * 1.2f;
                    skin.bodyCompression = .035f + .02f * Mathf.Cos(phase * 4f * Mathf.PI);
                    skin.sideBandSwing = wave * 12f;
                    skin.headTilt = -skin.lean * .35f;
                    skin.breathe = wave * .004f;
                    break;
                case PaintedPose.Jump:
                    skin.lean = 6f;
                    skin.sideBandSwing = -10f;
                    skin.squashY = 1.045f;
                    skin.squashX = .975f;
                    skin.headTilt = -3f;
                    break;
                case PaintedPose.Fall:
                    skin.lean = 3f;
                    skin.sideBandSwing = 4f;
                    skin.squashY = 1.02f;
                    skin.squashX = .99f;
                    skin.headTilt = 2f;
                    break;
                case PaintedPose.Land:
                    float impact = Mathf.Clamp01(landing / .16f);
                    skin.squashY = 1f - .075f * impact;
                    skin.squashX = 1f + .05f * impact;
                    skin.lean = 6f;
                    skin.sideBandSwing = -6f;
                    skin.headTilt = -2f;
                    break;
                case PaintedPose.Stumble:
                    float amount = Mathf.Clamp01(player.StumblePoseAmount);
                    skin.lean = 6f + 14f * amount;
                    skin.squashY = 1f - .03f * amount;
                    skin.sideBandSwing = 8f * amount;
                    skin.headTilt = -4f * amount;
                    break;
            }
        }

        public void ResetPose()
        {
            phase = landing = clock = 0f;
            grounded = false;
            CurrentPose = PaintedPose.Idle;
            CurrentPhase = 0f;
            if (skin != null) skin.ResetPose();
        }
    }
}
