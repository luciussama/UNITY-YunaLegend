using System;

namespace YunaLegend.BearChase.Core
{
    public enum BearChaseObstacleKind
    {
        Bookcase = 0,
        Armchair = 1,
        Stool = 2,
        BlockTower = 3,
        ToyTrain = 4,
        PlushBunny = 5,
        SmallBear = 6,
        RubberDuck = 7
    }

    public readonly struct BearChaseObstacleSpec
    {
        public readonly int Id;
        public readonly float Z;
        public readonly float LaneOffsetX;
        public readonly float HalfWidth;
        public readonly float HalfDepth;
        public readonly BearChaseObstacleKind Kind;

        public BearChaseObstacleSpec(
            int id,
            float z,
            float laneOffsetX,
            float halfWidth,
            float halfDepth,
            BearChaseObstacleKind kind)
        {
            Id = id;
            Z = z;
            LaneOffsetX = laneOffsetX;
            HalfWidth = halfWidth;
            HalfDepth = halfDepth;
            Kind = kind;
        }
    }

    /// <summary>
    /// Parametros puros de cadencia, pista curva e obstaculos da Fase 4.
    /// </summary>
    public static class BearChaseConfig
    {
        // Cadencia da cena de entrada (~6.5s total)
        public const float EntranceWalkDuration = 4.0f;
        public const float EntranceNudgeDuration = 1.0f;
        public const float BearWakeUpDuration = 1.0f;
        public const float TransitionDuration = 0.5f;

        // Parametros da corrida 3D (~65s de duracao alvo)
        public const float TrackLength = 280f;
        public const float YunaForwardSpeed = 4.35f;
        public const float YunaLateralSpeed = 6.2f;
        public const float LaneHalfWidth = 3.15f;
        public const float YunaCollisionRadius = 0.42f;

        // Urso: dispara na frente com boa margem e mantem ritmo vivo
        public const float BearInitialLead = 3.2f;
        public const float BearTargetLead = 11.5f;
        public const float BearCatchupThreshold = 2.0f;

        // Vidas, tropeço e modo facil
        public const int NormalMaxHearts = 3;
        public const int EasyMaxHearts = 6;
        public const int FailuresToOfferEasyMode = 2;
        public const float StumbleDuration = 0.55f;
        public const float InvulnerabilityDuration = 1.35f;
        public const float FallenBannerDuration = 1.05f;

        /// <summary>
        /// Curva suave em S ao longo do corredor para manter leitura visual natural.
        /// </summary>
        public static float TrackCenterX(float z)
        {
            float clampedZ = Math.Max(0f, Math.Min(TrackLength + 20f, z));
            float fadeIn = Math.Min(1f, clampedZ / 18f);
            float fadeOut = Math.Min(1f, Math.Max(0f, (TrackLength - clampedZ) / 22f));
            float envelope = fadeIn * fadeOut;

            double primary = Math.Sin(clampedZ * 0.042) * 3.6;
            double secondary = Math.Sin(clampedZ * 0.019 + 0.7) * 1.8;
            return (float)((primary + secondary) * envelope);
        }

        /// <summary>
        /// Derivada aproximada da curva para orientar camera, Yuna e Urso ao longo do corredor.
        /// </summary>
        public static float TrackTangentSlopeX(float z)
        {
            const float eps = 0.5f;
            return (TrackCenterX(z + eps) - TrackCenterX(z - eps)) / (2f * eps);
        }

        public static BearChaseObstacleSpec[] BuildDefaultObstacles()
        {
            var kinds = new[]
            {
                BearChaseObstacleKind.BlockTower,
                BearChaseObstacleKind.Stool,
                BearChaseObstacleKind.ToyTrain,
                BearChaseObstacleKind.Bookcase,
                BearChaseObstacleKind.PlushBunny,
                BearChaseObstacleKind.Armchair,
                BearChaseObstacleKind.RubberDuck,
                BearChaseObstacleKind.SmallBear
            };

            // Padrao ritmado com folga generosa nos primeiros metros e aumento gradual de cadencia
            float[] lanePattern = { -1.75f, 1.65f, 0.0f, -1.95f, 1.85f, -0.55f, 0.75f, -2.05f, 2.0f, 0.1f };
            int count = 34;
            var list = new BearChaseObstacleSpec[count];

            float z = 14f;
            for (int i = 0; i < count; i++)
            {
                BearChaseObstacleKind kind = kinds[i % kinds.Length];
                float lane = lanePattern[i % lanePattern.Length];
                GetFootprint(kind, out float hw, out float hd);
                list[i] = new BearChaseObstacleSpec(i, z, lane, hw, hd, kind);

                float progress = i / (float)(count - 1);
                float spacing = 7.6f - progress * 1.6f;
                z += spacing;
            }

            return list;
        }

        private static void GetFootprint(BearChaseObstacleKind kind, out float halfWidth, out float halfDepth)
        {
            switch (kind)
            {
                case BearChaseObstacleKind.Bookcase:
                    halfWidth = 0.85f;
                    halfDepth = 0.42f;
                    break;
                case BearChaseObstacleKind.Armchair:
                    halfWidth = 0.72f;
                    halfDepth = 0.58f;
                    break;
                case BearChaseObstacleKind.Stool:
                    halfWidth = 0.48f;
                    halfDepth = 0.48f;
                    break;
                case BearChaseObstacleKind.BlockTower:
                    halfWidth = 0.52f;
                    halfDepth = 0.52f;
                    break;
                case BearChaseObstacleKind.ToyTrain:
                    halfWidth = 0.55f;
                    halfDepth = 0.65f;
                    break;
                default:
                    halfWidth = 0.42f;
                    halfDepth = 0.42f;
                    break;
            }
        }
    }
}
