using System;
using System.Collections.Generic;

namespace YunaLegend.Arena.Core
{
    /// <summary>
    /// Simulacao deterministica de combate 60 Hz da Fase 3 (Arena Imaginaria).
    /// Mantem fisica 1D/2D (x, y), defesa direcional, transferencia de coracoes,
    /// IA dos 4 brinquedos e especial da Espada Arco-Iris sem dependencias de engine.
    /// </summary>
    public sealed class ArenaCombatSimulation
    {
        private const float FixedStepSeconds = 1f / 60f;
        public const float ChestDepositTargetSimX = 270f;
        private const float ChestOriginX = 1120f;
        private const float PlayerStartX = 420f;
        private const float EnemyStartX = 1120f;

        private readonly ArenaTuning tuning;
        private readonly IReadOnlyDictionary<ArenaEnemyId, ArenaEnemySpec> enemySpecs;

        private float accumulator;
        private int waveIndex;
        private int specialTick;
        private bool specialImpactApplied;
        private int attackChoiceCounter;
        private bool chainPending;
        private int comboResetTimer;
        private int nextComboStep;

        public ArenaTuning Tuning => tuning;
        public ArenaEnemySpec CurrentEnemySpec { get; private set; }
        public ArenaBattleOutcome Outcome { get; private set; } = ArenaBattleOutcome.Ongoing;
        public bool EasyMode { get; }
        public int WaveIndex => waveIndex;
        public int TotalWaves => ArenaCatalog.WaveOrder.Length;

        // Estado da menina
        public float PlayerX { get; private set; }
        public float PlayerY { get; private set; }
        public float PlayerVy { get; private set; }
        public float PlayerKnockbackVx { get; private set; }
        public int PlayerFacing { get; private set; } = 1;
        public int PlayerHp { get; private set; }
        public int PlayerMaxHp => tuning.MaxPlayerHp;
        public bool IsCrouching { get; private set; }
        public bool IsGuarding { get; private set; }
        public int SwordTick { get; private set; }
        public int SwordComboStep { get; private set; }
        public int SwordCooldown { get; private set; }
        public int PlayerImmunity { get; private set; }
        public int PlayerHitRecovery { get; private set; }
        public int BlockRecovery { get; private set; }
        public bool SwordHitAppliedThisSwing { get; private set; }

        // Estado do inimigo atual
        public float EnemyX { get; private set; }
        public float EnemyY { get; private set; }
        public float EnemyVy { get; private set; }
        public float EnemyKnockbackVx { get; private set; }
        public int EnemyFacing { get; private set; } = -1;
        public int EnemyHp { get; private set; }
        public int EnemyMaxHp => CurrentEnemySpec.MaxHp;
        public float EnemySpawnScale { get; private set; } = 0.16f;
        public float EnemyDepositArcHeight { get; private set; }
        public float EnemyDepositPitchDegrees { get; private set; }
        public ArenaEnemyState EnemyState { get; private set; }
        public int EnemyStateTick { get; private set; }
        public ArenaAttackKind ActiveAttackKind { get; private set; }
        public int EvadeCooldown { get; private set; }
        public bool EnemyHitAppliedThisAttack { get; private set; }
        public bool EnemyProtectedResponse { get; private set; }
        public bool BearEnraged { get; private set; }
        private float defeatedStartX;

        // Especial Espada Arco-Iris
        public bool SpecialUsed { get; private set; }
        public ArenaSpecialPhase SpecialPhase { get; private set; } = ArenaSpecialPhase.Inactive;
        public int SpecialTick => specialTick;

        public bool PlayerOnGround => PlayerY >= tuning.GroundY - 0.01f;
        public bool EnemyOnGround => EnemyY >= tuning.GroundY - 0.01f;
        public bool IsSpecialActive => SpecialPhase != ArenaSpecialPhase.Inactive;
        public string StatusHint { get; private set; } = string.Empty;

