using System;
using System.Collections.Generic;

namespace YunaLegend.Arena.Core
{
    public enum ArenaEnemyId
    {
        Train = 0,
        Robot = 1,
        Dinosaur = 2,
        Bear = 3
    }

    public enum ArenaEnemyState
    {
        Spawning,
        Idle,
        Approaching,
        Retreating,
        Evading,
        Windup,
        ActiveAttack,
        Recovery,
        HitReaction,
        Defeated
    }

    public enum ArenaAttackKind
    {
        None,
        SteamBlast,
        FrontCharge,
        LaserBeam,
        RocketPunch,
        Bite,
        Rush,
        BearHug,
        BellyCharge,
        ToyLeap
    }

    public enum ArenaSpecialPhase
    {
        Inactive,
        Charge,
        Pullback,
        Launch,
        Impact,
        Fade
    }

    public enum ArenaBattleOutcome
    {
        Ongoing,
        Victory,
        Defeat
    }

    public struct ArenaInputState
    {
        public float MoveX;
        public bool JumpPressed;
        public bool CrouchHeld;
        public bool AttackPressed;
        public bool SpecialPressed;
    }

    public sealed class ArenaTuning
    {
        public float LeftBound { get; set; } = 160f;
        public float RightBound { get; set; } = 1440f;
        public float GroundY { get; set; } = 690f;
        public float PlayerSpeed { get; set; } = 6.2f;
        public float JumpSpeed { get; set; } = 11f;
        public float Gravity { get; set; } = 0.32f;
        public float PlayerBodyWidth { get; set; } = 120f;
        public float PlayerBodyHeight { get; set; } = 140f;
        public float SwordContactScale { get; set; } = 1.1f;
        public int SwordDurationTicks { get; set; } = 18;
        public int SwordWindupTicks { get; set; } = 6;
        public int SwordActiveTicks { get; set; } = 6;
        public int SwordCooldownTicks { get; set; } = 36;
        public int BlockRecoveryTicks { get; set; } = 48;
        public float AttackMoveScale { get; set; } = 0.2f;
        public float RecoveryMoveScale { get; set; } = 0.55f;
        public int EnemyWindupTicks { get; set; } = 54;
        public int EnemyInterruptibleTicks { get; set; } = 18;
        public int EnemyActiveTicks { get; set; } = 18;
        public int PlayerImmunityTicks { get; set; } = 48;
        public int PlayerHitRecoveryTicks { get; set; } = 24;
        public int EnemyHitReactionTicks { get; set; } = 20;
        public int EnemyDeathTicks { get; set; } = 60;
        public int EnemySpawnTicks { get; set; } = 60;
        public float KnockbackDistance { get; set; } = 173.6f;
        public float BodySeparationPadding { get; set; } = 32f;
        public float KnockbackFriction { get; set; } = 0.78f;
        public float SwordHitEnemyKnockbackVelocity { get; set; } = 12.5f;
        public float EnemyHitPlayerKnockbackVelocity { get; set; } = 25.0f;
        public float EnemyBlockPlayerKnockbackVelocity { get; set; } = 10.0f;
        public int MaxPlayerHp { get; set; } = 3;
    }

    public sealed class ArenaEnemySpec
    {
        public ArenaEnemyId Id { get; set; }
        public string Title { get; set; }
        public int MaxHp { get; set; }
        public float WalkSpeed { get; set; }
        public float ChargeSpeed { get; set; }
        public float BodyWidth { get; set; }
        public float BodyHeight { get; set; }
        public int IdleTicks { get; set; }
        public int RetreatTicks { get; set; }
        public int RecoveryTicks { get; set; }
        public float EvadeSpeed { get; set; }
        public int EvadeTicks { get; set; }
        public int EvadeCooldownTicks { get; set; }
        public bool PressureChainEnabled { get; set; }
        public ArenaAttackKind PrimaryAttack { get; set; }
        public ArenaAttackKind SecondaryAttack { get; set; }
        public ArenaAttackKind TertiaryAttack { get; set; }
    }

    public static class ArenaSpecialSpec
    {
        public const int ChargeEndTick = 78;
        public const int PullbackEndTick = 88;
        public const int LaunchTick = 94;
        public const int ImpactTick = 118;
        public const int FadeStartTick = 138;
        public const int TotalDurationTicks = 156;
    }

    public static class ArenaCatalog
    {
        public static readonly ArenaEnemyId[] WaveOrder =
        {
            ArenaEnemyId.Train,
            ArenaEnemyId.Robot,
            ArenaEnemyId.Dinosaur,
            ArenaEnemyId.Bear
        };

        public static ArenaTuning CreateTuning(bool easyMode)
        {
            return new ArenaTuning
            {
                SwordContactScale = easyMode ? 1.43f : 1.1f
            };
        }

