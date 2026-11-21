namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Fases sequenciais da Fase 4 (Perseguicao do Urso Fujao).
    /// </summary>
    public enum BearChasePhase
    {
        EntranceWalk = 0,
        EntranceNudge = 1,
        BearWakeUp = 2,
        TransitionToChase = 3,
        Chasing = 4,
        Stumble = 5,
        Fallen = 6,
        RetryModal = 7,
        Victory = 8
    }

    /// <summary>
    /// Estado comportamental atual do urso para coordenacao dos animadores.
    /// </summary>
    public enum BearChaseBearAction
    {
        Sleeping = 0,
        WakingUp = 1,
        Running = 2,
        LookingBack = 3,
        ThrowingToy = 4,
        Caught = 5
    }

    /// <summary>
    /// Helpers puros de consulta sobre BearChasePhase.
    /// </summary>
    public static class BearChasePhaseExtensions
    {
        public static bool IsEntrance(this BearChasePhase phase)
        {
            return phase == BearChasePhase.EntranceWalk
                || phase == BearChasePhase.EntranceNudge
                || phase == BearChasePhase.BearWakeUp
                || phase == BearChasePhase.TransitionToChase;
        }
    }
}
