using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Adaptador fino da Yuna 3D na corrida da Fase 4:
    /// acompanha curvas horizontais, elevacao e inclinacao do terreno em alta velocidade,
    /// sem espada atravancando a silhueta, com sombra de contato e animacao expressiva de tropeço.
    /// </summary>
    public sealed class BearChasePlayerController : MonoBehaviour
    {
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform ponytailPivot;
        [SerializeField] private BearChaseRigBones rig;
        [SerializeField] private Vector3 baseHipsPos;
        [SerializeField] private Transform contactShadowTransform;
        [SerializeField] private ParticleSystem footstepDust;

        private bool hasRig;
        private float sprintCycle;
        private float smoothedInput;
        private float animClock;
        private Renderer[] renderers;

        public void Configure(
            Transform body,
            Transform ponytail,
            BearChaseRigBones rigBones,
            Transform contactShadow,
            ParticleSystem dustParticles)
        {
            bodyRoot = body;
            ponytailPivot = ponytail;
            rig = rigBones;
            contactShadowTransform = contactShadow;
            footstepDust = dustParticles;
            hasRig = rig.IsValid;
            if (hasRig)
            {
                baseHipsPos = rig.Hips.localPosition;
            }
        }

        private void Awake()
        {
            if (!rig.IsValid)
            {
                CacheRigFromHierarchy();
            }

            hasRig = rig.IsValid;
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void UpdatePlayer(BearChaseSimulation sim, Vector3 chaseOrigin, float dt)
        {
            if (sim == null || sim.IsEntrancePhase)
            {
                return;
            }

            if (!hasRig)
            {
                CacheRigFromHierarchy();
            }

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }

            animClock += Mathf.Max(0f, dt);

            bool blinkOff = sim.InvulnerabilityRemaining > 0f
                && sim.Phase != BearChasePhase.Fallen
                && Mathf.FloorToInt(sim.InvulnerabilityRemaining * 15f) % 2 == 1;
            SetRenderersVisible(!blinkOff);

            float z = sim.YunaZ;
            BearChaseTrackSpec track = sim.TrackSpec;
            float centerX = BearChaseTrackSampler.CenterX(z, track);
            float groundY = BearChaseTrackSampler.ElevationY(z, track);
            float yawDeg = BearChaseTrackSampler.YawRadians(z, track) * Mathf.Rad2Deg;
            float pitchDeg = BearChaseTrackSampler.PitchRadians(z, track) * Mathf.Rad2Deg;
            float bankDeg = BearChaseTrackSampler.BankDegrees(z, track);

            Vector3 groundPos = chaseOrigin + new Vector3(centerX + sim.YunaLaneOffsetX, groundY, z);
            transform.position = groundPos;

            switch (sim.Phase)
            {
                case BearChasePhase.Chasing:
                case BearChasePhase.Stumble:
                {
                    float blend = 1f - Mathf.Exp(-14f * Mathf.Max(0f, dt));
                    smoothedInput = Mathf.Lerp(smoothedInput, sim.YunaLateralInput, blend);
                    transform.rotation = Quaternion.Euler(pitchDeg, yawDeg + smoothedInput * 25f, bankDeg);

                    bool stumbling = sim.Phase == BearChasePhase.Stumble;
                    float cycleRate = Mathf.Max(12f, sim.CurrentForwardSpeed * 1.92f);
                    sprintCycle += dt * (stumbling ? cycleRate * 0.72f : cycleRate);
                    float lean = stumbling ? 30f : 22f;
                    float roll = -smoothedInput * 16f + (stumbling ? Mathf.Sin(animClock * 28f) * 14f : 0f);

                    ApplySprintPose(sprintCycle, lean, roll, stumbling);
                    UpdateDustEmission(!stumbling && sim.CurrentForwardSpeed > 6f);
                    UpdateContactShadow(groundPos, pitchDeg, yawDeg, 1f);
                    break;
                }

                case BearChasePhase.Fallen:
                case BearChasePhase.RetryModal:
                {
                    transform.rotation = Quaternion.Euler(pitchDeg, yawDeg, 0f);
                    ApplyFallenPose(Mathf.Clamp01(sim.PhaseTimer / 0.32f));
                    UpdateDustEmission(false);
                    UpdateContactShadow(groundPos, pitchDeg, yawDeg, 1.25f);
                    break;
                }

                case BearChasePhase.Victory:
                {
                    transform.rotation = Quaternion.Euler(pitchDeg, yawDeg, 0f);
                    ApplyVictoryPose();
                    UpdateDustEmission(false);
                    UpdateContactShadow(groundPos, pitchDeg, yawDeg, 1f);
                    break;
                }
            }
        }

        private void ApplySprintPose(float cycle, float forwardLean, float bankRoll, bool stumbling)
        {
            if (!hasRig)
            {
                return;
            }

            float s = Mathf.Sin(cycle);
            float c = Mathf.Cos(cycle);
            float absS = Mathf.Abs(s);

            if (rig.Hips != null)
            {
                float hop = absS * 0.068f - 0.012f;
                rig.Hips.localPosition = baseHipsPos + new Vector3(0f, hop, 0f);
                rig.Hips.localRotation = Quaternion.Euler(forwardLean * 0.25f, s * 9f, bankRoll);
            }

            if (rig.Spine != null)
            {
                rig.Spine.localRotation = Quaternion.Euler(forwardLean * 0.65f, -s * 7f, stumbling ? c * 8f : 0f);
            }

            if (rig.Chest != null)
            {
                rig.Chest.localRotation = Quaternion.Euler(forwardLean * 0.35f, -s * 5f, 0f);
            }

            if (rig.Head != null)
            {
                rig.Head.localRotation = Quaternion.Euler(-forwardLean * 0.62f, 0f, -bankRoll * 0.35f);
            }

            float thighL = s * 54f;
            float shinL = Mathf.Max(8f, -c * 58f + 26f);
            float footL = -(thighL + shinL) * 0.38f;

            float thighR = -s * 54f;
            float shinR = Mathf.Max(8f, c * 58f + 26f);
            float footR = -(thighR + shinR) * 0.38f;

            SetLocalRotation(rig.ThighL, thighL, 0f, 0f);
            SetLocalRotation(rig.ShinL, shinL, 0f, 0f);
            SetLocalRotation(rig.FootL, footL, 0f, 0f);

            SetLocalRotation(rig.ThighR, thighR, 0f, 0f);
            SetLocalRotation(rig.ShinR, shinR, 0f, 0f);
            SetLocalRotation(rig.FootR, footR, 0f, 0f);

            if (stumbling)
            {
                SetLocalRotation(rig.UpperArmL, -46f + s * 22f, 0f, 52f);
                SetLocalRotation(rig.ForearmL, -25f, 0f, 0f);
                SetLocalRotation(rig.UpperArmR, -46f - s * 22f, 0f, -52f);
                SetLocalRotation(rig.ForearmR, -25f, 0f, 0f);
            }
            else
            {
                SetLocalRotation(rig.UpperArmL, -s * 52f - 8f, 0f, 22f);
                SetLocalRotation(rig.ForearmL, -74f + c * 18f, 0f, 0f);
                SetLocalRotation(rig.UpperArmR, s * 52f - 8f, 0f, -22f);
                SetLocalRotation(rig.ForearmR, -74f - c * 18f, 0f, 0f);
            }

            if (ponytailPivot != null)
            {
                ponytailPivot.localRotation = Quaternion.Euler(26f + absS * 18f, 0f, -s * 12f - bankRoll * 0.4f);
            }
        }

        private void ApplyFallenPose(float t)
        {
            if (!hasRig)
            {
                return;
            }

            if (rig.Hips != null)
            {
                rig.Hips.localPosition = baseHipsPos + new Vector3(0f, Mathf.Lerp(0f, -0.46f, t), 0f);
                rig.Hips.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 82f, t), 0f, 0f);
            }

            SetLocalRotation(rig.Spine, 10f, 0f, 0f);
            SetLocalRotation(rig.Head, -24f, 18f, 0f);
            SetLocalRotation(rig.UpperArmL, -70f, 0f, -25f);
            SetLocalRotation(rig.UpperArmR, -55f, 0f, 28f);
            SetLocalRotation(rig.ThighL, -12f, 0f, -6f);
            SetLocalRotation(rig.ThighR, 8f, 0f, 6f);
        }

        private void ApplyVictoryPose()
        {
            if (!hasRig)
            {
                return;
            }

            float bounce = Mathf.Abs(Mathf.Sin(animClock * 5f)) * 0.04f;
            if (rig.Hips != null)
            {
                rig.Hips.localPosition = baseHipsPos + new Vector3(0f, bounce, 0f);
                rig.Hips.localRotation = Quaternion.identity;
            }

            SetLocalRotation(rig.Spine, -4f, 0f, 0f);
            SetLocalRotation(rig.Chest, 0f, 0f, 0f);
            SetLocalRotation(rig.Head, -8f, 0f, 0f);
            SetLocalRotation(rig.UpperArmL, -125f, 0f, 28f);
            SetLocalRotation(rig.UpperArmR, -125f, 0f, -28f);
            SetLocalRotation(rig.ThighL, 0f, 0f, 0f);
            SetLocalRotation(rig.ShinL, 0f, 0f, 0f);
            SetLocalRotation(rig.ThighR, 0f, 0f, 0f);
            SetLocalRotation(rig.ShinR, 0f, 0f, 0f);
        }

        private void UpdateContactShadow(Vector3 groundPos, float pitchDeg, float yawDeg, float scaleMul)
        {
            if (contactShadowTransform == null)
            {
                return;
            }

            contactShadowTransform.position = groundPos + new Vector3(0f, 0.024f, 0f);
            contactShadowTransform.rotation = Quaternion.Euler(pitchDeg + 90f, yawDeg, 0f);
            contactShadowTransform.localScale = new Vector3(0.95f * scaleMul, 0.95f * scaleMul, 1f);
        }

        private void UpdateDustEmission(bool active)
        {
            if (footstepDust == null)
            {
                return;
            }

            ParticleSystem.EmissionModule emission = footstepDust.emission;
            emission.enabled = active;
        }

        private void CacheRigFromHierarchy()
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

            rig = new BearChaseRigBones
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
            if (hasRig && rig.Hips != null)
            {
                baseHipsPos = rig.Hips.localPosition;
            }
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

        private static void SetLocalRotation(Transform target, float x, float y, float z)
        {
            if (target != null)
            {
                target.localRotation = Quaternion.Euler(x, y, z);
            }
        }
    }
}
