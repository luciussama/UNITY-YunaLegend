using System;

namespace YunaLegend.DreamForest.Core
{
    /// <summary>
    /// Deterministic opt-in QA pilot for DreamForest_Gameplay (AGENTS.md §9).
    /// Active ONLY when WebQaRuntime.Current.AutoPlay is enabled via query string (?qa=1&amp;qa_autoplay=1).
    /// </summary>
    public static class DreamForestQaPilot
    {
        public static DreamForestInput ComputeInput(DreamForestSimulation sim)
        {
            if (sim == null) return default;

            if (sim.Stage == DreamForestStage.Opening
                || sim.Stage == DreamForestStage.NightmareIntro
                || sim.Stage == DreamForestStage.NightmareWake)
            {
                return new DreamForestInput(0f, 0f, false, advance: true);
            }

            if (sim.Stage == DreamForestStage.Gliding)
            {
                float targetY = DreamForestLevelData.GlideRouteY(sim.Player.X + 180f);
                float dy = targetY - sim.Player.Y;
                float steerY = Math.Max(-1f, Math.Min(1f, dy / 140f));
                return new DreamForestInput(1f, steerY, false);
            }

            if (sim.Stage == DreamForestStage.Stealth && sim.Nightmare != null)
            {
                var n = sim.Nightmare;
                bool danger = sim.IsNightmarePlayerIlluminated(detection: false);
                if (n.MudMode == DreamForestMudMode.Swim)
                {
                    // Submerge under mud when spotlight sweeps nearby, otherwise swim right
                    return danger
                        ? new DreamForestInput(0f, 1f, false)
                        : new DreamForestInput(1f, -0.4f, false);
                }
                if (danger)
                {
                    return new DreamForestInput(0f, 0f, false);
                }
                // Jump up onto soil hills when approaching their steep front edge
                float aheadGround = sim.Level.StealthGroundYAt(n.X + 45f);
                bool needJump = n.Grounded && (n.Y - aheadGround > 10f);
                return new DreamForestInput(1f, 0f, needJump);
            }

            if (sim.Stage == DreamForestStage.Running)
            {
                if (sim.Player.Climb != null)
                {
                    // Climb up to upper third of blade/web, then leap forward
                    bool readyToLeap = sim.Player.Climb.U <= 0.25f;
                    return new DreamForestInput(1f, readyToLeap ? 0f : -1f, readyToLeap);
                }
                if (sim.Player.SupportId != null)
                {
                    var obj = sim.Level.ById[sim.Player.SupportId];
                    if (obj.Flight != null)
                    {
                        var frame = DreamForestLevelData.EvaluateObjectFrame(obj, sim.Time, sim.Rides);
                        if (frame.FlightProgress < 0.92f)
                        {
                            return new DreamForestInput(0f, 0f, false);
                        }
                    }
                    var f = DreamForestLevelData.EvaluateObjectFrame(obj, sim.Time, sim.Rides);
                    bool nearRightEdge = sim.Player.X >= f.X + f.W * 0.72f;
                    return new DreamForestInput(1f, 0f, nearRightEdge);
                }
                return new DreamForestInput(1f, 0f, false);
            }

            return default;
        }
    }
}
