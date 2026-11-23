using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controla a camera da Fase 4: acompanha curvas horizontais e ondulacoes verticais do corredor,
    /// com FOV dinamico proporcional a velocidade, leve inclinacao nas curvas e shake isolado da posicao base.
    /// </summary>
    public sealed class BearChaseCameraController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float minChaseFov = 48f;
        [SerializeField] private float maxChaseFov = 56f;

        private Vector3 baseCameraPosition;
        private bool hasBasePosition;
        private float shakeTime;
        private float shakeMagnitude;
        private float animClock;
        private float smoothedRoll;

        public void Configure(Camera cam)
        {
            targetCamera = cam;
        }

        public void AddShake(float magnitude, float duration)
        {
            shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
            shakeTime = Mathf.Max(shakeTime, duration);
        }

        public void ResetCameraPose()
        {
            hasBasePosition = false;
            shakeTime = 0f;
            shakeMagnitude = 0f;
            smoothedRoll = 0f;
        }

        public void UpdateCamera(BearChaseSimulation sim, Vector3 chaseOrigin, float dt)
        {
            if (targetCamera == null || sim == null)
            {
                return;
            }

            if (sim.IsEntrancePhase)
            {
                targetCamera.fieldOfView = 36f;
                targetCamera.transform.position = new Vector3(0f, 2f, -10f);
                targetCamera.transform.rotation = Quaternion.identity;
                hasBasePosition = false;
                return;
            }

            animClock += Mathf.Max(0f, dt);

            float yunaZ = sim.YunaZ;
            BearChaseTrackSpec track = sim.TrackSpec;
            float centerX = BearChaseTrackSampler.CenterX(yunaZ, track);
            float groundY = BearChaseTrackSampler.ElevationY(yunaZ, track);
            float yunaWorldX = centerX + sim.YunaLaneOffsetX;
            float camX = Mathf.Lerp(centerX, yunaWorldX, 0.56f);

            float behindZ = yunaZ - 4.45f;
            float behindGroundY = BearChaseTrackSampler.ElevationY(Mathf.Max(0f, behindZ), track);
            float camGroundY = Mathf.Lerp(behindGroundY, groundY, 0.45f);
            float sprintBob = sim.Phase == BearChasePhase.Chasing ? Mathf.Abs(Mathf.Sin(animClock * 11.5f)) * 0.045f : 0f;

            Vector3 desiredBasePos = chaseOrigin + new Vector3(camX, camGroundY + 2.28f + sprintBob, behindZ);
            if (dt <= 0f || !hasBasePosition || Vector3.Distance(baseCameraPosition, desiredBasePos) > 12f)
            {
                baseCameraPosition = desiredBasePos;
                hasBasePosition = true;
            }
            else
            {
                float posLerp = 1f - Mathf.Exp(-14f * dt);
                baseCameraPosition = Vector3.Lerp(baseCameraPosition, desiredBasePos, posLerp);
            }

            float aheadZ = Mathf.Min(track.TrackLength + 12f, yunaZ + 9.8f);
            float aheadX = BearChaseTrackSampler.CenterX(aheadZ, track);
            float aheadY = BearChaseTrackSampler.ElevationY(aheadZ, track);

            Vector3 lookTarget = chaseOrigin + new Vector3(
                Mathf.Lerp(yunaWorldX, aheadX, 0.58f),
                Mathf.Lerp(groundY, aheadY, 0.62f) + 0.35f,
                aheadZ);

            float speedAlpha = Mathf.InverseLerp(sim.TuningSpec.BaseForwardSpeed, sim.TuningSpec.MaxForwardSpeed, sim.CurrentForwardSpeed);
            float desiredFov = Mathf.Lerp(minChaseFov, maxChaseFov, speedAlpha);
            targetCamera.fieldOfView = dt <= 0f
                ? desiredFov
                : Mathf.Lerp(targetCamera.fieldOfView, desiredFov, 1f - Mathf.Exp(-6f * dt));

            Vector3 shakeOffset = Vector3.zero;
            if (shakeTime > 0f && dt > 0f)
            {
                shakeTime = Mathf.Max(0f, shakeTime - dt);
                shakeOffset = new Vector3(
                    Mathf.Sin(animClock * 52f) * shakeMagnitude,
                    Mathf.Cos(animClock * 43f) * shakeMagnitude * 0.72f,
                    0f);
                shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0f, 1f - Mathf.Exp(-10f * dt));
            }

            targetCamera.transform.position = baseCameraPosition + shakeOffset;
            targetCamera.transform.LookAt(lookTarget, Vector3.up);

            float targetRoll = BearChaseTrackSampler.BankDegrees(yunaZ, track) * 0.48f - sim.YunaLateralInput * 1.8f;
            smoothedRoll = dt <= 0f
                ? targetRoll
                : Mathf.Lerp(smoothedRoll, targetRoll, 1f - Mathf.Exp(-8f * dt));
            targetCamera.transform.Rotate(0f, 0f, smoothedRoll, Space.Self);
        }
    }
}
