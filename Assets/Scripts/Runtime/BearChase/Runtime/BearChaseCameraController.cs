using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controla a camera da Fase 4: enquadramento lateral 2D fixo na cena de entrada
    /// e camera 3D em terceira pessoa atras da Yuna durante a perseguicao no corredor curvo.
    /// </summary>
    public sealed class BearChaseCameraController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Vector3 entranceCamPos = new Vector3(0f, 1.55f, -7.85f);
        [SerializeField] private Vector3 entranceLookAt = new Vector3(0f, 1.35f, 0f);
        [SerializeField] private float entranceFov = 32f;
        [SerializeField] private float chaseFov = 46f;

        private float shakeTime;
        private float shakeMagnitude;
        private Vector3 smoothVelocity;

        public void Configure(Camera cam)
        {
            targetCamera = cam;
        }

        public void AddShake(float magnitude, float duration)
        {
            shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
            shakeTime = Mathf.Max(shakeTime, duration);
        }

        public void ApplyEntrancePose()
        {
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.fieldOfView = entranceFov;
            targetCamera.transform.position = entranceCamPos;
            targetCamera.transform.LookAt(entranceLookAt);
        }

        public void UpdateCamera(BearChaseSimulation sim, Vector3 yunaChaseWorldPos, float dt)
        {
            if (targetCamera == null || sim == null)
            {
                return;
            }

            if (sim.Phase == BearChasePhase.EntranceWalk
                || sim.Phase == BearChasePhase.EntranceNudge
                || sim.Phase == BearChasePhase.BearWakeUp
                || sim.Phase == BearChasePhase.TransitionToChase)
            {
                ApplyEntrancePose();
                return;
            }

            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, chaseFov, Mathf.Min(1f, dt * 6f));

            float aheadLocalZ = sim.YunaZ + 6.8f;
            float aheadWorldZ = yunaChaseWorldPos.z + 6.8f;
            float aheadX = BearChaseConfig.TrackCenterX(aheadLocalZ);
            float centerX = BearChaseConfig.TrackCenterX(sim.YunaZ);
            float camX = Mathf.Lerp(centerX, yunaChaseWorldPos.x, 0.52f);

            Vector3 desiredPos = new Vector3(camX, 2.35f, yunaChaseWorldPos.z - 4.75f);
            if (dt <= 0f || Vector3.Distance(targetCamera.transform.position, desiredPos) > 10f)
            {
                targetCamera.transform.position = desiredPos;
                smoothVelocity = Vector3.zero;
            }
            else
            {
                targetCamera.transform.position = Vector3.SmoothDamp(
                    targetCamera.transform.position,
                    desiredPos,
                    ref smoothVelocity,
                    0.10f,
                    Mathf.Infinity,
                    dt);
            }

            Vector3 lookTarget = new Vector3(
                Mathf.Lerp(yunaChaseWorldPos.x, aheadX, 0.45f),
                1.22f,
                aheadWorldZ);

            if (shakeTime > 0f)
            {
                shakeTime = Mathf.Max(0f, shakeTime - dt);
                Vector3 offset = new Vector3(
                    Mathf.Sin(Time.time * 48f) * shakeMagnitude,
                    Mathf.Cos(Time.time * 41f) * shakeMagnitude * 0.7f,
                    0f);
                targetCamera.transform.position += offset;
                shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0f, Mathf.Min(1f, dt * 10f));
            }

            targetCamera.transform.LookAt(lookTarget);
        }
    }
}
