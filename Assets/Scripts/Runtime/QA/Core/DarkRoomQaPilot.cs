using System;

namespace YunaLegend.Qa.Core
{
    /// <summary>
    /// Piloto deterministico puro (sem dependencias de engine) para o Modo QA opt-in da Fase 1
    /// (O Quarto Escuro / Fase-Jogavel-Completa). Decide o instante de salto a partir da borda
    /// do apoio atual ou distancia ate a proxima plataforma obrigatoria.
    /// </summary>
    public static class DarkRoomQaPilot
    {
        public static bool ShouldJump(
            float playerX,
            float runDirection,
            float runSpeed,
            bool isGrounded,
            bool hasCurrentSupportEdge,
            float currentSupportForwardEdgeX,
            bool hasNextPlatform,
            float nextPlatformNearEdgeX,
            float leadSeconds = 0.08f)
        {
            if (!isGrounded)
            {
                return false;
            }

            float dir = runDirection >= 0f ? 1f : -1f;
            float leadDistance = Math.Max(0.25f, Math.Abs(runSpeed) * Math.Max(0.02f, leadSeconds));

            if (hasCurrentSupportEdge)
            {
                float distToEdge = (currentSupportForwardEdgeX - playerX) * dir;
                if (distToEdge <= leadDistance && distToEdge >= -0.35f)
                {
                    return true;
                }
            }

            if (hasNextPlatform)
            {
                float distToNextNear = (nextPlatformNearEdgeX - playerX) * dir;
                float launchDistance = Math.Max(1.05f, Math.Abs(runSpeed) * 0.24f);
                if (distToNextNear <= launchDistance && distToNextNear >= 0.05f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
