using System;

namespace YunaLegend.Picnic.Core
{
    /// <summary>
    /// Opt-in deterministic QA pilot in pure Core (per AGENTS.md §9).
    /// Only invoked when WebQaRuntime.Current.AutoPlay is enabled via ?qa=1&qa_autoplay=1.
    /// </summary>
    public static class PicnicQaPilot
    {
        public static PicnicInput Read(PicnicSimulation sim)
        {
            if (sim == null) return default;
            if (sim.IsReadingStory)
            {
                return new PicnicInput(0f, false, advance: false, skip: true);
            }
            if (sim.Stage != PicnicStage.Running)
            {
                return default;
            }

            var platforms = sim.Level.QueryPlatforms(sim.Time, sim.Clouds, sim.Difficulty, sim.PlayerX, 600f);
            PicnicPlatformSpec current = null;
            for (int i = 0; i < platforms.Count; i++)
            {
                if (platforms[i].Id == sim.SupportId) { current = platforms[i]; break; }
            }

            bool jump = false;
            if (sim.OnGround && current != null)
            {
                float distToEdge = (current.X + current.Width) - sim.PlayerX;
                if (distToEdge < 55f)
                {
                    // Check if next ground seamlessly continues at same or lower Y
                    bool seamless = false;
                    for (int i = 0; i < platforms.Count; i++)
                    {
                        var p = platforms[i];
                        if (p.Id != current.Id && p.Ground && Math.Abs(p.X - (current.X + current.Width)) < 4f && p.Y >= current.Y - 10f)
                        {
                            seamless = true;
                            break;
                        }
                    }
                    if (!seamless) jump = true;
                }

                // Jump over nearby enemies or emerging worms ahead
                if (!jump)
                {
                    foreach (var e in sim.Enemies)
                    {
                        if (e.Alive && e.X > sim.PlayerX && e.X - sim.PlayerX < 130f && Math.Abs(e.Y - sim.PlayerY) < 90f)
                        {
                            jump = true;
                            break;
                        }
                    }
                }
                if (!jump)
                {
                    foreach (var w in sim.Encounters.Worms)
                    {
                        if (w.Exposure > 0.1f && w.X > sim.PlayerX && w.X - sim.PlayerX < 120f)
                        {
                            jump = true;
                            break;
                        }
                    }
                }
            }
            else if (!sim.OnGround && sim.PlayerVy < 0f)
            {
                // Hold jump during ascent
                jump = true;
            }

            return new PicnicInput(1f, jump);
        }
    }
}
