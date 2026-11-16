using System;

namespace YunaLegend.ToyRoom.Core
{
    public enum ToyRoomPhase
    {
        Intro,
        Exploring,
        RecurrenceMystery,
        InvestigationCutscene,
        SeekingSword,
        SwordPickup,
        Completed
    }

    /// <summary>
    /// Maquina de estados pura da Fase 2 (Toy Room), coordenando fluxo narrativo e eventos de dominio.
    /// </summary>
    public sealed class ToyRoomStateMachine
    {
        public ToyRoomPhase CurrentPhase { get; private set; }
        public ToyRecurrencePlanner Planner { get; }

        public event Action<ToyRoomPhase> OnPhaseChanged;
        public event Action OnStrangeDialogueTriggered;
        public event Action OnInvestigationTriggered;
        public event Action<string, ToyRoomPoint> OnToyRespawnedOutsideView;
        public event Action OnSwordPickupStarted;
        public event Action OnStageCompleted;

        public ToyRoomStateMachine(ToyRecurrencePlanner planner = null)
        {
            CurrentPhase = ToyRoomPhase.Intro;
            Planner = planner ?? new ToyRecurrencePlanner();

            Planner.OnToyRespawned += HandleToyRespawned;
            Planner.OnStrangeDialogueReached += HandleStrangeDialogueReached;
            Planner.OnInvestigationReached += HandleInvestigationReached;
        }

        public void CompleteIntro()
        {
            if (CurrentPhase != ToyRoomPhase.Intro)
            {
                return;
            }

            TransitionTo(ToyRoomPhase.Exploring);
        }

        public bool TryPickUpToy(string toyId)
        {
            if (CurrentPhase != ToyRoomPhase.Exploring && CurrentPhase != ToyRoomPhase.RecurrenceMystery)
            {
                return false;
            }

            return Planner.TryPickUp(toyId);
        }

        public bool TryStoreToyInChest(string toyId)
        {
            if (CurrentPhase != ToyRoomPhase.Exploring && CurrentPhase != ToyRoomPhase.RecurrenceMystery)
            {
                return false;
            }

            bool stored = Planner.TryStoreInChest(toyId);
            if (stored && Planner.IsRecurrenceActive && CurrentPhase == ToyRoomPhase.Exploring)
            {
                TransitionTo(ToyRoomPhase.RecurrenceMystery);
            }

            return stored;
        }

        public void Tick(
            float deltaSeconds,
            ToyRoomPoint playerPosition,
            Func<ToyRoomPoint, bool> isInsideMainCameraFocus)
        {
            if (CurrentPhase != ToyRoomPhase.Exploring && CurrentPhase != ToyRoomPhase.RecurrenceMystery)
            {
                return;
            }

            Planner.Tick(deltaSeconds, playerPosition, isInsideMainCameraFocus);
        }

        public void CompleteInvestigationCutscene()
        {
            if (CurrentPhase != ToyRoomPhase.InvestigationCutscene)
            {
                return;
            }

            TransitionTo(ToyRoomPhase.SeekingSword);
        }

        public void CollectToySword()
        {
            if (CurrentPhase != ToyRoomPhase.SeekingSword)
            {
                return;
            }

            TransitionTo(ToyRoomPhase.SwordPickup);
            OnSwordPickupStarted?.Invoke();
        }

        public void CompleteSwordPickup()
        {
            if (CurrentPhase != ToyRoomPhase.SwordPickup)
            {
                return;
            }

            TransitionTo(ToyRoomPhase.Completed);
            OnStageCompleted?.Invoke();
        }

        private void HandleToyRespawned(string toyId, ToyRoomPoint point)
        {
            OnToyRespawnedOutsideView?.Invoke(toyId, point);
        }

        private void HandleStrangeDialogueReached()
        {
            OnStrangeDialogueTriggered?.Invoke();
        }

        private void HandleInvestigationReached()
        {
            if (CurrentPhase == ToyRoomPhase.RecurrenceMystery || CurrentPhase == ToyRoomPhase.Exploring)
            {
                TransitionTo(ToyRoomPhase.InvestigationCutscene);
                OnInvestigationTriggered?.Invoke();
            }
        }

        private void TransitionTo(ToyRoomPhase nextPhase)
        {
            if (CurrentPhase == nextPhase)
            {
                return;
            }

            CurrentPhase = nextPhase;
            OnPhaseChanged?.Invoke(nextPhase);
        }
    }
}
