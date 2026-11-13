using UnityEngine;
using YunaLegend.DarkRoom;
namespace YunaLegend.YunaIllustration
{
    // Isolated T03.4 candidate: observe the existing controller, never move its root.
    [DefaultExecutionOrder(230)]
    public sealed class YunaStumbleIllustration : MonoBehaviour
    {
        public Yuna25DPlayerController player;
        public YunaRunIllustration run;
        public SpriteRenderer painting;
        public Sprite[] frames;
        public Collider reviewSupport;
        public static readonly float[] Baseline = {430,429,428,428,429,430,431,431};
        public static readonly float[] SoleX = {219,229.5f,233.5f,273.5f,327,177,180,157.5f};
        public int Pose { get; private set; } = -1;
        public string State { get; private set; } = "Run";
        public int Events { get; private set; }
        public float Elapsed { get; private set; }
        public float ContactX { get; private set; }
        public float ContactY { get; private set; }
        public float VisualShift { get; private set; }
        bool active, settling;
        float previousDelay, anchor, blendTime;
        Sprite frozen;
        Vector3 frozenPosition;
        int last = -1, stance = -1;
        public void ResetPresentation()
        {
            active=settling=false; Elapsed=VisualShift=0; Pose=-1; State="Reset"; stance=-1;
            painting.enabled=false; run.painting.enabled=true;
            run.Refresh();
        }
        public void SynchronizeEvent() { last=-1; Refresh(); }
        void LateUpdate() { Refresh(); }
        public void Refresh()
        {
            if (last == Time.frameCount || player == null || run == null || painting == null) return;
            last = Time.frameCount;
            bool started = player.IsStumbling && !active;
            if (started)
            {
                active = true; settling = false; Events++; Elapsed = 0; stance = -1;
                previousDelay = player.StumbleDelayRemaining;
                frozen = run.painting.sprite; frozenPosition = run.transform.localPosition;
            }
            if (!player.IsStumbling || player.IsDead)
            {
                if (active) { active = false; settling = true; blendTime = 0; }
                painting.enabled = false; run.painting.enabled = true; Pose = -1; State = "Run";
                // A reset clears controller gait and all visual state immediately.
                if ((Mathf.Abs(player.GaitPhase-.5f)<.00001f && Elapsed<.28f-Time.deltaTime*1.5f) || player.IsDead)
                { settling = false; VisualShift = 0; Elapsed = 0; stance = -1; State = "Reset"; }
                if (settling)
                {
                    State = "RunBlend"; blendTime += Time.deltaTime;
                    VisualShift = Mathf.MoveTowards(VisualShift, run.BodyShift, 2f * Time.deltaTime);
                    run.transform.localPosition += player.transform.InverseTransformVector(Vector3.right * (VisualShift-run.BodyShift));
                    if (blendTime >= .06f && Mathf.Abs(VisualShift-run.BodyShift)<.002f) settling=false;
                }
                return;
            }
            if (player.StumbleDelayRemaining > 0)
            {
                State = "LocalPause"; Pose = -1; painting.enabled = false; run.painting.enabled = true;
                run.painting.sprite = frozen; run.transform.localPosition = frozenPosition;
                previousDelay = player.StumbleDelayRemaining; return;
            }
            float consumedDelay = Mathf.Max(0, previousDelay-player.StumbleDelayRemaining);
            if (!started) Elapsed += Mathf.Max(0, Time.deltaTime-consumedDelay);
            previousDelay = player.StumbleDelayRemaining;
            Pose = Mathf.Clamp(Mathf.FloorToInt(Elapsed/.28f*8),0,7); State = "Recover";
            painting.enabled = true; run.painting.enabled = false;
            painting.sprite = frames[Pose]; painting.flipX = player.RunDirection < 0;
            transform.localRotation = Quaternion.AngleAxis(-player.transform.eulerAngles.y,Vector3.up);
            float drop=0; RaycastHit hit;
            if(player.IsGrounded && reviewSupport.Raycast(new Ray(player.transform.position+Vector3.up*.4f,Vector3.down),out hit,.6f))
                drop=Mathf.Clamp(player.transform.position.y-hit.point.y,0,.1f);
            transform.localPosition=Vector3.down*drop;
            float sign=painting.flipX ? -1:1;
            Vector3 point=new Vector3(sign*(SoleX[Pose]-painting.sprite.pivot.x)/painting.sprite.pixelsPerUnit,0,0);
            var world=transform.TransformPoint(point);
            if(stance!=Pose) { stance=Pose; anchor=world.x+sign*player.RunSpeed*(.28f/8)*.5f; }
            VisualShift=anchor-world.x;
            transform.localPosition+=player.transform.InverseTransformVector(Vector3.right*VisualShift);
            world=transform.TransformPoint(point);ContactX=world.x;ContactY=world.y;
        }
    }
}
