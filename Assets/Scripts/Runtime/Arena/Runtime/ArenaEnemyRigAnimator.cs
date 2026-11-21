using System;
using UnityEngine;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Controlador especialista de animacao esqueletal (SkinnedMeshRenderer) para os chefes
    /// articulados da Arena: Robo (passos, quadril, RocketPunch pelo proprio braco),
    /// Dinossauro (passos, caguete rapido na cauda, corpo arqueado mordendo) e
    /// Urso (quadril/joelhos/pes, cotovelos/bracos nos golpes, olhar fixo na Yuna),
    /// incluindo reacoes de dor e recuperacao para todos.
    /// </summary>
    public sealed class ArenaEnemyRigAnimator : MonoBehaviour
    {
        [Serializable]
        public struct RigBones
        {
            public Transform Hips;
            public Transform Spine;
            public Transform Chest;
            public Transform Neck;
            public Transform Head;
            public Transform Jaw;
            public Transform UpperArmL;
            public Transform ForearmL;
            public Transform HandL;
            public Transform UpperArmR;
            public Transform ForearmR;
            public Transform HandR;
            public Transform ThighL;
            public Transform ShinL;
            public Transform FootL;
            public Transform ThighR;
            public Transform ShinR;
            public Transform FootR;
            public Transform Tail1;
            public Transform Tail2;
            public Transform Tail3;
        }

        [SerializeField] private ArenaEnemyId enemyId;
        [SerializeField] private RigBones bones;
        [SerializeField] private Vector3 hipsRestLocalPos;
        [SerializeField] private Vector3 handRRestLocalPos;
        [SerializeField] private Vector3 handLRestLocalPos;

        private float animTimer;

        public void Configure(ArenaEnemyId id, RigBones rigBones)
        {
            enemyId = id;
            bones = rigBones;
            hipsRestLocalPos = bones.Hips != null ? bones.Hips.localPosition : Vector3.zero;
            handRRestLocalPos = bones.HandR != null ? bones.HandR.localPosition : Vector3.zero;
            handLRestLocalPos = bones.HandL != null ? bones.HandL.localPosition : Vector3.zero;
        }

        public void UpdateRig(ArenaCombatSimulation sim, Vector3 playerWorldPos, float dt)
        {
            if (sim == null || bones.Hips == null)
            {
                return;
            }

            animTimer += dt;
            bool isStriking = sim.EnemyState == ArenaEnemyState.ActiveAttack;
            float blend = Mathf.Clamp01(dt * (isStriking ? 30f : 18f));

            switch (enemyId)
            {
                case ArenaEnemyId.Robot:
                    AnimateRobot(sim, blend);
                    break;
                case ArenaEnemyId.Dinosaur:
                    AnimateDinosaur(sim, blend);
                    break;
                case ArenaEnemyId.Bear:
                    AnimateBear(sim, playerWorldPos, blend);
                    break;
            }
        }

        private void AnimateRobot(ArenaCombatSimulation sim, float blend)
        {
            bool isWalking = sim.EnemyState == ArenaEnemyState.Approaching
                || sim.EnemyState == ArenaEnemyState.Retreating
                || sim.EnemyState == ArenaEnemyState.Evading;
            bool isPunchWindup = sim.EnemyState == ArenaEnemyState.Windup && sim.ActiveAttackKind == ArenaAttackKind.RocketPunch;
            bool isPunchActive = sim.EnemyState == ArenaEnemyState.ActiveAttack && sim.ActiveAttackKind == ArenaAttackKind.RocketPunch;
            bool isLaser = (sim.EnemyState == ArenaEnemyState.Windup || sim.EnemyState == ArenaEnemyState.ActiveAttack)
                && sim.ActiveAttackKind == ArenaAttackKind.LaserBeam;
            bool isHit = sim.EnemyState == ArenaEnemyState.HitReaction;

            float hipsY = 0f;
            float hipsPitch = 0f;
            float hipsYaw = 0f;
            float chestPitch = 0f;
            float chestYaw = 0f;
            float headPitch = 0f;

            float thighL = 0f, footL = 0f;
            float thighR = 0f, footR = 0f;

            Vector3 armLEuler = Vector3.zero;
            Vector3 forearmLEuler = Vector3.zero;
            Vector3 armREuler = Vector3.zero;
            Vector3 forearmREuler = Vector3.zero;
            Vector3 handRPos = handRRestLocalPos;
            Vector3 handRScale = Vector3.one;

            if (isHit)
            {
                // Reacao de dor/curto-circuito: recua tronco, cabeca tomba e bracos abrem no impacto
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyHitReactionTicks));
                float pain = Mathf.Sin(t * Mathf.PI);
                hipsY = -0.03f * pain;
                hipsPitch = -14f * pain;
                chestPitch = -18f * pain;
                chestYaw = 16f * pain;
                headPitch = -22f * pain;
                armLEuler = new Vector3(-28f * pain, 0f, -34f * pain);
                armREuler = new Vector3(-28f * pain, 0f, 34f * pain);
                thighL = -16f * pain;
                thighR = 14f * pain;
            }
            else if (isPunchWindup)
            {
                // Recolhe o torso e arma o braco direito de boxe para tras antes do soco-foguete
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyWindupTicks));
                float w = 1f - Mathf.Pow(1f - t, 2f);
                hipsY = -0.04f * w;
                hipsYaw = 18f * w;
                chestYaw = 28f * w;
                chestPitch = -8f * w;
                armREuler = new Vector3(32f * w, 0f, 18f * w);
                forearmREuler = new Vector3(-48f * w, 0f, 0f);
                armLEuler = new Vector3(-35f * w, 0f, -10f * w);
                thighL = -24f * w;
                thighR = 22f * w;
            }
            else if (isPunchActive)
            {
                // ROCKET PUNCH real feito pelo PROPRIO BRACO DIREITO do robo (golpe rapido e seco!)
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyActiveTicks));
                float ext = Mathf.Pow(Mathf.Sin(t * Mathf.PI), 0.65f);
                hipsY = -0.05f;
                hipsYaw = -16f * ext;
                chestYaw = -34f * ext;
                chestPitch = 14f * ext;
                headPitch = -10f * ext;
                // Braco direito levanta na horizontal apontando direto para a Yuna e dispara a luva sem deformar!
                armREuler = new Vector3(-86f * ext, 0f, 6f);
                forearmREuler = new Vector3(-8f * ext, 0f, 0f);
                handRPos = handRRestLocalPos + new Vector3(0f, -0.42f * ext, 0.08f * ext);
                handRScale = new Vector3(1f + 0.14f * ext, 1f + 0.14f * ext, 1f + 0.14f * ext);
                // Braco esquerdo vai para tras como contrapeso
                armLEuler = new Vector3(38f * ext, 0f, -16f * ext);
                thighL = -32f * ext;
                thighR = 30f * ext;
            }
            else if (isLaser)
            {
                // Base firme para disparo do Laser pelo visor
                hipsY = -0.04f;
                chestPitch = 8f;
                headPitch = -6f;
                armLEuler = new Vector3(16f, 0f, -18f);
                armREuler = new Vector3(16f, 0f, 18f);
                thighL = -20f;
                thighR = 20f;
            }
            else if (isWalking)
            {
                // Passo mecanico articulado trocando as pernas e balancando o quadril
                float cycle = animTimer * 9.5f;
                float s = Mathf.Sin(cycle);
                float c = Mathf.Cos(cycle);
                hipsY = Mathf.Abs(s) * 0.035f;
                hipsYaw = s * 11f;
                chestPitch = 5f;
                chestYaw = -s * 8f;
                thighL = s * 32f;
                footL = Mathf.Clamp(-c * 16f, -12f, 18f);
                thighR = -s * 32f;
                footR = Mathf.Clamp(c * 16f, -12f, 18f);
                armLEuler = new Vector3(-s * 24f, 0f, -4f);
                armREuler = new Vector3(s * 24f, 0f, 4f);
            }
            else
            {
                // Idle atento de combate
                float breath = Mathf.Sin(animTimer * 3.0f);
                hipsY = breath * 0.014f;
                thighL = -10f;
                thighR = 10f;
                armLEuler = new Vector3(-14f + breath * 4f, 0f, -6f);
                armREuler = new Vector3(-14f - breath * 4f, 0f, 6f);
            }

            ApplyTransform(bones.Hips, hipsRestLocalPos + new Vector3(0f, hipsY, 0f), Quaternion.Euler(hipsPitch, hipsYaw, 0f), blend);
            ApplyRot(bones.Chest, Quaternion.Euler(chestPitch, chestYaw, 0f), blend);
            ApplyRot(bones.Head, Quaternion.Euler(headPitch, 0f, 0f), blend);
            ApplyRot(bones.ThighL, Quaternion.Euler(thighL, 0f, 0f), blend);
            ApplyRot(bones.FootL, Quaternion.Euler(footL, 0f, 0f), blend);
            ApplyRot(bones.ThighR, Quaternion.Euler(thighR, 0f, 0f), blend);
            ApplyRot(bones.FootR, Quaternion.Euler(footR, 0f, 0f), blend);
            ApplyRot(bones.UpperArmL, Quaternion.Euler(armLEuler), blend);
            ApplyRot(bones.ForearmL, Quaternion.Euler(forearmLEuler), blend);
            ApplyRot(bones.UpperArmR, Quaternion.Euler(armREuler), blend);
            ApplyRot(bones.ForearmR, Quaternion.Euler(forearmREuler), blend);
            if (bones.HandR != null)
            {
                float punchBlend = isPunchActive ? Mathf.Clamp01(blend * 2.2f) : blend;
                bones.HandR.localPosition = Vector3.Lerp(bones.HandR.localPosition, handRPos, punchBlend);
                bones.HandR.localScale = Vector3.Lerp(bones.HandR.localScale, handRScale, punchBlend);
            }
        }

        private void AnimateDinosaur(ArenaCombatSimulation sim, float blend)
        {
            bool isWalking = sim.EnemyState == ArenaEnemyState.Approaching
                || sim.EnemyState == ArenaEnemyState.Retreating
                || sim.EnemyState == ArenaEnemyState.Evading;
            bool isWindup = sim.EnemyState == ArenaEnemyState.Windup;
            bool isAttack = sim.EnemyState == ArenaEnemyState.ActiveAttack;
            bool isHit = sim.EnemyState == ArenaEnemyState.HitReaction;

            float hipsY = 0f;
            float hipsZ = 0f;
            float hipsPitch = 0f;
            float spinePitch = 0f;
            float neckPitch = 0f;
            float headPitch = 0f;
            float jawOpen = 0f;
            float thighL = 0f, thighR = 0f;
            float tail1Yaw = Mathf.Sin(animTimer * 4.2f) * 7f;
            float tail2Yaw = Mathf.Sin(animTimer * 4.2f - 0.7f) * 12f;
            float tail3Yaw = Mathf.Sin(animTimer * 4.2f - 1.4f) * 18f;
            float tailPitch = 0f;

            if (isHit)
            {
                // Reacao de dor: joga cabeca/pescoco para tras, abre a boca e balanca a cauda sem torcer a base
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyHitReactionTicks));
                float pain = Mathf.Sin(t * Mathf.PI);
                hipsY = -0.025f * pain;
                hipsZ = -0.05f * pain;
                hipsPitch = -14f * pain;
                spinePitch = -18f * pain;
                neckPitch = -16f * pain;
                headPitch = -14f * pain;
                jawOpen = 36f * pain;
                tail1Yaw = Mathf.Sin(t * Mathf.PI * 3f) * 14f;
                tail2Yaw = Mathf.Sin(t * Mathf.PI * 3f - 0.5f) * 22f;
                tail3Yaw = Mathf.Sin(t * Mathf.PI * 3f - 1.0f) * 30f;
                thighL = -16f * pain;
                thighR = 14f * pain;
            }
            else if (isWindup)
            {
                // CAGUETE (Telegraph): balancada rapida e organica na ponta da cauda (base estavel sem vincar barriga!)
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyWindupTicks));
                float rapidWag = Mathf.Sin(animTimer * 32f);
                hipsY = -0.04f * t;
                hipsZ = -0.06f * t;
                spinePitch = -12f * t;
                neckPitch = -14f * t;
                jawOpen = 38f * t; // Abre a bocarra preparando a mordida
                tail1Yaw = rapidWag * 12f;
                tail2Yaw = Mathf.Sin(animTimer * 32f - 0.6f) * 22f;
                tail3Yaw = Mathf.Sin(animTimer * 32f - 1.2f) * 32f;
                tailPitch = 6f * t;
                thighL = -14f * t;
                thighR = 14f * t;
            }
            else if (isAttack)
            {
                // GOLPE (Mordida / Investida): bote rapido e impactante alcancando a menina!
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyActiveTicks));
                float lunge = Mathf.Pow(Mathf.Sin(t * Mathf.PI), 0.68f);
                hipsY = -0.045f * lunge;
                hipsZ = 0.14f * lunge;
                hipsPitch = 10f * lunge;
                spinePitch = 24f * lunge;
                neckPitch = 22f * lunge;
                headPitch = 10f * lunge;
                // Abre no inicio do bote e abocanha com forca no pico (sem vincar o focinho)!
                jawOpen = t < 0.45f ? Mathf.Lerp(20f, 24f, t / 0.45f) : Mathf.Lerp(24f, -2f, (t - 0.45f) / 0.55f);
                tailPitch = -6f * lunge;
                tail1Yaw = Mathf.Sin(t * Mathf.PI * 2f) * 8f;
                tail2Yaw = Mathf.Sin(t * Mathf.PI * 2f - 0.6f) * 14f;
                tail3Yaw = Mathf.Sin(t * Mathf.PI * 2f - 1.2f) * 20f;
                thighL = -18f * lunge;
                thighR = 16f * lunge;
            }
            else if (isWalking)
            {
                // Passos alternados ao caminhar + balanco natural da cauda
                float cycle = animTimer * 11f;
                float s = Mathf.Sin(cycle);
                hipsY = Mathf.Abs(s) * 0.03f;
                hipsPitch = 6f;
                spinePitch = 6f + Mathf.Abs(s) * 4f;
                thighL = s * 22f;
                thighR = -s * 22f;
                tail1Yaw = s * 10f;
                tail2Yaw = Mathf.Sin(cycle - 0.6f) * 16f;
                tail3Yaw = Mathf.Sin(cycle - 1.2f) * 22f;
            }
            else
            {
                float breath = Mathf.Sin(animTimer * 3.2f);
                hipsY = breath * 0.012f;
                spinePitch = 4f + breath * 3f;
                jawOpen = 4f + Mathf.Max(0f, breath) * 6f;
                thighL = -10f;
                thighR = 10f;
            }

            ApplyTransform(bones.Hips, hipsRestLocalPos + new Vector3(0f, hipsY, hipsZ), Quaternion.Euler(hipsPitch, 0f, 0f), blend);
            ApplyRot(bones.Spine, Quaternion.Euler(spinePitch, 0f, 0f), blend);
            ApplyRot(bones.Neck, Quaternion.Euler(neckPitch, 0f, 0f), blend);
            ApplyRot(bones.Head, Quaternion.Euler(headPitch, 0f, 0f), blend);
            ApplyRot(bones.Jaw, Quaternion.Euler(jawOpen, 0f, 0f), blend);
            ApplyRot(bones.ThighL, Quaternion.Euler(thighL, 0f, 0f), blend);
            ApplyRot(bones.ThighR, Quaternion.Euler(thighR, 0f, 0f), blend);
            ApplyRot(bones.Tail1, Quaternion.Euler(tailPitch, tail1Yaw, 0f), blend);
            ApplyRot(bones.Tail2, Quaternion.Euler(tailPitch * 0.6f, tail2Yaw, 0f), blend);
            ApplyRot(bones.Tail3, Quaternion.Euler(tailPitch * 0.4f, tail3Yaw, 0f), blend);
        }

        private void AnimateBear(ArenaCombatSimulation sim, Vector3 playerWorldPos, float blend)
        {
            bool isWalking = sim.EnemyState == ArenaEnemyState.Approaching
                || sim.EnemyState == ArenaEnemyState.Retreating
                || sim.EnemyState == ArenaEnemyState.Evading;
            bool isWindup = sim.EnemyState == ArenaEnemyState.Windup;
            bool isAttack = sim.EnemyState == ArenaEnemyState.ActiveAttack;
            bool isHit = sim.EnemyState == ArenaEnemyState.HitReaction;

            float hipsY = 0f;
            float hipsZ = 0f;
            float hipsPitch = 0f;
            float hipsYaw = 0f;
            float spinePitch = 0f;
            float chestPitch = 0f;

            float thighL = -12f, shinL = 16f, footL = -4f;
            float thighR = 12f, shinR = 10f, footR = -12f;

            Vector3 armLEuler = new Vector3(-12f, 0f, -8f);
            Vector3 forearmLEuler = new Vector3(-20f, 0f, 0f);
            Vector3 armREuler = new Vector3(-12f, 0f, 8f);
            Vector3 forearmREuler = new Vector3(-20f, 0f, 0f);

            if (isHit)
            {
                // Reacao de dor pesada: tronco recua, joelhos dobram absorvendo, bracos sobem se protegendo
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyHitReactionTicks));
                float pain = Mathf.Sin(t * Mathf.PI);
                hipsY = -0.055f * pain;
                hipsZ = -0.06f * pain;
                hipsPitch = -10f * pain;
                spinePitch = -16f * pain;
                chestPitch = -14f * pain;
                thighL = -32f * pain;
                shinL = 44f * pain;
                footL = -12f * pain;
                thighR = 26f * pain;
                shinR = 16f * pain;
                armLEuler = new Vector3(-42f * pain, 14f * pain, -18f * pain);
                forearmLEuler = new Vector3(-38f * pain, 0f, 0f);
                armREuler = new Vector3(-42f * pain, -14f * pain, 18f * pain);
                forearmREuler = new Vector3(-38f * pain, 0f, 0f);
            }
            else if (isWindup)
            {
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyWindupTicks));
                float w = 1f - Mathf.Pow(1f - t, 2f);
                if (sim.ActiveAttackKind == ArenaAttackKind.BearHug)
                {
                    // Abre os dois bracos enormes dobrando cotovelos preparando o abraco/patada dupla!
                    hipsY = -0.05f * w;
                    spinePitch = -10f * w;
                    chestPitch = -10f * w;
                    thighL = -34f * w;
                    shinL = 42f * w;
                    thighR = 26f * w;
                    shinR = 14f * w;
                    armLEuler = new Vector3(-28f * w, -28f * w, -34f * w);
                    forearmLEuler = new Vector3(-32f * w, 18f * w, 0f);
                    armREuler = new Vector3(-28f * w, 28f * w, 34f * w);
                    forearmREuler = new Vector3(-32f * w, -18f * w, 0f);
                }
                else if (sim.ActiveAttackKind == ArenaAttackKind.ToyLeap)
                {
                    // Agacha profundamente nos joelhos pronto para o salto sismico!
                    hipsY = -0.10f * w;
                    spinePitch = 18f * w;
                    thighL = -52f * w;
                    shinL = 74f * w;
                    footL = -22f * w;
                    thighR = 32f * w;
                    shinR = 54f * w;
                    armLEuler = new Vector3(28f * w, 0f, -26f * w);
                    armREuler = new Vector3(28f * w, 0f, 26f * w);
                }
                else
                {
                    // BellyCharge: joga bracos para tras estufando o peito
                    hipsY = -0.05f * w;
                    spinePitch = -14f * w;
                    thighL = -30f * w;
                    shinL = 38f * w;
                    thighR = 28f * w;
                    armLEuler = new Vector3(36f * w, 0f, -28f * w);
                    armREuler = new Vector3(36f * w, 0f, 28f * w);
                }
            }
            else if (isAttack)
            {
                float t = Mathf.Clamp01(sim.EnemyStateTick / (float)Mathf.Max(1, sim.Tuning.EnemyActiveTicks));
                float strike = Mathf.Pow(Mathf.Sin(t * Mathf.PI), 0.68f);
                if (sim.ActiveAttackKind == ArenaAttackKind.BearHug)
                {
                    // Fecha os dois bracos com forca esmagadora a frente (golpe real dos bracos!)
                    hipsY = -0.06f * strike;
                    hipsZ = 0.08f * strike;
                    spinePitch = 16f * strike;
                    chestPitch = 14f * strike;
                    thighL = -44f * strike;
                    shinL = 52f * strike;
                    footL = -8f * strike;
                    thighR = 36f * strike;
                    shinR = 10f * strike;
                    armLEuler = new Vector3(-58f * strike, 28f * strike, 8f * strike);
                    forearmLEuler = new Vector3(-32f * strike, 22f * strike, 0f);
                    armREuler = new Vector3(-58f * strike, -28f * strike, -8f * strike);
                    forearmREuler = new Vector3(-32f * strike, -22f * strike, 0f);
                }
                else if (sim.ActiveAttackKind == ArenaAttackKind.ToyLeap)
                {
                    // No ar ergue os dois bracos acima da cabeca e desaba golpeando no pouso!
                    bool airborne = !sim.EnemyOnGround;
                    hipsY = airborne ? 0.02f : -0.09f;
                    spinePitch = airborne ? -12f : 24f;
                    thighL = airborne ? -38f : -50f;
                    shinL = airborne ? 58f : 64f;
                    thighR = airborne ? 26f : 38f;
                    shinR = airborne ? 44f : 12f;
                    armLEuler = airborne ? new Vector3(-135f, 0f, -22f) : new Vector3(-55f, 18f, -12f);
                    forearmLEuler = new Vector3(-24f, 0f, 0f);
                    armREuler = airborne ? new Vector3(-135f, 0f, 22f) : new Vector3(-55f, -18f, 12f);
                    forearmREuler = new Vector3(-24f, 0f, 0f);
                }
                else
                {
                    // BellyCharge: bracos golpeando para frente enquanto avanca
                    float cycle = animTimer * 14f;
                    float s = Mathf.Sin(cycle);
                    hipsY = -0.04f;
                    spinePitch = 12f;
                    thighL = s * 38f;
                    shinL = Mathf.Max(8f, -Mathf.Cos(cycle) * 44f + 16f);
                    thighR = -s * 38f;
                    shinR = Mathf.Max(8f, Mathf.Cos(cycle) * 44f + 16f);
                    armLEuler = new Vector3(-68f + s * 22f, 18f, -16f);
                    forearmLEuler = new Vector3(-34f, 0f, 0f);
                    armREuler = new Vector3(-68f - s * 22f, -18f, 16f);
                    forearmREuler = new Vector3(-34f, 0f, 0f);
                }
            }
            else if (isWalking)
            {
                // Passo pesado de urso articulando quadril, joelhos e pes + balanco de bracos
                float cycle = animTimer * (sim.BearEnraged ? 10.5f : 8.2f);
                float s = Mathf.Sin(cycle);
                float c = Mathf.Cos(cycle);
                hipsY = Mathf.Abs(s) * 0.032f - 0.02f;
                hipsYaw = s * 10f;
                spinePitch = 6f;
                thighL = s * 32f;
                shinL = Mathf.Max(6f, -c * 42f + 16f);
                footL = -(thighL + shinL) * 0.35f;
                thighR = -s * 32f;
                shinR = Mathf.Max(6f, c * 42f + 16f);
                footR = -(thighR + shinR) * 0.35f;
                armLEuler = new Vector3(-s * 28f - 10f, 0f, -10f);
                forearmLEuler = new Vector3(-26f, 0f, 0f);
                armREuler = new Vector3(s * 28f - 10f, 0f, 10f);
                forearmREuler = new Vector3(-26f, 0f, 0f);
            }
            else
            {
                float breath = Mathf.Sin(animTimer * (sim.BearEnraged ? 5.2f : 2.8f));
                hipsY = breath * 0.015f - 0.015f;
                spinePitch = 4f + breath * 3f;
                armLEuler = new Vector3(-16f + breath * 5f, 0f, -12f);
                forearmLEuler = new Vector3(-26f, 0f, 0f);
                armREuler = new Vector3(-16f + breath * 5f, 0f, 12f);
                forearmREuler = new Vector3(-26f, 0f, 0f);
            }

            ApplyTransform(bones.Hips, hipsRestLocalPos + new Vector3(0f, hipsY, hipsZ), Quaternion.Euler(hipsPitch, hipsYaw, 0f), blend);
            ApplyRot(bones.Spine, Quaternion.Euler(spinePitch, 0f, 0f), blend);
            ApplyRot(bones.Chest, Quaternion.Euler(chestPitch, 0f, 0f), blend);

            ApplyRot(bones.ThighL, Quaternion.Euler(thighL, 0f, 0f), blend);
            ApplyRot(bones.ShinL, Quaternion.Euler(shinL, 0f, 0f), blend);
            ApplyRot(bones.FootL, Quaternion.Euler(footL, 0f, 0f), blend);

            ApplyRot(bones.ThighR, Quaternion.Euler(thighR, 0f, 0f), blend);
            ApplyRot(bones.ShinR, Quaternion.Euler(shinR, 0f, 0f), blend);
            ApplyRot(bones.FootR, Quaternion.Euler(footR, 0f, 0f), blend);

            ApplyRot(bones.UpperArmL, Quaternion.Euler(armLEuler), blend);
            ApplyRot(bones.ForearmL, Quaternion.Euler(forearmLEuler), blend);
            ApplyRot(bones.UpperArmR, Quaternion.Euler(armREuler), blend);
            ApplyRot(bones.ForearmR, Quaternion.Euler(forearmREuler), blend);

            // CABECA DO URSO OLHA FIXAMENTE PARA A MENINA O TEMPO TODO!
            if (bones.Head != null && bones.Head.parent != null)
            {
                Vector3 targetEye = playerWorldPos + new Vector3(0f, 1.45f, 0f);
                Vector3 localDir = bones.Head.parent.InverseTransformDirection((targetEye - bones.Head.position).normalized);
                if (localDir.sqrMagnitude > 0.001f)
                {
                    float pitchToGirl = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(localDir.y, -0.85f, 0.85f)) * Mathf.Rad2Deg, -18f, 22f);
                    float yawToGirl = Mathf.Clamp(Mathf.Atan2(localDir.x, Mathf.Max(0.15f, localDir.z)) * Mathf.Rad2Deg, -26f, 26f);
                    ApplyRot(bones.Head, Quaternion.Euler(pitchToGirl, yawToGirl, 0f), Mathf.Clamp01(blend * 1.4f));
                }
            }
        }

        private static void ApplyRot(Transform t, Quaternion target, float blend)
        {
            if (t != null)
            {
                t.localRotation = Quaternion.Slerp(t.localRotation, target, blend);
            }
        }

        private static void ApplyTransform(Transform t, Vector3 pos, Quaternion rot, float blend)
        {
            if (t != null)
            {
                t.localPosition = Vector3.Lerp(t.localPosition, pos, blend);
                t.localRotation = Quaternion.Slerp(t.localRotation, rot, blend);
            }
        }
    }
}
