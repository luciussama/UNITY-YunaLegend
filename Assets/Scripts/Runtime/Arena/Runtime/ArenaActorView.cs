using UnityEngine;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Apresentacao 2.5D da menina e do brinquedo adversario na Arena Imaginaria.
    /// Atualiza sprites fatiados, espelhamento, squash/inclinacao de impacto e efeitos de golpe/bloqueio.
    /// </summary>
    public sealed class ArenaActorView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private SpriteRenderer slashArcRenderer;

        private float animTimer;
        private float hitFlashTimer;
        private float slashFadeTimer;
        private Vector3 baseScale = Vector3.one;

        public void Configure(
            SpriteRenderer renderer,
            Transform root,
            ParticleSystem particles = null,
            SpriteRenderer slashRenderer = null)
        {
            spriteRenderer = renderer;
            visualRoot = root;
            impactParticles = particles;
            slashArcRenderer = slashRenderer;
            if (visualRoot != null)
            {
                baseScale = visualRoot.localScale;
            }
        }

        public void TriggerImpactFeedback(Color burstColor)
        {
            hitFlashTimer = 0.16f;
            if (impactParticles != null)
            {
                var main = impactParticles.main;
                main.startColor = burstColor;
                impactParticles.Play();
            }
        }

        public void TriggerSlashArc()
        {
            slashFadeTimer = 0.22f;
        }

        public void UpdatePlayerVisual(ArenaCombatSimulation sim, ArenaSpriteCatalog catalog, float moveX, float dt)
        {
            if (sim == null || catalog == null || spriteRenderer == null)
            {
                return;
            }

            transform.position = ArenaCoordinateMapper.ToWorldPosition(sim.PlayerX, sim.PlayerY, 0f);
            spriteRenderer.flipX = sim.PlayerFacing < 0;

            animTimer += dt;
            Sprite[] frames = ResolvePlayerFrames(sim, catalog, moveX, out float fps, out bool loop);
            spriteRenderer.sprite = SampleFrame(frames, animTimer, fps, loop);

            if (sim.IsSpecialActive)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 18f);
                spriteRenderer.color = Color.Lerp(Color.white, new Color(0.85f, 0.95f, 1f), pulse);
            }
            else if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= dt;
                spriteRenderer.color = new Color(1f, 0.55f, 0.55f, 1f);
            }
            else if (sim.PlayerImmunity > 0 && (Mathf.FloorToInt(Time.time * 18f) % 2 == 0))
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.65f);
            }
            else
            {
                spriteRenderer.color = Color.white;
            }

            UpdateSlashArc(sim.PlayerFacing, dt);
        }

        public void UpdateEnemyVisual(ArenaCombatSimulation sim, ArenaSpriteCatalog catalog, float dt)
        {
            if (sim == null || catalog == null || spriteRenderer == null)
            {
                return;
            }

            transform.position = ArenaCoordinateMapper.ToWorldPosition(sim.EnemyX, sim.EnemyY, 0.05f);
            // Os sprites dos inimigos olham naturalmente para a direita; espelhamos quando olham para a esquerda.
            spriteRenderer.flipX = sim.EnemyFacing < 0;

            animTimer += dt;
            if (catalog.TryGetEnemySet(sim.CurrentEnemySpec.Id, out var set))
            {
                Sprite[] frames = ResolveEnemyFrames(sim, set, out float fps, out bool loop);
                spriteRenderer.sprite = SampleFrame(frames, animTimer, fps, loop);
            }

            float spawnScale = sim.EnemySpawnScale;
            float squashX = 1f;
            float squashY = 1f;
            float tiltZ = 0f;

            if (sim.EnemyState == ArenaEnemyState.HitReaction)
            {
                float t = Mathf.Clamp01(sim.EnemyStateTick / 16f);
                float impulse = Mathf.Sin(t * Mathf.PI);
                squashX = 1f + impulse * 0.14f;
                squashY = 1f - impulse * 0.12f;
                tiltZ = -sim.EnemyFacing * impulse * 9f;
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = new Vector3(
                    baseScale.x * spawnScale * squashX,
                    baseScale.y * spawnScale * squashY,
                    baseScale.z * spawnScale);
                visualRoot.localRotation = Quaternion.Euler(0f, 0f, tiltZ);
            }

            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= dt;
                spriteRenderer.color = new Color(1f, 0.85f, 0.72f, 1f);
            }
            else if (sim.BearEnraged && sim.CurrentEnemySpec.Id == ArenaEnemyId.Bear)
            {
                spriteRenderer.color = new Color(1f, 0.78f, 0.72f, 1f);
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }

        private void UpdateSlashArc(int facing, float dt)
        {
            if (slashArcRenderer == null)
            {
                return;
            }

            if (slashFadeTimer <= 0f)
            {
                slashArcRenderer.enabled = false;
                return;
            }

            slashFadeTimer -= dt;
            slashArcRenderer.enabled = true;
            slashArcRenderer.flipX = facing < 0;
            slashArcRenderer.transform.localPosition = new Vector3(facing * 1.1f, 0.9f, -0.05f);
            float alpha = Mathf.Clamp01(slashFadeTimer / 0.22f);
            slashArcRenderer.color = new Color(0.95f, 0.92f, 1f, alpha * 0.9f);
        }

        private static Sprite[] ResolvePlayerFrames(
            ArenaCombatSimulation sim,
            ArenaSpriteCatalog catalog,
            float moveX,
            out float fps,
            out bool loop)
        {
            if (sim.PlayerHitRecovery > 0 && catalog.GirlDamage != null && catalog.GirlDamage.Length > 0)
            {
                fps = 12f;
                loop = false;
                return catalog.GirlDamage;
            }

            if (sim.SwordTick > 0 && catalog.GirlAttack != null && catalog.GirlAttack.Length > 0)
            {
                fps = 18f;
                loop = false;
                return catalog.GirlAttack;
            }

            if (sim.IsGuarding && catalog.GirlGuard != null && catalog.GirlGuard.Length > 0)
            {
                fps = 12f;
                loop = false;
                return catalog.GirlGuard;
            }

            if (sim.IsCrouching && catalog.GirlCrouch != null && catalog.GirlCrouch.Length > 0)
            {
                fps = 8f;
                loop = false;
                return catalog.GirlCrouch;
            }

            if (!sim.PlayerOnGround && catalog.GirlJump != null && catalog.GirlJump.Length > 0)
            {
                fps = 8f;
                loop = false;
                return catalog.GirlJump;
            }

            if (Mathf.Abs(moveX) > 0.1f && catalog.GirlRun != null && catalog.GirlRun.Length > 0)
            {
                fps = 10f;
                loop = true;
                return catalog.GirlRun;
            }

            fps = 6f;
            loop = true;
            return catalog.GirlIdle;
        }

        private static Sprite[] ResolveEnemyFrames(
            ArenaCombatSimulation sim,
            ArenaEnemySpriteSet set,
            out float fps,
            out bool loop)
        {
            switch (sim.EnemyState)
            {
                case ArenaEnemyState.Defeated:
                    fps = 8f;
                    loop = false;
                    return Fallback(set.Death, set.Idle);
                case ArenaEnemyState.HitReaction:
                    fps = 12f;
                    loop = false;
                    return Fallback(set.Hit, set.Idle);
                case ArenaEnemyState.Windup:
                case ArenaEnemyState.ActiveAttack:
                    fps = 10f;
                    loop = true;
                    return ResolveAttackFrames(sim.ActiveAttackKind, set);
                case ArenaEnemyState.Approaching:
                case ArenaEnemyState.Retreating:
                case ArenaEnemyState.Evading:
                    fps = 9f;
                    loop = true;
                    return Fallback(set.Walk, set.Idle);
                default:
                    fps = 6f;
                    loop = true;
                    return set.Idle;
            }
        }

        private static Sprite[] ResolveAttackFrames(ArenaAttackKind kind, ArenaEnemySpriteSet set)
        {
            return kind switch
            {
                ArenaAttackKind.FrontCharge or ArenaAttackKind.LaserBeam or ArenaAttackKind.Rush or ArenaAttackKind.BellyCharge
                    => Fallback(set.SecondaryAttack, set.PrimaryAttack, set.Idle),
                ArenaAttackKind.ToyLeap
                    => Fallback(set.TertiaryAttack, set.SecondaryAttack, set.Idle),
                _ => Fallback(set.PrimaryAttack, set.Idle)
            };
        }

        private static Sprite[] Fallback(params Sprite[][] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] != null && candidates[i].Length > 0)
                {
                    return candidates[i];
                }
            }

            return null;
        }

        private static Sprite SampleFrame(Sprite[] frames, float time, float fps, bool loop)
        {
            if (frames == null || frames.Length == 0)
            {
                return null;
            }

            int index = Mathf.FloorToInt(time * fps);
            index = loop
                ? Mathf.Abs(index) % frames.Length
                : Mathf.Clamp(index, 0, frames.Length - 1);
            return frames[index];
        }
    }
}
