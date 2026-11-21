namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Fases sequenciais da Fase 4 (Perseguicao do Urso Fujao),
    /// preservando a cadencia exata da obra legada.
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
}
