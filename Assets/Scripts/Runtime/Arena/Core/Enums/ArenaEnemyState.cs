using System;
using System.Collections.Generic;

namespace YunaLegend.Arena.Core
{
    public enum ArenaEnemyState
    {
        Spawning,
        Idle,
        Approaching,
        Retreating,
        Evading,
        Windup,
        ActiveAttack,
        Recovery,
        HitReaction,
        Defeated
    }
}
