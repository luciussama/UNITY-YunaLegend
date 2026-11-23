using System;

namespace YunaLegend.Picnic.Core
{
    public readonly struct PicnicInput
    {
        public float Axis { get; }
        public bool Jump { get; }
        public PicnicInput(float axis, bool jump)
        {
            Axis = axis;
            Jump = jump;
        }
    }

    /// <summary>Opt-in pilot uses ordinary movement/jump commands, never teleports or suppresses collisions.</summary>
    public static class PicnicQaPilot
    {
        public static PicnicInput Read(PicnicSimulation sim)
        {
            if (sim.Phase != PicnicPhase.Playing) return new PicnicInput(0f, false);
            bool hazard = false;
            foreach (var creature in sim.Creatures)
            {
                float dx = creature.X - sim.X;
                if (creature.Active && dx > -0.15f && dx < (sim.Terrain == PicnicTerrain.Gravel ? 2.6f : 1.3f)
                    && creature.Y < sim.Y + 1.5f) hazard = true;
            }
            if (!sim.Grounded) return new PicnicInput(1f, false);
            var support = sim.Stage.Supports[sim.SupportIndex];
            float remaining = support.Right(sim.Time) - sim.X;
            float speed = sim.Panic ? 3.2f : PicnicStageSpec.Speed(sim.Terrain);
            bool needsGapJump = !HasFloor(sim, sim.X + 0.5f, sim.Y, sim.Time + 0.2f);
            bool cloud = sim.Terrain == PicnicTerrain.Cloud;
            bool wheel = support.Wheel >= 0;
            bool jump = hazard || ((needsGapJump || cloud || wheel) && remaining < Math.Max(0.35f, speed * 0.2f));
            if (jump && !hazard && !CanLandAhead(sim, speed))
            {
                // Riding a wheel until an ascending neighbour is reachable is ordinary player input.
                return new PicnicInput(sim.Panic ? 1f : 0f, false);
            }
            return new PicnicInput(1f, jump);
        }

        private static bool HasFloor(PicnicSimulation sim, float x, float y, float time)
        {
            for (int i = 0; i < sim.Stage.Supports.Count; i++)
            {
                var support = sim.Stage.Supports[i];
                if (!sim.IsSupportActive(i) || x < support.Left(time) || x > support.Right(time)) continue;
                float height = support.Height(time, x);
                if (height <= y + 0.2f && height > y - 0.6f) return true;
            }
            return false;
        }

        private static bool CanLandAhead(PicnicSimulation sim, float speed)
        {
            for (float t = 0.4f; t < 1.1f; t += 0.025f)
            {
                float x = sim.X + speed * t;
                float y = sim.Y + PicnicStageSpec.JumpSpeed * t - 0.5f * PicnicStageSpec.Gravity * t * t;
                for (int i = 0; i < sim.Stage.Supports.Count; i++)
                {
                    var support = sim.Stage.Supports[i];
                    if (i == sim.SupportIndex || !sim.IsSupportActive(i)) continue;
                    if (x < support.Left(sim.Time + t) + 0.08f || x > support.Right(sim.Time + t) - 0.08f) continue;
                    float height = support.Height(sim.Time + t, x);
                    if (Math.Abs(y - height) < 0.22f && x > sim.X + 0.4f) return true;
                }
            }
            return false;
        }
    }
}
