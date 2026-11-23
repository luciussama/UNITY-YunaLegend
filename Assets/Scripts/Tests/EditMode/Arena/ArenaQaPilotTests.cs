using NUnit.Framework;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Tests
{
    public sealed class ArenaQaPilotTests
    {
        [Test]
        public void PrepareFinalWaveForQa_WithQaPilot_ReachesVictoryDeterministically()
        {
            var sim = new ArenaCombatSimulation(easyMode: false, mobileTouchAssist: false);
            sim.PrepareFinalWaveForQa(enemyHp: 2);

            Assert.AreEqual(sim.TotalWaves - 1, sim.WaveIndex);
            Assert.AreEqual(2, sim.EnemyHp);

            const float dt = 1f / 60f;
            for (int tick = 0; tick < 60 * 30 && sim.Outcome == ArenaBattleOutcome.Ongoing; tick++)
            {
                ArenaInputState input = ArenaQaPilot.ComputeInput(sim);
                sim.Step(dt, input);
            }

            Assert.AreEqual(ArenaBattleOutcome.Victory, sim.Outcome, "ArenaQaPilot deveria vencer a onda final em QA.");
        }
    }
}
