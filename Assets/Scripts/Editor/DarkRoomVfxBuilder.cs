using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomVfxBuilder
    {
        private const string MaterialsFolder = "Assets/Materials";

        [MenuItem("YunaLegend/Gerar Efeitos Visuais (VFX)")]
        public static void GenerateAllVfx()
        {
            if (!Directory.Exists(MaterialsFolder))
            {
                Directory.CreateDirectory(MaterialsFolder);
            }

            // 1. Gerar Texturas de Particulas e Feixe de Luar
            SaveTextureAsPng(GenerateSoftGlowParticleTexture(), Path.Combine(MaterialsFolder, "T_Sparkle_Glow.png"));
            SaveTextureAsPng(GenerateLightShaftGradientTexture(), Path.Combine(MaterialsFolder, "T_LightShaft_Gradient.png"));

            AssetDatabase.Refresh();

            var sparkleTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(MaterialsFolder, "T_Sparkle_Glow.png"));
            var shaftTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(MaterialsFolder, "T_LightShaft_Gradient.png"));

            // 2. Criar Materiais Transparentes / Aditivos URP
            CreateParticleMaterial("M_FairySparkle_Particle", new Color(1.0f, 0.90f, 0.45f, 1f), sparkleTex, isAdditive: true);
            CreateParticleMaterial("M_AmbientDust_Particle", new Color(0.80f, 0.90f, 1.0f, 0.7f), sparkleTex, isAdditive: true);
            CreateParticleMaterial("M_Moonbeam_Shaft", new Color(0.60f, 0.78f, 1.0f, 0.38f), shaftTex, isAdditive: true);
            CreateParticleMaterial("M_LandingPuff_Particle", new Color(0.90f, 0.85f, 0.75f, 0.6f), sparkleTex, isAdditive: false);

            AssetDatabase.SaveAssets();
            Debug.Log("[DarkRoomVfxBuilder] Texturas e Materiais VFX gerados com sucesso!");
        }

        private static void SaveTextureAsPng(Texture2D tex, string path)
        {
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static Texture2D GenerateSoftGlowParticleTexture()
        {
            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float center = size * 0.5f;
            float maxR = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / maxR;
                    float alpha = Mathf.Clamp01(1.0f - dist);
                    alpha = Mathf.Pow(alpha, 2.2f); // Decaimento exponencial suave

                    // Adicionar uma pequena cruz estelar no nucleo
                    float crossX = Mathf.Clamp01(1.0f - Mathf.Abs(x - center) / 4f) * Mathf.Clamp01(1.0f - Mathf.Abs(y - center) / maxR);
                    float crossY = Mathf.Clamp01(1.0f - Mathf.Abs(y - center) / 4f) * Mathf.Clamp01(1.0f - Mathf.Abs(x - center) / maxR);
                    float star = (crossX + crossY) * 0.45f;

                    float finalA = Mathf.Clamp01(alpha + star);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, finalA));
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateLightShaftGradientTexture()
        {
            int width = 256;
            int height = 512;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            for (int y = 0; y < height; y++)
            {
                float normY = (float)y / height; // 0 na base, 1 no topo (janela)
                // Gradiente vertical: mais intenso no topo junto a janela, desaparecendo suavemente na base
                float vertAlpha = Mathf.SmoothStep(0f, 1f, normY);

                for (int x = 0; x < width; x++)
                {
                    float normX = (float)x / width; // 0..1
                    float distFromCenter = Mathf.Abs(normX - 0.5f) * 2f; // 0 no meio, 1 nas bordas
                    float horizAlpha = Mathf.Clamp01(1.0f - distFromCenter);
                    horizAlpha = Mathf.Pow(horizAlpha, 1.8f);

                    float alpha = vertAlpha * horizAlpha;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return tex;
        }

        private static void CreateParticleMaterial(string name, Color baseColor, Texture2D tex, bool isAdditive)
        {
            var matPath = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Mobile/Particles/Additive");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            // Configuracao de transparencia URP
            mat.SetColor("_BaseColor", baseColor);
            if (tex != null) mat.SetTexture("_BaseMap", tex);

            if (isAdditive)
            {
                mat.SetFloat("_Surface", 1f); // Transparent
                mat.SetFloat("_Blend", 1f);   // Additive
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.One);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = (int)RenderQueue.Transparent + 10;
            }
            else
            {
                mat.SetFloat("_Surface", 1f); // Transparent
                mat.SetFloat("_Blend", 0f);   // Alpha
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = (int)RenderQueue.Transparent;
            }

            EditorUtility.SetDirty(mat);
        }
    }
}
