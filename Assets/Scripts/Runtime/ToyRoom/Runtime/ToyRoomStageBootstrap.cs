using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Composition Root da Fase 2 (Toy Room): conecta maquina de estados pura,
    /// movimentacao da Yuna, bau, brinquedos e Nanda azul por eventos.
    /// </summary>
    public sealed class ToyRoomStageBootstrap : MonoBehaviour
    {
        [SerializeField] private Camera isometricCamera;
        [SerializeField] private ToyRoomPlayerController player;
        [SerializeField] private ToyChestController chest;
        [SerializeField] private NandaToyRoomGuideController nandaGuide;
        [SerializeField] private List<ToyItemView> toys = new List<ToyItemView>();
        [SerializeField] private bool startInExploring = true;

        private readonly Dictionary<string, ToyItemView> toyLookup = new Dictionary<string, ToyItemView>();

        public ToyRoomStateMachine StateMachine { get; private set; }

        public void Configure(
            Camera mainCam,
            ToyRoomPlayerController playerController,
            ToyChestController chestController,
            NandaToyRoomGuideController guideController,
            List<ToyItemView> toyViews,
            bool autoExplore = true)
        {
            isometricCamera = mainCam;
            player = playerController;
            chest = chestController;
            nandaGuide = guideController;
            toys = toyViews ?? new List<ToyItemView>();
            startInExploring = autoExplore;
        }

        private void Awake()
        {
            StateMachine = new ToyRoomStateMachine();
            BuildToyLookup();
            SubscribeEvents();

            if (startInExploring)
            {
                StateMachine.CompleteIntro();
            }
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
                player.TryInteract(toys, chest);
            }

            if (chest != null)
            {
                bool carryingNear = player.CarriedToy != null && chest.IsWithinDeliveryRange(player.transform.position);
                chest.SetPlayerCarryingNear(carryingNear);
            }

            var playerPoint = new ToyRoomPoint(player.transform.position.x, player.transform.position.z);
            StateMachine.Tick(Time.deltaTime, playerPoint, IsInsideMainCameraFocus);
        }

        public void RequestPlayerInteraction()
        {
            if (player != null)
            {
                player.TryInteract(toys, chest);
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

            if (nandaGuide != null && StateMachine.CurrentPhase != ToyRoomPhase.SeekingSword)
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
            }
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