        public event Action OnSwordSwing;
        public event Action<bool> OnSwordHit; // true = acertou e curou, false = bloqueado na protecao
        public event Action OnAttackBlocked;
        public event Action OnPlayerDamaged;
        public event Action<ArenaEnemyId, ArenaAttackKind> OnEnemyAttackStarted;
        public event Action<ArenaEnemyId, ArenaAttackKind> OnEnemyAttackActive;
        public event Action<ArenaEnemyId> OnEnemyChanged;
        public event Action OnBearEnraged;
        public event Action<ArenaSpecialPhase> OnSpecialPhaseChanged;
        public event Action<ArenaBattleOutcome> OnBattleEnded;

        public ArenaCombatSimulation(bool easyMode = false, bool mobileTouchAssist = false)
        {
            EasyMode = easyMode;
            tuning = ArenaCatalog.CreateTuning(easyMode, mobileTouchAssist);
            enemySpecs = ArenaCatalog.CreateEnemies(easyMode, mobileTouchAssist);
            ResetBattle();
        }

        public void ResetBattle()
        {
            accumulator = 0f;
            waveIndex = 0;
            Outcome = ArenaBattleOutcome.Ongoing;
            SpecialUsed = false;
            SpecialPhase = ArenaSpecialPhase.Inactive;
            specialTick = 0;
            specialImpactApplied = false;
            BearEnraged = false;
            StatusHint = string.Empty;

            PlayerX = PlayerStartX;
            PlayerY = tuning.GroundY;
            PlayerVy = 0f;
            PlayerKnockbackVx = 0f;
            PlayerFacing = 1;
            PlayerHp = tuning.MaxPlayerHp;
            IsCrouching = false;
            IsGuarding = false;
            SwordTick = 0;
            SwordComboStep = 0;
            nextComboStep = 0;
            comboResetTimer = 0;
            SwordCooldown = 0;
            PlayerImmunity = 0;
            PlayerHitRecovery = 0;
            BlockRecovery = 0;
            SwordHitAppliedThisSwing = false;

            StartWave(0);
        }

        public int Step(float dt, ArenaInputState input)
        {
            if (Outcome != ArenaBattleOutcome.Ongoing)
            {
                return 0;
            }

            int ticksExecuted = 0;
            accumulator += Math.Min(dt, 0.1f);
            while (accumulator >= FixedStepSeconds)
            {
                accumulator -= FixedStepSeconds;
                TickFixed(input);
                ticksExecuted++;
                if (Outcome != ArenaBattleOutcome.Ongoing)
                {
                    break;
                }
            }

            return ticksExecuted;
        }

        public void TickFixed(ArenaInputState input)
        {
            if (Outcome != ArenaBattleOutcome.Ongoing)
            {
                return;
            }

            if (IsSpecialActive)
            {
                TickSpecialSequence();
                return;
            }

            if (TryStartSpecial(input))
            {
                return;
            }

            UpdatePlayer(input);
            UpdateEnemy();
            ApplyKnockbackVelocities();
            ResolveBodySeparation();
            UpdateStatusHint();
        }

        private void StartWave(int index)
        {
            waveIndex = index;
            ArenaEnemyId id = ArenaCatalog.WaveOrder[waveIndex];
            CurrentEnemySpec = enemySpecs[id];
            EnemyHp = CurrentEnemySpec.MaxHp;
            EnemyX = ChestOriginX;
            EnemyY = tuning.GroundY;
            EnemyVy = 0f;
            EnemyKnockbackVx = 0f;
            EnemyFacing = -1;
            EnemySpawnScale = 0.16f;
            EnemyDepositArcHeight = 0f;
            EnemyDepositPitchDegrees = 0f;
            defeatedStartX = ChestOriginX;
            EnemyState = ArenaEnemyState.Spawning;
            EnemyStateTick = 0;
            ActiveAttackKind = ArenaAttackKind.None;
            EvadeCooldown = 0;
            EnemyHitAppliedThisAttack = false;
            EnemyProtectedResponse = false;
            chainPending = false;
            OnEnemyChanged?.Invoke(id);
        }

