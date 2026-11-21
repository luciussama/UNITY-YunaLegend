using System;
using System.Collections.Generic;
using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Gerencia o pool visual dos brinquedos arremessados pelo urso, suas sombras projetadas no piso
    /// (acompanhando curvas e subidas/descidas) e a poeira de impacto no pouso.
    /// </summary>
    public sealed class BearChaseObstacleController : MonoBehaviour
    {
        [Serializable]
        public struct ObstacleTemplateEntry
        {
            public BearChaseObstacleKind Kind;
            public GameObject PrefabTemplate;
        }

        [SerializeField] private ObstacleTemplateEntry[] templates;
        [SerializeField] private Material contactShadowMaterial;
        [SerializeField] private ParticleSystem landingDustTemplate;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Dictionary<int, PooledToyInstance> activeInstances = new Dictionary<int, PooledToyInstance>();
        private readonly Dictionary<BearChaseObstacleKind, Stack<PooledToyInstance>> freePools = new Dictionary<BearChaseObstacleKind, Stack<PooledToyInstance>>();
        private readonly HashSet<int> liveIds = new HashSet<int>();
        private readonly List<int> staleIds = new List<int>();
        private readonly List<ParticleSystem> dustPool = new List<ParticleSystem>();
        private MaterialPropertyBlock shadowPropertyBlock;

        private sealed class PooledToyInstance
        {
            public BearChaseObstacleKind Kind;
            public GameObject Root;
            public Transform VisualPivot;
            public Transform ShadowTransform;
            public Renderer ShadowRenderer;
        }

        public void Configure(
            ObstacleTemplateEntry[] obstacleTemplates,
            Material shadowMat,
            ParticleSystem dustBurst)
        {
            templates = obstacleTemplates;
            contactShadowMaterial = shadowMat;
            landingDustTemplate = dustBurst;
        }

        public void ResetAll()
        {
            foreach (var kvp in activeInstances)
            {
                ReleaseInstance(kvp.Value);
            }

            activeInstances.Clear();
        }

        public void PlayLandEffect(BearChaseThrownToyState toy, Vector3 chaseOrigin, BearChaseTrackSpec trackSpec)
        {
            if (toy == null || landingDustTemplate == null)
            {
                return;
            }

            float z = toy.CurrentZ;
            float cx = BearChaseTrackSampler.CenterX(z, trackSpec) + toy.CurrentLaneX;
            float cy = BearChaseTrackSampler.ElevationY(z, trackSpec) + 0.08f;
            Vector3 worldPos = chaseOrigin + new Vector3(cx, cy, z);

            ParticleSystem ps = AcquireDustBurst();
            if (ps != null)
            {
                ps.transform.position = worldPos;
                ps.Clear(true);
                ps.Play(true);
            }
        }

        public void SyncToys(
            IReadOnlyList<BearChaseThrownToyState> activeToys,
            Vector3 chaseOrigin,
            BearChaseTrackSpec trackSpec,
            bool isChaseActive)
        {
            if (!isChaseActive || activeToys == null)
            {
                if (activeInstances.Count > 0)
                {
                    ResetAll();
                }

                return;
            }

            liveIds.Clear();
            for (int i = 0; i < activeToys.Count; i++)
            {
                BearChaseThrownToyState toy = activeToys[i];
                if (toy.Phase == BearChaseThrownToyPhase.Expired)
                {
                    continue;
                }

                liveIds.Add(toy.Id);
                if (!activeInstances.TryGetValue(toy.Id, out PooledToyInstance inst))
                {
                    inst = AcquireInstance(toy.Kind);
                    if (inst == null)
                    {
                        continue;
                    }

                    activeInstances.Add(toy.Id, inst);
                }

                UpdateToyTransform(inst, toy, chaseOrigin, trackSpec);
            }

            staleIds.Clear();
            foreach (var kvp in activeInstances)
            {
                if (!liveIds.Contains(kvp.Key))
                {
                    staleIds.Add(kvp.Key);
                }
            }

            for (int i = 0; i < staleIds.Count; i++)
            {
                int id = staleIds[i];
                if (activeInstances.TryGetValue(id, out PooledToyInstance inst))
                {
                    ReleaseInstance(inst);
                    activeInstances.Remove(id);
                }
            }
        }

        private void UpdateToyTransform(
            PooledToyInstance inst,
            BearChaseThrownToyState toy,
            Vector3 chaseOrigin,
            BearChaseTrackSpec trackSpec)
        {
            float z = toy.CurrentZ;
            float centerX = BearChaseTrackSampler.CenterX(z, trackSpec);
            float groundY = BearChaseTrackSampler.ElevationY(z, trackSpec);
            float yawRad = BearChaseTrackSampler.YawRadians(z, trackSpec);
            float pitchRad = BearChaseTrackSampler.PitchRadians(z, trackSpec);

            float worldX = centerX + toy.CurrentLaneX;
            Vector3 groundPos = chaseOrigin + new Vector3(worldX, groundY, z);
            inst.Root.transform.position = groundPos + new Vector3(0f, toy.HeightAboveGround, 0f);

            Quaternion trackRot = Quaternion.Euler(pitchRad * Mathf.Rad2Deg, yawRad * Mathf.Rad2Deg, 0f);
            if (inst.VisualPivot != null)
            {
                if (toy.Phase == BearChaseThrownToyPhase.InFlight || toy.Phase == BearChaseThrownToyPhase.Deflected)
                {
                    inst.VisualPivot.rotation = trackRot * Quaternion.Euler(toy.SpinAngleDeg * 0.85f, toy.SpinAngleDeg * 0.55f, toy.SpinAngleDeg * 0.35f);
                }
                else
                {
                    inst.VisualPivot.rotation = trackRot * Quaternion.Euler(0f, toy.SpinAngleDeg * 0.2f, 0f);
                }
            }

            if (inst.ShadowTransform != null)
            {
                inst.ShadowTransform.position = groundPos + new Vector3(0f, 0.034f, 0f);
                inst.ShadowTransform.rotation = trackRot * Quaternion.Euler(90f, 0f, 0f);

                float heightFactor = Mathf.Clamp01(toy.HeightAboveGround / 2.2f);
                float shadowScale = Mathf.Lerp(1.42f, 0.90f, heightFactor);
                inst.ShadowTransform.localScale = new Vector3(shadowScale, shadowScale, 1f);
                if (inst.ShadowRenderer != null)
                {
                    if (shadowPropertyBlock == null)
                    {
                        shadowPropertyBlock = new MaterialPropertyBlock();
                    }

                    float alpha = Mathf.Lerp(0.86f, 0.52f, heightFactor);
                    Color c = new Color(0.06f, 0.03f, 0.02f, alpha);
                    inst.ShadowRenderer.GetPropertyBlock(shadowPropertyBlock);
                    shadowPropertyBlock.SetColor(BaseColorId, c);
                    inst.ShadowRenderer.SetPropertyBlock(shadowPropertyBlock);
                }
            }
        }

        private PooledToyInstance AcquireInstance(BearChaseObstacleKind kind)
        {
            if (freePools.TryGetValue(kind, out Stack<PooledToyInstance> stack) && stack.Count > 0)
            {
                PooledToyInstance reused = stack.Pop();
                reused.Root.SetActive(true);
                return reused;
            }

            GameObject template = FindTemplate(kind);
            if (template == null)
            {
                return null;
            }

            var container = new GameObject($"ThrownToy_{kind}");
            container.transform.SetParent(transform, false);

            var visual = Instantiate(template, container.transform);
            visual.name = "VisualPivot";
            visual.SetActive(true);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            GameObject shadowGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadowGo.name = "LandingShadow";
            shadowGo.transform.SetParent(container.transform, false);
            Collider shadowCol = shadowGo.GetComponent<Collider>();
            if (shadowCol != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(shadowCol);
                }
                else
                {
                    DestroyImmediate(shadowCol);
                }
            }

            var shadowRend = shadowGo.GetComponent<Renderer>();
            if (shadowRend != null && contactShadowMaterial != null)
            {
                shadowRend.sharedMaterial = contactShadowMaterial;
                shadowRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            return new PooledToyInstance
            {
                Kind = kind,
                Root = container,
                VisualPivot = visual.transform,
                ShadowTransform = shadowGo.transform,
                ShadowRenderer = shadowRend
            };
        }

        private void ReleaseInstance(PooledToyInstance inst)
        {
            if (inst == null || inst.Root == null)
            {
                return;
            }

            inst.Root.SetActive(false);
            if (!freePools.TryGetValue(inst.Kind, out Stack<PooledToyInstance> stack))
            {
                stack = new Stack<PooledToyInstance>();
                freePools[inst.Kind] = stack;
            }

            stack.Push(inst);
        }

        private GameObject FindTemplate(BearChaseObstacleKind kind)
        {
            if (templates == null)
            {
                return null;
            }

            for (int i = 0; i < templates.Length; i++)
            {
                if (templates[i].Kind == kind && templates[i].PrefabTemplate != null)
                {
                    return templates[i].PrefabTemplate;
                }
            }

            return templates.Length > 0 ? templates[0].PrefabTemplate : null;
        }

        private ParticleSystem AcquireDustBurst()
        {
            for (int i = 0; i < dustPool.Count; i++)
            {
                if (dustPool[i] != null && !dustPool[i].isPlaying)
                {
                    return dustPool[i];
                }
            }

            if (dustPool.Count >= 6)
            {
                return dustPool[0];
            }

            ParticleSystem inst = Instantiate(landingDustTemplate, transform);
            inst.gameObject.SetActive(true);
            dustPool.Add(inst);
            return inst;
        }
    }
}
