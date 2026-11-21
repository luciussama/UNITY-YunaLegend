using System;
using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controlador de iluminacao atmosferica da Fase 4:
    /// gerencia ambient/fog luminoso quente do corredor e um pool fixo de 3 SpotLights reais
    /// que acompanham as 3 janelas mais proximas da Yuna (respeitando limite URP de luzes adicionais),
    /// somadas a luz azul-ciano real de Nanda.
    /// </summary>
    public sealed class BearChaseLightingController : MonoBehaviour
    {
        [Serializable]
        public struct WindowLightAnchor
        {
            public float LocalZ;
            public Vector3 WorldPosition;
            public Vector3 AimTarget;
        }

        [SerializeField] private Light sunLight;
        [SerializeField] private WindowLightAnchor[] windowAnchors;
        [SerializeField] private Light[] windowSpotPool;

        private int[] nearestIndicesBuffer = Array.Empty<int>();
        private float[] nearestDistancesBuffer = Array.Empty<float>();

        public void Configure(Light mainSun, WindowLightAnchor[] anchors, Light[] spotPool)
        {
            sunLight = mainSun;
            windowAnchors = anchors;
            windowSpotPool = spotPool;
        }

        public void ApplyAtmosphere()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.58f, 0.50f, 0.44f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.34f, 0.30f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.18f, 0.16f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.95f, 0.89f, 0.76f);
            RenderSettings.fogStartDistance = 24f;
            RenderSettings.fogEndDistance = 66f;

            if (sunLight != null)
            {
                sunLight.color = new Color(1.0f, 0.93f, 0.80f);
                sunLight.intensity = 0.48f;
            }
        }

        public void UpdateWindowLights(BearChaseSimulation sim)
        {
            if (sim == null || windowSpotPool == null || windowSpotPool.Length == 0)
            {
                return;
            }

            if (sim.IsEntrancePhase || windowAnchors == null || windowAnchors.Length == 0)
            {
                SetPoolActive(false);
                return;
            }

            float targetZ = sim.YunaZ + 6.0f;
            int[] bestIndices = FindNearestWindowIndices(targetZ, windowSpotPool.Length);

            for (int i = 0; i < windowSpotPool.Length; i++)
            {
                Light spot = windowSpotPool[i];
                if (spot == null)
                {
                    continue;
                }

                int anchorIdx = bestIndices[i];
                if (anchorIdx < 0 || anchorIdx >= windowAnchors.Length)
                {
                    spot.enabled = false;
                    continue;
                }

                WindowLightAnchor anchor = windowAnchors[anchorIdx];
                spot.enabled = true;
                spot.transform.position = anchor.WorldPosition;
                spot.transform.LookAt(anchor.AimTarget);
            }
        }

        private void SetPoolActive(bool active)
        {
            for (int i = 0; i < windowSpotPool.Length; i++)
            {
                if (windowSpotPool[i] != null)
                {
                    windowSpotPool[i].enabled = active;
                }
            }
        }

        private int[] FindNearestWindowIndices(float targetZ, int count)
        {
            if (nearestIndicesBuffer.Length != count)
            {
                nearestIndicesBuffer = new int[count];
                nearestDistancesBuffer = new float[count];
            }

            for (int i = 0; i < count; i++)
            {
                nearestIndicesBuffer[i] = -1;
                nearestDistancesBuffer[i] = float.MaxValue;
            }

            for (int w = 0; w < windowAnchors.Length; w++)
            {
                float dist = Mathf.Abs(windowAnchors[w].LocalZ - targetZ);
                for (int slot = 0; slot < count; slot++)
                {
                    if (dist < nearestDistancesBuffer[slot])
                    {
                        for (int shift = count - 1; shift > slot; shift--)
                        {
                            nearestDistancesBuffer[shift] = nearestDistancesBuffer[shift - 1];
                            nearestIndicesBuffer[shift] = nearestIndicesBuffer[shift - 1];
                        }

                        nearestDistancesBuffer[slot] = dist;
                        nearestIndicesBuffer[slot] = w;
                        break;
                    }
                }
            }

            return nearestIndicesBuffer;
        }
    }
}
