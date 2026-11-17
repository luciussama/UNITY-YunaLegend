using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Controla a movimentacao livre em 3D (X/Z) da Yuna na Sala de Brinquedos,
    /// aplicando o fator de peso do brinquedo carregado e disparando pedidos de coleta/entrega.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class ToyRoomPlayerController : MonoBehaviour
    {
        [SerializeField] private float baseMoveSpeed = 4.6f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float pickupRadius = 1.35f;
        [SerializeField] private Transform carryAnchor;

        private CharacterController characterController;
        private bool controlsEnabled = true;

        public ToyItemView CarriedToy { get; private set; }
        public Transform CarryAnchor => carryAnchor;
        public Vector2? ScriptedMoveInput { get; set; }
        public float TurnDeltaDeg { get; private set; }
        public bool ControlsEnabled
        {
            get => controlsEnabled;
            set => controlsEnabled = value;
        }

        public event Action<ToyItemView> OnPickUpRequested;
        public event Action<ToyItemView> OnStoreRequested;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterController.minMoveDistance = 0f;
            EnsureCarryAnchor();
        }

        public void Configure(Transform customCarryAnchor)
        {
            carryAnchor = customCarryAnchor;
            EnsureCarryAnchor();
        }

        public void SetCarriedToy(ToyItemView toy)
        {
            CarriedToy = toy;
            if (toy != null)
            {
                toy.AttachToCarrier(carryAnchor);
            }
        }

        public void ClearCarriedToy()
        {
            CarriedToy = null;
        }

        public void TryInteract(IReadOnlyList<ToyItemView> allToys, ToyChestController chest)
        {
            if (!controlsEnabled)
            {
                return;
            }

            if (CarriedToy != null)
            {
                if (chest != null && chest.IsWithinDeliveryRange(transform.position))
                {
                    OnStoreRequested?.Invoke(CarriedToy);
                }

                return;
            }

            ToyItemView nearest = FindNearestToyInRange(allToys);
            if (nearest != null)
            {
                OnPickUpRequested?.Invoke(nearest);
            }
        }

        private void Update()
        {
            if (!controlsEnabled)
            {
                return;
            }

            Vector2 move = ReadMoveInput();
            Vector3 planar = new Vector3(move.x, 0f, move.y);
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize();
            }

            float weightFactor = CarriedToy != null ? CarriedToy.WeightMultiplier : 1.0f;
            float speed = baseMoveSpeed * weightFactor;
            Vector3 velocity = planar * speed;
            velocity.y = -2.0f; // Mantem apoio no piso

            characterController.Move(velocity * Time.deltaTime);

            bool isMoving = planar.sqrMagnitude > 0.001f;
            if (isMoving)
            {
                float signedTurn = Vector3.SignedAngle(transform.forward, planar, Vector3.up);
                TurnDeltaDeg = Mathf.Lerp(TurnDeltaDeg, signedTurn, Time.deltaTime * 10f);
                Quaternion targetRot = Quaternion.LookRotation(planar, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
            else
            {
                TurnDeltaDeg = Mathf.Lerp(TurnDeltaDeg, 0f, Time.deltaTime * 10f);
            }

            if (CarriedToy != null)
            {
                CarriedToy.UpdateCarryBounce(isMoving, Time.deltaTime);
            }
        }

        private Vector2 ReadMoveInput()
        {
            if (ScriptedMoveInput.HasValue)
            {
                return ScriptedMoveInput.Value;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float x = 0f;
            float z = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;

            return new Vector2(x, z);
        }

        private ToyItemView FindNearestToyInRange(IReadOnlyList<ToyItemView> allToys)
        {
            if (allToys == null)
            {
                return null;
            }

            ToyItemView best = null;
            float bestDistSq = pickupRadius * pickupRadius;

            for (int i = 0; i < allToys.Count; i++)
            {
                var toy = allToys[i];
                if (toy == null || !toy.gameObject.activeInHierarchy || toy.IsCarried || toy.IsStored)
                {
                    continue;
                }

                Vector3 delta = toy.transform.position - transform.position;
                delta.y = 0f;
                float distSq = delta.sqrMagnitude;
                if (distSq <= bestDistSq)
                {
                    bestDistSq = distSq;
                    best = toy;
                }
            }

            return best;
        }

        private void EnsureCarryAnchor()
        {
            if (carryAnchor != null)
            {
                return;
            }

            var anchorGo = new GameObject("CarryAnchor");
            anchorGo.transform.SetParent(transform, false);
            anchorGo.transform.localPosition = new Vector3(0f, 0.85f, 0.55f);
            carryAnchor = anchorGo.transform;
        }
    }
}
