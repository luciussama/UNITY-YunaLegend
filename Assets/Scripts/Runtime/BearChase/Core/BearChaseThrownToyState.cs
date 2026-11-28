namespace YunaLegend.BearChase.Core
{

    /// <summary>
    /// Snapshot imutavel de um brinquedo arremessado pelo urso durante a perseguiçao.
    /// </summary>
    public sealed class BearChaseThrownToyState
    {
        public int Id { get; internal set; }
        public BearChaseObstacleKind Kind { get; internal set; }
        public BearChaseThrownToyPhase Phase { get; internal set; }
        public float StartZ { get; internal set; }
        public float StartLaneX { get; internal set; }
        public float TargetZ { get; internal set; }
        public float TargetLaneX { get; internal set; }
        public float CurrentZ { get; internal set; }
        public float CurrentLaneX { get; internal set; }
        public float HeightAboveGround { get; internal set; }
        public float NormalizedFlight { get; internal set; }
        public float BounceTime { get; internal set; }
        public float SpinAngleDeg { get; internal set; }
        public bool WasHit { get; internal set; }
        public float DeflectDirX { get; internal set; }
    }
}