        private bool TryStartSpecial(ArenaInputState input)
        {
            if (!input.SpecialPressed || SpecialUsed)
            {
                return false;
            }

            if (EnemyState == ArenaEnemyState.Spawning || EnemyState == ArenaEnemyState.Defeated)
            {
                return false;
            }

            if (PlayerHitRecovery > 0 || SwordTick > 0)
            {
                return false;
            }

            SpecialUsed = true;
            specialTick = 0;
            specialImpactApplied = false;
            IsGuarding = false;
            SetSpecialPhase(ArenaSpecialPhase.Charge);
            return true;
        }

        private void TickSpecialSequence()
        {
            specialTick++;

            if (specialTick <= ArenaSpecialSpec.ChargeEndTick)
            {
                SetSpecialPhase(ArenaSpecialPhase.Charge);
            }
            else if (specialTick <= ArenaSpecialSpec.PullbackEndTick)
            {
                SetSpecialPhase(ArenaSpecialPhase.Pullback);
            }
            else if (specialTick < ArenaSpecialSpec.ImpactTick)
            {
                SetSpecialPhase(ArenaSpecialPhase.Launch);
            }
            else if (specialTick < ArenaSpecialSpec.FadeStartTick)
            {
                SetSpecialPhase(ArenaSpecialPhase.Impact);
                if (!specialImpactApplied)
                {
                    specialImpactApplied = true;
                    ApplySpecialImpact();
                }
            }
            else if (specialTick < ArenaSpecialSpec.TotalDurationTicks)
            {
                SetSpecialPhase(ArenaSpecialPhase.Fade);
            }
            else
            {
                SetSpecialPhase(ArenaSpecialPhase.Inactive);
                if (EnemyHp <= 0 && EnemyState != ArenaEnemyState.Defeated)
                {
                    EnterEnemyDefeated();
                }
            }
        }

        private void ApplySpecialImpact()
        {
            if (CurrentEnemySpec.Id == ArenaEnemyId.Bear)
            {
                EnemyHp = 1;
                CheckBearEnrage();
            }
            else
            {
                EnemyHp = 0;
            }

            EnemyHitAppliedThisAttack = true;
            EnemyProtectedResponse = false;
            EnemyState = ArenaEnemyState.HitReaction;
            EnemyStateTick = 0;
        }

        private void SetSpecialPhase(ArenaSpecialPhase next)
        {
            if (SpecialPhase == next)
            {
                return;
            }

            SpecialPhase = next;
            OnSpecialPhaseChanged?.Invoke(next);
        }

        private void UpdatePlayer(ArenaInputState input)
        {
            if (SwordCooldown > 0) SwordCooldown--;
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer == 0 && SwordTick == 0)
                {
                    nextComboStep = 0;
                }
            }
            if (PlayerImmunity > 0) PlayerImmunity--;
            if (PlayerHitRecovery > 0) PlayerHitRecovery--;
            if (BlockRecovery > 0) BlockRecovery--;

            PlayerFacing = EnemyX >= PlayerX ? 1 : -1;

            if (PlayerHitRecovery > 0)
            {
                IsGuarding = false;
                IsCrouching = false;
                ApplyPlayerPhysics();
                return;
            }

            if (input.AttackPressed && SwordTick == 0 && SwordCooldown == 0 && BlockRecovery == 0)
            {
                SwordTick = 1;
                SwordComboStep = nextComboStep;
                nextComboStep = (nextComboStep + 1) % 3;
                comboResetTimer = 115;
                SwordCooldown = tuning.SwordCooldownTicks;
                SwordHitAppliedThisSwing = false;
                IsGuarding = false;
                OnSwordSwing?.Invoke();
            }

            bool attacking = SwordTick > 0;
            if (attacking)
            {
                if (SwordTick > tuning.SwordWindupTicks && SwordTick <= tuning.SwordWindupTicks + tuning.SwordActiveTicks)
                {
                    TryApplySwordHit();
                }

                SwordTick++;
                if (SwordTick > tuning.SwordDurationTicks)
                {
                    SwordTick = 0;
                }
            }

            IsCrouching = PlayerOnGround && input.CrouchHeld && !attacking;

