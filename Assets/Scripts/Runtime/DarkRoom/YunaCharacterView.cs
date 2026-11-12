using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public enum YunaStudyPose { Idle, Run, Jump, Fall, Land, Stumble }

    /// <summary>First articulated study: presentation only, never moves the player root or its collider.</summary>
    [DefaultExecutionOrder(200)]
    public class YunaCharacterView : MonoBehaviour
    {
        [SerializeField] float unitScale = 1;
        readonly Dictionary<string, Transform> joints = new Dictionary<string, Transform>();
        readonly Dictionary<string, Vector3> restPositions = new Dictionary<string, Vector3>();
        Yuna25DPlayerController player;
        float phase, landing;
        bool grounded;
        Transform fillLight;
        public YunaStudyPose CurrentPose { get; private set; }
        public float PoseLeanDegrees => joints.TryGetValue("Spine", out var spine)
            ? Mathf.DeltaAngle(0, spine.localEulerAngles.x) : 0;
        public void Configure(float scale) { unitScale = scale; Bind(); }
        void Awake() => Bind();
        void Bind()
        {
            player = GetComponentInParent<Yuna25DPlayerController>();
            if (joints.Count > 0) return;
            foreach (var t in GetComponentsInChildren<Transform>())
            {
                if (t == transform) continue;
                joints[t.name] = t; restPositions[t.name] = t.localPosition;
            }
        }
        void LateUpdate()
        {
            if (player == null) { Bind(); if (player == null) return; }
            UpdatePresentationLight();
            float dt = Time.deltaTime;
            bool onGround = player.IsGrounded;
            if (onGround && !grounded) landing = .14f;
            grounded = onGround; landing = Mathf.Max(0, landing - dt);
            if (player.StumbleDelayRemaining > 0) return; // Freeze the actual gait during the same local 80 ms.
            bool running = player.AutoRunActive && !player.IsDead && Mathf.Abs(player.Velocity.x) > .01f;
            phase = running && onGround ? player.GaitPhase : phase;
            var pose = player.IsStumbling ? YunaStudyPose.Stumble : !onGround && (running || player.IsCinematicFalling)
                ? (player.Velocity.y > 0 ? YunaStudyPose.Jump : YunaStudyPose.Fall)
                : landing > 0 && running ? YunaStudyPose.Land : running ? YunaStudyPose.Run : YunaStudyPose.Idle;
            Evaluate(pose, pose == YunaStudyPose.Idle ? Time.time : phase,
                pose == YunaStudyPose.Stumble ? player.StumblePoseAmount : 1);
        }
        public void UpdatePresentationLight()
        {
            if (player == null) Bind();
            if (player == null) return;
            if (fillLight == null) fillLight = player.transform.Find("Yuna_FillLight");
            // Keep the existing soft fill on the camera's side for either run direction.
            if (fillLight != null) fillLight.position = player.transform.position + new Vector3(-.2f, 1.1f, -1.4f);
        }
        void Rotate(string bone, float x, float y = 0, float z = 0)
        {
            if (joints.TryGetValue(bone, out var t)) t.localRotation = Quaternion.Euler(x, y, z);
        }
        void Position(string bone, Vector3 offset)
        {
            if (joints.TryGetValue(bone, out var t)) t.localPosition = restPositions[bone] + offset * unitScale;
        }
        public void ResetPose()
        {
            Bind(); phase = landing = 0; grounded = false; CurrentPose = YunaStudyPose.Idle;
            foreach (var pair in joints) { pair.Value.localPosition = restPositions[pair.Key]; pair.Value.localRotation = Quaternion.identity; }
        }
        /// <param name="time">Run cycle in [0,1); idle time in seconds.</param>
        public void Evaluate(YunaStudyPose pose, float time, float amount = 1)
        {
            Bind(); if (!joints.ContainsKey("Hips")) return;
            CurrentPose = pose;
            foreach (var pair in joints) { pair.Value.localPosition = restPositions[pair.Key]; pair.Value.localRotation = Quaternion.identity; }
            float cycle = Mathf.Repeat(time, 1), wave = Mathf.Sin(cycle * Mathf.PI * 2);
            float lean = 0, bounce = 0;
            if (pose == YunaStudyPose.Idle)
            {
                Rotate("Spine", Mathf.Sin(time * 1.7f) * .8f); Rotate("Head", 0, Mathf.Sin(time * .8f) * 2);
                Rotate("LeftUpperArm", 0, 0, -3); Rotate("RightUpperArm", 0, 0, 3);
                Rotate("PonytailTip", Mathf.Sin(time * 1.4f) * 2);
                return;
            }
            if (pose == YunaStudyPose.Run || pose == YunaStudyPose.Stumble)
            {
                bounce = -.040f + .004f * Mathf.Cos(cycle * 4 * Mathf.PI);
                Position("Hips", Vector3.up * bounce);
                SolveLeg("Left", cycle); SolveLeg("Right", Mathf.Repeat(cycle + .5f, 1));
                lean = pose == YunaStudyPose.Stumble ? 26 * amount : 7;
                Rotate("LeftUpperArm", -wave * 25 - lean * .8f, 0, -8 - (pose == YunaStudyPose.Stumble ? 24 * amount : 0));
                Rotate("RightUpperArm", wave * 25 - lean * .8f, 0, 8 + (pose == YunaStudyPose.Stumble ? 24 * amount : 0));
                Rotate("LeftLowerArm", -23); Rotate("RightLowerArm", -23);
                Rotate("LeftSkirt", -wave * 12); Rotate("RightSkirt", wave * 12);
                Rotate("FrontSkirt", -8 - Mathf.Abs(wave) * 7); Rotate("BackSkirt", 8 + Mathf.Abs(wave) * 7);
            }
            else
            {
                float knee = pose == YunaStudyPose.Jump ? 58 : pose == YunaStudyPose.Land ? 35 : 15;
                lean = pose == YunaStudyPose.Land ? 16 : 5;
                Position("Hips", Vector3.up * (pose == YunaStudyPose.Land ? -.04f : 0));
                Rotate("LeftUpperLeg", -knee * .6f); Rotate("RightUpperLeg", -knee * .35f);
                Rotate("LeftLowerLeg", knee); Rotate("RightLowerLeg", knee * .8f);
                if (pose == YunaStudyPose.Land)
                {
                    PlaceLeg("Left", .055f, 0, 0);
                    PlaceLeg("Right", -.055f, 0, 0);
                }
                Rotate("LeftUpperArm", -26, 0, -24); Rotate("RightUpperArm", -26, 0, 24);
                Rotate("LeftLowerArm", -20); Rotate("RightLowerArm", -20);
                Rotate("FrontSkirt", -12); Rotate("BackSkirt", 12);
            }
            Rotate("Spine", lean); Rotate("Chest", lean * .15f); Rotate("Head", -lean * .6f);
            Rotate("Ponytail", -lean * .5f + wave * 6); Rotate("PonytailTip", wave * 11);
            Rotate("Bow", wave * 3);
        }
        void SolveLeg(string side, float cycle)
        {
            // Two-bone presentation IK: sole stays at y=0 during stance, foot swings clear during recovery.
            bool stance = cycle < .5f; float t = stance ? cycle * 2 : (cycle - .5f) * 2;
            float z = stance ? Mathf.Lerp(.16f, -.16f, t) : Mathf.Lerp(-.16f, .16f, t);
            float lift = stance ? 0 : .105f * Mathf.Sin(t * Mathf.PI);
            PlaceLeg(side, z, lift, stance ? 0 : -Mathf.Sin(t * Mathf.PI) * 15);
        }
        void PlaceLeg(string side, float z, float lift, float pitch)
        {
            var hip = joints[side + "UpperLeg"]; var shin = joints[side + "LowerLeg"]; var foot = joints[side + "Foot"];
            Vector3 hipPoint = transform.InverseTransformPoint(hip.position);
            var target = new Vector3(hipPoint.x, .095f * unitScale + lift * unitScale, z * unitScale);
            float l1 = restPositions[side + "LowerLeg"].magnitude, l2 = restPositions[side + "Foot"].magnitude;
            var delta = target - hipPoint; float distance = Mathf.Clamp(delta.magnitude, .001f, l1 + l2 - .0001f);
            float along = (l1 * l1 - l2 * l2 + distance * distance) / (2 * distance);
            float bend = Mathf.Sqrt(Mathf.Max(0, l1 * l1 - along * along));
            var direction = delta.normalized; var perpendicular = new Vector3(0, direction.z, -direction.y).normalized;
            var knee = hipPoint + direction * along + perpendicular * bend;
            Quaternion upper = Quaternion.FromToRotation(Vector3.down, knee - hipPoint);
            Quaternion lower = Quaternion.FromToRotation(Vector3.down, target - knee);
            hip.localRotation = upper; shin.localRotation = Quaternion.Inverse(upper) * lower;
            foot.localRotation = Quaternion.Inverse(lower) * Quaternion.Euler(pitch, 0, 0);
        }
    }
}
