using System;
using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>Local projected lights for the illustrated pilot. No global shader/pipeline changes.</summary>
    [DefaultExecutionOrder(290)]
    public sealed class PenumbraLightingT054 : MonoBehaviour
    {
        public RoomBackgroundT054 room;
        public Transform nanda;
        public Shader lightingShader;
        [Range(0,1)] public float moonStrength = 1f;
        [Range(0,1)] public float nandaStrength = 1f;
        [Range(0,.1f)] public float phosphor = .035f;
        public Vector3 MoonPosition { get; private set; }
        public int BoundRenderers => bindings.Count;
        sealed class Binding
        {
            public Renderer renderer;
            public Material original, owned;
            public MaterialPropertyBlock block = new MaterialPropertyBlock();
        }
        readonly List<Binding> bindings = new List<Binding>();

        public void Bind(Renderer renderer, int role, bool includeDisabled = false, bool blink = false, bool suppressHaze = false)
        {
            if (renderer == null || (!includeDisabled && !renderer.enabled)) return;
            foreach (var existing in bindings)
                if (existing.renderer == renderer) throw new InvalidOperationException("Renderer already bound");
            var shader = lightingShader != null ? lightingShader : Shader.Find("YunaLegend/RoomPenumbraT054");
            if (shader == null || !shader.isSupported) throw new InvalidOperationException("Penumbra shader unavailable");
            var source = renderer.sharedMaterial;
            var material = new Material(shader) { name = "T054_Penumbra_" + role };
            bool sprite = renderer is SpriteRenderer;
            if (source != null && source.mainTexture != null) material.mainTexture = source.mainTexture;
            if (blink && source != null && source.HasProperty("_BlinkTex"))
                material.SetTexture("_BlinkTex", source.GetTexture("_BlinkTex"));
            material.SetFloat("_BlinkEnabled", blink ? 1f : 0f);
            material.SetFloat("_SuppressHaze", suppressHaze ? 1f : 0f);
            material.SetFloat("_Sprite", sprite ? 1f : 0f);
            material.SetFloat("_Role", role);
            int queue = source != null ? source.renderQueue : 3000;
            material.SetFloat("_ZWrite", sprite || queue >= 3000 ? 0f : 1f);
            material.renderQueue = queue;
            bindings.Add(new Binding { renderer = renderer, original = source, owned = material });
            renderer.sharedMaterial = material;
        }

        public void Apply()
        {
            if (room == null || room.Far == null) return;
            var bounds = room.Far.bounds;
            // Source UV measured on the approved panorama-v4, follows its parallax.
            MoonPosition = new Vector3(bounds.min.x + bounds.size.x * .564f,
                bounds.min.y + bounds.size.y * .558f, 0f);
            foreach (var binding in bindings)
            {
                if (binding.renderer == null) continue;
                binding.renderer.GetPropertyBlock(binding.block);
                binding.block.SetVector("_MoonPosition", MoonPosition);
                binding.block.SetVector("_NandaPosition", nanda != null ? nanda.position : Vector3.zero);
                binding.block.SetFloat("_MoonStrength", moonStrength);
                binding.block.SetFloat("_NandaStrength", nanda != null ? nandaStrength : 0f);
                binding.block.SetFloat("_Phosphor", phosphor);
                binding.renderer.SetPropertyBlock(binding.block);
            }
        }
        void LateUpdate() => Apply();
        void OnDestroy()
        {
            foreach (var binding in bindings)
            {
                if (binding.renderer != null && binding.renderer.sharedMaterial == binding.owned)
                {
                    binding.renderer.sharedMaterial = binding.original;
                    binding.renderer.SetPropertyBlock(null);
                }
                if (binding.owned != null)
                {
                    if (Application.isPlaying) Destroy(binding.owned);
                    else DestroyImmediate(binding.owned);
                }
            }
            bindings.Clear();
        }
    }
}