            bool holdingBack = Math.Sign(input.MoveX) == -PlayerFacing && Math.Abs(input.MoveX) > 0.2f;
            bool enemyThreatening = EnemyState == ArenaEnemyState.Windup || EnemyState == ArenaEnemyState.ActiveAttack;
            IsGuarding = PlayerOnGround && holdingBack && enemyThreatening && !attacking && !IsCrouching && BlockRecovery == 0;

            if (input.JumpPressed && PlayerOnGround && !attacking && !IsCrouching)
            {
                PlayerVy = -tuning.JumpSpeed;
                IsGuarding = false;
            }

            float moveScale = 1f;
            if (IsGuarding || BlockRecovery > 0)
            {
                moveScale = 0f;
            }
            else if (attacking)
            {
                moveScale = tuning.AttackMoveScale;
            }
            else if (SwordCooldown > tuning.SwordCooldownTicks - 12)
            {
                moveScale = tuning.RecoveryMoveScale;
            }
            else if (IsCrouching)
            {
                moveScale = 0.35f;
            }

            PlayerX += Math.Clamp(input.MoveX, -1f, 1f) * tuning.PlayerSpeed * moveScale;
            PlayerX = ClampToArena(PlayerX, tuning.PlayerBodyWidth * 0.5f);
            ApplyPlayerPhysics();
        }

        private void ApplyPlayerPhysics()
        {
            if (!PlayerOnGround || PlayerVy < 0f)
            {
                PlayerY += PlayerVy;
                PlayerVy += tuning.Gravity;
                if (PlayerY >= tuning.GroundY)
                {
                    PlayerY = tuning.GroundY;
                    PlayerVy = 0f;
                }
            }
        }

        private void TryApplySwordHit()
        {
            if (SwordHitAppliedThisSwing)
            {
                return;
            }

            if (EnemyState == ArenaEnemyState.Spawning || EnemyState == ArenaEnemyState.Defeated)
            {
                return;
            }

            float swordRange = ArenaCatalog.GetSwordRange(CurrentEnemySpec, tuning);
            float dx = Math.Abs(EnemyX - PlayerX);
            if (dx > swordRange)
            {
                return;
            }

            float verticalDelta = Math.Abs((PlayerY - tuning.PlayerBodyHeight * 0.5f) - (EnemyY - CurrentEnemySpec.BodyHeight * 0.5f));
            if (verticalDelta > (CurrentEnemySpec.BodyHeight * 0.55f + 70f))
            {
                return;
            }

            SwordHitAppliedThisSwing = true;

            bool isProtected = EnemyState == ArenaEnemyState.HitReaction
                || EnemyProtectedResponse
                || (EnemyState == ArenaEnemyState.Windup && EnemyStateTick > tuning.EnemyInterruptibleTicks)
                || EnemyState == ArenaEnemyState.ActiveAttack;

            if (isProtected)
            {
                BlockRecovery = tuning.BlockRecoveryTicks;
                OnSwordHit?.Invoke(false);
                return;
            }

            EnemyHp = Math.Max(0, EnemyHp - 1);
            if (PlayerHp < tuning.MaxPlayerHp)
            {
                PlayerHp++;
            }

            float pushDir = EnemyX >= PlayerX ? 1f : -1f;
            EnemyKnockbackVx = pushDir * tuning.SwordHitEnemyKnockbackVelocity;

            OnSwordHit?.Invoke(true);
            CheckBearEnrage();

            if (EnemyHp <= 0)
            {
                EnterEnemyDefeated();
                return;
            }

            EnemyState = ArenaEnemyState.HitReaction;
            EnemyStateTick = 0;
            EnemyProtectedResponse = CurrentEnemySpec.Id != ArenaEnemyId.Train;
        }

        private void CheckBearEnrage()
        {
            if (CurrentEnemySpec.Id != ArenaEnemyId.Bear || BearEnraged || EnemyHp <= 0)
            {
                return;
            }

            int threshold = (int)Math.Ceiling(CurrentEnemySpec.MaxHp * 0.5);
            if (EnemyHp <= threshold)
            {
                BearEnraged = true;
                OnBearEnraged?.Invoke();
            }
        }

        private void EnterEnemyDefeated()
        {
            defeatedStartX = EnemyX;
            EnemyKnockbackVx = 0f;
            EnemyVy = 0f;
            EnemyDepositArcHeight = 0f;
            EnemyDepositPitchDegrees = 0f;
            EnemyState = ArenaEnemyState.Defeated;
            EnemyStateTick = 0;
            ActiveAttackKind = ArenaAttackKind.None;
            EnemyProtectedResponse = false;
        }

        private void UpdateEnemy()
        {
            if (EvadeCooldown > 0) EvadeCooldown--;
            EnemyStateTick++;
            EnemyFacing = PlayerX >= EnemyX ? 1 : -1;

            switch (EnemyState)
            {
                case ArenaEnemyState.Spawning:
                    float spawnT = Math.Clamp(EnemyStateTick / (float)tuning.EnemySpawnTicks, 0f, 1f);
                    EnemySpawnScale = 0.16f + (1f - 0.16f) * spawnT;
                    EnemyX = ChestOriginX + (EnemyStartX - ChestOriginX) * spawnT;
                    if (EnemyStateTick >= tuning.EnemySpawnTicks)
                    {
                        EnemySpawnScale = 1f;
                        EnemyState = ArenaEnemyState.Idle;
                        EnemyStateTick = 0;
                    }
                    break;

                case ArenaEnemyState.Idle:
                    if (EnemyStateTick >= CurrentEnemySpec.IdleTicks)
                    {
                        DecideNextEnemyAction();
                    }
                    break;

                case ArenaEnemyState.Approaching:
                    float speed = CurrentEnemySpec.WalkSpeed * (BearEnraged ? 1.18f : 1f);
                    EnemyX += EnemyFacing * speed;
                    EnemyX = ClampToArena(EnemyX, CurrentEnemySpec.BodyWidth * 0.5f);

                    float attackTriggerRange = ArenaCatalog.GetSwordRange(CurrentEnemySpec, tuning) + 18f;
                    if (Math.Abs(PlayerX - EnemyX) <= attackTriggerRange || EnemyStateTick >= 70)
                    {
                        StartEnemyWindup();
                    }
                    break;

                case ArenaEnemyState.Retreating:
                    EnemyX -= EnemyFacing * CurrentEnemySpec.WalkSpeed * 0.85f;
                    EnemyX = ClampToArena(EnemyX, CurrentEnemySpec.BodyWidth * 0.5f);
                    if (EnemyStateTick >= CurrentEnemySpec.RetreatTicks)
                    {
                        EnemyState = ArenaEnemyState.Idle;
                        EnemyStateTick = 0;
                    }
                    break;

                case ArenaEnemyState.Evading:
                    float backStep = -EnemyFacing * CurrentEnemySpec.EvadeSpeed;
                    float nextX = EnemyX + backStep;
                    float halfW = CurrentEnemySpec.BodyWidth * 0.5f;
                    if (nextX <= tuning.LeftBound + halfW + 10f || nextX >= tuning.RightBound - halfW - 10f)
                    {
                        StartEnemyWindup();
                        break;
                    }

                    EnemyX = ClampToArena(nextX, halfW);
                    if (EnemyStateTick >= CurrentEnemySpec.EvadeTicks)
                    {
                        EnemyState = ArenaEnemyState.Idle;
                        EnemyStateTick = 0;
                    }
                    break;

                case ArenaEnemyState.Windup:
                    if (SwordTick > 0 && SwordTick <= tuning.SwordWindupTicks
                        && CurrentEnemySpec.EvadeSpeed > 0f && EvadeCooldown == 0 && !EnemyProtectedResponse
                        && EnemyStateTick < tuning.EnemyInterruptibleTicks)
                    {
                        EvadeCooldown = CurrentEnemySpec.EvadeCooldownTicks;
                        EnemyState = ArenaEnemyState.Evading;
                        EnemyStateTick = 0;
                        break;
                    }

                    if (EnemyStateTick >= tuning.EnemyWindupTicks)
                    {
                        EnemyState = ArenaEnemyState.ActiveAttack;
                        EnemyStateTick = 0;
                        EnemyHitAppliedThisAttack = false;
                        if (ActiveAttackKind == ArenaAttackKind.ToyLeap)
                        {
                            EnemyVy = -11.5f;
                        }

                        OnEnemyAttackActive?.Invoke(CurrentEnemySpec.Id, ActiveAttackKind);
                    }
                    break;

                case ArenaEnemyState.ActiveAttack:
                    UpdateEnemyActiveAttack();
                    if (EnemyStateTick >= tuning.EnemyActiveTicks && EnemyOnGround)
                    {
                        EnemyProtectedResponse = false;
                        EnemyState = ArenaEnemyState.Recovery;
                        EnemyStateTick = 0;
                    }
                    break;

                case ArenaEnemyState.Recovery:
                    if (EnemyStateTick >= CurrentEnemySpec.RecoveryTicks)
                    {
                        if (chainPending && CurrentEnemySpec.PressureChainEnabled)
                        {
                            chainPending = false;
                            EnemyProtectedResponse = true;
                            StartEnemyWindup();
                        }
                        else
                        {
                            EnemyState = ArenaEnemyState.Idle;
                            EnemyStateTick = 0;
                        }
                    }
                    break;

                case ArenaEnemyState.HitReaction:
                    if (EnemyStateTick >= tuning.EnemyHitReactionTicks)
                    {
                        if (EnemyProtectedResponse)
                        {
                            StartEnemyWindup();
                        }
                        else
                        {
                            EnemyState = ArenaEnemyState.Idle;
                            EnemyStateTick = 0;
                        }
                    }
                    break;

                case ArenaEnemyState.Defeated:
                    float exitT = Math.Clamp(EnemyStateTick / (float)tuning.EnemyDeathTicks, 0f, 1f);
                    float smoothT = exitT * exitT * (3f - (2f * exitT));
                    // Regressao 4: Brinquedo derrotado voa em arco parabolico da Arena ate mergulhar dentro da boca da Caixa de Brinquedos a esquerda (ChestDepositTargetSimX = 270)
                    EnemyX = defeatedStartX + ((ChestDepositTargetSimX - defeatedStartX) * smoothT);
                    float parabola = 4f * exitT * (1f - exitT);
                    // No final (exitT -> 1), cai para dentro da cavidade interna do bau (ArcHeight = +24px acima do piso, abaixo da borda frontal do bau que tem 95px de altura!)
                    EnemyDepositArcHeight = (parabola * 245f) + (smoothT * 24f);
                    EnemyY = tuning.GroundY - EnemyDepositArcHeight;
                    EnemyDepositPitchDegrees = smoothT * 34f;
                    EnemySpawnScale = 1f - (0.76f * smoothT);
                    if (EnemyStateTick >= tuning.EnemyDeathTicks)
                    {
                        if (waveIndex + 1 < ArenaCatalog.WaveOrder.Length)
                        {
                            StartWave(waveIndex + 1);
                        }
                        else
                        {
                            Outcome = ArenaBattleOutcome.Victory;
                            OnBattleEnded?.Invoke(ArenaBattleOutcome.Victory);
                        }
                    }
                    break;
            }
        }

        private void DecideNextEnemyAction()
        {
            float dist = Math.Abs(PlayerX - EnemyX);
            float triggerDist = ArenaCatalog.GetSwordRange(CurrentEnemySpec, tuning) + 25f;

            if (dist > triggerDist)
            {
                EnemyState = ArenaEnemyState.Approaching;
                EnemyStateTick = 0;
                return;
            }

            StartEnemyWindup();
        }

        public void ForceEnemyWindupForTest()
        {
            StartEnemyWindup();
        }

        private void StartEnemyWindup()
        {
            ActiveAttackKind = PickAttackKind();
            EnemyState = ArenaEnemyState.Windup;
            EnemyStateTick = 0;
            EnemyHitAppliedThisAttack = false;
            OnEnemyAttackStarted?.Invoke(CurrentEnemySpec.Id, ActiveAttackKind);
        }

        private ArenaAttackKind PickAttackKind()
        {
            attackChoiceCounter++;
            if (CurrentEnemySpec.TertiaryAttack != ArenaAttackKind.None && (attackChoiceCounter % 3 == 0))
            {
                return CurrentEnemySpec.TertiaryAttack;
            }

            return (attackChoiceCounter % 2 == 0)
                ? CurrentEnemySpec.PrimaryAttack
                : CurrentEnemySpec.SecondaryAttack;
        }

        private void UpdateEnemyActiveAttack()
        {
            bool isCharge = ActiveAttackKind == ArenaAttackKind.FrontCharge
                || ActiveAttackKind == ArenaAttackKind.Rush
                || ActiveAttackKind == ArenaAttackKind.BellyCharge
                || ActiveAttackKind == ArenaAttackKind.ToyLeap;

            if (isCharge)
            {
                float spd = CurrentEnemySpec.ChargeSpeed * (BearEnraged ? 1.15f : 1f);
                EnemyX += EnemyFacing * spd;
                EnemyX = ClampToArena(EnemyX, CurrentEnemySpec.BodyWidth * 0.5f);
            }

            if (!EnemyOnGround || EnemyVy < 0f)
            {
                EnemyY += EnemyVy;
                EnemyVy += tuning.Gravity * 1.15f;
                if (EnemyY >= tuning.GroundY)
                {
                    EnemyY = tuning.GroundY;
                    EnemyVy = 0f;
                }
            }

            if (EnemyHitAppliedThisAttack)
            {
                return;
            }

            float reach = GetEnemyAttackReach(ActiveAttackKind);
            float dx = Math.Abs(PlayerX - EnemyX);
            if (dx > reach)
            {
                return;
            }

            if (ActiveAttackKind == ArenaAttackKind.LaserBeam && IsCrouching)
            {
                return;
            }

            if (!PlayerOnGround && PlayerY < tuning.GroundY - 75f && ActiveAttackKind != ArenaAttackKind.ToyLeap)
            {
                return;
            }

            EnemyHitAppliedThisAttack = true;

            if (IsGuarding)
            {
                BlockRecovery = tuning.BlockRecoveryTicks;
                float guardKnockDir = PlayerX >= EnemyX ? 1f : -1f;
                PlayerKnockbackVx = guardKnockDir * tuning.EnemyBlockPlayerKnockbackVelocity;
                OnAttackBlocked?.Invoke();
                return;
            }

            if (PlayerImmunity > 0)
            {
                return;
            }

            PlayerHp = Math.Max(0, PlayerHp - 1);
            PlayerImmunity = tuning.PlayerImmunityTicks;
            PlayerHitRecovery = tuning.PlayerHitRecoveryTicks;
            SwordTick = 0;
            IsGuarding = false;
            chainPending = CurrentEnemySpec.PressureChainEnabled && !EnemyProtectedResponse;

            float knockDir = PlayerX >= EnemyX ? 1f : -1f;
            PlayerKnockbackVx = knockDir * tuning.EnemyHitPlayerKnockbackVelocity;

            OnPlayerDamaged?.Invoke();

            if (PlayerHp <= 0)
            {
                Outcome = ArenaBattleOutcome.Defeat;
                OnBattleEnded?.Invoke(ArenaBattleOutcome.Defeat);
            }
        }

        private float GetEnemyAttackReach(ArenaAttackKind kind)
        {
            float baseReach = (tuning.PlayerBodyWidth * 0.5f) + (CurrentEnemySpec.BodyWidth * 0.5f) + 52f;
            return kind switch
            {
                ArenaAttackKind.LaserBeam => baseReach + 240f,
                ArenaAttackKind.RocketPunch => baseReach + 170f,
                ArenaAttackKind.SteamBlast => baseReach + 110f,
                _ => baseReach
            };
        }

        public float GetMinBodySeparation()
        {
            if (CurrentEnemySpec == null)
            {
                return tuning.PlayerBodyWidth;
            }

            return (tuning.PlayerBodyWidth * 0.5f)
                + (CurrentEnemySpec.BodyWidth * 0.5f)
                + tuning.BodySeparationPadding;
        }

        private void ApplyKnockbackVelocities()
        {
            if (Math.Abs(PlayerKnockbackVx) > 0.08f)
            {
                PlayerX = ClampToArena(PlayerX + PlayerKnockbackVx, tuning.PlayerBodyWidth * 0.5f);
                PlayerKnockbackVx *= tuning.KnockbackFriction;
            }
            else
            {
                PlayerKnockbackVx = 0f;
            }

            if (Math.Abs(EnemyKnockbackVx) > 0.08f)
            {
                EnemyX = ClampToArena(EnemyX + EnemyKnockbackVx, CurrentEnemySpec.BodyWidth * 0.5f);
                EnemyKnockbackVx *= tuning.KnockbackFriction;
            }
            else
            {
                EnemyKnockbackVx = 0f;
            }
        }

        private void ResolveBodySeparation()
        {
            if (EnemyState == ArenaEnemyState.Defeated)
            {
                return;
            }

            if (EnemyState == ArenaEnemyState.Spawning && EnemySpawnScale < 0.45f)
            {
                return;
            }

            if (!PlayerOnGround && PlayerY < tuning.GroundY - (CurrentEnemySpec.BodyHeight * 0.62f))
            {
                return;
            }

            float minSep = GetMinBodySeparation();
            float delta = PlayerX - EnemyX;
            float dist = Math.Abs(delta);
            if (dist >= minSep)
            {
                return;
            }

            float dir = Math.Abs(delta) > 0.001f ? (delta > 0f ? 1f : -1f) : -PlayerFacing;
            float overlap = minSep - dist;
            float playerHalf = tuning.PlayerBodyWidth * 0.5f;
            float enemyHalf = CurrentEnemySpec.BodyWidth * 0.5f;

            if (IsGuarding)
            {
                EnemyX = ClampToArena(EnemyX - dir * overlap, enemyHalf);
            }
            else
            {
                PlayerX = ClampToArena(PlayerX + dir * (overlap * 0.5f), playerHalf);
                EnemyX = ClampToArena(EnemyX - dir * (overlap * 0.5f), enemyHalf);
            }

            // Garante separacao solida mesmo quando um dos dois encosta na parede da arena
            float remainingOverlap = minSep - Math.Abs(PlayerX - EnemyX);
            if (remainingOverlap > 0.01f)
            {
                float postDir = Math.Abs(PlayerX - EnemyX) > 0.001f
                    ? (PlayerX > EnemyX ? 1f : -1f)
                    : dir;
                float prevPlayerX = PlayerX;
                PlayerX = ClampToArena(PlayerX + postDir * remainingOverlap, playerHalf);
                float movedPlayer = Math.Abs(PlayerX - prevPlayerX);
                float stillRemaining = remainingOverlap - movedPlayer;
                if (stillRemaining > 0.01f)
                {
                    EnemyX = ClampToArena(EnemyX - postDir * stillRemaining, enemyHalf);
                }
            }
        }

        private void UpdateStatusHint()
        {
            if (EnemyState == ArenaEnemyState.Spawning)
            {
                StatusHint = $"{CurrentEnemySpec.Title} saiu do baú!";
                return;
            }

            if (EnemyState == ArenaEnemyState.Windup)
            {
                StatusHint = "Segure para trás para defender ou salte!";
                return;
            }

            if (EnemyProtectedResponse || EnemyState == ArenaEnemyState.HitReaction)
            {
                StatusHint = "Contra-ataque protegido — aguarde ou esquive!";
                return;
            }

            if (EnemyState == ArenaEnemyState.Recovery)
            {
                StatusHint = "Abertura! Ataque com a espada!";
                return;
            }

            StatusHint = string.Empty;
        }

        private float ClampToArena(float x, float halfWidth)
        {
            return Math.Clamp(x, tuning.LeftBound + halfWidth, tuning.RightBound - halfWidth);
        }
    }
}