        public static IReadOnlyDictionary<ArenaEnemyId, ArenaEnemySpec> CreateEnemies(bool easyMode)
        {
            float walkScale = easyMode ? 0.80f : 1f;
            float chargeScale = easyMode ? 0.85f : 1f;
            float idleScale = easyMode ? 1.70f : 1f;
            float retreatScale = easyMode ? 1.50f : 1f;
            float recoveryScale = easyMode ? 1.55f : 1f;
            float evadeCdScale = easyMode ? 1.60f : 1f;
            bool chain = !easyMode;

            return new Dictionary<ArenaEnemyId, ArenaEnemySpec>
            {
                [ArenaEnemyId.Train] = new ArenaEnemySpec
                {
                    Id = ArenaEnemyId.Train,
                    Title = "Locomotiva de Cerco",
                    MaxHp = 3,
                    WalkSpeed = 3.0f * walkScale,
                    ChargeSpeed = 7.0f * chargeScale,
                    BodyWidth = 340f,
                    BodyHeight = 225f,
                    IdleTicks = (int)Math.Round(45 * idleScale),
                    RetreatTicks = (int)Math.Round(30 * retreatScale),
                    RecoveryTicks = (int)Math.Round(36 * recoveryScale),
                    EvadeSpeed = 0f,
                    EvadeTicks = 0,
                    EvadeCooldownTicks = 0,
                    PressureChainEnabled = false,
                    PrimaryAttack = ArenaAttackKind.SteamBlast,
                    SecondaryAttack = ArenaAttackKind.FrontCharge,
                    TertiaryAttack = ArenaAttackKind.None
                },
                [ArenaEnemyId.Robot] = new ArenaEnemySpec
                {
                    Id = ArenaEnemyId.Robot,
                    Title = "Sentinela de Lata",
                    MaxHp = 4,
                    WalkSpeed = 3.8f * walkScale,
                    ChargeSpeed = 7.5f * chargeScale,
                    BodyWidth = 140f,
                    BodyHeight = 250f,
                    IdleTicks = (int)Math.Round(30 * idleScale),
                    RetreatTicks = (int)Math.Round(24 * retreatScale),
                    RecoveryTicks = (int)Math.Round(32 * recoveryScale),
                    EvadeSpeed = 14.0f,
                    EvadeTicks = 8,
                    EvadeCooldownTicks = (int)Math.Round(150 * evadeCdScale),
                    PressureChainEnabled = chain,
                    PrimaryAttack = ArenaAttackKind.RocketPunch,
                    SecondaryAttack = ArenaAttackKind.LaserBeam,
                    TertiaryAttack = ArenaAttackKind.None
                },
                [ArenaEnemyId.Dinosaur] = new ArenaEnemySpec
                {
                    Id = ArenaEnemyId.Dinosaur,
                    Title = "Dragão de Brinquedo",
                    MaxHp = 4,
                    WalkSpeed = 4.8f * walkScale,
                    ChargeSpeed = 8.2f * chargeScale,
                    BodyWidth = 250f,
                    BodyHeight = 250f,
                    IdleTicks = (int)Math.Round(20 * idleScale),
                    RetreatTicks = (int)Math.Round(18 * retreatScale),
                    RecoveryTicks = (int)Math.Round(28 * recoveryScale),
                    EvadeSpeed = 11.5f,
                    EvadeTicks = 8,
                    EvadeCooldownTicks = (int)Math.Round(120 * evadeCdScale),
                    PressureChainEnabled = chain,
                    PrimaryAttack = ArenaAttackKind.Bite,
                    SecondaryAttack = ArenaAttackKind.Rush,
                    TertiaryAttack = ArenaAttackKind.None
                },
                [ArenaEnemyId.Bear] = new ArenaEnemySpec
                {
                    Id = ArenaEnemyId.Bear,
                    Title = "Guardião de Pelúcia",
                    MaxHp = 8,
                    WalkSpeed = 5.4f * walkScale,
                    ChargeSpeed = 9.0f * chargeScale,
                    BodyWidth = 235f,
                    BodyHeight = 325f,
                    IdleTicks = (int)Math.Round(12 * idleScale),
                    RetreatTicks = (int)Math.Round(14 * retreatScale),
                    RecoveryTicks = (int)Math.Round(24 * recoveryScale),
                    EvadeSpeed = 12.5f,
                    EvadeTicks = 8,
                    EvadeCooldownTicks = (int)Math.Round(95 * evadeCdScale),
                    PressureChainEnabled = chain,
                    PrimaryAttack = ArenaAttackKind.BearHug,
                    SecondaryAttack = ArenaAttackKind.BellyCharge,
                    TertiaryAttack = ArenaAttackKind.ToyLeap
                }
            };
        }

        public static float GetSwordRange(ArenaEnemySpec enemy, ArenaTuning tuning)
        {
            if (enemy == null || tuning == null)
            {
                return 209f;
            }

            float minDistance = (tuning.PlayerBodyWidth * 0.5f) + (enemy.BodyWidth * 0.5f);
            float nominal = 190f * tuning.SwordContactScale;
            float derived = (minDistance + 40f) * tuning.SwordContactScale;
            return Math.Max(nominal, derived);
        }
    }
}
