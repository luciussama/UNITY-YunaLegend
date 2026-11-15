using System;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Máquina de estados pura (C# sem dependência de MonoBehaviour/UI) para o fluxo
    /// de abertura, gameplay, cutscene de descanso, checkpoint e conclusão.
    /// </summary>
    public sealed class StageFlowStateMachine
    {
        public enum FlowPhase
        {
            OpeningNandaWake1,
            OpeningPoeticIntro,
            OpeningNandaWake2,
            PreJumpCue,
            Stage1Gameplay,
            CastleCutsceneStep0,
            CastleCutsceneStep1,
            Stage2Gameplay,
            Stage2Completed,
            FalseDoorFalling,
            FalseDoorCutsceneStep0,
            FalseDoorCutsceneStep1,
            Stage3Gameplay,
            TruePortalVictory
        }

        private const float NandaWake1Duration = 4.4f;
        private const float PoeticIntroDuration = 4.4f;
        private const float NandaWake2Duration = 3.4f;

        public FlowPhase Phase { get; private set; } = FlowPhase.OpeningNandaWake1;
        public float StepTimer { get; private set; }
        public bool Stage2CheckpointReached { get; private set; }
        public bool Stage3CheckpointReached { get; private set; }
        public bool CastleCutsceneTriggered { get; private set; }
        public bool FalseDoorTriggered { get; private set; }
        public bool ShowIdlePose =>
            Phase != FlowPhase.Stage1Gameplay
            && Phase != FlowPhase.Stage2Gameplay
            && Phase != FlowPhase.Stage3Gameplay
            && Phase != FlowPhase.FalseDoorFalling;

        public event Action<FlowPhase> OnPhaseChanged;

        public void Reset()
        {
            StepTimer = 0f;
            Stage2CheckpointReached = false;
            Stage3CheckpointReached = false;
            CastleCutsceneTriggered = false;
            FalseDoorTriggered = false;
            SetPhase(FlowPhase.OpeningNandaWake1);
        }

        public void Tick(float deltaTime, bool confirmPressed, bool jumpOrStartPressed)
        {
            StepTimer += deltaTime;

            switch (Phase)
            {
                case FlowPhase.OpeningNandaWake1:
                    if (StepTimer >= NandaWake1Duration || confirmPressed)
                    {
                        SetPhase(FlowPhase.OpeningPoeticIntro);
                    }
                    break;

                case FlowPhase.OpeningPoeticIntro:
                    if (StepTimer >= PoeticIntroDuration || confirmPressed)
                    {
                        SetPhase(FlowPhase.OpeningNandaWake2);
                    }
                    break;

                case FlowPhase.OpeningNandaWake2:
                    if (StepTimer >= NandaWake2Duration || confirmPressed)
                    {
                        SetPhase(FlowPhase.PreJumpCue);
                    }
                    break;

                case FlowPhase.PreJumpCue:
                    if (jumpOrStartPressed)
                    {
                        SetPhase(FlowPhase.Stage1Gameplay);
                    }
                    break;

                case FlowPhase.CastleCutsceneStep0:
                    if (confirmPressed)
                    {
                        SetPhase(FlowPhase.CastleCutsceneStep1);
                    }
                    break;

                case FlowPhase.CastleCutsceneStep1:
                    if (confirmPressed)
                    {
                        Stage2CheckpointReached = true;
                        SetPhase(FlowPhase.Stage2Gameplay);
                    }
                    break;

                case FlowPhase.FalseDoorCutsceneStep0:
                    if (confirmPressed)
                    {
                        SetPhase(FlowPhase.FalseDoorCutsceneStep1);
                    }
                    break;

                case FlowPhase.FalseDoorCutsceneStep1:
                    if (confirmPressed)
                    {
                        Stage3CheckpointReached = true;
                        SetPhase(FlowPhase.Stage3Gameplay);
                    }
                    break;
            }
        }

        public void AdvanceStep()
        {
            switch (Phase)
            {
                case FlowPhase.OpeningNandaWake1:
                    SetPhase(FlowPhase.OpeningPoeticIntro);
                    break;
                case FlowPhase.OpeningPoeticIntro:
                    SetPhase(FlowPhase.OpeningNandaWake2);
                    break;
                case FlowPhase.OpeningNandaWake2:
                    SetPhase(FlowPhase.PreJumpCue);
                    break;
                case FlowPhase.PreJumpCue:
                    SetPhase(FlowPhase.Stage1Gameplay);
                    break;
                case FlowPhase.CastleCutsceneStep0:
                    SetPhase(FlowPhase.CastleCutsceneStep1);
                    break;
                case FlowPhase.CastleCutsceneStep1:
                    Stage2CheckpointReached = true;
                    SetPhase(FlowPhase.Stage2Gameplay);
                    break;
                case FlowPhase.FalseDoorCutsceneStep0:
                    SetPhase(FlowPhase.FalseDoorCutsceneStep1);
                    break;
                case FlowPhase.FalseDoorCutsceneStep1:
                    Stage3CheckpointReached = true;
                    SetPhase(FlowPhase.Stage3Gameplay);
                    break;
            }
        }

        public bool TryTriggerCastleCutscene()
        {
            if (CastleCutsceneTriggered || Phase != FlowPhase.Stage1Gameplay)
            {
                return false;
            }

            CastleCutsceneTriggered = true;
            SetPhase(FlowPhase.CastleCutsceneStep0);
            return true;
        }

        public void RespawnAtStage2Checkpoint()
        {
            Stage2CheckpointReached = true;
            SetPhase(FlowPhase.Stage2Gameplay);
        }

        public bool TryCompleteStage2()
        {
            if (Phase != FlowPhase.Stage2Gameplay)
            {
                return false;
            }

            SetPhase(FlowPhase.Stage2Completed);
            return true;
        }

        public bool TryTriggerFalseDoorFall()
        {
            if (FalseDoorTriggered || Phase != FlowPhase.Stage2Gameplay)
            {
                return false;
            }

            FalseDoorTriggered = true;
            SetPhase(FlowPhase.FalseDoorFalling);
            return true;
        }

        public bool NotifyCinematicFallLanded()
        {
            if (Phase != FlowPhase.FalseDoorFalling)
            {
                return false;
            }

            SetPhase(FlowPhase.FalseDoorCutsceneStep0);
            return true;
        }

        public void RespawnAtStage3Checkpoint()
        {
            Stage2CheckpointReached = true;
            Stage3CheckpointReached = true;
            SetPhase(FlowPhase.Stage3Gameplay);
        }

        public bool TryCompleteTruePortal()
        {
            if (Phase != FlowPhase.Stage3Gameplay)
            {
                return false;
            }

            SetPhase(FlowPhase.TruePortalVictory);
            return true;
        }

        private void SetPhase(FlowPhase next)
        {
            Phase = next;
            StepTimer = 0f;
            OnPhaseChanged?.Invoke(next);
        }
    }
}
