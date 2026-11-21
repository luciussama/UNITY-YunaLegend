using NUnit.Framework;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Tests
{
    [TestFixture]
    public sealed class ArenaCombatSimulationTests
    {
        [Test]
        public void Catalog_MatchesCanonicalSwordRanges_InNormalAndEasy()
        {
            var normalTuning = ArenaCatalog.CreateTuning(easyMode: false);
            var normalEnemies = ArenaCatalog.CreateEnemies(easyMode: false);

            Assert.AreEqual(297f, ArenaCatalog.GetSwordRange(normalEnemies[ArenaEnemyId.Train], normalTuning), 0.1f);
            Assert.AreEqual(209f, ArenaCatalog.GetSwordRange(normalEnemies[ArenaEnemyId.Robot], normalTuning), 0.1f);
            Assert.AreEqual(247.5f, ArenaCatalog.GetSwordRange(normalEnemies[ArenaEnemyId.Dinosaur], normalTuning), 0.1f);
            Assert.AreEqual(239.25f, ArenaCatalog.GetSwordRange(normalEnemies[ArenaEnemyId.Bear], normalTuning), 0.1f);

            var easyTuning = ArenaCatalog.CreateTuning(easyMode: true);
            var easyEnemies = ArenaCatalog.CreateEnemies(easyMode: true);
            Assert.AreEqual(386.1f, ArenaCatalog.GetSwordRange(easyEnemies[ArenaEnemyId.Train], easyTuning), 0.1f);
            Assert.AreEqual(271.7f, ArenaCatalog.GetSwordRange(easyEnemies[ArenaEnemyId.Robot], easyTuning), 0.1f);
        }

        [Test]
        public void SwordHit_DamagesTrain_AndTransfersHeartWhenPlayerWounded()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(sim, 70, default);
            Assert.AreEqual(ArenaEnemyState.Idle, sim.EnemyState);

            // Aproxima a menina do Trem enquanto ainda esta em abertura (Idle/Approaching/Windup<=18)
            for (int i = 0; i < 65; i++)
            {
                sim.TickFixed(new ArenaInputState { MoveX = 1f });
                if (System.Math.Abs(sim.EnemyX - sim.PlayerX) <= 260f)
                {
                    break;
                }
            }

            // Ataca na abertura
            bool hitTriggered = false;
            sim.OnSwordHit += valid =>
            {
                if (valid) hitTriggered = true;
            };

            sim.TickFixed(new ArenaInputState { AttackPressed = true });
            AdvanceTicks(sim, 12, default);

            Assert.IsTrue(hitTriggered, "Espada deve acertar o Trem na janela ativa.");
            Assert.AreEqual(2, sim.EnemyHp, "Trem deve perder 1 HP.");
        }

        [Test]
        public void HoldingBack_BlocksEnemyAttack_WithoutLosingHearts()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(sim, 70, default);

            // Aproxima ate o Trem iniciar o windup
            int safety = 0;
            while (sim.EnemyState != ArenaEnemyState.Windup && safety++ < 250)
            {
                sim.TickFixed(new ArenaInputState { MoveX = 1f });
            }

            Assert.AreEqual(ArenaEnemyState.Windup, sim.EnemyState);

            bool blocked = false;
            sim.OnAttackBlocked += () => blocked = true;

            // Segura direcao oposta ao inimigo (esquerda, ja que PlayerFacing == 1)
            AdvanceTicks(sim, 80, new ArenaInputState { MoveX = -1f });

            Assert.IsTrue(blocked, "Segurar para tras durante o ataque deve bloquear.");
            Assert.AreEqual(3, sim.PlayerHp, "Bloqueio nao deve retirar coracoes da menina.");
        }

        [Test]
        public void RainbowSpecial_DefeatsTrainInOneUse_AndLeavesBearAtOneHp()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(sim, 70, default);

            sim.TickFixed(new ArenaInputState { SpecialPressed = true });
            Assert.IsTrue(sim.IsSpecialActive);
            Assert.IsTrue(sim.SpecialUsed);

            AdvanceTicks(sim, 165, default);
            Assert.IsFalse(sim.IsSpecialActive);
            Assert.AreEqual(ArenaEnemyState.Defeated, sim.EnemyState);
            Assert.AreEqual(0, sim.EnemyHp);

            // Segundo especial nao pode ser usado na mesma batalha
            AdvanceTicks(sim, 130, default);
            Assert.AreEqual(ArenaEnemyId.Robot, sim.CurrentEnemySpec.Id);
            sim.TickFixed(new ArenaInputState { SpecialPressed = true });
            Assert.IsFalse(sim.IsSpecialActive);
        }

        [Test]
        public void FullGamethrough_DefeatsAllFourWaves_TriggersBearEnrage_AndReachesVictory()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            bool bearEnragedTriggered = false;
            bool victoryTriggered = false;
            sim.OnBearEnraged += () => bearEnragedTriggered = true;
            sim.OnBattleEnded += outcome =>
            {
                if (outcome == ArenaBattleOutcome.Victory) victoryTriggered = true;
            };

            // Simula jogador ativo: anda ate alcance de espada, bloqueia durante windup e ataca nas aberturas
            int maxTicks = 12000;
            for (int t = 0; t < maxTicks && sim.Outcome == ArenaBattleOutcome.Ongoing; t++)
            {
                float dist = System.Math.Abs(sim.EnemyX - sim.PlayerX);
                float range = ArenaCatalog.GetSwordRange(sim.CurrentEnemySpec, sim.Tuning) - 20f;

                var input = new ArenaInputState();
                bool enemyThreat = sim.EnemyState == ArenaEnemyState.Windup || sim.EnemyState == ArenaEnemyState.ActiveAttack;
                bool enemyOpen = sim.EnemyState == ArenaEnemyState.Idle
                    || sim.EnemyState == ArenaEnemyState.Approaching
                    || sim.EnemyState == ArenaEnemyState.Recovery;

                if (enemyThreat)
                {
                    // Segura para tras para defender
                    input.MoveX = -sim.PlayerFacing;
                }
                else if (dist > range)
                {
                    input.MoveX = sim.PlayerFacing;
                }
                else if (enemyOpen && sim.SwordCooldown == 0 && sim.SwordTick == 0 && sim.BlockRecovery == 0)
                {
                    input.AttackPressed = true;
                }

                sim.TickFixed(input);
            }

            Assert.IsTrue(bearEnragedTriggered, "Urso deve entrar em furia ao atingir HP <= 4.");
            Assert.IsTrue(victoryTriggered, "Gamethrough completo deve vencer as 4 ondas (Trem -> Robo -> Dinossauro -> Urso).");
            Assert.AreEqual(ArenaBattleOutcome.Victory, sim.Outcome);
            Assert.AreEqual(ArenaEnemyId.Bear, sim.CurrentEnemySpec.Id);
        }

        [TestCase(30f)]
        [TestCase(60f)]
        [TestCase(120f)]
        public void JumpInput_WithLatching_WorksReliably_At30_60_And120Hz(float renderHz)
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            float dt = 1f / renderHz;
            float jumpBuffer = 0.14f;
            bool leftGround = false;
            float minY = sim.Tuning.GroundY;

            // Simula 1 segundo de renderizacao com o pulo pressionado apenas no 1o frame
            int frames = (int)System.Math.Ceiling(renderHz * 1.4f);
            for (int f = 0; f < frames; f++)
            {
                if (f > 0 && jumpBuffer > 0f)
                {
                    jumpBuffer = System.Math.Max(0f, jumpBuffer - dt);
                }

                int ticks = sim.Step(dt, new ArenaInputState { JumpPressed = jumpBuffer > 0f });
                if (ticks > 0 && !sim.PlayerOnGround)
                {
                    jumpBuffer = 0f;
                    leftGround = true;
                }

                if (sim.PlayerY < minY)
                {
                    minY = sim.PlayerY;
                }
            }

            Assert.IsTrue(leftGround, $"Menina deve sair do chao ao pular em {renderHz} Hz.");
            Assert.Less(minY, sim.Tuning.GroundY - 120f, $"Altura do pulo deve ser significativa em {renderHz} Hz.");
            Assert.IsTrue(sim.PlayerOnGround, $"Menina deve retornar ao chao apos o pulo em {renderHz} Hz.");
        }

        [Test]
        public void BodyCollision_PreventsPlayerAndEnemyOverlap_EvenAgainstArenaWall()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(sim, 70, default);
            float minSep = sim.GetMinBodySeparation();

            // Empurra continuamente a menina contra o Trem ate encostar na parede direita
            for (int i = 0; i < 240; i++)
            {
                sim.TickFixed(new ArenaInputState { MoveX = 1f });
                float dist = System.Math.Abs(sim.EnemyX - sim.PlayerX);
                Assert.GreaterOrEqual(dist, minSep - 0.1f, $"Sobreposicao detectada no tick {i}: dist={dist}, minSep={minSep}");
            }
        }

        [Test]
        public void SwordHit_PushesEnemyBackLightly_SmoothlyOverTicks()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(sim, 70, default);

            for (int i = 0; i < 80; i++)
            {
                sim.TickFixed(new ArenaInputState { MoveX = 1f });
                if (System.Math.Abs(sim.EnemyX - sim.PlayerX) <= 275f)
                {
                    break;
                }
            }

            float enemyXBeforeHit = sim.EnemyX;
            bool hit = false;
            sim.OnSwordHit += valid => { if (valid) hit = true; };

            sim.TickFixed(new ArenaInputState { AttackPressed = true });
            AdvanceTicks(sim, 18, default);

            Assert.IsTrue(hit, "Espada deve acertar o Trem.");
            float recoil = sim.EnemyX - enemyXBeforeHit;
            Assert.Greater(recoil, 25f, "Golpe da espada deve empurrar o Trem de leve para tras.");
            Assert.Less(recoil, 100f, "Recuo do Trem ao golpe da espada deve ser suave, sem teleporte.");
        }

        [Test]
        public void EnemyAttack_PushesPlayerBack_OnHitAndOnBlock()
        {
            // Caso 1: Menina leva o golpe do Trem e e empurrada para tras
            var simHit = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(simHit, 70, default);
            int guardSafety = 0;
            while (simHit.EnemyState != ArenaEnemyState.Windup && guardSafety++ < 250)
            {
                simHit.TickFixed(new ArenaInputState { MoveX = 1f });
            }

            float playerXBeforeHit = simHit.PlayerX;
            bool damaged = false;
            simHit.OnPlayerDamaged += () => damaged = true;
            AdvanceTicks(simHit, 90, default);

            Assert.IsTrue(damaged, "Ataque do Trem deve acertar a menina sem defesa.");
            Assert.Less(simHit.PlayerX, playerXBeforeHit - 50f, "Ataque do Trem deve empurrar a menina para tras.");

            // Caso 2: Menina bloqueia o ataque e sente um recuo leve de guarda
            var simBlock = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(simBlock, 70, default);
            guardSafety = 0;
            while (simBlock.EnemyState != ArenaEnemyState.Windup && guardSafety++ < 250)
            {
                simBlock.TickFixed(new ArenaInputState { MoveX = 1f });
            }

            float playerXBeforeBlock = simBlock.PlayerX;
            bool blocked = false;
            simBlock.OnAttackBlocked += () => blocked = true;
            AdvanceTicks(simBlock, 85, new ArenaInputState { MoveX = -1f });

            Assert.IsTrue(blocked, "Defesa direcional deve bloquear o ataque.");
            Assert.Less(simBlock.PlayerX, playerXBeforeBlock - 20f, "Bloqueio deve aplicar recuo fisico leve na menina.");
        }

        [Test]
        public void SwordCombo_CyclesThreeSteps_AndResetsAfterPause()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(sim, 70, default);

            // 1o golpe: corte vertical (step 0)
            sim.TickFixed(new ArenaInputState { AttackPressed = true });
            Assert.AreEqual(0, sim.SwordComboStep, "Primeiro golpe do combo deve ser vertical (0).");
            AdvanceTicks(sim, sim.Tuning.SwordCooldownTicks + 2, default);

            // 2o golpe dentro da janela de combo: corte horizontal (step 1)
            sim.TickFixed(new ArenaInputState { AttackPressed = true });
            Assert.AreEqual(1, sim.SwordComboStep, "Segundo golpe do combo deve ser horizontal (1).");
            AdvanceTicks(sim, sim.Tuning.SwordCooldownTicks + 2, default);

            // 3o golpe dentro da janela de combo: estocada (step 2)
            sim.TickFixed(new ArenaInputState { AttackPressed = true });
            Assert.AreEqual(2, sim.SwordComboStep, "Terceiro golpe do combo deve ser estocada (2).");

            // Aguarda pausa longa para resetar o combo para 0
            AdvanceTicks(sim, 130, default);
            sim.TickFixed(new ArenaInputState { AttackPressed = true });
            Assert.AreEqual(0, sim.SwordComboStep, "Apos pausa longa, combo deve voltar ao golpe vertical (0).");
        }

        [Test]
        public void MobileTouchAssist_ReducesEnemyAggression_WithoutChangingDesktopDefaults()
        {
            var desktopTuning = ArenaCatalog.CreateTuning(easyMode: false, mobileTouchAssist: false);
            var mobileTuning = ArenaCatalog.CreateTuning(easyMode: false, mobileTouchAssist: true);
            var desktopEnemies = ArenaCatalog.CreateEnemies(easyMode: false, mobileTouchAssist: false);
            var mobileEnemies = ArenaCatalog.CreateEnemies(easyMode: false, mobileTouchAssist: true);

            Assert.Greater(mobileTuning.EnemyWindupTicks, desktopTuning.EnemyWindupTicks, "Mobile deve dar mais tempo de leitura no windup.");
            Assert.Greater(mobileTuning.SwordContactScale, desktopTuning.SwordContactScale, "Mobile deve ter margem de espada ligeiramente maior.");
            Assert.Greater(mobileEnemies[ArenaEnemyId.Robot].IdleTicks, desktopEnemies[ArenaEnemyId.Robot].IdleTicks);
            Assert.Greater(mobileEnemies[ArenaEnemyId.Bear].RecoveryTicks, desktopEnemies[ArenaEnemyId.Bear].RecoveryTicks);
            Assert.IsFalse(mobileEnemies[ArenaEnemyId.Bear].PressureChainEnabled, "Mobile nao deve encadear combos de pressao.");
            Assert.IsTrue(desktopEnemies[ArenaEnemyId.Bear].PressureChainEnabled, "Desktop deve manter pressao original.");
        }

        [Test]
        public void DefeatedEnemy_ReturnsToToyChestInArc_AndShrinksIntoChest()
        {
            var sim = new ArenaCombatSimulation(easyMode: false);
            AdvanceTicks(sim, 70, default);

            // Faz o Trem caminhar para a esquerda (longe da caixa em x=1120)
            for (int i = 0; i < 65; i++)
            {
                sim.TickFixed(default);
            }

            float defeatedPos = sim.EnemyX;
            Assert.Less(defeatedPos, 1020f, "Trem deve ter se afastado da caixa antes de ser derrotado.");

            // Usa o especial para derrotar o Trem imediatamente
            sim.TickFixed(new ArenaInputState { SpecialPressed = true });
            AdvanceTicks(sim, ArenaSpecialSpec.TotalDurationTicks + 1, default);
            Assert.AreEqual(ArenaEnemyState.Defeated, sim.EnemyState);

            // No meio da animacao de deposito (tick 30 de 60), deve estar subindo em arco rumo a caixa (x=1120)
            AdvanceTicks(sim, 30, default);
            Assert.Greater(sim.EnemyX, defeatedPos + 30f, "Brinquedo derrotado deve se mover de volta para a caixa.");
            Assert.Less(sim.EnemyY, sim.Tuning.GroundY - 80f, "Brinquedo derrotado deve subir em arco ao entrar na caixa.");
            Assert.Greater(sim.EnemyDepositPitchDegrees, 15f, "Brinquedo derrotado deve inclinar ao mergulhar na caixa.");
            Assert.Less(sim.EnemySpawnScale, 0.70f, "Brinquedo derrotado deve diminuir de escala ao entrar na caixa.");

            // Ao final do tick 59 (ultimo frame antes da proxima onda), chega na caixa (x=1120)
            AdvanceTicks(sim, 29, default);
            Assert.AreEqual(1120f, sim.EnemyX, 5f, "Brinquedo derrotado deve terminar depositado exatamente na posicao da caixa.");
            Assert.Less(sim.EnemySpawnScale, 0.20f, "Brinquedo deve estar recolhido dentro da caixa.");
        }

        private static void AdvanceTicks(ArenaCombatSimulation sim, int ticks, ArenaInputState input)
        {
            for (int i = 0; i < ticks; i++)
            {
                sim.TickFixed(input);
            }
        }
    }
}
