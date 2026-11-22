using System;
using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Animador procedural do Urso Fujao dourado-mel com laco vermelho:
    /// acompanha o relevo curvo e ondulado em disparada saltitante,
    /// olha para tras e ergue a pata com o brinquedo na telegrafia de arremesso.
    /// </summary>
    public sealed class BearChaseBearAnimator : MonoBehaviour
    {
        [Serializable]
        public struct ThrowPreviewEntry
        {
            public BearChaseObstacleKind Kind;
            public GameObject PreviewObject;
        }

        [SerializeField] private BearChaseRigBones bones;
        [SerializeField] private Vector3 hipsRestPos;
        [SerializeField] private Transform sleepingEyelids;
        [SerializeField] private Transform wideEyes;
        [SerializeField] private Transform throwToyAnchor;
        [SerializeField] private ThrowPreviewEntry[] throwPreviews;
        [SerializeField] private Transform contactShadowTransform;

        private bool hasRig;
        private float runCycle;
        private float prevLaneX;
        private float animClock;
        private float smoothedSpineTwist;
        private float smoothedHeadYaw;
        private float smoothedHeadPitch;
        private float smoothedThrowBlend;
        private float smoothedSteerYaw;
        private float smoothedLateralLean;

        public void Configure(
            BearChaseRigBones rigBones,
            Transform eyelids,
            Transform alertEyes,
            Transform handAnchor,
            ThrowPreviewEntry[] previews,
            Transform shadowTransform)
        {
            bones = rigBones;
            sleepingEyelids = eyelids;
            wideEyes = alertEyes;
            throwToyAnchor = handAnchor;
            throwPreviews = previews;
            contactShadowTransform = shadowTransform;
            hasRig = bones.Hips != null;
            if (hasRig)
            {
                hipsRestPos = bones.Hips.localPosition;
            }

            HideAllThrowPreviews();
        }

        private void Awake()
        {
            if (bones.Hips == null)
            {
                CacheFromHierarchy();
            }

            hasRig = bones.Hips != null;
            HideAllThrowPreviews();
        }

        public void UpdateBear(BearChaseSimulation sim, Vector3 chaseOrigin, float dt)
        {
            if (sim == null || sim.IsEntrancePhase)
            {
                return;
            }

            if (!hasRig)
            {
                CacheFromHierarchy();
            }

            animClock += Mathf.Max(0f, dt);

            float z = sim.BearZ;
            BearChaseTrackSpec track = sim.TrackSpec;
            float trackX = BearChaseTrackSampler.CenterX(z, track);
            float groundY = BearChaseTrackSampler.ElevationY(z, track);
            float yawDeg = BearChaseTrackSampler.YawRadians(z, track) * Mathf.Rad2Deg;
            float pitchDeg = BearChaseTrackSampler.PitchRadians(z, track) * Mathf.Rad2Deg;
            float bankDeg = BearChaseTrackSampler.BankDegrees(z, track);

            float worldX = trackX + sim.BearLaneOffsetX;
            Vector3 groundPos = chaseOrigin + new Vector3(worldX, groundY, z);
            transform.position = groundPos;

            SetEyeState(asleep: false);

            if (sim.Phase == BearChasePhase.Victory)
            {
                transform.rotation = Quaternion.Euler(0f, yawDeg + 180f, 0f);
                HideAllThrowPreviews();
                ApplyCaughtPose();
                UpdateContactShadow(groundPos, pitchDeg, yawDeg, 1f);
                return;
            }

            float lateralVel = dt > 0.0001f ? (sim.BearLaneOffsetX - prevLaneX) / dt : 0f;
            prevLaneX = sim.BearLaneOffsetX;

            float damp = dt > 0f ? 1f - Mathf.Exp(-9.5f * dt) : 1f;
            float targetSteerYaw = Mathf.Clamp(lateralVel * 5.0f, -14f, 14f);
            float targetLean = Mathf.Clamp(lateralVel * 0.22f, -0.8f, 0.8f);
            smoothedSteerYaw = Mathf.Lerp(smoothedSteerYaw, targetSteerYaw, damp);
            smoothedLateralLean = Mathf.Lerp(smoothedLateralLean, targetLean, damp);

            transform.rotation = Quaternion.Euler(pitchDeg, yawDeg + smoothedSteerYaw, bankDeg);

            float speedRate = Mathf.Clamp(sim.CurrentForwardSpeed * 0.92f, 8.6f, 12.6f);
            runCycle += dt * speedRate;

            bool isThrowing = sim.BearAction == BearChaseBearAction.ThrowingToy;
            UpdateThrowPreview(isThrowing, sim.BearPendingThrowKind);

            float hopScale = ApplySmoothSprintPose(
                runCycle,
                sim.BearLookingBack,
                isThrowing,
                sim.BearThrowWindupProgress,
                smoothedLateralLean,
                damp);

            UpdateContactShadow(groundPos, pitchDeg, yawDeg, hopScale);
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

        private float ApplySmoothSprintPose(
            float cycle,
            bool lookingBack,
            bool throwing,
            float throwWindup,
            float lateralLean,
            float blend)
        {
            if (!hasRig)
            {
                return 1f;
            }

            float s = Mathf.Sin(cycle);
            float c = Mathf.Cos(cycle);
            float absS = Mathf.Abs(s);
            float bounce = absS * 0.090f + 0.012f;

            if (bones.Hips != null)
            {
                bones.Hips.localPosition = hipsRestPos + new Vector3(0f, bounce, 0f);
                bones.Hips.localRotation = Quaternion.Euler(9f, s * 8f, -lateralLean * 10f + c * 3.5f);
            }

            float targetSpineTwist = throwing ? 14f : (lookingBack ? 10f : -s * 5f);
            float targetHeadYaw = throwing ? 48f : (lookingBack ? 36f : -s * 3.5f);
            float targetHeadPitch = (lookingBack || throwing) ? -5f : -9f + c * 3.5f;
            float targetThrowBlend = throwing ? 1f : 0f;

            smoothedSpineTwist = Mathf.Lerp(smoothedSpineTwist, targetSpineTwist, blend);
            smoothedHeadYaw = Mathf.Lerp(smoothedHeadYaw, targetHeadYaw, blend);
            smoothedHeadPitch = Mathf.Lerp(smoothedHeadPitch, targetHeadPitch, blend);
            smoothedThrowBlend = Mathf.Lerp(smoothedThrowBlend, targetThrowBlend, blend);

            SetLocalRotation(bones.Spine, 3f + absS * 1.5f, smoothedSpineTwist * 0.55f, 0f);
            SetLocalRotation(bones.Chest, 2.5f, smoothedSpineTwist * 0.45f, 0f);
            SetLocalRotation(bones.Head, smoothedHeadPitch, smoothedHeadYaw, 0f);

            float thighL = s * 32f;
            float shinL = Mathf.Max(8f, -c * 38f + 16f);
            float footL = -(thighL + shinL) * 0.35f;

            float thighR = -s * 32f;
            float shinR = Mathf.Max(8f, c * 38f + 16f);
            float footR = -(thighR + shinR) * 0.35f;

            SetLocalRotation(bones.ThighL, thighL, 0f, -7f);
            SetLocalRotation(bones.ShinL, shinL, 0f, 0f);
            SetLocalRotation(bones.FootL, footL, 0f, 0f);

            SetLocalRotation(bones.ThighR, thighR, 0f, 7f);
            SetLocalRotation(bones.ShinR, shinR, 0f, 0f);
            SetLocalRotation(bones.FootR, footR, 0f, 0f);

            SetLocalRotation(bones.UpperArmL, -s * 28f - 4f, 6f, -38f);
            SetLocalRotation(bones.ForearmL, -26f - c * 8f, 0f, -10f);

            float runArmRx = s * 28f - 4f;
            float runForearmRx = -26f + c * 8f;
            float cockBack = Mathf.Sin(throwWindup * Mathf.PI * 0.72f);
            float throwArmRx = Mathf.Lerp(-74f, -124f, cockBack);
            float throwForearmRx = Mathf.Lerp(-24f, -44f, cockBack);

            SetLocalRotation(
                bones.UpperArmR,
                Mathf.Lerp(runArmRx, throwArmRx, smoothedThrowBlend),
                Mathf.Lerp(-6f, 20f, smoothedThrowBlend),
                Mathf.Lerp(38f, 48f, smoothedThrowBlend));
            SetLocalRotation(
                bones.ForearmR,
                Mathf.Lerp(runForearmRx, throwForearmRx, smoothedThrowBlend),
                0f,
                Mathf.Lerp(10f, 10f, smoothedThrowBlend));

            return Mathf.Lerp(1.04f, 0.80f, absS);
        }

        private void ApplyCaughtPose()
        {
            if (!hasRig)
            {
                return;
            }

            if (bones.Hips != null)
            {
                float breath = Mathf.Sin(animClock * 4f) * 0.02f;
                bones.Hips.localPosition = hipsRestPos + new Vector3(0f, -0.06f + breath, 0f);
                bones.Hips.localRotation = Quaternion.identity;
            }

            SetLocalRotation(bones.Spine, 10f, 0f, 0f);
            SetLocalRotation(bones.Chest, 6f, 0f, 0f);
            SetLocalRotation(bones.Head, -10f, 0f, 0f);
            SetLocalRotation(bones.UpperArmL, -55f, 0f, -24f);
            SetLocalRotation(bones.UpperArmR, -55f, 0f, 24f);
            SetLocalRotation(bones.ThighL, -14f, 0f, -6f);
            SetLocalRotation(bones.ShinL, 24f, 0f, 0f);
            SetLocalRotation(bones.ThighR, -14f, 0f, 6f);
            SetLocalRotation(bones.ShinR, 24f, 0f, 0f);
        }

        private void UpdateThrowPreview(bool visible, BearChaseObstacleKind kind)
        {
            if (throwToyAnchor != null)
            {
                throwToyAnchor.gameObject.SetActive(visible);
            }

            if (throwPreviews == null)
            {
                return;
            }

            for (int i = 0; i < throwPreviews.Length; i++)
            {
                if (throwPreviews[i].PreviewObject != null)
                {
                    throwPreviews[i].PreviewObject.SetActive(visible && throwPreviews[i].Kind == kind);
                }
            }
        }

        private void HideAllThrowPreviews()
        {
            if (throwToyAnchor != null)
            {
                throwToyAnchor.gameObject.SetActive(false);
            }

            if (throwPreviews == null)
            {
                return;
            }

            for (int i = 0; i < throwPreviews.Length; i++)
            {
                if (throwPreviews[i].PreviewObject != null)
                {
                    throwPreviews[i].PreviewObject.SetActive(false);
                }
            }
        }

        private void UpdateContactShadow(Vector3 groundPos, float pitchDeg, float yawDeg, float scaleMul)
        {
            if (contactShadowTransform == null)
            {
                return;
            }

            contactShadowTransform.position = groundPos + new Vector3(0f, 0.026f, 0f);
            contactShadowTransform.rotation = Quaternion.Euler(pitchDeg + 90f, yawDeg, 0f);
            contactShadowTransform.localScale = new Vector3(1.18f * scaleMul, 1.18f * scaleMul, 1f);
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

            bones = new BearChaseRigBones
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

            if (head != null)
            {
                if (sleepingEyelids == null)
                {
                    sleepingEyelids = head.Find("SleepingEyelids");
                }

                if (wideEyes == null)
                {
                    wideEyes = head.Find("WideEyes");
                }
            }

            hipsRestPos = hips.localPosition;
            hasRig = true;
        }

        private static void SetLocalRotation(Transform target, float x, float y, float z)
        {
            if (target != null)
            {
                target.localRotation = Quaternion.Euler(x, y, z);
            }
        }
    }
}
