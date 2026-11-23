using System;
using System.Collections.Generic;

namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Piloto deterministico puro (sem dependencias de engine) para o Modo QA opt-in do Bear Chase.
    /// Avalia os brinquedos arremessados ativos e gera um eixo lateral legitimo [-1, 1]
    /// para desviar sem alterar fisica ou colisoes.
    /// </summary>
    public static class BearChaseQaPilot
    {
        private const int LaneSampleCount = 19;
        private const float LookAheadMeters = 32f;
        private const float LookBehindMeters = 1.6f;
        private const float SafetyMarginMeters = 0.42f;
        private const float SteeringGain = 3.2f;

        public static float ComputeLateralInput(BearChaseSimulation simulation)
        {
            if (simulation == null || simulation.IsEntrancePhase)
            {
                return 0f;
            }

            if (simulation.Phase != BearChasePhase.Chasing && simulation.Phase != BearChasePhase.Stumble)
            {
                return 0f;
            }

            float halfWidth = simulation.TrackSpec.LaneHalfWidth;
            float usableHalfWidth = halfWidth * 0.92f;
            float currentX = simulation.YunaLaneOffsetX;
            float yunaZ = simulation.YunaZ;
            float yunaRadius = simulation.TuningSpec.YunaCollisionRadius;
            IReadOnlyList<BearChaseThrownToyState> toys = simulation.ActiveThrownToys;

            float bestX = 0f;
            float bestScore = float.NegativeInfinity;

            for (int s = 0; s < LaneSampleCount; s++)
            {
                float t = LaneSampleCount > 1 ? (float)s / (LaneSampleCount - 1) : 0.5f;
                float candidateX = -usableHalfWidth + (2f * usableHalfWidth * t);
                float score = EvaluateCandidateLane(candidateX, currentX, yunaZ, yunaRadius, toys);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestX = candidateX;
                }
            }

            float deltaX = bestX - currentX;
            if (Math.Abs(deltaX) < 0.04f)
            {
                return 0f;
            }

            return Math.Max(-1f, Math.Min(1f, deltaX * SteeringGain));
        }

        private static float EvaluateCandidateLane(
            float candidateX,
            float currentX,
            float yunaZ,
            float yunaRadius,
            IReadOnlyList<BearChaseThrownToyState> toys)
        {
            float penalty = 0f;

            for (int i = 0; i < toys.Count; i++)
            {
                BearChaseThrownToyState toy = toys[i];
                if (toy.WasHit
                    || toy.Phase == BearChaseThrownToyPhase.Deflected
                    || toy.Phase == BearChaseThrownToyPhase.Expired)
                {
                    continue;
                }

                float relevantZ = toy.Phase == BearChaseThrownToyPhase.InFlight ? toy.TargetZ : toy.CurrentZ;
                float dz = relevantZ - yunaZ;
                if (dz < -LookBehindMeters || dz > LookAheadMeters)
                {
                    continue;
                }

                float hazardX = toy.Phase == BearChaseThrownToyPhase.InFlight ? toy.TargetLaneX : toy.CurrentLaneX;
                BearChaseObstacleDef def = BearChaseObstacleCatalog.Get(toy.Kind);
                float dangerHalfWidth = def.HalfWidth + yunaRadius + SafetyMarginMeters;
                float dx = Math.Abs(candidateX - hazardX);

                float urgency = 1f / (Math.Max(0.5f, dz + 2.5f));
                if (dx < dangerHalfWidth)
                {
                    float overlap = (dangerHalfWidth - dx) / dangerHalfWidth;
                    penalty += (40f + 80f * overlap) * urgency;
                }
                else
                {
                    penalty += (1.5f / (dx + 0.35f)) * urgency;
                }
            }

            float moveCost = Math.Abs(candidateX - currentX) * 0.12f;
            float centerBias = Math.Abs(candidateX) * 0.08f;
            return -(penalty + moveCost + centerBias);
        }
    }
}
