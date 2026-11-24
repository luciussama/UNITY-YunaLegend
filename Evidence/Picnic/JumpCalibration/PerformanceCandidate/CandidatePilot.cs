using System;
using System.Runtime.CompilerServices;

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

    /// <summary>Opt-in pilot forecasts ordinary commands on independent copies of the real domain state.</summary>
    public static class PicnicQaPilot
    {
        private sealed class Plan
        {
            public int Support = -1;
            public float Recheck;
            public float JumpAt = float.PositiveInfinity;
            public float Axis = 1f;
            public SearchNode[][] Layers;
            public readonly System.Collections.Generic.List<SearchNode> Beam =
                new System.Collections.Generic.List<SearchNode>(8);
            public readonly System.Collections.Generic.List<SearchNode> Candidates =
                new System.Collections.Generic.List<SearchNode>(40);
            public readonly SearchNode Root = new SearchNode();

            public void Initialize(PicnicSimulation source)
            {
                if (Layers != null)
                {
                    return;
                }
                Layers = new SearchNode[2][];
                for (int layer = 0; layer < Layers.Length; layer++)
                {
                    Layers[layer] = new SearchNode[40];
                    for (int i = 0; i < Layers[layer].Length; i++)
                    {
                        Layers[layer][i] = new SearchNode { State = source.CopyForPrediction() };
                    }
                }
            }
        }

        private static readonly ConditionalWeakTable<PicnicSimulation, Plan> Plans =
            new ConditionalWeakTable<PicnicSimulation, Plan>();

        public static PicnicInput Read(PicnicSimulation sim)
        {
            if (sim.Phase != PicnicPhase.Playing)
            {
                return new PicnicInput(0f, false);
            }
            Plan plan = Plans.GetOrCreateValue(sim);
            if (plan.Support != sim.SupportIndex || sim.Time >= plan.Recheck)
            {
                BuildPlan(sim, plan);
            }
            bool jump = sim.Time + 0.0001f >= plan.JumpAt;
            if (jump)
            {
                plan.Recheck = sim.Time + 0.099f;
                plan.JumpAt = float.PositiveInfinity;
            }
            return new PicnicInput(plan.Axis, jump);
        }

        private sealed class SearchNode
        {
            public PicnicSimulation State;
            public PicnicInput First;
            public float Score;
        }

        private static void BuildPlan(PicnicSimulation sim, Plan plan)
        {
            const int beamWidth = 8;
            const int horizon = 15;
            const float commandDuration = 0.1f;
            plan.Initialize(sim);
            var beam = plan.Beam;
            beam.Clear();
            plan.Root.State = sim;
            plan.Root.First = new PicnicInput(1f, false);
            plan.Root.Score = sim.X;
            beam.Add(plan.Root);
            SearchNode best = beam[0];
            for (int depth = 0; depth < horizon; depth++)
            {
                var candidates = plan.Candidates;
                candidates.Clear();
                int used = 0;
                foreach (SearchNode node in beam)
                {
                    for (int action = 0; action < 5; action++)
                    {
                        float axis = action == 2 ? 0f : action >= 3 ? -1f : 1f;
                        bool jump = action == 1 || action == 4;
                        if ((!node.State.Grounded && jump) || (node.State.Panic && axis != 1f))
                        {
                            continue;
                        }
                        SearchNode candidate = plan.Layers[depth % 2][used++];
                        PicnicSimulation forecast = candidate.State;
                        forecast.CopyPredictionFrom(node.State);
                        bool valid = true;
                        for (int step = 0; step < 12; step++)
                        {
                            forecast.Step(1f / 120f, axis, jump && step == 0);
                            if (!Safe(sim, forecast) || forecast.Y < -1f)
                            {
                                valid = false;
                                break;
                            }
                        }
                        if (!valid)
                        {
                            continue;
                        }
                        float score = forecast.X + (forecast.Grounded ? 0.05f : 0f)
                            + forecast.Y * 0.01f;
                        candidate.First = depth == 0 ? new PicnicInput(axis, jump) : node.First;
                        candidate.Score = score;
                        candidates.Add(candidate);
                    }
                }
                if (candidates.Count == 0)
                {
                    break;
                }
                candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
                beam.Clear();
                // Keep spatially distinct futures; identical command histories add no coverage.
                foreach (SearchNode candidate in candidates)
                {
                    bool duplicate = false;
                    foreach (SearchNode kept in beam)
                    {
                        if (Math.Abs(kept.State.X - candidate.State.X) < 0.07f
                            && Math.Abs(kept.State.Y - candidate.State.Y) < 0.09f
                            && Math.Abs(kept.State.VelocityY - candidate.State.VelocityY) < 0.8f)
                        {
                            duplicate = true;
                            break;
                        }
                    }
                    if (!duplicate)
                    {
                        beam.Add(candidate);
                    }
                    if (beam.Count == beamWidth)
                    {
                        break;
                    }
                }
                best = beam[0];
            }
            plan.Support = sim.SupportIndex;
            plan.Recheck = sim.Time + commandDuration - 0.001f;
            plan.Axis = best.First.Axis;
            plan.JumpAt = best.First.Jump ? sim.Time : float.PositiveInfinity;
        }

        private static bool Safe(PicnicSimulation original, PicnicSimulation forecast)
        {
            if (forecast.Phase == PicnicPhase.Defeat || forecast.Phase == PicnicPhase.Rescuing
                || forecast.Hearts < original.Hearts || forecast.Falls != original.Falls)
            {
                return false;
            }
            return true;
        }
    }
}


