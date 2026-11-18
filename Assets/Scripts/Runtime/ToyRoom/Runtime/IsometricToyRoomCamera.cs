using System.Collections.Generic;
using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Camera 3D rebaixada que acompanha Yuna pela sala ampla, permitindo ver seu rosto
    /// e aplicando translucidez dithered estilo Diablo em objetos que bloqueiam a visao.
    /// </summary>
    public sealed class IsometricToyRoomCamera : MonoBehaviour
    {
        private static readonly int OcclusionFadePropId = Shader.PropertyToID("_OcclusionFade");

        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 offset = new(0f, 4.35f, -9.20f);
        [SerializeField] private Vector3 lookAtOffset = new(0f, 0.95f, 0.15f);
        [SerializeField] private float smoothTime = 0.22f;
        [SerializeField] private float occludedAlpha = 0.28f;
        [SerializeField] private float fadeSpeed = 8.0f;

        private Transform overrideTarget;
        private Vector3 velocity;
        private readonly Dictionary<Renderer, float> activeFades = new();
        private readonly HashSet<Renderer> currentlyOccluding = new();
        private readonly List<Renderer> fadeCleanupList = new();
        private MaterialPropertyBlock mpb;

        public void Configure(Transform target)
        {
            followTarget = target;
            SnapToTarget();
        }

        public void SetOverrideTarget(Transform target)
        {
            overrideTarget = target;
        }

        public void SnapToTarget()
        {
            Transform active = overrideTarget != null ? overrideTarget : followTarget;
            if (active == null)
            {
                return;
            }

            Vector3 desiredFocus = ClampFocus(active.position + lookAtOffset);
            transform.position = desiredFocus + offset;
            transform.LookAt(desiredFocus);
        }

        private void LateUpdate()
        {
            Transform active = overrideTarget != null ? overrideTarget : followTarget;
            if (active == null)
            {
                return;
            }

            Vector3 desiredFocus = ClampFocus(active.position + lookAtOffset);
            Vector3 desiredPos = desiredFocus + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
            transform.LookAt(transform.position - offset);

            UpdateDiabloOcclusionFade(active);
        }

        private void UpdateDiabloOcclusionFade(Transform target)
        {
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }

            currentlyOccluding.Clear();

            Vector3 targetCenter = target.position + Vector3.up * 0.95f;
            Vector3 dir = targetCenter - transform.position;
            float dist = dir.magnitude;
            if (dist > 0.2f)
            {
                RaycastHit[] hits = Physics.SphereCastAll(
                    transform.position,
                    0.42f,
                    dir.normalized,
                    Mathf.Max(0.1f, dist - 0.65f),
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);

                for (int i = 0; i < hits.Length; i++)
                {
                    Transform hitRoot = hits[i].transform;
                    if (hitRoot == null || hitRoot.IsChildOf(target) || hitRoot.name.Contains("Floor") || hitRoot.name.Contains("Carpet"))
                    {
                        continue;
                    }

                    var renderers = hitRoot.GetComponentsInChildren<Renderer>(true);
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        Renderer rend = renderers[r];
                        if (rend != null && rend.enabled)
                        {
                            currentlyOccluding.Add(rend);
                            if (!activeFades.ContainsKey(rend))
                            {
                                activeFades[rend] = 1.0f;
                            }
                        }
                    }
                }
            }

            fadeCleanupList.Clear();
            var keys = new List<Renderer>(activeFades.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Renderer rend = keys[i];
                if (rend == null)
                {
                    fadeCleanupList.Add(rend);
                    continue;
                }

                float targetAlpha = currentlyOccluding.Contains(rend) ? occludedAlpha : 1.0f;
                float nextAlpha = Mathf.MoveTowards(activeFades[rend], targetAlpha, Time.deltaTime * fadeSpeed);
                activeFades[rend] = nextAlpha;

                rend.GetPropertyBlock(mpb);
                mpb.SetFloat(OcclusionFadePropId, nextAlpha);
                rend.SetPropertyBlock(mpb);

                if (nextAlpha >= 0.999f && targetAlpha >= 0.999f)
                {
                    fadeCleanupList.Add(rend);
                }
            }

            for (int i = 0; i < fadeCleanupList.Count; i++)
            {
                activeFades.Remove(fadeCleanupList[i]);
            }
        }

        private static Vector3 ClampFocus(Vector3 focus)
        {
            float maxX = ToyRoomCatalog.HalfWidth - 3.2f;
            float maxZ = ToyRoomCatalog.HalfDepth - 2.6f;
            focus.x = Mathf.Clamp(focus.x, -maxX, maxX);
            focus.z = Mathf.Clamp(focus.z, -maxZ, maxZ);
            return focus;
        }
    }
}
