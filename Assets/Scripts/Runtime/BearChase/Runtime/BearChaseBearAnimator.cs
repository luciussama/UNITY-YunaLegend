using UnityEngine;
using YunaLegend.Arena.Runtime;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controlador de animacao procedural de alta mobilidade para o Urso Fujão (16 ossos):
    /// dormindo no chao na entrada, acordando assustado com olhos abertos,
    /// e correndo com passada natural, balanco de bracos, inclinacao de tronco e olhar para tras.
    /// </summary>
    public sealed class BearChaseBearAnimator : MonoBehaviour
    {
        private ArenaEnemyRigAnimator.RigBones bones;
        private Vector3 hipsRestPos;
        private bool hasRig;
        private Transform sleepingEyelids;
        private Transform wideEyes;
        private float runCycle;
        private float prevLaneX;

        public void Configure(ArenaEnemyRigAnimator.RigBones rigBones, Transform eyelids, Transform alertEyes)
        {
            bones = rigBones;
            sleepingEyelids = eyelids;
            wideEyes = alertEyes;
            hasRig = bones.Hips != null;
            if (hasRig)
            {
                hipsRestPos = bones.Hips.localPosition;
            }
        }

        public void UpdateBear(
            BearChaseSimulation sim,
            Vector3 entranceSleepPos,
            Vector3 chaseOrigin,
            float dt)
        {
            if (sim == null)
            {
                return;
            }

            if (!hasRig)
            {
                CacheFromHierarchy();
            }

            switch (sim.Phase)
            {
                case BearChasePhase.EntranceWalk:
                case BearChasePhase.EntranceNudge:
                    transform.position = entranceSleepPos + new Vector3(0f, 0.24f, 0f);
                    transform.rotation = Quaternion.Euler(0f, -90f, 0f);
                    SetEyeState(asleep: true);
                    ApplySleepingPose(Time.time);
                    break;

                case BearChasePhase.BearWakeUp:
                case BearChasePhase.TransitionToChase:
                {
                    float wakeT = Mathf.Clamp01(sim.PhaseTimer / BearChaseConfig.BearWakeUpDuration);
                    float lift = Mathf.Lerp(0.24f, 0f, Mathf.Sin(Mathf.Min(1f, wakeT * 1.4f) * Mathf.PI * 0.5f));
                    transform.position = entranceSleepPos + new Vector3(0f, lift, 0f);
                    transform.rotation = Quaternion.Euler(0f, Mathf.Lerp(-90f, -70f, wakeT), 0f);
                    SetEyeState(asleep: false);
                    ApplyWakeUpPose(wakeT);
                    break;
                }

                case BearChasePhase.Chasing:
                case BearChasePhase.Stumble:
                case BearChasePhase.Fallen:
                case BearChasePhase.RetryModal:
                {
                    float trackX = BearChaseConfig.TrackCenterX(sim.BearZ);
                    float worldX = trackX + sim.BearLaneOffsetX;
                    transform.position = chaseOrigin + new Vector3(worldX, 0f, sim.BearZ);

                    float slope = BearChaseConfig.TrackTangentSlopeX(sim.BearZ);
                    float lateralVel = dt > 0.0001f ? (sim.BearLaneOffsetX - prevLaneX) / dt : 0f;
                    prevLaneX = sim.BearLaneOffsetX;

                    float yaw = Mathf.Atan2(slope, 1f) * Mathf.Rad2Deg + Mathf.Clamp(lateralVel * 8f, -18f, 18f);
                    transform.rotation = Quaternion.Euler(0f, yaw, 0f);

                    SetEyeState(asleep: false);
                    runCycle += dt * 11.4f;
                    ApplyNaturalSprintPose(runCycle, sim.BearLookingBack, Mathf.Clamp(lateralVel * 0.25f, -1f, 1f));
                    break;
                }

                case BearChasePhase.Victory:
                {
                    float worldX = BearChaseConfig.TrackCenterX(sim.BearZ) + sim.BearLaneOffsetX * 0.35f;
                    transform.position = chaseOrigin + new Vector3(worldX, 0f, sim.BearZ);
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    SetEyeState(asleep: false);
                    ApplyCaughtPose(Mathf.Clamp01(sim.PhaseTimer / 0.4f));
                    break;
                }
            }
        }

        private void SetEyeState(bool asleep)
        {
            if (sleepingEyelids != null)
            {
                sleepingEyelids.gameObject.SetActive(asleep);
            }

            if (wideEyes != null)
            {
                wideEyes.gameObject.SetActive(!asleep);
            }
        }

        private void ApplySleepingPose(float time)
        {
            if (!hasRig)
            {
                return;
            }

            float breath = Mathf.Sin(time * 2.1f);
            // Deitado de brucos sobre o tapete (acima do piso, sem afundar), respiracao suave
            bones.Hips.localPosition = hipsRestPos + new Vector3(0f, -0.06f, 0f);
            bones.Hips.localRotation = Quaternion.Euler(76f, 0f, 10f);
            bones.Spine.localRotation = Quaternion.Euler(6f + breath * 2.5f, 0f, 0f);
            bones.Chest.localRotation = Quaternion.Euler(4f + breath * 3.0f, 0f, 0f);
            bones.Head.localRotation = Quaternion.Euler(-44f, -16f, 8f);

            bones.UpperArmL.localRotation = Quaternion.Euler(-38f, 12f, -24f);
            bones.ForearmL.localRotation = Quaternion.Euler(-22f, 0f, 0f);
            bones.UpperArmR.localRotation = Quaternion.Euler(-44f, -10f, 22f);
            bones.ForearmR.localRotation = Quaternion.Euler(-28f, 0f, 0f);

            bones.ThighL.localRotation = Quaternion.Euler(-16f, 0f, -14f);
            bones.ShinL.localRotation = Quaternion.Euler(26f, 0f, 0f);
            bones.FootL.localRotation = Quaternion.Euler(14f, 0f, 0f);

            bones.ThighR.localRotation = Quaternion.Euler(-10f, 0f, 14f);
            bones.ShinR.localRotation = Quaternion.Euler(22f, 0f, 0f);
            bones.FootR.localRotation = Quaternion.Euler(14f, 0f, 0f);
        }

        private void ApplyWakeUpPose(float t)
        {
            if (!hasRig)
            {
                return;
            }

            float spring = Mathf.Sin(Mathf.Min(1f, t * 1.4f) * Mathf.PI * 0.5f);
            float startleBounce = Mathf.Sin(t * Mathf.PI) * 0.16f;

            // Sobe para postura alerta e assustada diante da Yuna
            float hipsPitch = Mathf.Lerp(76f, 6f, spring);
            float hipsY = Mathf.Lerp(-0.06f, 0.06f, spring) + startleBounce;
            bones.Hips.localPosition = hipsRestPos + new Vector3(0f, hipsY, 0f);
            bones.Hips.localRotation = Quaternion.Euler(hipsPitch, 0f, Mathf.Lerp(10f, 0f, spring));

            bones.Spine.localRotation = Quaternion.Euler(Mathf.Lerp(6f, 10f, spring), 0f, 0f);
            bones.Chest.localRotation = Quaternion.Euler(Mathf.Lerp(4f, 6f, spring), 0f, 0f);
            bones.Head.localRotation = Quaternion.Euler(Mathf.Lerp(-44f, -12f, spring), Mathf.Lerp(-16f, -8f, spring), 0f);

            bones.UpperArmL.localRotation = Quaternion.Euler(Mathf.Lerp(-38f, -68f, spring), 0f, -26f);
            bones.ForearmL.localRotation = Quaternion.Euler(-38f, 0f, 0f);
            bones.UpperArmR.localRotation = Quaternion.Euler(Mathf.Lerp(-44f, -68f, spring), 0f, 26f);
            bones.ForearmR.localRotation = Quaternion.Euler(-38f, 0f, 0f);

            bones.ThighL.localRotation = Quaternion.Euler(Mathf.Lerp(-16f, -18f, spring), 0f, -6f);
            bones.ShinL.localRotation = Quaternion.Euler(28f, 0f, 0f);
            bones.ThighR.localRotation = Quaternion.Euler(Mathf.Lerp(-10f, -18f, spring), 0f, 6f);
            bones.ShinR.localRotation = Quaternion.Euler(28f, 0f, 0f);
        }

        private void ApplyNaturalSprintPose(float cycle, bool lookingBack, float lateralLean)
        {
            if (!hasRig)
            {
                return;
            }

            float s = Mathf.Sin(cycle);
            float c = Mathf.Cos(cycle);
            float bounce = Mathf.Abs(s) * 0.065f - 0.02f;

            // Alta mobilidade: quadril gira e inclina, tronco acompanha corrida, passadas amplas e joelhos dobrados
            bones.Hips.localPosition = hipsRestPos + new Vector3(0f, bounce, 0f);
            bones.Hips.localRotation = Quaternion.Euler(6f, s * 12f, -lateralLean * 12f + c * 4f);
            bones.Spine.localRotation = Quaternion.Euler(16f + Mathf.Abs(c) * 4f, -s * 8f, 0f);
            bones.Chest.localRotation = Quaternion.Euler(8f, -s * 6f, 0f);

            float headYaw = lookingBack ? 42f : -s * 4f;
            float headPitch = lookingBack ? -12f : -14f + c * 4f;
            bones.Head.localRotation = Quaternion.Euler(headPitch, headYaw, 0f);

            // Pernas com passada ampla e natural
            float thighL = s * 48f;
            float shinL = Mathf.Max(10f, -c * 58f + 24f);
            float footL = -(thighL + shinL) * 0.35f;

            float thighR = -s * 48f;
            float shinR = Mathf.Max(10f, c * 58f + 24f);
            float footR = -(thighR + shinR) * 0.35f;

            bones.ThighL.localRotation = Quaternion.Euler(thighL, 0f, -5f);
            bones.ShinL.localRotation = Quaternion.Euler(shinL, 0f, 0f);
            bones.FootL.localRotation = Quaternion.Euler(footL, 0f, 0f);

            bones.ThighR.localRotation = Quaternion.Euler(thighR, 0f, 5f);
            bones.ShinR.localRotation = Quaternion.Euler(shinR, 0f, 0f);
            bones.FootR.localRotation = Quaternion.Euler(footR, 0f, 0f);

            // Bracos em contra-balanco vigoroso
            bones.UpperArmL.localRotation = Quaternion.Euler(-s * 44f - 20f, 12f, -18f);
            bones.ForearmL.localRotation = Quaternion.Euler(-52f - c * 16f, 0f, 0f);

            bones.UpperArmR.localRotation = Quaternion.Euler(s * 44f - 20f, -12f, 18f);
            bones.ForearmR.localRotation = Quaternion.Euler(-52f + c * 16f, 0f, 0f);
        }

        private void ApplyCaughtPose(float t)
        {
            if (!hasRig)
            {
                return;
            }

            bones.Hips.localPosition = hipsRestPos + new Vector3(0f, -0.08f * t, 0f);
            bones.Hips.localRotation = Quaternion.Euler(0f, 0f, 0f);
            bones.Spine.localRotation = Quaternion.Euler(12f, 0f, 0f);
            bones.Chest.localRotation = Quaternion.Euler(6f, 0f, 0f);
            bones.Head.localRotation = Quaternion.Euler(-10f, 0f, 0f);
            bones.UpperArmL.localRotation = Quaternion.Euler(-55f, 0f, -24f);
            bones.UpperArmR.localRotation = Quaternion.Euler(-55f, 0f, 24f);
            bones.ThighL.localRotation = Quaternion.Euler(-14f, 0f, -6f);
            bones.ShinL.localRotation = Quaternion.Euler(24f, 0f, 0f);
            bones.ThighR.localRotation = Quaternion.Euler(-14f, 0f, 6f);
            bones.ShinR.localRotation = Quaternion.Euler(24f, 0f, 0f);
        }

        private void CacheFromHierarchy()
        {
            Transform rigRoot = transform.Find("RigScaleRoot");
            if (rigRoot == null)
            {
                return;
            }

            Transform hips = rigRoot.Find("Hips");
            if (hips == null)
            {
                return;
            }

            Transform spine = hips.Find("Spine");
            Transform chest = spine != null ? spine.Find("Chest") : null;
            Transform head = chest != null ? chest.Find("Head") : null;
            Transform upperArmL = chest != null ? chest.Find("UpperArm_L") : null;
            Transform forearmL = upperArmL != null ? upperArmL.Find("Forearm_L") : null;
            Transform upperArmR = chest != null ? chest.Find("UpperArm_R") : null;
            Transform forearmR = upperArmR != null ? upperArmR.Find("Forearm_R") : null;
            Transform thighL = hips.Find("Thigh_L");
            Transform shinL = thighL != null ? thighL.Find("Shin_L") : null;
            Transform thighR = hips.Find("Thigh_R");
            Transform shinR = thighR != null ? thighR.Find("Shin_R") : null;

            bones = new ArenaEnemyRigAnimator.RigBones
            {
                Hips = hips,
                Spine = spine,
                Chest = chest,
                Head = head,
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

            hipsRestPos = hips.localPosition;
            hasRig = true;
        }
    }
}
