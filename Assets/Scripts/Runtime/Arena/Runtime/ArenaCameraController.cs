using UnityEngine;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Camera da Arena: aproxima os lutadores quando estao perto, abre o quadro
    /// quando se afastam ou em golpes longos, e aplica tremor de impacto.
    /// </summary>
    public sealed class ArenaCameraController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float minWorldWidth = 8.4f;
        [SerializeField] private float maxWorldWidth = 14.2f;
        [SerializeField] private float smoothSpeed = 6.5f;

        private float shakeTimer;
        private float shakeAmplitude;

        public void Configure(Camera cam)
        {
            targetCamera = cam;
        }

        public void AddShake(float amplitude, float duration)
        {
            shakeAmplitude = Mathf.Max(shakeAmplitude, amplitude);
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

            float centerX = Mathf.Clamp((pPos.x + ePos.x) * 0.5f, -3.2f, 3.2f);
            float separation = Mathf.Abs(pPos.x - ePos.x);
            float padding = (sim.EnemyState == ArenaEnemyState.Windup || sim.IsSpecialActive) ? 5.6f : 4.2f;
            float desiredWidth = Mathf.Clamp(separation + padding, minWorldWidth, maxWorldWidth);

            float aspect = Mathf.Max(0.45f, targetCamera.aspect);
            float desiredOrthoSize = (desiredWidth / aspect) * 0.5f;
            desiredOrthoSize = Mathf.Clamp(desiredOrthoSize, 3.1f, 6.2f);

            targetCamera.orthographic = true;
            targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, desiredOrthoSize, dt * smoothSpeed);

            float centerY = Mathf.Max(1.35f, (pPos.y + ePos.y) * 0.35f + desiredOrthoSize * 0.32f);
            Vector3 targetPos = new Vector3(centerX, centerY, -10f);

            if (shakeTimer > 0f)
            {
                shakeTimer -= dt;
                targetPos += new Vector3(
                    Random.Range(-shakeAmplitude, shakeAmplitude),
                    Random.Range(-shakeAmplitude, shakeAmplitude),
                    0f);
                if (shakeTimer <= 0f)
                {
                    shakeAmplitude = 0f;
                }
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, dt * smoothSpeed);
        }
    }
}
