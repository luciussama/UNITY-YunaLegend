using System.Collections.Generic;

namespace YunaLegend.BearChase.Core
{
    public enum BearChaseObstacleKind
    {
        ToyTrain = 0,
        BlockTower = 1,
        PlushBunny = 2,
        RubberDuck = 3,
        SmallDrum = 4
    }

    /// <summary>
    /// Descritor data-driven de cada brinquedo arremessavel pelo urso.
    /// </summary>
    public readonly struct BearChaseObstacleDef
    {
        public readonly BearChaseObstacleKind Kind;
        public readonly string PropKey;
        public readonly float HalfWidth;
        public readonly float HalfDepth;
        public readonly float VisualScale;
        public readonly float FlightDuration;
        public readonly float ArcHeight;
        public readonly float SlideSpeed;
        public readonly float BounceHeight;
        public readonly float SpinSpeedDeg;

        public BearChaseObstacleDef(
            BearChaseObstacleKind kind,
            string propKey,
            float halfWidth,
            float halfDepth,
            float visualScale,
            float flightDuration,
            float arcHeight,
            float slideSpeed,
            float bounceHeight,
            float spinSpeedDeg)
        {
            Kind = kind;
            PropKey = propKey;
            HalfWidth = halfWidth;
            HalfDepth = halfDepth;
            VisualScale = visualScale;
            FlightDuration = flightDuration;
            ArcHeight = arcHeight;
            SlideSpeed = slideSpeed;
            BounceHeight = bounceHeight;
            SpinSpeedDeg = spinSpeedDeg;
        }
    }

    /// <summary>
    /// Catalogo unico de brinquedos arremessaveis pelo urso na corrida.
    /// </summary>
    public static class BearChaseObstacleCatalog
    {
        private static readonly BearChaseObstacleDef[] Definitions =
        {
            new BearChaseObstacleDef(
                BearChaseObstacleKind.ToyTrain,
                "train",
                halfWidth: 0.48f,
                halfDepth: 0.56f,
                visualScale: 1.12f,
                flightDuration: 0.64f,
                arcHeight: 1.32f,
                slideSpeed: 2.35f,
                bounceHeight: 0.22f,
                spinSpeedDeg: 240f),
            new BearChaseObstacleDef(
                BearChaseObstacleKind.BlockTower,
                "blocks",
                halfWidth: 0.46f,
                halfDepth: 0.46f,
                visualScale: 1.40f,
                flightDuration: 0.62f,
                arcHeight: 1.40f,
                slideSpeed: 1.45f,
                bounceHeight: 0.28f,
                spinSpeedDeg: 320f),
            new BearChaseObstacleDef(
                BearChaseObstacleKind.PlushBunny,
                "bunny",
                halfWidth: 0.44f,
                halfDepth: 0.44f,
                visualScale: 1.22f,
                flightDuration: 0.66f,
                arcHeight: 1.48f,
                slideSpeed: 1.75f,
                bounceHeight: 0.34f,
                spinSpeedDeg: 280f),
            new BearChaseObstacleDef(
                BearChaseObstacleKind.RubberDuck,
                "duck",
                halfWidth: 0.42f,
                halfDepth: 0.42f,
                visualScale: 1.22f,
                flightDuration: 0.65f,
                arcHeight: 1.45f,
                slideSpeed: 1.90f,
                bounceHeight: 0.36f,
                spinSpeedDeg: 340f),
            new BearChaseObstacleDef(
                BearChaseObstacleKind.SmallDrum,
                "drum",
                halfWidth: 0.48f,
                halfDepth: 0.48f,
                visualScale: 1.28f,
                flightDuration: 0.68f,
                arcHeight: 1.36f,
                slideSpeed: 2.15f,
                bounceHeight: 0.26f,
                spinSpeedDeg: 260f)
        };

        public static IReadOnlyList<BearChaseObstacleDef> All => Definitions;

        public static BearChaseObstacleDef Get(BearChaseObstacleKind kind)
        {
            for (int i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Kind == kind)
                {
                    return Definitions[i];
                }
            }

            return Definitions[0];
        }
    }
}
