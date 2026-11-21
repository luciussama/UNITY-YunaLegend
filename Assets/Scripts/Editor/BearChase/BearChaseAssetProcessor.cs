using System.IO;
using UnityEditor;
using UnityEngine;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Processa os assets migrados do legado para a Fase 4:
    /// adapta a coloracao do urso na entrada para o dourado-mel com laco vermelho da versao atual,
    /// e recorta/compoe texturas de alta resolucao para piso, teto, moveis, sombras e brilhos.
    /// </summary>
    public static class BearChaseAssetProcessor
    {
        public const string EntranceResDir = "Assets/Art/BearChase/Resources/BearChaseEntrance";
        public const string TexturesDir = "Assets/Art/BearChase/Textures";

        public static string AdaptedApproachAtlasPath => $"{EntranceResDir}/approach-atlas-adapted-v1.png";
        public static string EntranceFloorPath => $"{EntranceResDir}/entrance-floor-v1.png";
        public static string CorridorFloorTexPath => $"{TexturesDir}/T_BearChase_CorridorFloor.png";
        public static string CeilingTexPath => $"{TexturesDir}/T_BearChase_Ceiling.png";
        public static string RedVelvetTexPath => $"{TexturesDir}/T_BearChase_RedVelvet.png";
        public static string CabinetWoodTexPath => $"{TexturesDir}/T_BearChase_CabinetWood.png";
        public static string BunnyPlushTexPath => $"{TexturesDir}/T_BearChase_BunnyPlush.png";
        public static string BearPlushTexPath => $"{TexturesDir}/T_BearChase_BearPlush.png";
        public static string ContactShadowTexPath => $"{TexturesDir}/T_BearChase_ContactShadow.png";
        public static string SoftGlowTexPath => $"{TexturesDir}/T_BearChase_SoftGlow.png";

        public static void EnsureProcessedAssets()
        {
            Directory.CreateDirectory(EntranceResDir);
            Directory.CreateDirectory(TexturesDir);

            GenerateAdaptedApproachAtlas();
            GenerateFloorAndCeilingTextures();
            ExtractMaterialSlices();
            GenerateGradientHelpers();

            AssetDatabase.Refresh();
            ConfigureImportSettings();
        }

        private static void GenerateAdaptedApproachAtlas()
        {
            string srcPath = $"{EntranceResDir}/approach-atlas-v1.png";
            if (!File.Exists(srcPath))
            {
                return;
            }

            Texture2D src = LoadReadableTexture(srcPath);
            if (src == null)
            {
                return;
            }

            int w = src.width;
            int h = src.height;
            const int bearRowTopY = 385;
            Color32[] pixels = src.GetPixels32();

            // Recolore somente a linha inferior (urso) para pelagem dourado-mel preservando focinho creme e laco vermelho
            for (int y = 0; y < bearRowTopY; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    Color32 c = pixels[idx];
                    if (c.a < 8)
                    {
                        continue;
                    }

                    Color col = c;
                    Color.RGBToHSV(col, out float hue, out float sat, out float val);

                    bool isOutline = val < 0.22f;
                    bool isCreamMuzzleOrPaw = sat < 0.42f && val > 0.62f;
                    bool isRedRibbon = (col.r > col.g * 1.48f && col.r > col.b * 1.75f && val > 0.24f);

                    if (!isOutline && !isCreamMuzzleOrPaw && !isRedRibbon && hue < 0.14f)
                    {
                        float targetHue = Mathf.Clamp01(hue + 0.022f);
                        float targetSat = Mathf.Clamp01(sat * 0.96f);
                        float targetVal = Mathf.Clamp01(val * 1.24f);
                        Color recolored = Color.HSVToRGB(targetHue, targetSat, targetVal);
                        recolored.r = Mathf.Clamp01(recolored.r * 1.04f);
                        recolored.g = Mathf.Clamp01(recolored.g * 1.02f);
                        recolored.a = col.a;
                        pixels[idx] = recolored;
                    }
                }
            }

            var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outTex.SetPixels32(pixels);
            outTex.Apply();
            File.WriteAllBytes(AdaptedApproachAtlasPath, outTex.EncodeToPNG());
            Object.DestroyImmediate(src);
            Object.DestroyImmediate(outTex);
        }

        private static void GenerateFloorAndCeilingTextures()
        {
            string decoPath = $"{TexturesDir}/decorative-atlas-v3.png";
            if (!File.Exists(decoPath))
            {
                return;
            }

            Texture2D deco = LoadReadableTexture(decoPath);
            if (deco == null)
            {
                return;
            }

            int cell = deco.width / 4;
            Color32[] warmWood = ExtractCell(deco, 3 * cell, 0, cell, cell);
            Color32[] honeyWood = ExtractCell(deco, 0, 3 * cell, cell, cell);
            Color32[] parchment = ExtractCell(deco, 1 * cell, 0, cell, cell);

            BuildCorridorFloorTexture(warmWood, honeyWood, cell);
            BuildEntranceFloorTexture(warmWood, honeyWood, cell);
            BuildCeilingTexture(parchment, cell);

            Object.DestroyImmediate(deco);
        }

        private static void BuildCorridorFloorTexture(Color32[] warmWood, Color32[] honeyWood, int cellSize)
        {
            const int size = 1024;
            var outPixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                int plankRow = y / 256;
                bool isCrossSeam = (y % 256) < 7 || (y % 256) > 251;

                for (int x = 0; x < size; x++)
                {
                    int shift = (plankRow % 2) * 128;
                    int sx = (x + shift) % size;
                    int plankCol = sx / 256;
                    bool isVertSeam = (sx % 256) < 6 || (sx % 256) > 252;

                    Color32[] src = ((plankRow + plankCol) % 2 == 0) ? honeyWood : warmWood;
                    int u = (sx * 2 + plankCol * 37) % cellSize;
                    int v = (y * 2 + plankRow * 53) % cellSize;
                    Color c = src[v * cellSize + u];

                    float shade = ((plankRow * 3 + plankCol) % 3) switch
                    {
                        0 => 1.00f,
                        1 => 0.93f,
                        _ => 0.97f
                    };

                    if (isCrossSeam)
                    {
                        shade *= 0.48f;
                    }
                    else if (isVertSeam)
                    {
                        shade *= 0.58f;
                    }

                    c.r = Mathf.Clamp01(c.r * shade * 1.03f);
                    c.g = Mathf.Clamp01(c.g * shade * 0.98f);
                    c.b = Mathf.Clamp01(c.b * shade * 0.88f);
                    c.a = 1f;
                    outPixels[y * size + x] = c;
                }
            }

            SaveTexturePng(CorridorFloorTexPath, size, size, outPixels);
        }

        private static void BuildEntranceFloorTexture(Color32[] warmWood, Color32[] honeyWood, int cellSize)
        {
            const int w = 1024;
            const int h = 512;
            var outPixels = new Color32[w * h];

            for (int y = 0; y < h; y++)
            {
                int plankRow = y / 64;
                bool seamH = (y % 64) < 3;
                float depthShade = Mathf.Lerp(1.04f, 0.72f, y / (float)h);

                for (int x = 0; x < w; x++)
                {
                    int shift = (plankRow % 2) * 256;
                    int sx = (x + shift) % w;
                    bool seamV = (sx % 512) < 3;

                    Color32[] src = (plankRow % 2 == 0) ? honeyWood : warmWood;
                    int u = (sx + plankRow * 61) % cellSize;
                    int v = ((y % 64) * 4 + plankRow * 29) % cellSize;
                    Color c = src[v * cellSize + u];

                    float shade = depthShade * (seamH ? 0.38f : (seamV ? 0.48f : 1.0f));
                    c.r = Mathf.Clamp01(c.r * shade);
                    c.g = Mathf.Clamp01(c.g * shade * 0.96f);
                    c.b = Mathf.Clamp01(c.b * shade * 0.86f);
                    c.a = 1f;
                    outPixels[y * w + x] = c;
                }
            }

            SaveTexturePng(EntranceFloorPath, w, h, outPixels);
        }

        private static void BuildCeilingTexture(Color32[] parchment, int cellSize)
        {
            const int size = 512;
            var outPixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                int v = (y * cellSize) / size;
                for (int x = 0; x < size; x++)
                {
                    int u = (x * cellSize) / size;
                    Color c = parchment[v * cellSize + u];
                    c.r = Mathf.Clamp01(c.r * 0.62f);
                    c.g = Mathf.Clamp01(c.g * 0.50f);
                    c.b = Mathf.Clamp01(c.b * 0.40f);
                    c.a = 1f;
                    outPixels[y * size + x] = c;
                }
            }

            SaveTexturePng(CeilingTexPath, size, size, outPixels);
        }

        private static void ExtractMaterialSlices()
        {
            ExtractCellFromAtlas($"{TexturesDir}/decorative-atlas-v3.png", 4, 0, 0, RedVelvetTexPath);
            ExtractCellFromAtlas($"{TexturesDir}/cabinet-atlas-v1.png", 2, 0, 1, CabinetWoodTexPath);
            ExtractCellFromAtlas($"{TexturesDir}/bunny-atlas-v1.png", 2, 1, 1, BunnyPlushTexPath);

            string bearAtlas = $"{TexturesDir}/bear-atlas-v2.png";
            if (File.Exists(bearAtlas))
            {
                Texture2D src = LoadReadableTexture(bearAtlas);
                if (src != null)
                {
                    int cell = src.width / 2;
                    Color32[] pixels = ExtractCell(src, 0, cell, cell, cell);
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        Color c = pixels[i];
                        Color.RGBToHSV(c, out float h, out float s, out float v);
                        Color recolored = Color.HSVToRGB(Mathf.Clamp01(h + 0.02f), s, Mathf.Clamp01(v * 1.22f));
                        recolored.a = 1f;
                        pixels[i] = recolored;
                    }

                    SaveTexturePng(BearPlushTexPath, cell, cell, pixels);
                    Object.DestroyImmediate(src);
                }
            }
        }

        private static void GenerateGradientHelpers()
        {
            const int size = 128;
            var shadowPixels = new Color32[size * size];
            var glowPixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float falloff = Mathf.Clamp01(1f - r);
                    float smooth = falloff * falloff * (3f - 2f * falloff);
                    byte alpha = (byte)Mathf.RoundToInt(smooth * 255f);

                    shadowPixels[y * size + x] = new Color32(18, 10, 6, alpha);
                    glowPixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            SaveTexturePng(ContactShadowTexPath, size, size, shadowPixels);
            SaveTexturePng(SoftGlowTexPath, size, size, glowPixels);
        }

        private static void ExtractCellFromAtlas(string atlasPath, int grid, int col, int row, string outputPath)
        {
            if (!File.Exists(atlasPath))
            {
                return;
            }

            Texture2D src = LoadReadableTexture(atlasPath);
            if (src == null)
            {
                return;
            }

            int cell = src.width / grid;
            Color32[] pixels = ExtractCell(src, col * cell, row * cell, cell, cell);
            SaveTexturePng(outputPath, cell, cell, pixels);
            Object.DestroyImmediate(src);
        }

        private static Color32[] ExtractCell(Texture2D src, int startX, int startY, int w, int h)
        {
            Color[] raw = src.GetPixels(startX, startY, w, h);
            var pixels = new Color32[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                pixels[i] = raw[i];
            }

            return pixels;
        }

        private static Texture2D LoadReadableTexture(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                Object.DestroyImmediate(tex);
                return null;
            }

            return tex;
        }

        private static void SaveTexturePng(string path, int w, int h, Color32[] pixels)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void ConfigureImportSettings()
        {
            ConfigureSingleTexture(AdaptedApproachAtlasPath, isReadable: true, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{EntranceResDir}/approach-atlas-v1.png", isReadable: true, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{EntranceResDir}/reach-atlas-v1.png", isReadable: true, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{EntranceResDir}/illustrated-fairy-v1.png", isReadable: true, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{EntranceResDir}/armchair-v1.png", isReadable: true, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{EntranceResDir}/window-curtains-v1.png", isReadable: true, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{EntranceResDir}/rectangular-rug-v1.png", isReadable: true, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{EntranceResDir}/wallpaper-v1.png", isReadable: false, isSprite: false, wrap: TextureWrapMode.Repeat, alphaIsTrans: false, noNpotScale: false);
            ConfigureSingleTexture($"{EntranceResDir}/oak-trim-v1.png", isReadable: false, isSprite: false, wrap: TextureWrapMode.Repeat, alphaIsTrans: false, noNpotScale: false);
            ConfigureSingleTexture(EntranceFloorPath, isReadable: false, isSprite: false, wrap: TextureWrapMode.Repeat, alphaIsTrans: false, noNpotScale: false);

            ConfigureSingleTexture(CorridorFloorTexPath, isReadable: false, isSprite: false, wrap: TextureWrapMode.Repeat, alphaIsTrans: false, noNpotScale: false);
            ConfigureSingleTexture(CeilingTexPath, isReadable: false, isSprite: false, wrap: TextureWrapMode.Repeat, alphaIsTrans: false, noNpotScale: false);
            ConfigureSingleTexture($"{TexturesDir}/wallpaper-v1.png", isReadable: false, isSprite: false, wrap: TextureWrapMode.Repeat, alphaIsTrans: false, noNpotScale: false);
            ConfigureSingleTexture($"{TexturesDir}/oak-trim-v1.png", isReadable: false, isSprite: false, wrap: TextureWrapMode.Repeat, alphaIsTrans: false, noNpotScale: false);
            ConfigureSingleTexture($"{TexturesDir}/window-curtains-v1.png", isReadable: false, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture($"{TexturesDir}/rectangular-rug-v1.png", isReadable: false, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: true);
            ConfigureSingleTexture(ContactShadowTexPath, isReadable: false, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: false);
            ConfigureSingleTexture(SoftGlowTexPath, isReadable: false, isSprite: false, wrap: TextureWrapMode.Clamp, alphaIsTrans: true, noNpotScale: false);
        }

        private static void ConfigureSingleTexture(
            string path,
            bool isReadable,
            bool isSprite,
            TextureWrapMode wrap,
            bool alphaIsTrans,
            bool noNpotScale)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool dirty = false;
            TextureImporterType desiredType = isSprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (importer.textureType != desiredType)
            {
                importer.textureType = desiredType;
                dirty = true;
            }

            if (noNpotScale && importer.npotScale != TextureImporterNPOTScale.None)
            {
                importer.npotScale = TextureImporterNPOTScale.None;
                dirty = true;
            }

            if (noNpotScale && importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                dirty = true;
            }

            if (importer.isReadable != isReadable)
            {
                importer.isReadable = isReadable;
                dirty = true;
            }

            if (importer.wrapMode != wrap)
            {
                importer.wrapMode = wrap;
                dirty = true;
            }

            if (importer.alphaIsTransparency != alphaIsTrans)
            {
                importer.alphaIsTransparency = alphaIsTrans;
                dirty = true;
            }

            if (importer.maxTextureSize < 2048)
            {
                importer.maxTextureSize = 2048;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }
    }
}
