using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.YunaIllustration
{
    /// <summary>T03.2 candidate: drawn legs, with the existing physical controller as the clock.</summary>
    [DefaultExecutionOrder(210)]
    public sealed class YunaRunIllustration : MonoBehaviour
    {
        public SpriteRenderer painting;
        public Sprite[] frames;
        public Collider reviewSupport;
        public Yuna25DPlayerController player;
        public bool reviewFixtureAutoRun;
        public bool footPlant = true;
        public int CurrentFrame { get; private set; }
        public float VisualDrop { get; private set; }
        public float Phase { get; private set; }
        public float FootWorldX { get; private set; }
        public float FootWorldY { get; private set; }
        public float BodyShift => bodyShift;
        public float LegShift { get; private set; }
        public float AnchorCellWeight { get; private set; }
        public bool ContactPose => CurrentFrame % 4 != 3;
        // Pixel authoring in each 362x543 cell, top-left origin. No per-pose rescaling.
        public static readonly float[] FloorY = { 493, 496, 495, 495, 495, 496, 496, 495 };
        public static readonly float[] FootX = { 276.07f, 197.64f, 158.91f, 74.59f, 278.82f, 193.13f, 152.03f, 73.33f };
        public static readonly float[] BottomY = { 493, 496, 495, 438, 495, 496, 496, 435 };
        float anchorX, bodyShift;
        bool anchored;
        int stance = -1;

        void Awake()
        {
            if (reviewFixtureAutoRun && player != null)
            {
                player.SetFloorDeathActive(false);
                player.AutoRunActive = true;
            }
        }
        void LateUpdate() { Refresh(); }

        public void Refresh()
        {
            if (player == null || painting == null || frames == null || frames.Length != 8) return;
            Phase = player.GaitPhase;
            CurrentFrame = Mathf.Min(7, Mathf.FloorToInt(Phase * 8f));
            painting.sprite = frames[CurrentFrame];
            painting.flipX = player.RunDirection < 0f;
            // The isolated flat support is a real collider. Never move the physical root.
            VisualDrop = 0f;
            if (player.IsGrounded && reviewSupport != null)
            {
                RaycastHit hit;
                var ray = new Ray(player.transform.position + Vector3.up * .4f, Vector3.down);
                if (reviewSupport.Raycast(ray, out hit, .6f))
                {
                    float gap = player.transform.position.y - hit.point.y;
                    if (gap >= 0f && gap <= .1f) VisualDrop = gap;
                }
            }
            transform.localPosition = Vector3.down * VisualDrop;
            transform.localRotation = Quaternion.AngleAxis(-player.transform.eulerAngles.y, Vector3.up);
            float sign = painting.flipX ? -1f : 1f;
            float ppu = painting.sprite.pixelsPerUnit;
            var point = new Vector3(sign * (FootX[CurrentFrame] - painting.sprite.pivot.x) / ppu,
                (FloorY[CurrentFrame] - BottomY[CurrentFrame]) / ppu, 0f);
            var world = transform.TransformPoint(point);
            float correction = 0f;
            if (footPlant && player.IsGrounded && ContactPose)
            {
                if (!anchored || stance != CurrentFrame / 4)
                {
                    // Centre the pose root motion around the capsule, rather than accumulate
                    // its whole displacement behind it and saturate at different frame rates.
                    anchorX = world.x + player.RunDirection * .2f;
                    anchored = true; stance = CurrentFrame / 4;
                }
                correction = Mathf.Clamp(anchorX - world.x, -.35f, .35f);
            }
            else anchored = false;
            bodyShift = correction; // Authored pose root motion: never warp the painted limbs.
            transform.localPosition += player.transform.InverseTransformVector(Vector3.right * bodyShift);
            LegShift = 0f; AnchorCellWeight = 1f;
            world = transform.TransformPoint(point);
            FootWorldX = world.x; FootWorldY = world.y;
        }
    }
}
