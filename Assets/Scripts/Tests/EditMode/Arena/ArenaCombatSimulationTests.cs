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

        private static void AdvanceTicks(ArenaCombatSimulation sim, int ticks, ArenaInputState input)
        {
            for (int i = 0; i < ticks; i++)
            {
                sim.TickFixed(input);
            }
        }
    }
}
