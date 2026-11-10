using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace YunaLegend.DarkRoom
{
    [RequireComponent(typeof(CharacterController))]
    public class Yuna25DPlayerController : MonoBehaviour
    {
        [Header("Locomocao Automatica (Runner 2.5D)")]
        [Tooltip("Velocidade constante da corrida")]
        [SerializeField] private float runSpeed = 5.2f;

        [Tooltip("Direcao atual da corrida: +1 para direita (Subida), -1 para esquerda (Volta Caotica)")]
        [SerializeField] private float runDirection = 1f;

        [Tooltip("Define se a corrida automatica esta ativa")]
        [SerializeField] private bool autoRunActive = false;

        [Header("Fisica de Salto Calibrada")]
        [SerializeField] private float jumpHeight = 3.0f;
        [SerializeField] private float gravity = 28f;
        [SerializeField] private float fallMultiplier = 1.8f;
        [SerializeField] private float lowJumpMultiplier = 2.4f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.14f;

        [Header("Limites de Derrota (Game Over por Queda / Esmagamento)")]
        [Tooltip("Altura Y do chao onde qualquer queda causa derrota")]
        [SerializeField] private float floorDeathY = -9.2f;

        [Header("Restricao de Eixo 2.5D")]
        [SerializeField] private float lockedZ = 0f;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip jumpSfx;
        [SerializeField] private AudioClip landSfx;

        private CharacterController controller;
        private Vector3 velocity;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private bool jumpHeld;
        private bool wasGrounded;
        private bool isDead;

        public event Action OnGameOverTriggered;

        public bool IsGrounded => controller != null && controller.isGrounded;
        public Vector3 Velocity => velocity;
        public float RunDirection { get => runDirection; set => SetRunDirection(value); }
        public bool AutoRunActive { get => autoRunActive; set => autoRunActive = value; }
        public bool IsDead => isDead;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            if (isDead) return;

            ReadJumpInput();
            UpdateTimers();
            ApplyForwardRun();
            ApplyJumpAndGravity();

            controller.Move(velocity * Time.deltaTime);
            Enforce2DPlane();
            DetectLanding();
            CheckFloorDeath();
        }

        private void ReadJumpInput()
        {
            if (!autoRunActive)
            {
                jumpHeld = false;
                return;
            }

            // O jogador controla APENAS o pulo: Teclado (Espaço/Seta Cima/W), Gamepad (Botão Sul) ou Toque
            bool jumpPressed = false;
            jumpHeld = false;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                {
                    jumpPressed = true;
                }
                if (Keyboard.current.spaceKey.isPressed || Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                {
                    jumpHeld = true;
                }
            }

            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                {
                    jumpPressed = true;
                }
                if (Gamepad.current.buttonSouth.isPressed)
                {
                    jumpHeld = true;
                }
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                jumpPressed = true;
                jumpHeld = true;
            }

            if (jumpPressed)
            {
                jumpBufferCounter = jumpBufferTime;
            }
        }

        private void UpdateTimers()
        {
            if (controller.isGrounded)
            {
                coyoteCounter = coyoteTime;
            }
            else
            {
                coyoteCounter -= Time.deltaTime;
            }

            jumpBufferCounter -= Time.deltaTime;
        }

        private void ApplyForwardRun()
        {
            if (!autoRunActive)
            {
                velocity.x = 0f;
                return;
            }

            // Corrida constante para a frente sem controle de X pelo jogador
            velocity.x = runDirection * runSpeed;

            // Orientacao visual: virada para a direita ou esquerda
            float targetRotationY = runDirection >= 0 ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0f, targetRotationY, 0f);
        }

        private void ApplyJumpAndGravity()
        {
            if (controller.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            // Salto calibrado com timing exigente
            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                velocity.y = Mathf.Sqrt(2f * gravity * jumpHeight);
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
                PlaySound(jumpSfx);
            }

            if (velocity.y < 0f)
            {
                velocity.y -= gravity * fallMultiplier * Time.deltaTime;
            }
            else if (velocity.y > 0f && !jumpHeld)
            {
                velocity.y -= gravity * lowJumpMultiplier * Time.deltaTime;
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
            }
        }

        private void Enforce2DPlane()
        {
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.z - lockedZ) > 0.001f)
            {
                pos.z = lockedZ;
                transform.position = pos;
            }
        }

        private void DetectLanding()
        {
            if (!wasGrounded && controller.isGrounded)
            {
                PlaySound(landSfx);
            }
            wasGrounded = controller.isGrounded;
        }

        private void CheckFloorDeath()
        {
            // Qualquer queda no chao da sala e Game Over imediato
            if (transform.position.y <= floorDeathY)
            {
                TriggerDeath("Yuna caiu no chao escuro do quarto!");
            }
        }

        public void TriggerDeath(string reason)
        {
            if (isDead) return;
            isDead = true;
            autoRunActive = false;
            velocity = Vector3.zero;
            Debug.Log($"[GameOver]: {reason}");
            OnGameOverTriggered?.Invoke();
        }

        public void SetRunDirection(float dir)
        {
            runDirection = Mathf.Sign(dir);
        }

        public void ResetPlayer(Vector3 startPosition, float dir = 1f)
        {
            isDead = false;
            runDirection = dir;
            velocity = Vector3.zero;
            transform.position = startPosition;
            autoRunActive = true;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
