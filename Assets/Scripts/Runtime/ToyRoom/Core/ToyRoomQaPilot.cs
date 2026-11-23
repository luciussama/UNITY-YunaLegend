using System;

namespace YunaLegend.ToyRoom.Core
{
    public readonly struct ToyRoomQaDecision
    {
        public float MoveX { get; }
        public float MoveZ { get; }
        public bool ShouldInteract { get; }
        public bool ShouldAdvanceDialogue { get; }

        public ToyRoomQaDecision(float moveX, float moveZ, bool shouldInteract, bool shouldAdvanceDialogue)
        {
            MoveX = moveX;
            MoveZ = moveZ;
            ShouldInteract = shouldInteract;
            ShouldAdvanceDialogue = shouldAdvanceDialogue;
        }
    }

    /// <summary>
    /// Piloto deterministico puro (sem dependencias de engine) para o Modo QA opt-in da Fase 2
    /// (Quarto dos Brinquedos). Conduz Yuna ate o alvo ativo (brinquedo, bau ou espada),
    /// dispara interacao quando ao alcance e avanca dialogos automaticamente.
    /// </summary>
    public static class ToyRoomQaPilot
    {
        public static ToyRoomQaDecision ComputeAction(
            ToyRoomPhase phase,
            ToyRoomPoint playerPos,
            bool hasTarget,
            ToyRoomPoint targetPos,
            bool withinInteractRange,
            bool dialogueOrIntroActive)
        {
            if (dialogueOrIntroActive)
            {
                return new ToyRoomQaDecision(0f, 0f, shouldInteract: false, shouldAdvanceDialogue: true);
            }

            if (phase == ToyRoomPhase.Completed || phase == ToyRoomPhase.SwordPickup || !hasTarget)
            {
                return new ToyRoomQaDecision(0f, 0f, shouldInteract: false, shouldAdvanceDialogue: false);
            }

            if (withinInteractRange)
            {
                return new ToyRoomQaDecision(0f, 0f, shouldInteract: true, shouldAdvanceDialogue: false);
            }

            float dx = targetPos.X - playerPos.X;
            float dz = targetPos.Z - playerPos.Z;
            float dist = (float)Math.Sqrt(dx * dx + dz * dz);
            if (dist < 0.05f)
            {
                return new ToyRoomQaDecision(0f, 0f, shouldInteract: true, shouldAdvanceDialogue: false);
            }

            return new ToyRoomQaDecision(dx / dist, dz / dist, shouldInteract: false, shouldAdvanceDialogue: false);
        }
    }
}
