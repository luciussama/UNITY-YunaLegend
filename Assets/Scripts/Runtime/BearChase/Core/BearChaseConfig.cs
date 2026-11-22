namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Fachada de compatibilidade e constantes de referencia do dominio da Fase 4.
    /// Todas as curvas e parametros vivem em BearChaseTrackSpec, BearChaseTrackSampler e BearChaseTuningSpec.
    /// </summary>
    public static class BearChaseConfig
    {
        public static float TrackLength => BearChaseTrackSpec.Default.TrackLength;
        public static float LaneHalfWidth => BearChaseTrackSpec.Default.LaneHalfWidth;
        public static float CorridorHalfWidth => BearChaseTrackSpec.Default.CorridorHalfWidth;
        public static float WallHeight => BearChaseTrackSpec.Default.WallHeight;

        public static float EntranceWalkDuration => BearChaseEntranceTimeline.ApproachEnd;
        public static float EntranceNudgeDuration => BearChaseEntranceTimeline.ReachEnd - BearChaseEntranceTimeline.ApproachEnd;
        public static float BearWakeUpDuration => BearChaseEntranceTimeline.WakeUpEnd - BearChaseEntranceTimeline.ReachEnd;
        public static float TransitionDuration => BearChaseEntranceTimeline.TotalDuration - BearChaseEntranceTimeline.WakeUpEnd;

        public static int NormalMaxHearts => BearChaseTuningSpec.Default.NormalMaxHearts;
        public static int EasyMaxHearts => BearChaseTuningSpec.Default.EasyMaxHearts;
        public static int FailuresToOfferEasyMode => BearChaseTuningSpec.Default.FailuresToOfferEasyMode;

        public static float TrackCenterX(float z)
        {
            return BearChaseTrackSampler.CenterX(z);
        }

        public static float TrackElevationY(float z)
        {
            return BearChaseTrackSampler.ElevationY(z);
        }

        public static float TrackTangentSlopeX(float z)
        {
            return BearChaseTrackSampler.TangentSlopeX(z);
        }

        public static float TrackTangentSlopeY(float z)
        {
            return BearChaseTrackSampler.TangentSlopeY(z);
        }
    }
}
