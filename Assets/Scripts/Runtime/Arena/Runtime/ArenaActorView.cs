using UnityEngine;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Apresentacao 3D Painterly da Yuna (com Espada de Brinquedo, arco luminoso e feixe Arco-Iris)
    /// e dos 4 brinquedos adversarios (Trem, Robo, Dinossauro e Urso) na Arena Imaginaria.
    /// </summary>
    public sealed class ArenaActorView : MonoBehaviour
    {
        [Header("Base")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private ParticleSystem impactParticles;

        [Header("Yuna 3D Rig")]
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform legLeftPivot;
        [SerializeField] private Transform legRightPivot;
        [SerializeField] private Transform armLeftPivot;
        [SerializeField] private Transform armRightPivot;
        [SerializeField] private Transform forearmLeftPivot;
        [SerializeField] private Transform forearmRightPivot;
        [SerializeField] private Transform twoHandSwordPivot;
        [SerializeField] private Transform ponytailPivot;
        [SerializeField] private GameObject slashArcObject;
        [SerializeField] private GameObject rainbowBeamObject;

        [Header("Inimigos 3D")]
        [SerializeField] private GameObject trainModel;
        [SerializeField] private GameObject robotModel;
        [SerializeField] private GameObject dinosaurModel;
        [SerializeField] private GameObject bearModel;
        [SerializeField] private ParticleSystem steamBlastVfx;
        [SerializeField] private GameObject laserBeamObject;
        [SerializeField] private Light bearEnrageLight;

        [Header("Fallback Sprite (opcional)")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private float animTimer;
        private float hitFlashTimer;
        private float slashFadeTimer;
        private float visualComboTimer;
        private int activeVisualComboStep;
        private int lastSeenSwordTick;
        private Vector3 baseScale = Vector3.one;
        private Vector3 bodyBaseLocalPos;
        private Transform swordGlintFlare;
        private Transform gripTargetR;
        private Transform gripTargetL;
        private Transform guardTargetL;
        private Transform hipsBone;
        private Transform spineBone;
        private Transform chestBone;
        private Transform neckBone;
        private Transform headBone;
        private Transform shinLeftBone;
        private Transform footLeftBone;
        private Transform shinRightBone;
        private Transform footRightBone;
        private Transform handLeftBone;
        private Transform handRightBone;
        private bool rigBonesResolved;
        private GameObject steamJetObject;
        private ParticleSystem chargeSparksVfx;
        private Light trainHeadlight;
        private GameObject rocketPunchObject;
        private GameObject impactFlashObject;
        private Light impactFlashLight;
        private float impactFlashTimer;
        private float impactFlashSpin;
        private GameObject laserSightObject;
        private GameObject laserSightDotObject;
        private Light laserPointLight;
        private ArenaEnemyRigAnimator robotRigAnimator;
        private ArenaEnemyRigAnimator dinosaurRigAnimator;
        private ArenaEnemyRigAnimator bearRigAnimator;

        private const float VisualComboDuration = 0.30f;
        private const float ImpactFlashDuration = 0.20f;

        private struct MartialPose
        {
            public float BodyYOffset;
            public float BodyZOffset;
            public float HipsPitch;
            public float HipsYaw;
            public float SpinePitch;
            public float SpineYaw;
            public float ChestPitch;
            public float ChestYaw;
            public float HeadPitch;
            public float HeadYaw;
            public float ThighLPitch;
            public float ShinLPitch;
            public float FootLPitch;
            public float ThighRPitch;
            public float ShinRPitch;
            public float FootRPitch;
            public Vector3 SwordLocalPos;
            public Vector3 SwordLocalEuler;
        }

        public void Configure(
            SpriteRenderer renderer,
            Transform root,
            ParticleSystem particles = null,
            SpriteRenderer slashRenderer = null)
        {
            spriteRenderer = renderer;
            visualRoot = root;
            impactParticles = particles;
            _ = slashRenderer;
            if (visualRoot != null)
            {
                baseScale = visualRoot.localScale;
            }
        }

        public void ConfigurePlayer3D(
            Transform root,
            Transform body,
            Transform legL,
            Transform legR,
            Transform armL,
            Transform armR,
            Transform swordPivot,
            Transform ponytail,
            GameObject slashArc,
            GameObject rainbowBeam,
            ParticleSystem particles)
        {
            visualRoot = root;
            bodyRoot = body;
            legLeftPivot = legL;
            legRightPivot = legR;
            armLeftPivot = armL;
            armRightPivot = armR;
            forearmLeftPivot = armL != null ? armL.Find("Forearm_L") : null;
            forearmRightPivot = armR != null ? armR.Find("Forearm_R") : null;
            twoHandSwordPivot = swordPivot;
            ponytailPivot = ponytail;
            slashArcObject = slashArc;
            rainbowBeamObject = rainbowBeam;
            impactParticles = particles;

            if (visualRoot != null)
            {
                baseScale = visualRoot.localScale;
            }

            if (bodyRoot != null)
            {
                bodyBaseLocalPos = bodyRoot.localPosition;
            }

            if (slashArcObject != null) slashArcObject.SetActive(false);
            if (rainbowBeamObject != null) rainbowBeamObject.SetActive(false);

            if (twoHandSwordPivot != null)
            {
                swordGlintFlare = twoHandSwordPivot.Find("ToySword_3D/Sword_ReflectionGlint");
                gripTargetR = twoHandSwordPivot.Find("ToySword_3D/GripTarget_R");
                gripTargetL = twoHandSwordPivot.Find("ToySword_3D/GripTarget_L");
                guardTargetL = twoHandSwordPivot.Find("ToySword_3D/GuardTarget_L");
            }

            ResolveExtendedRigBones();
        }

        private void ResolveExtendedRigBones()
        {
            if (rigBonesResolved || bodyRoot == null)
            {
                return;
            }

            rigBonesResolved = true;
            Transform rigRoot = bodyRoot.Find("Yuna_RigRoot");
            if (rigRoot != null)
            {
                hipsBone = rigRoot.Find("Hips");
                if (hipsBone != null)
                {
                    spineBone = hipsBone.Find("Spine");
                    if (spineBone != null)
                    {
                        chestBone = spineBone.Find("Chest");
                        if (chestBone != null)
                        {
                            neckBone = chestBone.Find("Neck");
                            headBone = neckBone != null ? neckBone.Find("Head") : null;
                        }
                    }
                }
            }

            if (legLeftPivot != null)
            {
                shinLeftBone = legLeftPivot.Find("Shin_L");
                footLeftBone = shinLeftBone != null ? shinLeftBone.Find("Foot_L") : null;
            }

            if (legRightPivot != null)
            {
                shinRightBone = legRightPivot.Find("Shin_R");
                footRightBone = shinRightBone != null ? shinRightBone.Find("Foot_R") : null;
            }

            if (forearmLeftPivot == null && armLeftPivot != null) forearmLeftPivot = armLeftPivot.Find("Forearm_L");
            if (forearmRightPivot == null && armRightPivot != null) forearmRightPivot = armRightPivot.Find("Forearm_R");
            handLeftBone = forearmLeftPivot != null ? forearmLeftPivot.Find("Hand_L") : null;
            handRightBone = forearmRightPivot != null ? forearmRightPivot.Find("Hand_R") : null;

            if (twoHandSwordPivot != null)
            {
                if (swordGlintFlare == null) swordGlintFlare = twoHandSwordPivot.Find("ToySword_3D/Sword_ReflectionGlint");
                if (gripTargetR == null) gripTargetR = twoHandSwordPivot.Find("ToySword_3D/GripTarget_R");
                if (gripTargetL == null) gripTargetL = twoHandSwordPivot.Find("ToySword_3D/GripTarget_L");
                if (guardTargetL == null) guardTargetL = twoHandSwordPivot.Find("ToySword_3D/GuardTarget_L");
            }
        }

        public void ConfigureEnemies3D(
            Transform root,
            GameObject train,
            GameObject robot,
            GameObject dinosaur,
            GameObject bear,
            ParticleSystem particles,
            ParticleSystem steamVfx,
            GameObject laserBeam,
            Light enrageLight)
        {
            visualRoot = root;
            trainModel = train;
            robotModel = robot;
            dinosaurModel = dinosaur;
            bearModel = bear;
            impactParticles = particles;
            steamBlastVfx = steamVfx;
            laserBeamObject = laserBeam;
            bearEnrageLight = enrageLight;

            if (visualRoot != null)
            {
                baseScale = visualRoot.localScale;
            }

            if (laserBeamObject != null) laserBeamObject.SetActive(false);
            if (bearEnrageLight != null) bearEnrageLight.enabled = false;

            steamJetObject = transform.Find("SteamJet_3D")?.gameObject;
            chargeSparksVfx = transform.Find("ChargeSparks_VFX")?.GetComponent<ParticleSystem>();
            trainHeadlight = transform.Find("Train_Headlight")?.GetComponent<Light>();
            rocketPunchObject = transform.Find("RocketPunch_3D")?.gameObject;
            laserSightObject = transform.Find("LaserSight_3D")?.gameObject;
            laserSightDotObject = transform.Find("LaserSight_Dot")?.gameObject;
            laserPointLight = transform.Find("Laser_PointLight")?.GetComponent<Light>();

            if (steamJetObject != null) steamJetObject.SetActive(false);
            if (trainHeadlight != null) trainHeadlight.enabled = false;
            if (rocketPunchObject != null) rocketPunchObject.SetActive(false);
            if (laserSightObject != null) laserSightObject.SetActive(false);
            if (laserSightDotObject != null) laserSightDotObject.SetActive(false);
            if (laserPointLight != null) laserPointLight.enabled = false;

            robotRigAnimator = robotModel != null ? robotModel.GetComponent<ArenaEnemyRigAnimator>() : null;
            dinosaurRigAnimator = dinosaurModel != null ? dinosaurModel.GetComponent<ArenaEnemyRigAnimator>() : null;
            bearRigAnimator = bearModel != null ? bearModel.GetComponent<ArenaEnemyRigAnimator>() : null;
        }

        public void TriggerImpactFeedback(Color burstColor)
        {
            hitFlashTimer = 0.16f;
            impactFlashTimer = ImpactFlashDuration;
            impactFlashSpin = Random.Range(-35f, 35f);

            if (impactFlashObject == null) impactFlashObject = transform.Find("Impact_Flash_3D")?.gameObject;
            if (impactFlashLight == null) impactFlashLight = transform.Find("Impact_FlashLight")?.GetComponent<Light>();

            if (impactFlashLight != null)
            {
                impactFlashLight.color = Color.Lerp(burstColor, Color.white, 0.55f);
                impactFlashLight.intensity = 14.0f;
                impactFlashLight.range = 8.5f;
                impactFlashLight.enabled = true;
            }

            if (impactParticles != null)
            {
                var main = impactParticles.main;
                main.startColor = burstColor;
                impactParticles.Play();
            }
        }

        public void TriggerSlashArc()
        {
            slashFadeTimer = 0.30f;
            visualComboTimer = VisualComboDuration;
        }

        public void UpdatePlayerVisual(ArenaCombatSimulation sim, ArenaSpriteCatalog catalog, float moveX, float dt)
        {
            if (sim == null)
            {
                return;
            }

            _ = catalog;
            transform.position = ArenaCoordinateMapper.ToWorldPosition(sim.PlayerX, sim.PlayerY, 0f);
            animTimer += dt;

            if (visualRoot != null)
            {
                Quaternion targetFacing = Quaternion.Euler(0f, sim.PlayerFacing >= 0 ? 90f : -90f, 0f);
                visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetFacing, dt * 16f);
            }

            AnimatePlayer3DRig(sim, moveX, dt);
            UpdateSlashAndSpecialVfx(sim, dt);
            UpdateImpactBangFlash(sim.PlayerFacing, 0.55f, dt);
        }

        public void UpdateEnemyVisual(ArenaCombatSimulation sim, ArenaSpriteCatalog catalog, float dt)
        {
            if (sim == null)
            {
                return;
            }

            _ = catalog;
            transform.position = ArenaCoordinateMapper.ToWorldPosition(sim.EnemyX, sim.EnemyY, 0.05f);
            animTimer += dt;

            ArenaEnemyId activeId = sim.CurrentEnemySpec.Id;
            if (trainModel != null) trainModel.SetActive(activeId == ArenaEnemyId.Train);
            if (robotModel != null) robotModel.SetActive(activeId == ArenaEnemyId.Robot);
            if (dinosaurModel != null) dinosaurModel.SetActive(activeId == ArenaEnemyId.Dinosaur);
            if (bearModel != null) bearModel.SetActive(activeId == ArenaEnemyId.Bear);

            float spawnScale = sim.EnemySpawnScale;
            float squashX = 1f;
            float squashY = 1f;
            float tiltX = 0f;
            float lungeZ = 0f;

            if (sim.EnemyState == ArenaEnemyState.HitReaction)
            {
                float t = Mathf.Clamp01(sim.EnemyStateTick / 16f);
                float impulse = Mathf.Sin(t * Mathf.PI);
                squashX = 1f + impulse * 0.14f;
                squashY = 1f - impulse * 0.12f;
                tiltX = -impulse * 12f;
            }
            else if (sim.EnemyState == ArenaEnemyState.Windup)
            {
                if (sim.ActiveAttackKind == ArenaAttackKind.SteamBlast)
                {
                    // Pressao na caldeira (trem incha e vibra suavemente preparando o Hot Steam)
                    float pulse = Mathf.Sin(animTimer * 28f) * 0.04f;
                    tiltX = -5f;
                    squashY = 1.05f + pulse;
                    squashX = 1.04f - pulse;
                }
                else if (sim.ActiveAttackKind == ArenaAttackKind.FrontCharge
                    || sim.ActiveAttackKind == ArenaAttackKind.Rush
                    || sim.ActiveAttackKind == ArenaAttackKind.BellyCharge)
                {
                    // Preparacao de atropelamento/investida: recua comprimido e treme com faiscas nas rodas
                    tiltX = -14f;
                    squashY = 0.90f;
                    squashX = 1.09f;
                    lungeZ = -0.18f + Mathf.Sin(animTimer * 48f) * 0.04f;
                }
                else
                {
                    tiltX = -10f;
                    squashY = 0.94f;
                    squashX = 1.05f;
                }
            }
            else if (sim.EnemyState == ArenaEnemyState.ActiveAttack)
            {
                if (sim.ActiveAttackKind == ArenaAttackKind.SteamBlast)
                {
                    tiltX = 6f;
                    squashY = 0.96f;
                    squashX = 1.04f;
                    lungeZ = 0.08f;
                }
                else
                {
                    tiltX = 15f;
                    lungeZ = 0.28f;
                }
            }
            else if (sim.EnemyState == ArenaEnemyState.Approaching || sim.EnemyState == ArenaEnemyState.Retreating)
            {
                tiltX = Mathf.Sin(animTimer * 10f) * 4f;
            }

            ArenaEnemyRigAnimator activeRig = activeId switch
            {
                ArenaEnemyId.Robot => robotRigAnimator ?? (robotRigAnimator = robotModel != null ? robotModel.GetComponent<ArenaEnemyRigAnimator>() : null),
                ArenaEnemyId.Dinosaur => dinosaurRigAnimator ?? (dinosaurRigAnimator = dinosaurModel != null ? dinosaurModel.GetComponent<ArenaEnemyRigAnimator>() : null),
                ArenaEnemyId.Bear => bearRigAnimator ?? (bearRigAnimator = bearModel != null ? bearModel.GetComponent<ArenaEnemyRigAnimator>() : null),
                _ => null
            };

            if (visualRoot != null)
            {
                // Chefes com esqueleto articulado animam o corpo via bones (sem inclinar a caixa inteira como bloco rigido)
                float clampedTilt = activeRig != null
                    ? 0f
                    : (activeId == ArenaEnemyId.Train ? Mathf.Clamp(tiltX, -4.5f, 4.5f) : tiltX);
                float yaw = sim.EnemyFacing >= 0 ? 90f : -90f;
                visualRoot.localRotation = Quaternion.Slerp(
                    visualRoot.localRotation,
                    Quaternion.Euler(clampedTilt, yaw, 0f),
                    dt * 14f);
                visualRoot.localPosition = new Vector3(sim.EnemyFacing * (activeRig != null ? lungeZ * 0.35f : lungeZ), 0f, 0f);
                float rigSquashX = activeRig != null ? Mathf.Lerp(1f, squashX, 0.35f) : squashX;
                float rigSquashY = activeRig != null ? Mathf.Lerp(1f, squashY, 0.35f) : squashY;
                visualRoot.localScale = new Vector3(
                    baseScale.x * spawnScale * rigSquashX,
                    baseScale.y * spawnScale * rigSquashY,
                    baseScale.z * spawnScale * rigSquashX);
            }

            if (activeRig != null)
            {
                Vector3 playerWorldPos = ArenaCoordinateMapper.ToWorldPosition(sim.PlayerX, sim.PlayerY, 0f);
                activeRig.UpdateRig(sim, playerWorldPos, dt);
            }

            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= dt;
            }

            UpdateEnemyAttackVfx(sim);
            UpdateImpactBangFlash(sim.EnemyFacing, 1.15f, dt);
        }

        private void UpdateImpactBangFlash(int facingDir, float forwardOffset, float dt)
        {
            if (impactFlashObject == null) impactFlashObject = transform.Find("Impact_Flash_3D")?.gameObject;
            if (impactFlashLight == null) impactFlashLight = transform.Find("Impact_FlashLight")?.GetComponent<Light>();

            if (impactFlashTimer > 0f)
            {
                impactFlashTimer = Mathf.Max(0f, impactFlashTimer - dt);
                float t = 1f - Mathf.Clamp01(impactFlashTimer / ImpactFlashDuration);
                float pop = 1f - Mathf.Pow(1f - t, 3f);
                float fade = 1f - t;

                if (impactFlashObject != null)
                {
                    impactFlashObject.SetActive(true);
                    impactFlashObject.transform.localPosition = new Vector3(facingDir * forwardOffset, 1.15f, -0.75f);
                    impactFlashObject.transform.localRotation = Quaternion.Euler(0f, 0f, impactFlashSpin + t * 55f);
                    float scale = Mathf.Lerp(0.75f, 2.85f, pop) * Mathf.Clamp01(fade * 2.0f);
                    impactFlashObject.transform.localScale = Vector3.one * scale;
                }

                if (impactFlashLight != null)
                {
                    impactFlashLight.enabled = true;
                    impactFlashLight.transform.localPosition = new Vector3(facingDir * forwardOffset, 1.20f, -1.05f);
                    impactFlashLight.intensity = 14.0f * fade * fade;
                }
            }
            else
            {
                if (impactFlashObject != null && impactFlashObject.activeSelf) impactFlashObject.SetActive(false);
                if (impactFlashLight != null && impactFlashLight.enabled) impactFlashLight.enabled = false;
            }
        }

        private void AnimatePlayer3DRig(ArenaCombatSimulation sim, float moveX, float dt)
        {
            if (bodyRoot == null)
            {
                return;
            }

            ResolveExtendedRigBones();

            // Detecta inicio de um novo golpe do combo (tanto em runtime quanto em simulacoes de captura)
            if (sim.SwordTick > 0 && lastSeenSwordTick == 0)
            {
                activeVisualComboStep = sim.SwordComboStep;
                visualComboTimer = VisualComboDuration;
            }

            lastSeenSwordTick = sim.SwordTick;
            if (visualComboTimer > 0f)
            {
                visualComboTimer = Mathf.Max(0f, visualComboTimer - dt);
            }

            bool inVisualCombo = visualComboTimer > 0f && sim.PlayerHitRecovery == 0;
            MartialPose pose;

            if (sim.PlayerHitRecovery > 0)
            {
                visualComboTimer = 0f;
                pose = CreateNeutralStancePose();
                pose.BodyZOffset = -0.10f;
                pose.HipsPitch = -8f;
                pose.SpinePitch = -14f;
                pose.ChestPitch = -10f;
                pose.ThighLPitch = -18f;
                pose.ShinLPitch = 28f;
                pose.FootLPitch = -10f;
                pose.ThighRPitch = 16f;
                pose.ShinRPitch = 12f;
                pose.SwordLocalPos = new Vector3(0.04f, 0.02f, 0.16f);
                pose.SwordLocalEuler = new Vector3(24f, 12f, -15f);
            }
            else if (inVisualCombo)
            {
                float progress = 1f - Mathf.Clamp01(visualComboTimer / VisualComboDuration);
                pose = EvaluateMartialComboPose(activeVisualComboStep, progress);
            }
            else if (sim.IsGuarding || sim.BlockRecovery > 0)
            {
                // Pose de defesa com base firme, corpo arqueado e espada presa nas duas maos na lateral:
                // - Centro de massa calibrado (BodyYOffset -0.065m) para ambos os pes tocarem exatamente o chao (y = 0)
                // - Perna frontal (Direita): joelho dobrado em apoio firme (-48 / +56 / -8) com sola 100% plana no piso
                // - Perna traseira (Esquerda): esticada para tras em apoio (+32 / +5 / -37) com pe firme no chao sem flutuar
                // - Tronco arqueado na defesa (Spine+Chest) com torcao lateral apresentando a espada para receber o impacto
                // - Espada dentro do alcance real dos bracos (z=0.20m): mao direita firme na empunhadura e esquerda apoiando a lamina
                pose = CreateNeutralStancePose();
                pose.BodyYOffset = -0.075f;
                pose.BodyZOffset = -0.02f;
                pose.HipsPitch = 0f;
                pose.HipsYaw = -6f;
                pose.SpinePitch = 16f;
                pose.SpineYaw = -4f;
                pose.ChestPitch = 14f;
                pose.ChestYaw = -4f;
                pose.HeadPitch = -26f;
                pose.HeadYaw = 14f;
                // Ambas as solas niveladas exatamente no piso (sem afundar o pe da frente nem flutuar o de tras)
                pose.ThighRPitch = -52f;
                pose.ShinRPitch = 62f;
                pose.FootRPitch = -10f;
                pose.ThighLPitch = 36f;
                pose.ShinLPitch = 4f;
                pose.FootLPitch = -40f;
                // Espada HORIZONTAL atravessada a frente do peito/queixo (referencia frontal + lateral 3/4):
                // z=+90 deg deita a lamina da mao direita (+X, cabo) para a mao esquerda (-X, apoio na lamina),
                // y=+38 deg projeta a ponta em perspectiva para o lado esquerdo/frente (diminuindo na lateral).
                pose.SwordLocalPos = new Vector3(0.09f, 0.06f, 0.18f);
                pose.SwordLocalEuler = new Vector3(0f, 38f, 90f);
            }
            else if (sim.IsCrouching)
            {
                pose = CreateNeutralStancePose();
                pose.BodyYOffset = -0.22f;
                pose.SpinePitch = 16f;
                pose.ChestPitch = 10f;
                pose.HeadPitch = -20f;
                pose.ThighLPitch = -58f;
                pose.ShinLPitch = 92f;
                pose.FootLPitch = -34f;
                pose.ThighRPitch = 28f;
                pose.ShinRPitch = 66f;
                pose.FootRPitch = -45f;
                pose.SwordLocalPos = new Vector3(0f, -0.09f, 0.21f);
                pose.SwordLocalEuler = new Vector3(62f, 0f, 0f);
            }
            else if (!sim.PlayerOnGround)
            {
                pose = CreateNeutralStancePose();
                pose.SpinePitch = 8f;
                pose.ThighLPitch = -44f;
                pose.ShinLPitch = 68f;
                pose.FootLPitch = 12f;
                pose.ThighRPitch = 28f;
                pose.ShinRPitch = 36f;
                pose.FootRPitch = 18f;
                pose.SwordLocalPos = new Vector3(0f, 0.02f, 0.21f);
                pose.SwordLocalEuler = new Vector3(36f, 0f, 0f);
            }
            else if (Mathf.Abs(moveX) > 0.1f)
            {
                float cycle = animTimer * 13f;
                float s = Mathf.Sin(cycle);
                float c = Mathf.Cos(cycle);
                pose = CreateNeutralStancePose();
                pose.BodyYOffset = Mathf.Abs(s) * 0.04f - 0.02f;
                pose.SpinePitch = 10f;
                pose.ChestYaw = s * 10f;
                pose.HeadYaw = -s * 8f;
                pose.ThighLPitch = s * 36f;
                pose.ShinLPitch = Mathf.Max(8f, -c * 46f + 16f);
                pose.FootLPitch = -(pose.ThighLPitch + pose.ShinLPitch) * 0.35f;
                pose.ThighRPitch = -s * 36f;
                pose.ShinRPitch = Mathf.Max(8f, c * 46f + 16f);
                pose.FootRPitch = -(pose.ThighRPitch + pose.ShinRPitch) * 0.35f;
                pose.SwordLocalPos = new Vector3(0f, -0.07f + s * 0.015f, 0.22f);
                pose.SwordLocalEuler = new Vector3(54f + s * 5f, 0f, 0f);
            }
            else
            {
                // Base de luta neutra atletica (joelho da frente levemente dobrado, respiracao viva)
                float breath = Mathf.Sin(animTimer * 2.8f);
                pose = CreateNeutralStancePose();
                pose.BodyYOffset += breath * 0.014f;
                pose.SpinePitch += breath * 2f;
                pose.SwordLocalPos += new Vector3(0f, breath * 0.012f, 0f);
                pose.SwordLocalEuler += new Vector3(breath * 2.5f, 0f, 0f);
            }

            // Durante golpes do combo usa resposta direta/rapida para atingir 100% da amplitude marcial sem amortecer os picos
            float blend = inVisualCombo ? Mathf.Clamp01(dt * 38f) : Mathf.Clamp01(dt * 14f);

            Vector3 targetBodyPos = new Vector3(bodyBaseLocalPos.x, bodyBaseLocalPos.y + pose.BodyYOffset, bodyBaseLocalPos.z + pose.BodyZOffset);
            bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetBodyPos, blend);
            bodyRoot.localRotation = Quaternion.Slerp(bodyRoot.localRotation, Quaternion.identity, blend);

            if (hipsBone != null)
            {
                hipsBone.localRotation = Quaternion.Slerp(hipsBone.localRotation, Quaternion.Euler(pose.HipsPitch, pose.HipsYaw, 0f), blend);
            }

            if (spineBone != null)
            {
                spineBone.localRotation = Quaternion.Slerp(spineBone.localRotation, Quaternion.Euler(pose.SpinePitch, pose.SpineYaw, 0f), blend);
            }

            if (chestBone != null)
            {
                chestBone.localRotation = Quaternion.Slerp(chestBone.localRotation, Quaternion.Euler(pose.ChestPitch, pose.ChestYaw, 0f), blend);
            }

            if (headBone != null)
            {
                headBone.localRotation = Quaternion.Slerp(headBone.localRotation, Quaternion.Euler(pose.HeadPitch, pose.HeadYaw, 0f), blend);
            }
            else if (ponytailPivot != null)
            {
                ponytailPivot.localRotation = Quaternion.Slerp(ponytailPivot.localRotation, Quaternion.Euler(pose.HeadPitch, pose.HeadYaw, 0f), blend);
            }

            if (legLeftPivot != null) legLeftPivot.localRotation = Quaternion.Slerp(legLeftPivot.localRotation, Quaternion.Euler(pose.ThighLPitch, 0f, 0f), blend);
            if (shinLeftBone != null) shinLeftBone.localRotation = Quaternion.Slerp(shinLeftBone.localRotation, Quaternion.Euler(pose.ShinLPitch, 0f, 0f), blend);
            if (footLeftBone != null) footLeftBone.localRotation = Quaternion.Slerp(footLeftBone.localRotation, Quaternion.Euler(pose.FootLPitch, 0f, 0f), blend);

            if (legRightPivot != null) legRightPivot.localRotation = Quaternion.Slerp(legRightPivot.localRotation, Quaternion.Euler(pose.ThighRPitch, 0f, 0f), blend);
            if (shinRightBone != null) shinRightBone.localRotation = Quaternion.Slerp(shinRightBone.localRotation, Quaternion.Euler(pose.ShinRPitch, 0f, 0f), blend);
            if (footRightBone != null) footRightBone.localRotation = Quaternion.Slerp(footRightBone.localRotation, Quaternion.Euler(pose.FootRPitch, 0f, 0f), blend);

            if (twoHandSwordPivot != null)
            {
                // Compensa a inclinacao do tronco (Hips+Spine+Chest) ANTES de orientar a espada,
                // garantindo que SwordLocalEuler represente o angulo REAL no mundo mesmo com z=90 deg (defesa horizontal).
                // Na defesa, gira 90 deg no eixo longitudinal da lamina para manter a cruzeta/guarda vertical e a face da lamina de frente.
                float torsoPitch = pose.HipsPitch + pose.SpinePitch + pose.ChestPitch;
                bool isGuardingPose = (sim.IsGuarding || sim.BlockRecovery > 0) && !inVisualCombo && sim.PlayerHitRecovery == 0;
                Quaternion bladeRoll = isGuardingPose ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
                Quaternion worldAlignedSwordRot = Quaternion.Euler(-torsoPitch, 0f, 0f) * Quaternion.Euler(pose.SwordLocalEuler) * bladeRoll;
                twoHandSwordPivot.localPosition = Vector3.Lerp(twoHandSwordPivot.localPosition, pose.SwordLocalPos, blend);
                twoHandSwordPivot.localRotation = Quaternion.Slerp(twoHandSwordPivot.localRotation, worldAlignedSwordRot, blend);
            }

            // Resolve IK analitico de 2 ossos para ambos os bracos ficarem 100% presos ao cabo (ou lateral da lamina na defesa)
            SolveTwoHandedSwordArmsIK(pose.SwordLocalPos.y, sim.IsGuarding || sim.BlockRecovery > 0);
            UpdateSwordReflectionGlint(sim);
        }

        private static MartialPose CreateNeutralStancePose()
        {
            return new MartialPose
            {
                BodyYOffset = -0.03f,
                BodyZOffset = 0f,
                HipsPitch = 0f,
                HipsYaw = 6f,
                SpinePitch = 4f,
                SpineYaw = 4f,
                ChestPitch = 2f,
                ChestYaw = 4f,
                HeadPitch = -4f,
                HeadYaw = -12f,
                // Base de luta: perna esquerda a frente com joelho dobrado, perna direita esticada atras dando sustentacao
                ThighLPitch = -26f,
                ShinLPitch = 26f,
                FootLPitch = 0f,
                ThighRPitch = 20f,
                ShinRPitch = 6f,
                FootRPitch = -20f,
                SwordLocalPos = new Vector3(0f, -0.07f, 0.22f),
                SwordLocalEuler = new Vector3(48f, 0f, 0f)
            };
        }

        private static MartialPose EvaluateMartialComboPose(int comboStep, float t)
        {
            MartialPose neutral = CreateNeutralStancePose();

            if (comboStep == 1)
            {
                // GOLPE 2: Corte lateral completo — espada a 45 graus no lado esquerdo e giro explosivo ate 45 graus no lado direito
                MartialPose windupLeft45 = new MartialPose
                {
                    BodyYOffset = -0.055f,
                    BodyZOffset = -0.08f,
                    HipsPitch = 0f,
                    HipsYaw = -18f,
                    SpinePitch = 6f,
                    SpineYaw = -24f,
                    ChestPitch = 4f,
                    ChestYaw = -26f,
                    HeadPitch = -6f,
                    HeadYaw = 54f, // Olhos fixos no inimigo enquanto o tronco torce
                    ThighLPitch = -42f,
                    ShinLPitch = 38f,
                    FootLPitch = 4f,
                    ThighRPitch = 30f,
                    ShinRPitch = 8f,
                    FootRPitch = -26f,
                    SwordLocalPos = new Vector3(-0.15f, 0.04f, 0.23f),
                    SwordLocalEuler = new Vector3(68f, -45f, -45f)
                };

                MartialPose slashRight45 = new MartialPose
                {
                    BodyYOffset = -0.085f,
                    BodyZOffset = 0.22f,
                    HipsPitch = 0f,
                    HipsYaw = 20f,
                    SpinePitch = 14f,
                    SpineYaw = 26f,
                    ChestPitch = 10f,
                    ChestYaw = 28f,
                    HeadPitch = -18f,
                    HeadYaw = -56f,
                    // Base marcial profunda absorvendo o giro: joelho da frente bem dobrado, perna de tras esticada!
                    ThighLPitch = -56f,
                    ShinLPitch = 48f,
                    FootLPitch = 8f,
                    ThighRPitch = 44f,
                    ShinRPitch = 6f,
                    FootRPitch = -36f,
                    SwordLocalPos = new Vector3(0.14f, -0.02f, 0.28f),
                    SwordLocalEuler = new Vector3(74f, 45f, 38f)
                };

                return BlendThreePhasePose(neutral, windupLeft45, slashRight45, t);
            }

            if (comboStep == 2)
            {
                // GOLPE 3: Estocada frontal — passo nitido para tras carregando na perna traseira, depois avanco explosivo empurrando a espada!
                MartialPose stepBackCharge = new MartialPose
                {
                    BodyYOffset = -0.05f,
                    BodyZOffset = -0.28f, // Passo nitido para tras
                    HipsPitch = 0f,
                    HipsYaw = 18f,
                    SpinePitch = -6f,
                    SpineYaw = 18f,
                    ChestPitch = -4f,
                    ChestYaw = 18f,
                    HeadPitch = 8f,
                    HeadYaw = -42f,
                    // Peso todo carregado na perna traseira flexionada, perna dianteira leve
                    ThighLPitch = -14f,
                    ShinLPitch = 14f,
                    FootLPitch = 0f,
                    ThighRPitch = 22f,
                    ShinRPitch = 38f,
                    FootRPitch = -34f,
                    SwordLocalPos = new Vector3(0.03f, -0.05f, 0.14f),
                    SwordLocalEuler = new Vector3(76f, 2f, 0f)
                };

                MartialPose explosiveLungeThrust = new MartialPose
                {
                    BodyYOffset = -0.095f,
                    BodyZOffset = 0.44f, // Avanco explosivo de 0.72m (de -0.28 ate +0.44!)
                    HipsPitch = 0f,
                    HipsYaw = -10f,
                    SpinePitch = 18f,
                    SpineYaw = -10f,
                    ChestPitch = 14f,
                    ChestYaw = -8f,
                    HeadPitch = -26f,
                    HeadYaw = 22f,
                    // Base de esgrima profunda e visivel: joelho da frente bem flexionado a frente, perna de tras esticada!
                    ThighLPitch = -60f,
                    ShinLPitch = 52f,
                    FootLPitch = 8f,
                    ThighRPitch = 48f,
                    ShinRPitch = 4f,
                    FootRPitch = -38f,
                    // Bracos totalmente esticados empurrando a espada na horizontal perfeita (90 deg mundo!)
                    SwordLocalPos = new Vector3(0f, 0.02f, 0.30f),
                    SwordLocalEuler = new Vector3(90f, 0f, 0f)
                };

                return BlendThreePhasePose(neutral, stepBackCharge, explosiveLungeThrust, t);
            }

            // GOLPE 1 (padrao): Corte vertical — estica a espada ao maximo acima da testa e desce com tudo ate o chao!
            MartialPose overheadHigh = new MartialPose
            {
                BodyYOffset = 0.02f,
                BodyZOffset = -0.06f,
                HipsPitch = 0f,
                HipsYaw = 4f,
                SpinePitch = -10f,
                SpineYaw = 4f,
                ChestPitch = -10f,
                ChestYaw = 4f,
                HeadPitch = 12f,
                HeadYaw = -8f,
                ThighLPitch = -24f,
                ShinLPitch = 22f,
                FootLPitch = 2f,
                ThighRPitch = 22f,
                ShinRPitch = 8f,
                FootRPitch = -22f,
                // Maos esticadas ao maximo logo a frente da testa (sem atravessar o rosto), lamina erguida bem acima da cabeca!
                SwordLocalPos = new Vector3(0f, 0.31f, 0.24f),
                SwordLocalEuler = new Vector3(-12f, 0f, 0f)
            };

            MartialPose groundCleave = new MartialPose
            {
                BodyYOffset = -0.095f,
                BodyZOffset = 0.26f,
                HipsPitch = 0f,
                HipsYaw = -6f,
                SpinePitch = 18f,
                SpineYaw = -6f,
                ChestPitch = 16f,
                ChestYaw = -6f,
                HeadPitch = -24f,
                HeadYaw = 14f,
                // Base firme de impacto: joelho da frente flexionado a frente, perna de tras esticada criando apoio!
                ThighLPitch = -58f,
                ShinLPitch = 50f,
                FootLPitch = 8f,
                ThighRPitch = 46f,
                ShinRPitch = 6f,
                FootRPitch = -38f,
                // Espada desce com tudo a frente das pernas apontando direto para o chao (128 deg mundo)!
                SwordLocalPos = new Vector3(0f, -0.14f, 0.28f),
                SwordLocalEuler = new Vector3(128f, 0f, 0f)
            };

            return BlendThreePhasePose(neutral, overheadHigh, groundCleave, t);
        }

        private static MartialPose BlendThreePhasePose(MartialPose neutral, MartialPose windup, MartialPose strike, float t)
        {
            if (t < 0.22f)
            {
                // Fase 1: Antecipacao curta e explosiva (EaseOutCubic)
                float u = t / 0.22f;
                float eased = 1f - Mathf.Pow(1f - u, 3f);
                return LerpPose(neutral, windup, eased);
            }

            if (t < 0.44f)
            {
                // Fase 2: Disparo chicoteado de alto impacto (EaseInOutQuart)
                float u = (t - 0.22f) / 0.22f;
                float eased = u < 0.5f ? 8f * u * u * u * u : 1f - Mathf.Pow(-2f * u + 2f, 4f) * 0.5f;
                return LerpPose(windup, strike, eased);
            }

            if (t < 0.64f)
            {
                // Fase 3: Assentamento de peso no follow-through para leitura marcial
                return strike;
            }

            // Fase 4: Recuperacao fluida de volta a guarda (EaseInOutQuad)
            float r = (t - 0.64f) / 0.36f;
            float rec = r < 0.5f ? 2f * r * r : 1f - Mathf.Pow(-2f * r + 2f, 2f) * 0.5f;
            return LerpPose(strike, neutral, rec);
        }

        private static MartialPose LerpPose(MartialPose a, MartialPose b, float t)
        {
            return new MartialPose
            {
                BodyYOffset = Mathf.Lerp(a.BodyYOffset, b.BodyYOffset, t),
                BodyZOffset = Mathf.Lerp(a.BodyZOffset, b.BodyZOffset, t),
                HipsPitch = Mathf.Lerp(a.HipsPitch, b.HipsPitch, t),
                HipsYaw = Mathf.Lerp(a.HipsYaw, b.HipsYaw, t),
                SpinePitch = Mathf.Lerp(a.SpinePitch, b.SpinePitch, t),
                SpineYaw = Mathf.Lerp(a.SpineYaw, b.SpineYaw, t),
                ChestPitch = Mathf.Lerp(a.ChestPitch, b.ChestPitch, t),
                ChestYaw = Mathf.Lerp(a.ChestYaw, b.ChestYaw, t),
                HeadPitch = Mathf.Lerp(a.HeadPitch, b.HeadPitch, t),
                HeadYaw = Mathf.Lerp(a.HeadYaw, b.HeadYaw, t),
                ThighLPitch = Mathf.Lerp(a.ThighLPitch, b.ThighLPitch, t),
                ShinLPitch = Mathf.Lerp(a.ShinLPitch, b.ShinLPitch, t),
                FootLPitch = Mathf.Lerp(a.FootLPitch, b.FootLPitch, t),
                ThighRPitch = Mathf.Lerp(a.ThighRPitch, b.ThighRPitch, t),
                ShinRPitch = Mathf.Lerp(a.ShinRPitch, b.ShinRPitch, t),
                FootRPitch = Mathf.Lerp(a.FootRPitch, b.FootRPitch, t),
                SwordLocalPos = Vector3.Lerp(a.SwordLocalPos, b.SwordLocalPos, t),
                SwordLocalEuler = Vector3.Lerp(a.SwordLocalEuler, b.SwordLocalEuler, t)
            };
        }

        private void SolveTwoHandedSwordArmsIK(float swordLocalY, bool useGuardBladeBrace)
        {
            if (chestBone == null || gripTargetR == null || gripTargetL == null)
            {
                return;
            }

            // Quando a espada sobe acima da cabeca, levanta o polo dos cotovelos para fora/frente de forma natural
            float liftT = Mathf.InverseLerp(0.05f, 0.28f, swordLocalY);
            float poleY = Mathf.Lerp(-0.65f, -0.10f, liftT);
            float poleZ = Mathf.Lerp(-0.28f, 0.35f, liftT);
            Vector3 poleR = chestBone.TransformDirection(new Vector3(0.75f, useGuardBladeBrace ? -0.45f : poleY, useGuardBladeBrace ? 0.35f : poleZ));
            Vector3 poleL = chestBone.TransformDirection(new Vector3(-0.75f, useGuardBladeBrace ? -0.30f : poleY, useGuardBladeBrace ? 0.30f : poleZ));

            Transform leftTarget = useGuardBladeBrace && guardTargetL != null ? guardTargetL : gripTargetL;
            SolveTwoBoneArmIK(armRightPivot, forearmRightPivot, handRightBone, gripTargetR.position, poleR);
            SolveTwoBoneArmIK(armLeftPivot, forearmLeftPivot, handLeftBone, leftTarget.position, poleL);
        }

        private static void SolveTwoBoneArmIK(
            Transform upperArm,
            Transform forearm,
            Transform hand,
            Vector3 targetWorldPos,
            Vector3 poleWorldDir)
        {
            if (upperArm == null || forearm == null || hand == null)
            {
                return;
            }

            // Reseta para pose de repouso antes de resolver analiticamente (zero acumulo de torcao)
            upperArm.localRotation = Quaternion.identity;
            forearm.localRotation = Quaternion.identity;

            Vector3 a = upperArm.position;
            Vector3 b = forearm.position;
            Vector3 c = hand.position;

            float len1 = Vector3.Distance(a, b);
            float len2 = Vector3.Distance(b, c);
            if (len1 < 0.001f || len2 < 0.001f)
            {
                return;
            }

            Vector3 toTarget = targetWorldPos - a;
            float dist = toTarget.magnitude;
            if (dist < 0.001f)
            {
                return;
            }

            float maxReach = (len1 + len2) * 0.992f;
            float minReach = Mathf.Abs(len1 - len2) + 0.01f;
            float clampedDist = Mathf.Clamp(dist, minReach, maxReach);

            Vector3 atDir = toTarget / dist;
            float cosAlpha = Mathf.Clamp((len1 * len1 + clampedDist * clampedDist - len2 * len2) / (2f * len1 * clampedDist), -1f, 1f);
            float alpha = Mathf.Acos(cosAlpha);

            Vector3 bendDir = Vector3.ProjectOnPlane(poleWorldDir, atDir);
            if (bendDir.sqrMagnitude < 0.0001f)
            {
                bendDir = Vector3.Cross(atDir, Vector3.up);
                if (bendDir.sqrMagnitude < 0.0001f)
                {
                    bendDir = Vector3.right;
                }
            }

            bendDir.Normalize();
            Vector3 desiredElbowPos = a + (atDir * Mathf.Cos(alpha) + bendDir * Mathf.Sin(alpha)) * len1;

            Vector3 curUpperDir = (forearm.position - a).normalized;
            Vector3 desUpperDir = (desiredElbowPos - a).normalized;
            upperArm.rotation = Quaternion.FromToRotation(curUpperDir, desUpperDir) * upperArm.rotation;

            Vector3 elbowNow = forearm.position;
            Vector3 curForearmDir = (hand.position - elbowNow).normalized;
            Vector3 desForearmDir = (targetWorldPos - elbowNow).normalized;
            if (curForearmDir.sqrMagnitude > 0.0001f && desForearmDir.sqrMagnitude > 0.0001f)
            {
                forearm.rotation = Quaternion.FromToRotation(curForearmDir, desForearmDir) * forearm.rotation;
            }
        }

        private void UpdateSwordReflectionGlint(ArenaCombatSimulation sim)
        {
            if (swordGlintFlare == null && twoHandSwordPivot != null)
            {
                swordGlintFlare = twoHandSwordPivot.Find("ToySword_3D/Sword_ReflectionGlint");
            }

            if (swordGlintFlare == null)
            {
                return;
            }

            // Sincroniza a estrela de reflexo especular com a varredura de brilho metalico na lamina
            float cycle = Mathf.Repeat(animTimer * 0.44f + 0.28f, 1f);
            float sweepT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(cycle / 0.62f));
            float yAlongBlade = Mathf.Lerp(0.24f, 0.88f, sweepT);
            swordGlintFlare.localPosition = new Vector3(-0.06f, yAlongBlade, 0.04f);

            float pulse = cycle < 0.62f ? Mathf.Sin(sweepT * Mathf.PI) : 0f;
            if (sim.SwordTick > 0 || sim.IsGuarding)
            {
                pulse = Mathf.Max(pulse, 0.95f);
            }

            float scale = Mathf.Lerp(0.08f, 0.36f, pulse);
            swordGlintFlare.localScale = Vector3.one * scale;
            swordGlintFlare.localRotation = Quaternion.Euler(0f, 90f, animTimer * 75f);
        }

        private void UpdateSlashAndSpecialVfx(ArenaCombatSimulation sim, float dt)
        {
            if (slashArcObject != null)
            {
                bool showComboStreak = visualComboTimer > 0f && sim.PlayerHitRecovery == 0;
                float comboProgress = showComboStreak ? (1f - Mathf.Clamp01(visualComboTimer / VisualComboDuration)) : 0f;
                // O risco luminoso aparece exatamente durante o corte explosivo (0.22 -> 0.60), nunca durante a antecipacao
                if (showComboStreak && comboProgress >= 0.22f && comboProgress <= 0.60f)
                {
                    slashArcObject.SetActive(true);
                    float progress = Mathf.Clamp01((comboProgress - 0.22f) / 0.38f);
                    float fadeEnvelope = Mathf.Sin(progress * Mathf.PI);
                    int comboStep = activeVisualComboStep;

                    if (comboStep == 1)
                    {
                        // COMBO 2: Risco horizontal afiado varrendo da esquerda para a direita
                        float sweepX = Mathf.Lerp(0.45f, 2.05f, progress) * sim.PlayerFacing;
                        slashArcObject.transform.localPosition = new Vector3(sweepX, 1.05f, -0.45f);
                        slashArcObject.transform.localRotation = Quaternion.Euler(0f, sim.PlayerFacing >= 0 ? 0f : 180f, 90f);
                        slashArcObject.transform.localScale = new Vector3(0.36f + fadeEnvelope * 0.22f, 2.45f * fadeEnvelope + 0.65f, 1f);
                    }
                    else if (comboStep == 2)
                    {
                        // COMBO 3: Estocada frontal perfurante em linha reta acompanhando o avanco
                        float thrustX = Mathf.Lerp(0.85f, 2.45f, progress) * sim.PlayerFacing;
                        slashArcObject.transform.localPosition = new Vector3(thrustX, 1.00f, -0.45f);
                        slashArcObject.transform.localRotation = Quaternion.Euler(0f, sim.PlayerFacing >= 0 ? 0f : 180f, 90f);
                        slashArcObject.transform.localScale = new Vector3(0.24f + fadeEnvelope * 0.14f, 2.85f * fadeEnvelope + 0.80f, 1f);
                    }
                    else
                    {
                        // COMBO 1: Risco vertical afiado descendo de cima da cabeca ate o chao
                        float sweepY = Mathf.Lerp(2.15f, 0.55f, progress);
                        slashArcObject.transform.localPosition = new Vector3(sim.PlayerFacing * 1.28f, sweepY, -0.45f);
                        slashArcObject.transform.localRotation = Quaternion.Euler(0f, sim.PlayerFacing >= 0 ? 0f : 180f, -12f);
                        slashArcObject.transform.localScale = new Vector3(0.40f + fadeEnvelope * 0.22f, 2.85f * fadeEnvelope + 0.75f, 1f);
                    }
                }
                else
                {
                    slashArcObject.SetActive(false);
                }

                if (slashFadeTimer > 0f)
                {
                    slashFadeTimer = Mathf.Max(0f, slashFadeTimer - dt);
                }
            }

            if (rainbowBeamObject != null)
            {
                bool showBeam = sim.SpecialPhase == ArenaSpecialPhase.Launch
                    || sim.SpecialPhase == ArenaSpecialPhase.Impact
                    || sim.SpecialPhase == ArenaSpecialPhase.Fade;
                rainbowBeamObject.SetActive(showBeam);
                if (showBeam)
                {
                    float dx = (sim.EnemyX - sim.PlayerX) * ArenaCoordinateMapper.PixelsToMeters;
                    float length = Mathf.Max(2.0f, Mathf.Abs(dx) + 1.5f);
                    float dir = dx >= 0f ? 1f : -1f;
                    rainbowBeamObject.transform.localPosition = new Vector3(dir * length * 0.5f, 1.05f, 0f);
                    rainbowBeamObject.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    float radius = sim.SpecialPhase == ArenaSpecialPhase.Fade ? 0.25f : 0.62f;
                    rainbowBeamObject.transform.localScale = new Vector3(radius, length * 0.5f, radius);
                }
            }
        }

        private void UpdateEnemyAttackVfx(ArenaCombatSimulation sim)
        {
            if (steamJetObject == null) steamJetObject = transform.Find("SteamJet_3D")?.gameObject;
            if (chargeSparksVfx == null) chargeSparksVfx = transform.Find("ChargeSparks_VFX")?.GetComponent<ParticleSystem>();
            if (trainHeadlight == null) trainHeadlight = transform.Find("Train_Headlight")?.GetComponent<Light>();
            if (rocketPunchObject == null) rocketPunchObject = transform.Find("RocketPunch_3D")?.gameObject;

            bool isTrain = sim.CurrentEnemySpec.Id == ArenaEnemyId.Train;
            bool steamWindup = isTrain && sim.EnemyState == ArenaEnemyState.Windup && sim.ActiveAttackKind == ArenaAttackKind.SteamBlast;
            bool steamActive = isTrain && sim.EnemyState == ArenaEnemyState.ActiveAttack && sim.ActiveAttackKind == ArenaAttackKind.SteamBlast;

            if (steamBlastVfx != null)
            {
                // Orienta o jato de vapor quente diretamente para cima da Yuna (ou para cima da chamine no windup)
                if (steamActive)
                {
                    steamBlastVfx.transform.localPosition = new Vector3(sim.EnemyFacing * 1.45f, 0.85f, -0.05f);
                    steamBlastVfx.transform.localRotation = Quaternion.Euler(0f, sim.EnemyFacing >= 0 ? 90f : -90f, 0f);
                }
                else if (steamWindup)
                {
                    steamBlastVfx.transform.localPosition = new Vector3(sim.EnemyFacing * 0.95f, 2.05f, 0f);
                    steamBlastVfx.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                }

                bool emitSteam = steamActive || steamWindup;
                if (emitSteam && !steamBlastVfx.isPlaying) steamBlastVfx.Play();
                else if (!emitSteam && steamBlastVfx.isPlaying) steamBlastVfx.Stop();
            }

            if (steamJetObject != null)
            {
                steamJetObject.SetActive(steamActive);
                if (steamActive)
                {
                    float pulse = 1f + Mathf.Sin(animTimer * 30f) * 0.10f;
                    steamJetObject.transform.localPosition = new Vector3(sim.EnemyFacing * 2.25f, 0.90f, -0.45f);
                    steamJetObject.transform.localRotation = Quaternion.Euler(0f, sim.EnemyFacing >= 0 ? 180f : 0f, 0f);
                    steamJetObject.transform.localScale = new Vector3(3.15f * pulse, 1.45f * pulse, 1f);
                }
            }

            bool chargeTelegraphOrActive = isTrain
                && (sim.EnemyState == ArenaEnemyState.Windup || sim.EnemyState == ArenaEnemyState.ActiveAttack)
                && sim.ActiveAttackKind == ArenaAttackKind.FrontCharge;

            if (chargeSparksVfx != null)
            {
                chargeSparksVfx.transform.localPosition = new Vector3(sim.EnemyFacing * 0.45f, 0.22f, -0.08f);
                chargeSparksVfx.transform.localRotation = Quaternion.Euler(-18f, sim.EnemyFacing >= 0 ? -90f : 90f, 0f);
                if (chargeTelegraphOrActive && !chargeSparksVfx.isPlaying) chargeSparksVfx.Play();
                else if (!chargeTelegraphOrActive && chargeSparksVfx.isPlaying) chargeSparksVfx.Stop();
            }

            if (trainHeadlight != null)
            {
                trainHeadlight.enabled = chargeTelegraphOrActive;
                if (chargeTelegraphOrActive)
                {
                    trainHeadlight.transform.localPosition = new Vector3(sim.EnemyFacing * 1.65f, 1.05f, -0.25f);
                    trainHeadlight.intensity = sim.EnemyState == ArenaEnemyState.ActiveAttack ? 4.5f : 2.6f;
                }
            }

            if (laserSightObject == null) laserSightObject = transform.Find("LaserSight_3D")?.gameObject;
            if (laserSightDotObject == null) laserSightDotObject = transform.Find("LaserSight_Dot")?.gameObject;
            if (laserPointLight == null) laserPointLight = transform.Find("Laser_PointLight")?.GetComponent<Light>();

            bool isRobotLaser = sim.CurrentEnemySpec.Id == ArenaEnemyId.Robot && sim.ActiveAttackKind == ArenaAttackKind.LaserBeam;
            bool laserWindup = isRobotLaser && sim.EnemyState == ArenaEnemyState.Windup;
            bool laserActive = isRobotLaser && sim.EnemyState == ArenaEnemyState.ActiveAttack;

            // ETAPA 1 (Windup): Mira laser vermelha fina saindo do visor do Robo e apontando diretamente para a Yuna
            if (laserSightObject != null)
            {
                laserSightObject.SetActive(laserWindup);
                if (laserWindup)
                {
                    Vector3 visorWorld = transform.position + new Vector3(sim.EnemyFacing * 0.42f, 1.88f, -0.04f);
                    Vector3 yunaTargetWorld = ArenaCoordinateMapper.ToWorldPosition(sim.PlayerX, sim.PlayerY, 0f) + new Vector3(0f, sim.IsCrouching ? 0.72f : 1.22f, -0.04f);
                    Vector3 delta = yunaTargetWorld - visorWorld;
                    float dist = Mathf.Max(0.5f, delta.magnitude);
                    Vector3 midWorld = (visorWorld + yunaTargetWorld) * 0.5f;
                    float windupT = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyWindupTicks));
                    float pulse = 1f + Mathf.Sin(animTimer * Mathf.Lerp(16f, 44f, windupT)) * 0.25f;
                    float thinRadius = 0.016f * pulse;

                    laserSightObject.transform.position = midWorld;
                    laserSightObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
                    laserSightObject.transform.localScale = new Vector3(thinRadius, dist * 0.5f, thinRadius);

                    if (laserSightDotObject != null)
                    {
                        laserSightDotObject.SetActive(true);
                        laserSightDotObject.transform.position = yunaTargetWorld + new Vector3(0f, 0f, -0.18f);
                        float dotScale = Mathf.Lerp(0.16f, 0.28f, windupT) * pulse;
                        laserSightDotObject.transform.localScale = Vector3.one * dotScale;
                    }
                }
                else if (laserSightDotObject != null && laserSightDotObject.activeSelf)
                {
                    laserSightDotObject.SetActive(false);
                }
            }

            // ETAPA 2 (ActiveAttack): Disparo forte e espesso de energia
            if (laserBeamObject != null)
            {
                laserBeamObject.SetActive(laserActive);
                if (laserActive)
                {
                    float activeT = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyActiveTicks));
                    float powerEnv = Mathf.Sin(activeT * Mathf.PI);
                    float beamRadius = Mathf.Lerp(0.18f, 0.34f, powerEnv) + Mathf.Sin(animTimer * 60f) * 0.025f;
                    float beamHalfLen = 4.2f;
                    laserBeamObject.transform.localPosition = new Vector3(sim.EnemyFacing * (0.42f + beamHalfLen), 1.82f, -0.06f);
                    laserBeamObject.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    laserBeamObject.transform.localScale = new Vector3(beamRadius, beamHalfLen, beamRadius);
                }
            }

            if (laserPointLight != null)
            {
                laserPointLight.enabled = laserWindup || laserActive;
                if (laserActive)
                {
                    laserPointLight.color = new Color(1.0f, 0.32f, 0.16f);
                    laserPointLight.transform.localPosition = new Vector3(sim.EnemyFacing * 1.4f, 1.65f, -0.55f);
                    laserPointLight.intensity = 9.5f;
                    laserPointLight.range = 8.0f;
                }
                else if (laserWindup)
                {
                    laserPointLight.color = new Color(1.0f, 0.12f, 0.12f);
                    laserPointLight.transform.localPosition = new Vector3(sim.EnemyFacing * 0.55f, 1.88f, -0.35f);
                    laserPointLight.intensity = 2.4f;
                    laserPointLight.range = 3.5f;
                }
            }

            if (rocketPunchObject != null)
            {
                // Quando o Robo possui esqueleto articulado, o RocketPunch e executado pelo PROPRIO braco do Robo
                bool punchActive = robotRigAnimator == null
                    && sim.CurrentEnemySpec.Id == ArenaEnemyId.Robot
                    && sim.EnemyState == ArenaEnemyState.ActiveAttack
                    && sim.ActiveAttackKind == ArenaAttackKind.RocketPunch;
                rocketPunchObject.SetActive(punchActive);
                if (punchActive)
                {
                    float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyActiveTicks));
                    float ext = Mathf.Sin(t * Mathf.PI);
                    float reach = Mathf.Lerp(0.9f, 2.75f, ext);
                    rocketPunchObject.transform.localPosition = new Vector3(sim.EnemyFacing * reach, 0.72f, -0.05f);
                    rocketPunchObject.transform.localRotation = Quaternion.Euler(0f, sim.EnemyFacing >= 0 ? 0f : 180f, 0f);
                }
            }

            if (bearEnrageLight != null)
            {
                bearEnrageLight.enabled = sim.CurrentEnemySpec.Id == ArenaEnemyId.Bear && sim.BearEnraged;
            }
        }
    }
}
