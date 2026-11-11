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
        [Tooltip("Tolerancia ao saltar logo apos deixar uma borda.")]
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.14f;

        [Header("Limites de Derrota (Game Over por Queda / Esmagamento)")]
        [Tooltip("Altura Y do chao onde qualquer queda causa derrota")]
        [SerializeField] private float floorDeathY = -9.2f;

        [Header("Restricao de Eixo 2.5D")]
        [SerializeField] private float lockedZ = 0f;

        [Header("Audio SFX Dinamico")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip jumpSfx;
        [SerializeField] private AudioClip landSfx;
        [SerializeField] private AudioClip[] footstepWoodClips;
        [SerializeField] private AudioClip landPlushSfx;
        [SerializeField] private AudioClip drumBounceSfx;
        [SerializeField] private AudioClip comicFallThudSfx;
        [SerializeField] private AudioClip gameOverSfx;

        private float footstepTimer;

        private CharacterController controller;
        private Vector3 velocity;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private bool jumpHeld;
        private bool wasGrounded;
        private bool isDead;
        private bool isCinematicFalling;
        private int lastLandedPlatformIndex = -1;

        public event Action OnGameOverTriggered;
        public event Action<int> OnPlatformLanded;
        public event Action OnCinematicFallLanded;

        public bool IsGrounded => controller != null && controller.isGrounded;
        public Vector3 Velocity => velocity;
        public float RunDirection { get => runDirection; set => SetRunDirection(value); }
        public bool IsCinematicFalling => isCinematicFalling;
        public bool AutoRunActive
        {
            get => autoRunActive;
            set
            {
                if (autoRunActive != value)
                {
                    jumpBufferCounter = 0f;
                    jumpHeld = false;
                    requireJumpRelease = true;
                }
                autoRunActive = value;
            }
        }
        private bool requireJumpRelease = true;

        public void PrepareForOpening(Vector3 position)
        {
            ResetPlayer(position);
            AutoRunActive = false;
        }
        public bool IsDead => isDead;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        private bool floorLaunchAllowed;

        public float RunSpeed => runSpeed;
        public float JumpHeight => jumpHeight;

        private DarkRoomPlatformMarker[] routePlatforms;
        private float lastSupportTop;
        private bool hasRouteSupport;
        private bool launchJumped;

        public void AllowFloorLaunch(float x)
        {

            floorLaunchAllowed = true;
            launchJumped = false;
            hasRouteSupport = false;
            routePlatforms = FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        public void ValidateMandatoryRoute()
        {
            if (isDead || isCinematicFalling || !autoRunActive || !floorDeathActive) return;
            if (routePlatforms == null)
                routePlatforms = FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            int nextIndex = lastLandedPlatformIndex < 0 ? (runDirection > 0 ? 0 : 22) : lastLandedPlatformIndex + 1;
            float lowestReachableTop = lastSupportTop;
            foreach (var marker in routePlatforms)
            {
                if (marker == null || !marker.gameObject.activeInHierarchy || marker.spec.index != nextIndex) continue;
                lowestReachableTop = Mathf.Min(lastSupportTop, marker.GetTopCenterWorld().y);
                float farEdge = marker.transform.position.x + runDirection * marker.spec.width * 0.5f;
                if (runDirection * (transform.position.x - farEdge) > controller.radius)
                    TriggerDeath("A plataforma obrigatória ficou para trás.");
                break;
            }

            if (hasRouteSupport && velocity.y < 0f && transform.position.y < lowestReachableTop - 0.55f)
                TriggerDeath("Yuna falhou o salto e caiu da rota.");
            if (floorLaunchAllowed && launchJumped && controller.isGrounded && !hasRouteSupport)
                TriggerDeath("Yuna voltou ao chão sem alcançar a primeira plataforma.");
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;
            if (isDead) return;

            // Queda cinematica real (sem controle horizontal, apenas gravidade ate o chao)
            if (isCinematicFalling)
            {
                velocity.x = 0f;
                velocity.y -= gravity * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
                Enforce2DPlane();

                if (controller.isGrounded && velocity.y <= 0f)
                {
                    isCinematicFalling = false;
                    velocity = Vector3.zero;
                    PlaySound(comicFallThudSfx != null ? comicFallThudSfx : landSfx);
                    OnCinematicFallLanded?.Invoke();
                }
                return;
            }

            if (!autoRunActive) return;

            ReadJumpInput();
            TickMovement(Time.deltaTime);
        }

        public void ConfigureAudioClips(AudioClip jump, AudioClip landWood, AudioClip[] footsteps, AudioClip landPlush, AudioClip drumBounce, AudioClip comicFall, AudioClip gameOver)
        {
            jumpSfx = jump;
            landSfx = landWood;
            footstepWoodClips = footsteps;
            landPlushSfx = landPlush;
            drumBounceSfx = drumBounce;
            comicFallThudSfx = comicFall;
            gameOverSfx = gameOver;
        }

        private void UpdateFootsteps(float deltaTime)
        {
            if (!autoRunActive || isDead || isCinematicFalling || !controller.isGrounded) return;

            footstepTimer -= deltaTime;
            if (footstepTimer <= 0f)
            {
                float interval = 0.28f / Mathf.Max(0.5f, runSpeed / 3.6f);
                footstepTimer = interval;

                if (footstepWoodClips != null && footstepWoodClips.Length > 0 && audioSource != null)
                {
                    int idx = UnityEngine.Random.Range(0, footstepWoodClips.Length);
                    var clip = footstepWoodClips[idx];
                    if (clip != null)
                    {
                        audioSource.pitch = UnityEngine.Random.Range(0.92f, 1.08f);
                        audioSource.PlayOneShot(clip, 0.40f);
                        audioSource.pitch = 1.0f;
                    }
                }
            }
        }

        public void TickMovement(float deltaTime)
        {
            if (isDead || !autoRunActive || isCinematicFalling || deltaTime <= 0f) return;
            UpdateTimers(deltaTime);
            UpdateFootsteps(deltaTime);
            ApplyForwardRun();
            ApplyJumpAndGravity(deltaTime);

            // Midpoint displacement keeps the ballistic arc consistent across frame rates.
            Vector3 displacement = velocity * deltaTime;
            displacement.y += 0.5f * gravity * deltaTime * deltaTime;
            controller.Move(displacement);
            Enforce2DPlane();
            DetectLanding();
            ValidateMandatoryRoute();
            CheckFloorDeath();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (isDead) return;
            if (autoRunActive && floorDeathActive && !isCinematicFalling && velocity.y <= 0f
                && hit.normal.x * runDirection < -0.5f
                && hit.collider.GetComponentInParent<DarkRoomPlatformMarker>() != null)
            {
                TriggerDeath("Yuna não alcançou o topo da plataforma obrigatória.");
                return;
            }
            if (hit.normal.y > 0.5f)
            {

                // Progression is notified only after validating the mandatory landing below.

                var marker = hit.collider.GetComponentInParent<DarkRoomPlatformMarker>();
                if (marker != null && !isCinematicFalling && velocity.y <= 0f)
                {
                    int idx = marker.spec.index;
                    int expected = lastLandedPlatformIndex < 0 ? (runDirection > 0 ? 0 : 22) : lastLandedPlatformIndex + 1;
                    if (idx != lastLandedPlatformIndex && idx != expected)
                    {
                        TriggerDeath("Uma plataforma obrigatória foi pulada.");
                        return;
                    }
                    floorLaunchAllowed = false;
                    hasRouteSupport = true;
                    lastSupportTop = marker.GetTopCenterWorld().y;
                    if (idx != lastLandedPlatformIndex)
                    {
                        lastLandedPlatformIndex = idx;
                        OnPlatformLanded?.Invoke(idx);
                    }
                }
            }
        }

        public void RestoreRouteSupport(int index)
        {
            lastLandedPlatformIndex = index;
            routePlatforms = FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var marker in routePlatforms)
            {
                if (marker.spec.index != index) continue;
                lastSupportTop = marker.GetTopCenterWorld().y;
                hasRouteSupport = true;
                break;
            }
        }

        public void ResetLastPlatformIndex()
        {
            lastLandedPlatformIndex = -1;
        }

        public void StartCinematicFall()
        {
            autoRunActive = false;
            isCinematicFalling = true;
            floorDeathActive = false; // Nao letal nesta cena
            velocity = Vector3.zero;
        }

        private void ReadJumpInput()
        {
            if (!autoRunActive)
            {
                jumpHeld = false;
                return;
            }

            bool rawJumpHeld = false;
            bool rawJumpPressed = false;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                {
                    rawJumpPressed = true;
                }
                if (Keyboard.current.spaceKey.isPressed || Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                {
                    rawJumpHeld = true;
                }
            }

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    rawJumpPressed = true;
                }
                if (Mouse.current.leftButton.isPressed)
                {
                    rawJumpHeld = true;
                }
            }

            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                {
                    rawJumpPressed = true;
                }
                if (Gamepad.current.buttonSouth.isPressed)
                {
                    rawJumpHeld = true;
                }
            }

            if (Touchscreen.current != null)
            {
                if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                {
                    rawJumpPressed = true;
                }
                if (Touchscreen.current.primaryTouch.press.isPressed)
                {
                    rawJumpHeld = true;
                }
            }

            // Regra 4.1: comando que avanca o dialogo/abertura nao pode vazar como pulo no frame seguinte
            if (requireJumpRelease)
            {
                if (!rawJumpHeld)
                {
                    requireJumpRelease = false;
                }
                return;
            }

            jumpHeld = rawJumpHeld;
            if (rawJumpPressed)
            {
                jumpBufferCounter = jumpBufferTime;
            }
        }

        private void UpdateTimers(float deltaTime)
        {
            if (controller.isGrounded)
            {
                coyoteCounter = coyoteTime;
            }
            else
            {
                coyoteCounter -= deltaTime;
            }

            jumpBufferCounter -= deltaTime;
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

        private void ApplyJumpAndGravity(float deltaTime)
        {
            if (controller.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            // A press commits the whole arc; releasing never cuts its height.
            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                velocity.y = Mathf.Sqrt(2f * gravity * jumpHeight);
                if (floorLaunchAllowed) launchJumped = true;
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
                if (audioSource != null && jumpSfx != null)
                {
                    audioSource.pitch = Mathf.Lerp(0.95f, 1.25f, Mathf.Clamp01((runSpeed - 3.6f) / 2.4f));
                    audioSource.PlayOneShot(jumpSfx, 0.65f);
                    audioSource.pitch = 1.0f;
                }
            }

            velocity.y -= gravity * deltaTime;
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
                AudioClip clipToPlay = landSfx;
                if (lastLandedPlatformIndex == 0 && landPlushSfx != null) clipToPlay = landPlushSfx;
                else if (lastLandedPlatformIndex == 5 && drumBounceSfx != null) clipToPlay = drumBounceSfx;
                PlaySound(clipToPlay);
            }
            wasGrounded = controller.isGrounded;
        }

        public void SetRunSpeed(float speed)
        {
            runSpeed = speed;
        }

        public void SetJumpHeight(float height)
        {
            jumpHeight = height;
        }

        public void SetFloorDeathActive(bool active)
        {
            floorDeathActive = active;
        }

        private bool floorDeathActive = true;

        private void CheckFloorDeath()
        {
            if (!floorDeathActive) return;

            if (floorLaunchAllowed)
            {
                // Permite a arrancada inicial no chão no início da Fase 3 antes de atingir a Plataforma 22
                if (!hasRouteSupport)
                {
                    return;
                }
                floorLaunchAllowed = false;
            }

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
            if (audioSource != null && gameOverSfx != null)
            {
                audioSource.PlayOneShot(gameOverSfx, 0.14f); // 20% do volume anterior (0.70)
            }
            Debug.Log($"[GameOver]: {reason}");
            OnGameOverTriggered?.Invoke();
        }

        public void SetRunDirection(float dir)
        {
            runDirection = Mathf.Sign(dir);
        }

        public void ResetPlayer(Vector3 startPosition, float dir = 1f)
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = startPosition;
            controller.enabled = wasEnabled;
            isDead = false;
            isCinematicFalling = false;
            lastLandedPlatformIndex = -1;
            runDirection = dir;
            velocity = Vector3.zero;
            coyoteCounter = 0f;
            jumpBufferCounter = 0f;
            wasGrounded = false;
            floorLaunchAllowed = false;
            launchJumped = false;
            hasRouteSupport = false;
            routePlatforms = null;
            requireJumpRelease = true;
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
