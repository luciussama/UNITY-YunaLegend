namespace YunaLegend.BearChase.Core
{
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
}
