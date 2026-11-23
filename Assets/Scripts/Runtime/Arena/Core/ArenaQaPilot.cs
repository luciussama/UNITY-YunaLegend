using System;

namespace YunaLegend.Arena.Core
{
    /// <summary>
    /// Piloto deterministico puro (sem dependencias de engine) para o Modo QA opt-in da Arena.
    /// Usa apenas entradas legitimas de ArenaInputState (mover, agachar, defender e atacar).
    /// </summary>
    public static class ArenaQaPilot
    {
        public static ArenaInputState ComputeInput(ArenaCombatSimulation simulation)
        {
            if (simulation == null || simulation.Outcome != ArenaBattleOutcome.Ongoing || simulation.IsSpecialActive)
            {
                return default;
            }

            if (simulation.EnemyState == ArenaEnemyState.Spawning || simulation.EnemyState == ArenaEnemyState.Defeated)
            {
                return default;
            }

            int facing = simulation.EnemyX >= simulation.PlayerX ? 1 : -1;
            float distance = Math.Abs(simulation.EnemyX - simulation.PlayerX);
            float minSep = simulation.GetMinBodySeparation();
            float attackRange = minSep + 95f;

            bool enemyThreatening = simulation.EnemyState == ArenaEnemyState.Windup
                || simulation.EnemyState == ArenaEnemyState.Active;

            if (enemyThreatening)
            {
                if (simulation.ActiveAttackKind == ArenaAttackKind.LaserBeam)
                {
                    return new ArenaInputState
                    {
                        CrouchHeld = true
                    };
                }

                return new ArenaInputState
                {
                    MoveX = -facing
                };
            }

            if (simulation.EnemyProtectedResponse || simulation.EnemyState == ArenaEnemyState.HitReaction)
            {
                return new ArenaInputState
                {
                    MoveX = -facing
                };
            }

            if (distance > attackRange)
            {
                return new ArenaInputState
                {
                    MoveX = facing
                };
            }

            bool canAttack = simulation.SwordTick == 0
                && simulation.SwordCooldown == 0
                && simulation.BlockRecovery == 0
                && simulation.PlayerHitRecovery == 0;

            return new ArenaInputState
            {
                MoveX = 0f,
                AttackPressed = canAttack
            };
        }
    }
}
