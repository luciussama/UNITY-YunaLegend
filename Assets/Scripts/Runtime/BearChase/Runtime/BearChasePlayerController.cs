using UnityEngine;
using YunaLegend.BearChase.Core;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controla a pose e animacao procedural da Yuna 3D na Fase 4:
    /// caminhada lateral na entrada, toque no urso, corrida com desvio lateral,
    /// tropeço, queda e comemoracao.
    /// </summary>
    public sealed class BearChasePlayerController : MonoBehaviour
    {
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform legLeftPivot;
        [SerializeField] private Transform legRightPivot;
        [SerializeField] private Transform armLeftPivot;
        [SerializeField] private Transform armRightPivot;
        [SerializeField] private Transform ponytailPivot;

        private YunaToyRoomAnimator.RigBones rig;
        private Vector3 baseHipsPos;
        private bool hasRig;
        private float walkCycle;
        private float smoothedInput;
        private Renderer[] renderers;

        public void Configure(
            Transform body,
            Transform legL,
            Transform legR,
            Transform armL,
            Transform armR,
            Transform ponytail)
        {
            bodyRoot = body;
            legLeftPivot = legL;
            legRightPivot = legR;
            armLeftPivot = armL;
            armRightPivot = armR;
            ponytailPivot = ponytail;
            CacheRig();
        }

        private void Awake()
        {
            CacheRig();
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void CacheRig()
        {
            if (bodyRoot == null)
            {
                return;
            }

            Transform rigRoot = bodyRoot.Find("Yuna_RigRoot");
            if (rigRoot == null)
            {
                return;
            }

            Transform hips = rigRoot.Find("Hips");
            Transform spine = hips != null ? hips.Find("Spine") : null;
            Transform chest = spine != null ? spine.Find("Chest") : null;
            Transform neck = chest != null ? chest.Find("Neck") : null;
            Transform upperArmL = chest != null ? chest.Find("UpperArm_L") : null;
            Transform forearmL = upperArmL != null ? upperArmL.Find("Forearm_L") : null;
            Transform upperArmR = chest != null ? chest.Find("UpperArm_R") : null;
            Transform forearmR = upperArmR != null ? upperArmR.Find("Forearm_R") : null;
            Transform thighL = hips != null ? hips.Find("Thigh_L") : null;
            Transform shinL = thighL != null ? thighL.Find("Shin_L") : null;
            Transform thighR = hips != null ? hips.Find("Thigh_R") : null;
            Transform shinR = thighR != null ? thighR.Find("Shin_R") : null;

            rig = new YunaToyRoomAnimator.RigBones
            {
                Hips = hips,
                Spine = spine,
                Chest = chest,
                Neck = neck,
                Head = neck != null ? neck.Find("Head") : null,
                UpperArmL = upperArmL,
                ForearmL = forearmL,
                HandL = forearmL != null ? forearmL.Find("Hand_L") : null,
                UpperArmR = upperArmR,
                ForearmR = forearmR,
                HandR = forearmR != null ? forearmR.Find("Hand_R") : null,
                ThighL = thighL,
                ShinL = shinL,
                FootL = shinL != null ? shinL.Find("Foot_L") : null,
                ThighR = thighR,
                ShinR = shinR,
                FootR = shinR != null ? shinR.Find("Foot_R") : null
            };

            hasRig = rig.IsValid;
            if (hasRig)
            {
                baseHipsPos = rig.Hips.localPosition;
            }
        }

        public void UpdatePlayer(BearChaseSimulation sim, Vector3 entranceStartPos, Vector3 entranceNudgePos, Vector3 chaseOrigin, float dt)
        {
            if (sim == null)
            {
                return;
            }

            if (!hasRig)
            {
                CacheRig();
            }

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }

            bool blinkOff = sim.InvulnerabilityRemaining > 0f
                && sim.Phase != BearChasePhase.Fallen
                && Mathf.FloorToInt(sim.InvulnerabilityRemaining * 14f) % 2 == 1;
            SetRenderersVisible(!blinkOff);

            switch (sim.Phase)
            {
                case BearChasePhase.EntranceWalk:
                {
                    float t = Mathf.Clamp01(sim.PhaseTimer / BearChaseConfig.EntranceWalkDuration);
                    transform.position = Vector3.Lerp(entranceStartPos, entranceNudgePos, t);
                    transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    walkCycle += dt * 7.5f;
                    ApplyLocomotionPose(walkCycle, strideAmp: 0.72f, forwardLean: 6f, bankRoll: 0f, reachLeftArm: 0f, holdingSword: true);
                    break;
                }

                case BearChasePhase.EntranceNudge:
                {
                    transform.position = entranceNudgePos;
                    transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    float reach = Mathf.Sin(Mathf.Clamp01(sim.PhaseTimer / BearChaseConfig.EntranceNudgeDuration) * Mathf.PI);
                    ApplyLocomotionPose(0f, strideAmp: 0f, forwardLean: 14f * reach, bankRoll: 0f, reachLeftArm: reach, holdingSword: true);
                    break;
                }

                case BearChasePhase.BearWakeUp:
                case BearChasePhase.TransitionToChase:
                {
                    transform.position = entranceNudgePos;
                    transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    ApplyLocomotionPose(0f, strideAmp: 0f, forwardLean: -4f, bankRoll: 0f, reachLeftArm: 0f, holdingSword: true);
                    break;
                }

                case BearChasePhase.Chasing:
                case BearChasePhase.Stumble:
                {
                    float worldX = BearChaseConfig.TrackCenterX(sim.YunaZ) + sim.YunaLaneOffsetX;
                    transform.position = chaseOrigin + new Vector3(worldX, 0f, sim.YunaZ);

                    smoothedInput = Mathf.Lerp(smoothedInput, sim.YunaLateralInput, Mathf.Min(1f, dt * 12f));
                    float slope = BearChaseConfig.TrackTangentSlopeX(sim.YunaZ);
                    float yaw = Mathf.Atan2(slope, 1f) * Mathf.Rad2Deg + smoothedInput * 22f;
                    transform.rotation = Quaternion.Euler(0f, yaw, 0f);

                    float speedMul = sim.Phase == BearChasePhase.Stumble ? 0.55f : 1.0f;
                    walkCycle += dt * 11.8f * speedMul;
                    float lean = sim.Phase == BearChasePhase.Stumble ? 22f : 12f;
                    ApplyLocomotionPose(walkCycle, strideAmp: 1.0f, forwardLean: lean, bankRoll: -smoothedInput * 10f, reachLeftArm: 0f, holdingSword: true);
                    break;
                }

                case BearChasePhase.Fallen:
                case BearChasePhase.RetryModal:
                {
                    float worldX = BearChaseConfig.TrackCenterX(sim.YunaZ) + sim.YunaLaneOffsetX;
                    transform.position = chaseOrigin + new Vector3(worldX, 0f, sim.YunaZ);
                    transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(BearChaseConfig.TrackTangentSlopeX(sim.YunaZ), 1f) * Mathf.Rad2Deg, 0f);
                    ApplyFallenPose(Mathf.Clamp01(sim.PhaseTimer / 0.35f));
                    break;
                }

                case BearChasePhase.Victory:
                {
                    float worldX = BearChaseConfig.TrackCenterX(sim.YunaZ) + sim.YunaLaneOffsetX;
                    transform.position = chaseOrigin + new Vector3(worldX, 0f, sim.YunaZ);
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    ApplyLocomotionPose(0f, strideAmp: 0f, forwardLean: 0f, bankRoll: 0f, reachLeftArm: 0.45f, holdingSword: true);
                    break;
                }
            }
        }

        private void ApplyLocomotionPose(float cycle, float strideAmp, float forwardLean, float bankRoll, float reachLeftArm, bool holdingSword)
        {
            float s = Mathf.Sin(cycle);
            float c = Mathf.Cos(cycle);
            float absS = Mathf.Abs(s);

            if (hasRig)
            {
                rig.Hips.localPosition = baseHipsPos + new Vector3(0f, absS * 0.032f * strideAmp, 0f);
                rig.Hips.localRotation = Quaternion.Euler(0f, s * 6f * strideAmp, bankRoll);
                rig.Spine.localRotation = Quaternion.Euler(forwardLean, -s * 4f * strideAmp, 0f);
                rig.Chest.localRotation = Quaternion.Euler(forwardLean * 0.35f, 0f, 0f);
                rig.Head.localRotation = Quaternion.Euler(-forwardLean * 0.55f, 0f, 0f);

                float thighL = s * 38f * strideAmp;
                float shinL = Mathf.Max(4f, -c * 42f + 12f) * strideAmp;
                float thighR = -s * 38f * strideAmp;
                float shinR = Mathf.Max(4f, c * 42f + 12f) * strideAmp;

                rig.ThighL.localRotation = Quaternion.Euler(thighL, 0f, 0f);
                rig.ShinL.localRotation = Quaternion.Euler(shinL, 0f, 0f);
                rig.FootL.localRotation = Quaternion.Euler(-(thighL + shinL) * 0.3f, 0f, 0f);

                rig.ThighR.localRotation = Quaternion.Euler(thighR, 0f, 0f);
                rig.ShinR.localRotation = Quaternion.Euler(shinR, 0f, 0f);
                rig.FootR.localRotation = Quaternion.Euler(-(thighR + shinR) * 0.3f, 0f, 0f);

                float armLPitch = Mathf.Lerp(-s * 34f * strideAmp, -68f, reachLeftArm);
                rig.UpperArmL.localRotation = Quaternion.Euler(armLPitch, 0f, 24f);
                rig.ForearmL.localRotation = Quaternion.Euler(Mathf.Lerp(-42f, -10f, reachLeftArm), 0f, 0f);

                float armRPitch = holdingSword ? (s * 18f * strideAmp - 32f) : (s * 34f * strideAmp);
                rig.UpperArmR.localRotation = Quaternion.Euler(armRPitch, 0f, -24f);
                rig.ForearmR.localRotation = Quaternion.Euler(holdingSword ? -48f : -42f, 0f, 0f);
            }
            else
            {
                if (bodyRoot != null)
                {
                    bodyRoot.localPosition = new Vector3(0f, absS * 0.035f * strideAmp, 0f);
                    bodyRoot.localRotation = Quaternion.Euler(forwardLean, s * 5f * strideAmp, bankRoll);
                }

                if (legLeftPivot != null)
                {
                    legLeftPivot.localRotation = Quaternion.Euler(s * 36f * strideAmp, 0f, 0f);
                }

                if (legRightPivot != null)
                {
                    legRightPivot.localRotation = Quaternion.Euler(-s * 36f * strideAmp, 0f, 0f);
                }

                if (armLeftPivot != null)
                {
                    float armLPitch = Mathf.Lerp(-s * 34f * strideAmp, -64f, reachLeftArm);
                    armLeftPivot.localRotation = Quaternion.Euler(armLPitch, 0f, -12f);
                }

                if (armRightPivot != null)
                {
                    float armRPitch = holdingSword ? (s * 16f * strideAmp - 28f) : (s * 34f * strideAmp);
                    armRightPivot.localRotation = Quaternion.Euler(armRPitch, 0f, 12f);
                }
            }

            if (ponytailPivot != null)
            {
                ponytailPivot.localRotation = Quaternion.Euler(14f + absS * 12f * strideAmp, 0f, -s * 8f * strideAmp);
            }
        }

        private void ApplyFallenPose(float t)
        {
            if (!hasRig)
            {
                if (bodyRoot != null)
                {
                    bodyRoot.localPosition = new Vector3(0f, Mathf.Lerp(0f, -0.42f, t), 0f);
                    bodyRoot.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 82f, t), 0f, 0f);
                }

                return;
            }

            rig.Hips.localPosition = baseHipsPos + new Vector3(0f, Mathf.Lerp(0f, -0.46f, t), 0f);
            rig.Hips.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 82f, t), 0f, 0f);
            rig.Spine.localRotation = Quaternion.Euler(10f, 0f, 0f);
            rig.Head.localRotation = Quaternion.Euler(-25f, 18f, 0f);
            rig.UpperArmL.localRotation = Quaternion.Euler(-70f, 0f, -25f);
            rig.UpperArmR.localRotation = Quaternion.Euler(-55f, 0f, 28f);
            rig.ThighL.localRotation = Quaternion.Euler(-12f, 0f, -6f);
            rig.ThighR.localRotation = Quaternion.Euler(8f, 0f, 6f);
        }

        private void SetRenderersVisible(bool visible)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = visible;
                }
            }
        }
    }
}
