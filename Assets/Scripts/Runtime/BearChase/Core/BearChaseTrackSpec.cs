namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Descritor imutavel das dimensoes e perfil de curvatura/elevacao do corredor.
    /// </summary>
    public sealed class BearChaseTrackSpec
    {
        public const float FinaleStartZ = 460f;
        public const float DefaultTrackLength = 568f;
        public static readonly BearChaseTrackSpec Default = new BearChaseTrackSpec();

        public float TrackLength { get; }
        public float LaneHalfWidth { get; }
        public float CorridorHalfWidth { get; }
        public float WallHeight { get; }
        public float EnvelopeFadeIn { get; }
        public float EnvelopeFadeOut { get; }

        public BearChaseTrackSpec(
            float trackLength = DefaultTrackLength,
            float laneHalfWidth = 2.85f,
            float corridorHalfWidth = 4.35f,
            float wallHeight = 5.0f,
            float envelopeFadeIn = 18f,
            float envelopeFadeOut = 28f)
        {
            TrackLength = trackLength;
            LaneHalfWidth = laneHalfWidth;
            CorridorHalfWidth = corridorHalfWidth;
            WallHeight = wallHeight;
            EnvelopeFadeIn = envelopeFadeIn;
            EnvelopeFadeOut = envelopeFadeOut;
        }
    }
}
