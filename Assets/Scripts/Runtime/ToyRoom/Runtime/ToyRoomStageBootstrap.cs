using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Composition Root da Fase 2 (Toy Room): conecta maquina de estados pura,
    /// movimentacao da Yuna, bau, brinquedos, espada, HUD, narrativa e Nanda azul por eventos.
    /// </summary>
    public sealed class ToyRoomStageBootstrap : MonoBehaviour
    {
        [SerializeField] private Camera isometricCamera;
        [SerializeField] private IsometricToyRoomCamera cameraController;
        [SerializeField] private ToyRoomPlayerController player;
        [SerializeField] private ToyChestController chest;
        [SerializeField] private NandaToyRoomGuideController nandaGuide;
        [SerializeField] private ToySwordView swordView;
        [SerializeField] private ToyRoomHudController hud;
        [SerializeField] private ToyRoomNarrativeController narrative;
        [SerializeField] private ToyRoomAudioController audioController;
        [SerializeField] private List<ToyItemView> toys = new List<ToyItemView>();
        [SerializeField] private bool startInExploring;

        private readonly Dictionary<string, ToyItemView> toyLookup = new Dictionary<string, ToyItemView>();

        public ToyRoomStateMachine StateMachine { get; private set; }

        public void Configure(
            Camera mainCam,
            ToyRoomPlayerController playerController,
            ToyChestController chestController,
            NandaToyRoomGuideController guideController,
            ToySwordView sword,
            ToyRoomHudController hudController,
            ToyRoomNarrativeController narrativeController,
            ToyRoomAudioController audioCtrl,
            List<ToyItemView> toyViews,
            bool autoExplore = false)
        {
            isometricCamera = mainCam;
            cameraController = mainCam != null ? mainCam.GetComponent<IsometricToyRoomCamera>() : null;
            player = playerController;
            chest = chestController;
            nandaGuide = guideController;
            swordView = sword;
            hud = hudController;
            narrative = narrativeController;
            audioController = audioCtrl;
            toys = toyViews ?? new List<ToyItemView>();
            startInExploring = autoExplore;
        }

        private void Awake()
        {
            StateMachine = new ToyRoomStateMachine();
            BuildToyLookup();
            SubscribeEvents();
        }

        private void Start()
        {
            if (startInExploring || narrative == null)
            {
                StateMachine.CompleteIntro();
                return;
            }

            if (player != null)
            {
                player.ControlsEnabled = false;
            }

            narrative.PlayIntro(() => StateMachine.CompleteIntro());
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnPickUpRequested -= HandlePickUpRequested;
                player.OnStoreRequested -= HandleStoreRequested;
            }

            if (StateMachine != null)
            {
                StateMachine.OnToyRespawnedOutsideView -= HandleToyRespawned;
                StateMachine.OnStrangeDialogueTriggered -= HandleStrangeDialogue;
                StateMachine.OnInvestigationTriggered -= HandleInvestigationTriggered;
                StateMachine.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        private void Update()
        {
            if (StateMachine == null || player == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            {
                RequestPlayerInteraction();
            }

            if (chest != null)
            {
                bool carryingNear = player.CarriedToy != null && chest.IsWithinDeliveryRange(player.transform.position);
                chest.SetPlayerCarryingNear(carryingNear);
            }

            UpdateContextualPrompt();

            var playerPoint = new ToyRoomPoint(player.transform.position.x, player.transform.position.z);
            StateMachine.Tick(Time.deltaTime, playerPoint, IsInsideMainCameraFocus);
        }

        public void RequestPlayerInteraction()
        {
            if (StateMachine == null || player == null)
            {
                return;
            }

            if (StateMachine.CurrentPhase == ToyRoomPhase.SeekingSword)
            {
                if (swordView != null && swordView.IsWithinPickupRange(player.transform.position))
                {
                    swordView.Collect();
                    StateMachine.CollectToySword();
                }

                return;
            }

            player.TryInteract(toys, chest);
        }

        public void SkipActiveCutscene()
        {
            if (narrative != null)
            {
                narrative.SkipActiveSequence();
            }
        }

        private void BuildToyLookup()
        {
            toyLookup.Clear();
            for (int i = 0; i < toys.Count; i++)
            {
                var toy = toys[i];
                if (toy != null && !string.IsNullOrEmpty(toy.ToyId))
                {
                    toyLookup[toy.ToyId] = toy;
                }
            }
        }

        private void SubscribeEvents()
        {
            if (player != null)
            {
                player.OnPickUpRequested += HandlePickUpRequested;
                player.OnStoreRequested += HandleStoreRequested;
            }

            StateMachine.OnToyRespawnedOutsideView += HandleToyRespawned;
            StateMachine.OnStrangeDialogueTriggered += HandleStrangeDialogue;
            StateMachine.OnInvestigationTriggered += HandleInvestigationTriggered;
            StateMachine.OnPhaseChanged += HandlePhaseChanged;
        }

        private void HandlePickUpRequested(ToyItemView toy)
        {
            if (toy == null || player.CarriedToy != null)
            {
                return;
            }

            if (!StateMachine.TryPickUpToy(toy.ToyId))
            {
                return;
            }

            player.SetCarriedToy(toy);
            if (audioController != null)
            {
                audioController.PlayPickup();
            }

            if (nandaGuide != null)
            {
                nandaGuide.SetMode(NandaGuideTargetMode.BeaconOverChest);
            }
        }

        private void HandleStoreRequested(ToyItemView toy)
        {
            if (toy == null || player.CarriedToy != toy)
            {
                return;
            }

            if (!StateMachine.TryStoreToyInChest(toy.ToyId))
            {
                return;
            }

            player.ClearCarriedToy();
            toy.StoreInChest();

            if (chest != null)
            {
                chest.TriggerStoreFeedback();
            }

            if (audioController != null)
            {
                audioController.PlayStore();
            }

            RefreshCounterHud();

            if (nandaGuide != null && StateMachine.CurrentPhase != ToyRoomPhase.SeekingSword && StateMachine.CurrentPhase != ToyRoomPhase.InvestigationCutscene)
            {
                nandaGuide.SetMode(NandaGuideTargetMode.FollowPlayer);
            }
        }

        private void HandleToyRespawned(string toyId, ToyRoomPoint point)
        {
            if (toyLookup.TryGetValue(toyId, out var toy) && toy != null)
            {
                toy.RespawnAt(new Vector3(point.X, 0.25f, point.Z));
            }

            RefreshCounterHud();
        }

        private void HandleStrangeDialogue()
        {
            if (audioController != null)
            {
                audioController.PlayStrangeStinger();
            }

            if (narrative != null)
            {
                narrative.PlayStrangeDialogue();
            }
        }

        private void HandleInvestigationTriggered()
        {
            if (audioController != null)
            {
                audioController.PlaySwordReveal();
            }

            if (cameraController != null && swordView != null)
            {
                cameraController.SetOverrideTarget(swordView.transform);
            }

            if (nandaGuide != null)
            {
                nandaGuide.SetMode(NandaGuideTargetMode.HoverOverSword);
            }

            if (narrative != null)
            {
                narrative.PlayInvestigation(() => StateMachine.CompleteInvestigationCutscene());
            }
            else
            {
                if (swordView != null)
                {
                    swordView.Reveal();
                }

                StateMachine.CompleteInvestigationCutscene();
            }
        }

        private void HandlePhaseChanged(ToyRoomPhase phase)
        {
            if (player != null)
            {
                player.ControlsEnabled = phase == ToyRoomPhase.Exploring
                    || phase == ToyRoomPhase.RecurrenceMystery
                    || phase == ToyRoomPhase.SeekingSword;
            }

            if (nandaGuide != null && phase == ToyRoomPhase.SeekingSword)
            {
                nandaGuide.SetMode(NandaGuideTargetMode.HoverOverSword);
                if (cameraController != null)
                {
                    cameraController.SetOverrideTarget(null);
                }
            }

            if (phase == ToyRoomPhase.SwordPickup)
            {
                if (narrative != null)
                {
                    narrative.PlaySwordPickup(() => StateMachine.CompleteSwordPickup());
                }
                else
                {
                    StateMachine.CompleteSwordPickup();
                }
            }
        }

        private void RefreshCounterHud()
        {
            if (hud == null)
            {
                return;
            }

            int activeOnFloorOrCarried = 0;
            for (int i = 0; i < toys.Count; i++)
            {
                if (toys[i] != null && !toys[i].IsStored)
                {
                    activeOnFloorOrCarried++;
                }
            }

            hud.UpdateToyCounter(ToyRoomCatalog.TotalToyCount - activeOnFloorOrCarried);
        }

        private void UpdateContextualPrompt()
        {
            if (hud == null || player == null || !player.ControlsEnabled)
            {
                if (hud != null)
                {
                    hud.SetActionPrompt(null);
                }

                return;
            }

            if (StateMachine.CurrentPhase == ToyRoomPhase.SeekingSword)
            {
                bool nearSword = swordView != null && swordView.IsWithinPickupRange(player.transform.position);
                hud.SetActionPrompt(nearSword ? "PEGAR ESPADA" : null);
                return;
            }

            if (player.CarriedToy != null)
            {
                bool nearChest = chest != null && chest.IsWithinDeliveryRange(player.transform.position);
                hud.SetActionPrompt(nearChest ? "GUARDAR" : null);
                return;
            }

            for (int i = 0; i < toys.Count; i++)
            {
                var toy = toys[i];
                if (toy != null && toy.gameObject.activeInHierarchy && !toy.IsCarried && !toy.IsStored)
                {
                    Vector3 delta = toy.transform.position - player.transform.position;
                    delta.y = 0f;
                    if (delta.sqrMagnitude <= 1.35f * 1.35f)
                    {
                        hud.SetActionPrompt("PEGAR");
                        return;
                    }
                }
            }

            hud.SetActionPrompt(null);
        }

        private bool IsInsideMainCameraFocus(ToyRoomPoint point)
        {
            if (isometricCamera == null)
            {
                return false;
            }

            Vector3 viewport = isometricCamera.WorldToViewportPoint(new Vector3(point.X, 0.25f, point.Z));
            return viewport.z > 0f
                && viewport.x >= 0.16f && viewport.x <= 0.84f
                && viewport.y >= 0.16f && viewport.y <= 0.84f;
        }
    }
}
