using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Camera25D
{
    /// <summary>
    /// Regra Inviolavel 3.2: A camera em jogabilidade lateral move-se estritamente no eixo X.
    /// Proibida qualquer oscilacao vertical (Y), zoom (FOV) ou inclinacao em reacao a pulos ou pousos.
    /// Modo Runner: avanca de forma constante e esmaga o jogador caso ele fique para tras da borda da tela.
    /// </summary>
    [ExecuteAlways]
    public class SideScroll25DCamera : MonoBehaviour
    {
        [Header("Alvo")]
        [SerializeField] private Yuna25DPlayerController playerTarget;

        [Header("Modo de Rolagem Constante (Runner)")]
        [Tooltip("Velocidade constante de avanco da camera no eixo X")]
        [SerializeField] private float scrollSpeed = 5.2f;

        [Tooltip("Direcao do avanco: +1 para direita (Subida), -1 para esquerda (Volta Caotica)")]
        [SerializeField] private float scrollDirection = 1f;

        [Tooltip("Define se a camera esta em avanco automatico")]
        [SerializeField] private bool autoScrollActive = false;

        [Header("Margem de Esmagamento da Tela (Screen Crush)")]
        [Tooltip("Define se a camera causa derrota por esmagamento ao ficar para tras")]
        [SerializeField] private bool screenCrushActive = false;

        [Tooltip("Distancia maxima atras do centro da camera antes de considerar esmagamento")]
        [SerializeField] private float maxDistanceBehindCamera = 7.5f;

        [Header("Posicionamento Fixo Inviolavel (Regra 3.2)")]
        [SerializeField] private float fixedY = -4.5f;
        [SerializeField] private float fixedZ = -16.0f;
        [SerializeField] private float fixedFov = 45.0f;

        private Camera targetCamera;

        public bool AutoScrollActive { get => autoScrollActive; set => autoScrollActive = value; }
        public float ScrollDirection { get => scrollDirection; set => scrollDirection = Mathf.Sign(value); }
        public bool ScreenCrushActive { get => screenCrushActive; set => screenCrushActive = value; }
        public float FixedY => fixedY;
        public float FixedZ => fixedZ;
        public float FixedFov => fixedFov;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            ApplyFixedProperties();
        }

        private void LateUpdate()
        {
            if (autoScrollActive)
            {
                // Avanco estrito e continuo no eixo X (sem qualquer oscilacao em Y ou Z)
                float newX = transform.position.x + (scrollDirection * scrollSpeed * Time.deltaTime);
                transform.position = new Vector3(newX, fixedY, fixedZ);

                // Deteccao de Esmagamento pela borda da tela
                CheckScreenCrush();
            }
            else if (playerTarget != null && !Application.isPlaying)
            {
                // No editor, acompanha Yuna no X para visualizacao
                transform.position = new Vector3(playerTarget.transform.position.x + DarkRoomStartLayout.CameraOffset, fixedY, fixedZ);
            }
        }

        private void CheckScreenCrush()
        {
            if (!screenCrushActive || playerTarget == null || playerTarget.IsDead) return;

            float camX = transform.position.x;
            float playerX = playerTarget.transform.position.x;

            if (scrollDirection > 0f)
            {
                // Andando para a direita: se o jogador ficar muito atras da borda esquerda
                if (playerX < camX - maxDistanceBehindCamera)
                {
                    playerTarget.TriggerDeath("Yuna foi engolida pela penumbra da tela!");
                }
            }
            else
            {
                // Andando para a esquerda (Volta Caotica): se o jogador ficar para tras na borda direita
                if (playerX > camX + maxDistanceBehindCamera)
                {
                    playerTarget.TriggerDeath("Yuna foi engolida pela penumbra da tela!");
                }
            }
        }

        public void ConfigureSection(float targetFixedY, float targetFixedZ = -16.0f, float targetFov = 45.0f)
        {
            fixedY = targetFixedY;
            fixedZ = targetFixedZ;
            fixedFov = targetFov;
            ApplyFixedProperties();
            transform.position = new Vector3(transform.position.x, fixedY, fixedZ);
        }

        private void ApplyFixedProperties()
        {
            if (targetCamera != null)
            {
                targetCamera.fieldOfView = fixedFov;
            }
            transform.rotation = Quaternion.identity;
        }

        public void SetTarget(Transform newTarget)
        {
            if (newTarget != null)
            {
                playerTarget = newTarget.GetComponent<Yuna25DPlayerController>();
            }
        }

        public void SnapToTargetX(float targetX, float offset = 3.5f)
        {
            transform.position = new Vector3(targetX + offset, fixedY, fixedZ);
        }

        public void SetScrollState(bool active, float dir = 1f, float speed = 5.2f)
        {
            autoScrollActive = active;
            scrollDirection = Mathf.Sign(dir);
            scrollSpeed = speed;
        }
    }
}
