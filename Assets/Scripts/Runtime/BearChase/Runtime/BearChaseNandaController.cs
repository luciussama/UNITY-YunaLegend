using System.Collections.Generic;
using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Fadinha guia Nanda azul-ciano no corredor 3D:
    /// escala delicada proporcional ao legado, voo elastico acompanhando as esquivas,
    /// rastro de poeira estelar azul-ciano e emissao real de luz azulada equilibrada sobre Yuna e o piso.
    /// </summary>
    public sealed class BearChaseNandaController : MonoBehaviour
    {
        [SerializeField] private Transform yunaTransform;
        [SerializeField] private Light auraLight;
        [SerializeField] private ParticleSystem sparkleTrail;
        [SerializeField] private Transform floorBlueGlowTransform;
        [SerializeField] private Transform bodyHaloTransform;

        private Transform[] wings;
        private float animClock;
        private float alertPulse;

        public void Configure(
            Transform yuna,
            Light lightSource,
            ParticleSystem trailParticles,
            Transform floorBlueGlow,
            Transform bodyHalo = null)
        {
            yunaTransform = yuna;
            auraLight = lightSource;
            sparkleTrail = trailParticles;
            floorBlueGlowTransform = floorBlueGlow;
            if (bodyHalo != null)
            {
                bodyHaloTransform = bodyHalo;
            }

            CacheWings();
        }

        private void Awake()
        {
            CacheWings();
        }

        public void TriggerAlertPulse()
        {
            alertPulse = 1.0f;
        }

        public void UpdateNanda(BearChaseSimulation sim, Vector3 chaseOrigin, float dt)
        {
            if (yunaTransform == null || sim == null || sim.IsEntrancePhase)
            {
                return;
            }

            animClock += Mathf.Max(0f, dt);
            if (alertPulse > 0f)
            {
                alertPulse = Mathf.Max(0f, alertPulse - dt * 2.2f);
            }

            float lateralSway = Mathf.Cos(animClock * 3.1f) * 0.14f - sim.YunaLateralInput * 0.22f;
            float verticalBob = Mathf.Sin(animClock * 4.8f) * 0.09f;
            Vector3 localOffset = new Vector3(-0.78f + lateralSway, 1.18f + verticalBob, 0.16f);
            Vector3 desired = yunaTransform.position + yunaTransform.rotation * localOffset;

            if (dt <= 0f || Vector3.Distance(transform.position, desired) > 8f)
            {
                transform.position = desired;
            }
            else
            {
                float follow = 1f - Mathf.Exp(-10.5f * dt);
                transform.position = Vector3.Lerp(transform.position, desired, follow);
            }

            float hoverPitch = 22f + Mathf.Sin(animClock * 4.8f) * 6f;
            float bankRoll = -sim.YunaLateralInput * 18f;
            transform.rotation = yunaTransform.rotation * Quaternion.Euler(hoverPitch, 14f, bankRoll);

            if (auraLight != null)
            {
                float pulse = Mathf.Sin(animClock * 5.2f) * 0.25f + alertPulse * 0.65f;
                auraLight.intensity = 2.15f + pulse;
            }

            float localZ = transform.position.z - chaseOrigin.z;
            BearChaseTrackSpec track = sim.TrackSpec;
            float groundY = chaseOrigin.y + BearChaseTrackSampler.ElevationY(localZ, track);
            float pitchDeg = BearChaseTrackSampler.PitchRadians(localZ, track) * Mathf.Rad2Deg;
            float yawDeg = BearChaseTrackSampler.YawRadians(localZ, track) * Mathf.Rad2Deg;

            if (floorBlueGlowTransform != null)
            {
                floorBlueGlowTransform.position = new Vector3(transform.position.x, groundY + 0.036f, transform.position.z);
                floorBlueGlowTransform.rotation = Quaternion.Euler(90f + pitchDeg, yawDeg, 0f);
                float floorScale = 1.95f + Mathf.Sin(animClock * 4.8f) * 0.12f + alertPulse * 0.20f;
                floorBlueGlowTransform.localScale = new Vector3(floorScale, floorScale, 1f);
            }

            if (bodyHaloTransform != null)
            {
                bodyHaloTransform.rotation = Quaternion.Euler(pitchDeg, yawDeg, 0f);
                float haloScale = 0.92f + Mathf.Sin(animClock * 5.2f) * 0.08f + alertPulse * 0.16f;
                bodyHaloTransform.localScale = new Vector3(haloScale, haloScale, 1f);
            }

            if (sparkleTrail != null)
            {
                ParticleSystem.EmissionModule emission = sparkleTrail.emission;
                emission.enabled = !sim.IsEntrancePhase;
            }

            if (wings != null)
            {
                float flap = Mathf.Sin(animClock * 28f) * 24f;
                for (int i = 0; i < wings.Length; i++)
                {
                    if (wings[i] != null)
                    {
                        float sign = i % 2 == 0 ? 1f : -1f;
                        wings[i].localRotation = Quaternion.Euler(0f, flap * sign, 0f);
                    }
                }
            }
        }

        private void CacheWings()
        {
            if (bodyHaloTransform == null)
            {
                bodyHaloTransform = transform.Find("Nanda_BodyCyanHalo");
            }

            var list = new List<Transform>();
            Transform[] all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name.Contains("Wing"))
                {
                    list.Add(all[i]);
                }
            }

            wings = list.ToArray();
        }
    }
}
