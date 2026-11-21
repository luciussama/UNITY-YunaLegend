using UnityEngine;
using UnityEngine.InputSystem;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Camera 3D da Arena: enquadra Yuna e o inimigo ativo em perspectiva baixa (~7 graus),
    /// aproxima quando estao perto, abre o quadro em golpes longos e aplica tremor de impacto.
    /// </summary>
    public sealed class ArenaCameraController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float minWorldWidth = 6.2f;
        [SerializeField] private float maxWorldWidth = 17.6f;
        [SerializeField] private float smoothSpeed = 5.8f;

        private float shakeTimer;
        private float shakeDuration;
        private float shakeAmplitude;
        private Vector3 smoothedBasePos;
        private bool hasInitializedPos;

        public void Configure(Camera cam)
        {
            targetCamera = cam;
        }

        public void AddShake(float amplitude, float duration)
        {
            shakeAmplitude = Mathf.Max(shakeAmplitude, amplitude);
            shakeDuration = Mathf.Max(shakeDuration, duration);
            shakeTimer = Mathf.Max(shakeTimer, duration);
        }

        public void UpdateFraming(ArenaCombatSimulation sim, float dt)
        {
            if (sim == null)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            if (targetCamera == null)
            {
                return;
            }

            Vector3 pPos = ArenaCoordinateMapper.ToWorldPosition(sim.PlayerX, sim.PlayerY);
            Vector3 ePos = ArenaCoordinateMapper.ToWorldPosition(sim.EnemyX, sim.EnemyY);

            float aspect = Mathf.Max(0.45f, targetCamera.aspect);
            bool isMobile = Application.isMobilePlatform || Touchscreen.current != null;
            bool wideMobileMode = isMobile || aspect < 1.45f;

            float effMinWidth = wideMobileMode ? Mathf.Max(minWorldWidth, 12.8f) : minWorldWidth;
            float effMaxWidth = wideMobileMode ? Mathf.Max(maxWorldWidth, 19.5f) : maxWorldWidth;

            float separation = Mathf.Abs(pPos.x - ePos.x);
            // Aproxima bem quando perto e afasta por completo quando os personagens recuam;
            // no mobile abre o quadro para exibir toda a arena sem os controles cobrirem os personagens.
            float zoomT = Mathf.InverseLerp(2.4f, 10.5f, separation);
            float basePadding = wideMobileMode ? 5.6f : 3.4f;
            float padding = (sim.EnemyState == ArenaEnemyState.Windup || sim.IsSpecialActive) ? (basePadding + 1.0f) : basePadding;
            float desiredWidth = Mathf.Lerp(effMinWidth, effMaxWidth, zoomT);
            desiredWidth = Mathf.Clamp(Mathf.Max(desiredWidth, separation + padding), effMinWidth, effMaxWidth);

            // Quanto mais afastada a camera, mais centralizada na arena ela fica
            float maxCenterOffset = wideMobileMode ? Mathf.Lerp(1.8f, 0.5f, zoomT) : Mathf.Lerp(3.6f, 0.8f, zoomT);
            float centerX = Mathf.Clamp((pPos.x + ePos.x) * 0.5f, -maxCenterOffset, maxCenterOffset);

            targetCamera.orthographic = false;
            bool isPortrait = aspect < 1.0f;
            targetCamera.fieldOfView = isPortrait ? 44f : 34f;
            float halfFovRad = targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float distance = (desiredWidth * 0.5f) / (Mathf.Tan(halfFovRad) * aspect);
            if (isPortrait)
            {
                distance = Mathf.Clamp(distance, 18.0f, 38.0f);
            }
            else if (wideMobileMode)
            {
                distance = Mathf.Clamp(distance, 11.5f, 24.0f);
            }
            else
            {
                distance = Mathf.Clamp(distance, 7.2f, 18.5f);
            }

            float lookY = Mathf.Max(1.10f, (pPos.y + ePos.y) * 0.35f + Mathf.Lerp(1.12f, 1.48f, zoomT) - (wideMobileMode ? 0.12f : 0f));
            Vector3 targetPos = new Vector3(centerX, lookY + distance * 0.11f, -distance);

            if (!hasInitializedPos)
            {
                smoothedBasePos = targetPos;
                hasInitializedPos = true;
            }
            else
            {
                smoothedBasePos = Vector3.Lerp(smoothedBasePos, targetPos, Mathf.Clamp01(dt * smoothSpeed));
            }

            // Aplica a tremida de impacto APOS o Lerp para manter nitidez e peso real no golpe
            Vector3 shakeOffset = Vector3.zero;
            if (shakeTimer > 0f)
            {
                shakeTimer = Mathf.Max(0f, shakeTimer - dt);
                float decay = shakeDuration > 0.001f ? (shakeTimer / shakeDuration) : 0f;
                float currentAmp = shakeAmplitude * decay;
                shakeOffset = new Vector3(
                    Random.Range(-currentAmp, currentAmp),
                    Random.Range(-currentAmp, currentAmp),
                    0f);
                if (shakeTimer <= 0f)
                {
                    shakeAmplitude = 0f;
                    shakeDuration = 0f;
                }
            }

            transform.position = smoothedBasePos + shakeOffset;
            transform.LookAt(new Vector3(smoothedBasePos.x + shakeOffset.x * 0.35f, lookY + shakeOffset.y * 0.35f, 0f));
        }
    }
}
