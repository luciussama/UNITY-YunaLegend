using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Tests
{
    public sealed class BearChaseQaPilotTests
    {
        [Test]
        public void QaPilot_CompletesFullNormalChaseToVictoryWithThrowsAfter460m()
        {
            var sim = new BearChaseSimulation();
            int throwsAfter460 = 0;
            sim.OnBearToyThrown += _ =>
            {
                if (sim.YunaZ >= 460f)
                {
                    throwsAfter460++;
                }
            };

            const float dt = 1f / 60f;
            for (int tick = 0; tick < 60 * 90 && sim.Phase != BearChasePhase.Victory && sim.Phase != BearChasePhase.Fallen; tick++)
            {
                float lateral = BearChaseQaPilot.ComputeLateralInput(sim);
                sim.SetLateralInput(lateral);
                sim.Step(dt);
            }

            Assert.AreEqual(BearChasePhase.Victory, sim.Phase, $"Esperava Victory; terminou em {sim.Phase} a {sim.YunaZ:F1}m.");
            Assert.GreaterOrEqual(sim.YunaZ, 568f);
            Assert.GreaterOrEqual(throwsAfter460, 1);
        }
    }
}
