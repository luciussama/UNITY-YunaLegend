using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.YunaIllustration
{
    /// <summary>T03.3 authoring candidate. All translations are visual; the 3D controller owns the arc.</summary>
    [DefaultExecutionOrder(220)]
    public sealed class YunaJumpIllustration : MonoBehaviour
    {
        public Yuna25DPlayerController player;
        public YunaRunIllustration run;
        public SpriteRenderer painting;
        public Sprite[] frames;
        public Collider reviewSupport;
        // Uniform scale, common canvas. Only measured contact baselines vary by 1-3 source pixels.
        public static readonly float[] Baseline = { 511, 511, 511, 511, 511, 508, 510, 511 };
        public static readonly float[] SoleX = { 155, 155, 155, 155, 155, 255, 223, 235 };
        public int Pose { get; private set; } = -1;
        public string State { get; private set; } = "Run";
        public int Takeoffs { get; private set; }
        public int Landings { get; private set; }
        public float ContactX { get; private set; }
        public float ContactY { get; private set; }
        public float VisualShift { get; private set; }
        public bool LandingContact => State == "Land" || State == "Recover";
        bool airborne, landing;
        float airTime, landTime, anchorX, blendTime;
        bool settling;
        int evaluated = -1;

        void LateUpdate() { Refresh(); }

        public void Refresh()
        {
            if (player == null || painting == null || frames == null || frames.Length != 8 || run == null) return;
            if (evaluated == Time.frameCount) return;
            evaluated = Time.frameCount;
            float dt = Time.deltaTime;
            if (player.IsDead)
            {
                airborne = landing = false; Pose = -1; State = "Dead";
                painting.enabled = false; run.painting.enabled = true;
                return;
            }
            if (!player.IsGrounded && player.Velocity.y > 1f && !airborne)
            {
                airborne = true; landing = false; airTime = 0; Takeoffs++;
            }
            bool justLanded = airborne && player.IsGrounded && player.Velocity.y <= 0;
            if (justLanded)
            {
                airborne = false; landing = true; landTime = 0; Landings++;
            }
            if (airborne)
            {
                float vy = player.Velocity.y;
                if (airTime < .07f) { Pose = 0; State = "Takeoff"; }
                else if (vy > 7f) { Pose = 1; State = "Rise"; }
                else if (vy > 1.5f) { Pose = 2; State = "Rise"; }
                else if (vy >= -1.5f) { Pose = 3; State = "Apex"; }
                else
                {
                    RaycastHit hit;
                    bool near = reviewSupport != null && reviewSupport.Raycast(
                        new Ray(player.transform.position + Vector3.up * .1f, Vector3.down), out hit, .65f);
                    Pose = near ? 5 : 4; State = "Fall";
                }
                airTime += dt;
            }
            else if (landing)
            {
                if (!justLanded) landTime += dt;
                Pose = landTime < .03f ? 6 : 7;
                State = landTime < .03f ? "Land" : "Recover";
                // Short impact/recovery at the actual running speed; do not stop the physical root.
                if (landTime >= .06f)
                {
                    landing = false; Pose = -1; State = "Run";
                    settling = true; blendTime = 0;
                }
            }
            else { Pose = -1; State = "Run"; }

            painting.enabled = Pose >= 0;
            run.painting.enabled = Pose < 0;
            if (Pose < 0)
            {
                // Preserve the approved run unchanged once its visual offset has been reached.
                // While joining, limit correction velocity to avoid stacking a blend jump on a run pose jump.
                if (settling)
                {
                    blendTime += dt; State = "RunBlend";
                    VisualShift = Mathf.MoveTowards(VisualShift, run.BodyShift, 2f * dt);
                    run.transform.localPosition += player.transform.InverseTransformVector(Vector3.right * (VisualShift - run.BodyShift));
                    if (blendTime >= .06f && Mathf.Abs(VisualShift - run.BodyShift) < .002f) settling = false;
                }
                else VisualShift = run.BodyShift;
                return;
            }
            painting.sprite = frames[Pose];
            painting.flipX = player.RunDirection < 0f;
            transform.localRotation = Quaternion.AngleAxis(-player.transform.eulerAngles.y, Vector3.up);
            float drop = 0;
            RaycastHit support;
            if (player.IsGrounded && reviewSupport != null && reviewSupport.Raycast(
                new Ray(player.transform.position + Vector3.up * .4f, Vector3.down), out support, .6f))
                drop = Mathf.Clamp(player.transform.position.y - support.point.y, 0, .1f);
            transform.localPosition = Vector3.down * drop;
            float sign = painting.flipX ? -1 : 1;
            Vector3 point = new Vector3(sign * (SoleX[Pose] - painting.sprite.pivot.x) / painting.sprite.pixelsPerUnit, 0, 0);
            var world = transform.TransformPoint(point);
            VisualShift = 0;
            if (LandingContact)
            {
                if (justLanded) anchorX = world.x;
                VisualShift = Mathf.Clamp(anchorX - world.x, -.35f, .35f);
                transform.localPosition += player.transform.InverseTransformVector(Vector3.right * VisualShift);
            }
            world = transform.TransformPoint(point);
            ContactX = world.x; ContactY = world.y;
        }
    }
}
