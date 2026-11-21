using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Fabrica semantica de materiais e VolumeProfile exclusivos de BearChase.
    /// Grava exclusivamente em Assets/Art/BearChase/ para nunca corromper materiais da ToyRoom.
    /// </summary>
    public sealed class BearChaseMaterialFactory
    {
        private const string CorridorShaderName = "YunaLegend/BearChase_CorridorPainterly";
        private const string SunbeamShaderName = "YunaLegend/BearChase_Sunbeam";
        private const string MaterialsDir = "Assets/Art/BearChase/Materials";
        private const string VolumeProfilePath = "Assets/Art/BearChase/BearChase_PostProcessProfile.asset";

        private readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public Material GetOrCreatePainterly(
            string key,
            Color tint,
            string texturePath,
            Vector2? tiling = null,
            Color? emission = null,
            float outlineWidth = 0f,
            float glossStrength = 0f,
            float glossPower = 18f,
            float ambientScale = 0.72f)
        {
            if (cache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Directory.CreateDirectory(MaterialsDir);
            Shader shader = Shader.Find(CorridorShaderName) ?? Shader.Find("Universal Render Pipeline/Lit");
            string matPath = $"{MaterialsDir}/M_BearChase_{key}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_ShadowColor", new Color(0.58f, 0.46f, 0.50f, 1f));
            mat.SetColor("_RimColor", new Color(1.0f, 0.94f, 0.80f, 1f));
            mat.SetColor("_OutlineColor", new Color(0.22f, 0.12f, 0.09f, 1f));
            mat.SetFloat("_OutlineWidth", outlineWidth);
            mat.SetFloat("_BrushNoiseStrength", 0.035f);
            mat.SetFloat("_AmbientScale", ambientScale);
            mat.SetFloat("_GlossStrength", glossStrength);
            mat.SetFloat("_GlossPower", glossPower);

            Texture2D tex = string.IsNullOrEmpty(texturePath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            mat.SetTextureScale("_BaseMap", tiling ?? Vector2.one);
            mat.SetColor("_EmissionColor", emission ?? Color.black);

            EditorUtility.SetDirty(mat);
            cache[key] = mat;
            return mat;
        }

        public Material GetOrCreateCutoutLit(
            string key,
            Color tint,
            string texturePath,
            Color? emission = null,
            float cutoff = 0.35f)
        {
            if (cache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Directory.CreateDirectory(MaterialsDir);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/Unlit");
            string matPath = $"{MaterialsDir}/M_BearChase_{key}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            Texture2D tex = string.IsNullOrEmpty(texturePath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", cutoff);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.SetFloat("_Smoothness", 0.12f);
            mat.renderQueue = (int)RenderQueue.AlphaTest;

            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(mat);
            cache[key] = mat;
            return mat;
        }

        public Material GetOrCreateTransparentUnlit(string key, Color tint, string texturePath)
        {
            if (cache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Directory.CreateDirectory(MaterialsDir);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            string matPath = $"{MaterialsDir}/M_BearChase_{key}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            Texture2D tex = string.IsNullOrEmpty(texturePath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;

            EditorUtility.SetDirty(mat);
            cache[key] = mat;
            return mat;
        }

        public Material GetOrCreateSunbeam(string key, Color tint, string texturePath)
        {
            if (cache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Directory.CreateDirectory(MaterialsDir);
            Shader shader = Shader.Find(SunbeamShaderName) ?? Shader.Find("Universal Render Pipeline/Unlit");
            string matPath = $"{MaterialsDir}/M_BearChase_{key}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            Texture2D tex = string.IsNullOrEmpty(texturePath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_SoftPower", 1.6f);
            mat.SetFloat("_RadialOnly", 0f);

            EditorUtility.SetDirty(mat);
            cache[key] = mat;
            return mat;
        }

        public Material GetOrCreateRadialGlow(string key, Color tint, string texturePath)
        {
            Material mat = GetOrCreateSunbeam(key, tint, texturePath);
            mat.SetFloat("_RadialOnly", 1f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public Material GetOrCreateParticleUnlit(string key, Color tint, string texturePath, bool additive)
        {
            if (cache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Directory.CreateDirectory(MaterialsDir);
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit");
            string matPath = $"{MaterialsDir}/M_BearChase_{key}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            Texture2D tex = string.IsNullOrEmpty(texturePath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", additive ? 2f : 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;

            EditorUtility.SetDirty(mat);
            cache[key] = mat;
            return mat;
        }

        public VolumeProfile GetOrCreateVolumeProfile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(VolumeProfilePath) ?? "Assets/Art/BearChase");
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumeProfilePath);
            }

            if (!profile.TryGet(out Tonemapping tonemapping))
            {
                tonemapping = profile.Add<Tonemapping>(true);
            }
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.ACES;

            if (!profile.TryGet(out Vignette vignette))
            {
                vignette = profile.Add<Vignette>(true);
            }
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.22f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.45f;

            if (!profile.TryGet(out Bloom bloom))
            {
                bloom = profile.Add<Bloom>(true);
            }
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.96f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.28f;

            EditorUtility.SetDirty(profile);
            return profile;
        }
    }
}
