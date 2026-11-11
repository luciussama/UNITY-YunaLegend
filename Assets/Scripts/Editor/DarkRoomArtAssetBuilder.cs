using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomArtAssetBuilder
    {
        private const string MaterialsFolder = "Assets/Materials";

        [MenuItem("YunaLegend/Gerar Texturas e Materiais Estilizados")]
        public static void GenerateAllArtAssets()
        {
            EnsureFolderExists(MaterialsFolder);

            // 1. Gerar Texturas Procedurais de Alta Fidelidade (PNG)
            SaveTextureAsPng(GenerateOakPlanksTexture(), Path.Combine(MaterialsFolder, "T_DarkOak_Planks.png"));
            SaveTextureAsPng(GeneratePlushTexture(), Path.Combine(MaterialsFolder, "T_TeddyPlush.png"));
            SaveTextureAsPng(GenerateAbcBlocksTexture(), Path.Combine(MaterialsFolder, "T_ToyBlock_ABC.png"));
            SaveTextureAsPng(GenerateCastleBlocksTexture(), Path.Combine(MaterialsFolder, "T_Castle_Blocks.png"));
            SaveTextureAsPng(GenerateCircusDrumTexture(), Path.Combine(MaterialsFolder, "T_CircusDrum.png"));
            SaveTextureAsPng(GenerateVelvetTexture(), Path.Combine(MaterialsFolder, "T_Velvet_Purple.png"));
            SaveTextureAsPng(GenerateBookCoversTexture(), Path.Combine(MaterialsFolder, "T_BookCovers.png"));
            SaveTextureAsPng(GenerateTrainTrackTexture(), Path.Combine(MaterialsFolder, "T_TrainTrack_Wood.png"));
            SaveTextureAsPng(GenerateWallpaperTexture(), Path.Combine(MaterialsFolder, "T_Victorian_Wallpaper.png"));
            SaveTextureAsPng(GenerateWallBaseboardTexture(), Path.Combine(MaterialsFolder, "T_Wall_Baseboard.png"));
            SaveTextureAsPng(GenerateMoonlitWindowTexture(), Path.Combine(MaterialsFolder, "T_Moonlit_Window.png"));
            SaveTextureAsPng(GenerateWindowCookieTexture(), Path.Combine(MaterialsFolder, "T_Window_Cookie.png"));
            SaveTextureAsPng(GenerateMoonlightFloorShadowTexture(), Path.Combine(MaterialsFolder, "T_Moonlight_Floor_Shadow.png"));
            SaveTextureAsPng(GenerateDreamPortalTexture(), Path.Combine(MaterialsFolder, "T_DreamPortal.png"));
            SaveTextureAsPng(GenerateFalseDoorPosterTexture(), Path.Combine(MaterialsFolder, "T_FalseDoor_Poster.png"));

            AssetDatabase.Refresh();

            // Recarregar texturas importadas do AssetDatabase
            var oakTex = LoadTexture("T_DarkOak_Planks.png");
            var plushTex = LoadTexture("T_TeddyPlush.png");
            var abcTex = LoadTexture("T_ToyBlock_ABC.png");
            var castleTex = LoadTexture("T_Castle_Blocks.png");
            var drumTex = LoadTexture("T_CircusDrum.png");
            var velvetTex = LoadTexture("T_Velvet_Purple.png");
            var bookTex = LoadTexture("T_BookCovers.png");
            var trackTex = LoadTexture("T_TrainTrack_Wood.png");
            var wallpaperTex = LoadTexture("T_Victorian_Wallpaper.png");
            var baseboardTex = LoadTexture("T_Wall_Baseboard.png");
            var windowTex = LoadTexture("T_Moonlit_Window.png");
            var floorShadowTex = LoadTexture("T_Moonlight_Floor_Shadow.png");
            var portalTex = LoadTexture("T_DreamPortal.png");
            var doorTex = LoadTexture("T_FalseDoor_Poster.png");

            // 2. Criar / Atualizar Materiais URP com Paleta Legend of Mana (Tons pastéis, envelhecidos, aquarelados)
            // Piso e Madeira nobre com pátina quente
            CreateUrpMaterial("M_DarkOak_Floor", new Color(0.42f, 0.28f, 0.18f), 0.35f, oakTex, null, new Vector2(35f, 3f));
            CreateUrpMaterial("M_GiantWardrobe", new Color(0.36f, 0.24f, 0.15f), 0.32f, oakTex, null, new Vector2(4f, 5f));
            CreateUrpMaterial("M_Dresser_Oak", new Color(0.46f, 0.32f, 0.20f), 0.36f, oakTex, null, new Vector2(2f, 1.5f));
            CreateUrpMaterial("M_TrainTrack_Wood", Color.white, 0.38f, trackTex, null, new Vector2(3f, 1f));
            CreateUrpMaterial("M_Moonlight_Floor_Projection", new Color(0.75f, 0.88f, 1.0f, 0.70f), 0.20f, floorShadowTex);

            // Brinquedos e Blocos em têmpera pastel envelhecida
            CreateUrpMaterial("M_TeddyPlush", Color.white, 0.12f, plushTex);
            CreateUrpMaterial("M_ToyBlock_ABC", Color.white, 0.35f, abcTex);
            CreateUrpMaterial("M_ToyBlock_Red", new Color(0.72f, 0.38f, 0.35f), 0.32f);    // Terracota empoeirado
            CreateUrpMaterial("M_ToyBlock_Yellow", new Color(0.82f, 0.65f, 0.38f), 0.32f); // Ocre antigo / mostarda suave
            CreateUrpMaterial("M_ToyBlock_Blue", new Color(0.38f, 0.50f, 0.62f), 0.32f);   // Azul ardósia pastel
            CreateUrpMaterial("M_ToyBlock_Green", new Color(0.42f, 0.54f, 0.44f), 0.32f);  // Verde sálvia / musgo seco

            // Tecidos, couro e metais envelhecidos
            CreateUrpMaterial("M_CircusDrum", Color.white, 0.35f, drumTex);
            CreateUrpMaterial("M_RoyalVelvet_Purple", Color.white, 0.22f, velvetTex);
            CreateUrpMaterial("M_MusicBox_Rosewood", new Color(0.55f, 0.30f, 0.35f), 0.42f, oakTex, null, new Vector2(1.5f, 1.5f));
            CreateUrpMaterial("M_Brass_Gold", new Color(0.85f, 0.70f, 0.36f), 0.60f, null, new Color(0.25f, 0.20f, 0.08f)); // Latão com pátina

            // Livros Ilustrados antigos de conto de fadas
            CreateUrpMaterial("M_Book_Covers", Color.white, 0.36f, bookTex);
            CreateUrpMaterial("M_Book_Blue", new Color(0.32f, 0.44f, 0.56f), 0.35f);
            CreateUrpMaterial("M_Book_Burgundy", new Color(0.55f, 0.28f, 0.32f), 0.35f);
            CreateUrpMaterial("M_Book_Green", new Color(0.35f, 0.48f, 0.38f), 0.35f);
            CreateUrpMaterial("M_Book_GoldPages", new Color(0.90f, 0.85f, 0.72f), 0.25f);

            // Castelinho e Portais
            CreateUrpMaterial("M_Castle_Blocks", Color.white, 0.32f, castleTex, null, new Vector2(2f, 1f));
            CreateUrpMaterial("M_Castle_TurretBlue", new Color(0.34f, 0.44f, 0.58f), 0.32f);
            CreateUrpMaterial("M_FalseDoor_Poster", Color.white, 0.35f, doorTex);
            CreateUrpMaterial("M_DreamPortal_Gold", Color.white, 0.65f, portalTex, new Color(0.90f, 0.75f, 0.35f) * 1.8f);

            // Personagens (Yuna e Nanda) em estilo conto de fadas
            CreateUrpMaterial("M_Nanda_Glow", new Color(1.0f, 0.92f, 0.60f), 0.85f, null, new Color(1.0f, 0.85f, 0.40f) * 3.0f);
            CreateUrpMaterial("M_Yuna_Dress", new Color(0.94f, 0.90f, 0.82f), 0.25f);  // Linho bege suave
            CreateUrpMaterial("M_Yuna_Ribbon", new Color(0.72f, 0.28f, 0.32f), 0.35f); // Carmim desbotado
            CreateUrpMaterial("M_Yuna_Skin", new Color(0.98f, 0.86f, 0.78f), 0.25f);

            // Cenografia Monumental
            CreateUrpMaterial("M_Background_Wall", Color.white, 0.0f, wallpaperTex, new Color(0.76f, 0.76f, 0.84f), new Vector2(-15f, -2.5f), new Vector2(1f, 1f));
            CreateUrpMaterial("M_Wall_Baseboard", Color.white, 0.08f, baseboardTex, new Color(0.68f, 0.68f, 0.76f), new Vector2(-30f, -1f), new Vector2(1f, 1f));
            CreateUrpMaterial("M_Moonlit_Window", Color.white, 0.0f, windowTex, new Color(0.78f, 0.84f, 0.96f) * 0.95f, new Vector2(-1f, -1f), new Vector2(1f, 1f), alphaClip: true);

            // 3. Configurar Volume URP de Pos-Processamento
            ConfigurePostProcessingVolume();

            AssetDatabase.SaveAssets();
            Debug.Log("[DarkRoomArtAssetBuilder] Texturas ilustradas canônicas geradas com sucesso!");
        }

        private static void EnsureFolderExists(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        private static Texture2D LoadTexture(string filename)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(MaterialsFolder, filename));
        }

        private static void SaveTextureAsPng(Texture2D tex, string path)
        {
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void CreateUrpMaterial(string name, Color baseColor, float smoothness, Texture2D tex = null, Color? emission = null, Vector2? tiling = null, Vector2? offset = null, bool alphaClip = false)
        {
            var matPath = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
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

            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);

            if (alphaClip)
            {
                mat.SetFloat("_AlphaClip", 1f);
                mat.SetFloat("_Cutoff", 0.5f);
                mat.EnableKeyword("_ALPHATEST_ON");
            }
            else
            {
                mat.SetFloat("_AlphaClip", 0f);
                mat.DisableKeyword("_ALPHATEST_ON");
            }

            Vector2 scaleVal = tiling ?? Vector2.one;
            Vector2 offsetVal = offset ?? Vector2.zero;

            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", scaleVal);
                mat.SetTextureOffset("_BaseMap", offsetVal);
            }

            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                if (tex != null)
                {
                    mat.SetTexture("_EmissionMap", tex);
                    mat.SetTextureScale("_EmissionMap", scaleVal);
                    mat.SetTextureOffset("_EmissionMap", offsetVal);
                }
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(mat);
        }

        // =========================================================================
        // GERADORES PROCEDURAIS DE TEXTURA DE ALTA FIDELIDADE
        // =========================================================================

        private static Texture2D GenerateOakPlanksTexture()
        {
            int width = 1024;
            int height = 512;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            var baseWood = new Color(0.38f, 0.23f, 0.13f);
            var darkSeam = new Color(0.10f, 0.05f, 0.02f);
            var highlight = new Color(0.52f, 0.35f, 0.20f);
            var nailCol = new Color(0.70f, 0.60f, 0.35f);

            int plankHeight = 128; // 4 tábuas horizontais

            for (int y = 0; y < height; y++)
            {
                int plankIdx = y / plankHeight;
                int yInPlank = y % plankHeight;
                bool isHorizontalSeam = (yInPlank <= 4);
                bool isBevel = (yInPlank == 5 || yInPlank == 6);

                for (int x = 0; x < width; x++)
                {
                    if (isHorizontalSeam)
                    {
                        tex.SetPixel(x, y, darkSeam);
                        continue;
                    }

                    if (isBevel)
                    {
                        tex.SetPixel(x, y, highlight);
                        continue;
                    }

                    // Seam vertical a cada 512px alternado
                    int seamOffset = (plankIdx * 280) % width;
                    int distToVertSeam = Mathf.Abs(((x + seamOffset) % 512));
                    if (distToVertSeam <= 3)
                    {
                        tex.SetPixel(x, y, darkSeam);
                        continue;
                    }

                    // Pregos de latão nas extremidades das pranchas
                    if (distToVertSeam == 18 && (yInPlank == 22 || yInPlank == plankHeight - 22))
                    {
                        tex.SetPixel(x, y, nailCol);
                        continue;
                    }

                    // Grão de madeira rico com múltiplas frequências
                    float perlin = Mathf.PerlinNoise(x * 0.012f, y * 0.004f);
                    float grain = Mathf.Sin(x * 0.08f + perlin * 14.0f) * 0.12f;
                    float fineGrain = (Mathf.PerlinNoise(x * 0.06f, y * 0.02f) - 0.5f) * 0.08f;
                    float plankVar = (plankIdx % 2 == 0 ? 0.04f : -0.04f);

                    float f = 1.0f + grain + fineGrain + plankVar;
                    var c = new Color(baseWood.r * f, baseWood.g * f, baseWood.b * f, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        private static float WatercolorWash(float x, float y)
        {
            float w1 = Mathf.PerlinNoise(x * 0.015f, y * 0.015f) * 0.12f;
            float w2 = (Mathf.PerlinNoise(x * 0.04f, y * 0.04f) - 0.5f) * 0.06f;
            return 0.94f + w1 + w2;
        }

        private static Texture2D GenerateAbcBlocksTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            // 4 quadrantes de 256x256 com paleta Legend of Mana pastel envelhecida
            // Q0 (top-left): Terracota empoeirado com letra "A"
            // Q1 (top-right): Ocre suave / mostarda com letra "B"
            // Q2 (bottom-left): Azul ardósia pastel com letra "C"
            // Q3 (bottom-right): Verde sálvia com Estrela de brinquedo

            DrawToyBlockQuad(tex, 0, 256, 256, new Color(0.72f, 0.38f, 0.35f), 'A');
            DrawToyBlockQuad(tex, 256, 256, 256, new Color(0.82f, 0.65f, 0.38f), 'B');
            DrawToyBlockQuad(tex, 0, 0, 256, new Color(0.38f, 0.50f, 0.62f), 'C');
            DrawToyBlockQuad(tex, 256, 0, 256, new Color(0.42f, 0.54f, 0.44f), '*');

            tex.Apply();
            return tex;
        }

        private static void DrawToyBlockQuad(Texture2D tex, int ox, int oy, int qSize, Color blockColor, char symbol)
        {
            var woodBevel = new Color(0.86f, 0.78f, 0.65f);   // Pinho envelhecido
            var shadowBorder = new Color(0.28f, 0.22f, 0.18f); // Sombra suave de madeira
            int border = 24;

            for (int ly = 0; ly < qSize; ly++)
            {
                for (int lx = 0; lx < qSize; lx++)
                {
                    int gx = ox + lx;
                    int gy = oy + ly;

                    bool isOuterRim = (lx < 4 || lx >= qSize - 4 || ly < 4 || ly >= qSize - 4);
                    bool isBevel = (lx < border || lx >= qSize - border || ly < border || ly >= qSize - border);

                    if (isOuterRim)
                    {
                        tex.SetPixel(gx, gy, shadowBorder);
                    }
                    else if (isBevel)
                    {
                        float shade = (lx < border || ly >= qSize - border) ? 1.08f : 0.92f;
                        float wash = WatercolorWash(gx, gy);
                        tex.SetPixel(gx, gy, woodBevel * shade * wash);
                    }
                    else
                    {
                        float wash = WatercolorWash(gx, gy);
                        float woodNoise = (Mathf.PerlinNoise(gx * 0.05f, gy * 0.05f) - 0.5f) * 0.06f;
                        var bg = new Color((blockColor.r + woodNoise) * wash, (blockColor.g + woodNoise) * wash, (blockColor.b + woodNoise) * wash, 1f);

                        float cx = qSize * 0.5f;
                        float cy = qSize * 0.5f;
                        bool isLetter = CheckSymbolPixel(symbol, lx - cx, ly - cy);
                        bool isDropShadow = CheckSymbolPixel(symbol, lx - cx - 3, ly - cy + 3);

                        if (isLetter)
                        {
                            tex.SetPixel(gx, gy, new Color(0.96f, 0.94f, 0.88f) * wash); // Creme suave pergaminho
                        }
                        else if (isDropShadow)
                        {
                            tex.SetPixel(gx, gy, bg * 0.70f);
                        }
                        else
                        {
                            tex.SetPixel(gx, gy, bg);
                        }
                    }
                }
            }
        }

        private static bool CheckSymbolPixel(char symbol, float dx, float dy)
        {
            float r = 50f;
            float th = 10f;

            if (symbol == 'A')
            {
                bool leftLeg = Mathf.Abs(dy - (dx * 2.2f + 20f)) < th && dy > -r && dy < r;
                bool rightLeg = Mathf.Abs(dy - (-dx * 2.2f + 20f)) < th && dy > -r && dy < r;
                bool bar = Mathf.Abs(dy - (-5f)) < th * 0.8f && Mathf.Abs(dx) < 26f;
                return (leftLeg || rightLeg || bar);
            }
            if (symbol == 'B')
            {
                bool spine = Mathf.Abs(dx - (-25f)) < th && dy > -r && dy < r;
                bool topLoop = (dx > -25f) && Mathf.Abs(Vector2.Distance(new Vector2(dx, dy), new Vector2(-10f, 22f)) - 26f) < th;
                bool botLoop = (dx > -25f) && Mathf.Abs(Vector2.Distance(new Vector2(dx, dy), new Vector2(-10f, -22f)) - 26f) < th;
                return (spine || topLoop || botLoop);
            }
            if (symbol == 'C')
            {
                float dist = Vector2.Distance(new Vector2(dx, dy), Vector2.zero);
                bool arc = Mathf.Abs(dist - 38f) < th && !(dx > 12f && Mathf.Abs(dy) < 22f);
                return arc;
            }
            float dStar = Vector2.Distance(new Vector2(dx, dy), Vector2.zero);
            float angle = Mathf.Atan2(dy, dx);
            float starRadius = 36f + Mathf.Cos(angle * 5f) * 14f;
            return dStar < starRadius;
        }

        private static Texture2D GenerateCastleBlocksTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            var stoneBlue = new Color(0.40f, 0.50f, 0.62f);  // Ardósia cobalto envelhecida
            var stoneGrey = new Color(0.58f, 0.60f, 0.64f);  // Calcário antigo aquarelado
            var mortarDark = new Color(0.24f, 0.26f, 0.32f); // Argamassa suave
            var goldRim = new Color(0.82f, 0.68f, 0.38f);    // Ouro envelhecido
            var toothBlue = new Color(0.34f, 0.44f, 0.56f);  // Ameias azuis desbotadas

            int blockH = 64;
            int blockW = 128;

            for (int y = 0; y < size; y++)
            {
                int row = y / blockH;
                int yInRow = y % blockH;
                bool isHorizMortar = (yInRow <= 4);
                bool isTopCrenel = (y >= size - 48);

                for (int x = 0; x < size; x++)
                {
                    float wash = WatercolorWash(x, y);

                    if (isTopCrenel)
                    {
                        bool isTooth = (x % 96) < 48;
                        tex.SetPixel(x, y, (isTooth ? toothBlue : stoneGrey * 0.85f) * wash);
                        continue;
                    }

                    if (y >= size - 56 && y < size - 48)
                    {
                        tex.SetPixel(x, y, goldRim * wash);
                        continue;
                    }

                    int rowOffset = (row % 2 == 0) ? 0 : (blockW / 2);
                    int xInBlock = (x + rowOffset) % blockW;
                    bool isVertMortar = (xInBlock <= 4);

                    if (isHorizMortar || isVertMortar)
                    {
                        tex.SetPixel(x, y, mortarDark);
                    }
                    else
                    {
                        float bevel = (xInBlock < 12 || yInRow < 12) ? 0.90f : (xInBlock > blockW - 12 || yInRow > blockH - 12 ? 1.10f : 1.0f);
                        var baseColor = ((x / blockW + row) % 2 == 0) ? stoneBlue : stoneGrey;
                        float noise = (Mathf.PerlinNoise(x * 0.05f, y * 0.05f) - 0.5f) * 0.06f;
                        tex.SetPixel(x, y, (baseColor + new Color(noise, noise, noise, 0f)) * bevel * wash);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateCircusDrumTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            var crimson = new Color(0.70f, 0.32f, 0.30f);  // Terracota carmim envelhecido
            var ivory = new Color(0.92f, 0.88f, 0.80f);    // Marfim pergaminho suave
            var goldBand = new Color(0.82f, 0.68f, 0.35f); // Latão com pátina
            var brassDark = new Color(0.42f, 0.34f, 0.20f);

            int rimHeight = 44;

            for (int y = 0; y < size; y++)
            {
                bool isTopRim = (y >= size - rimHeight);
                bool isBotRim = (y < rimHeight);
                bool isRimEdge = (y == rimHeight - 2 || y == size - rimHeight + 2);

                for (int x = 0; x < size; x++)
                {
                    float wash = WatercolorWash(x, y);

                    if (isTopRim || isBotRim)
                    {
                        bool isRivet = (x % 64 >= 28 && x % 64 <= 36) && ((y % 22) >= 8 && (y % 22) <= 14);
                        tex.SetPixel(x, y, (isRivet ? brassDark : goldBand) * wash);
                    }
                    else if (isRimEdge)
                    {
                        tex.SetPixel(x, y, brassDark);
                    }
                    else
                    {
                        int bandY = y - rimHeight;
                        int totalBodyH = size - (rimHeight * 2);
                        float progressY = (float)bandY / totalBodyH;

                        int periodX = x % 128;
                        float wave = Mathf.Abs((periodX - 64f) / 64f);

                        bool isRed = (progressY > wave * 0.7f ^ (x / 128) % 2 == 0);
                        bool isCord = Mathf.Abs(progressY - wave * 0.7f) < 0.02f;

                        if (isCord)
                        {
                            tex.SetPixel(x, y, goldBand * wash);
                        }
                        else
                        {
                            tex.SetPixel(x, y, (isRed ? crimson : ivory) * wash);
                        }
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GeneratePlushTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var baseHoney = new Color(0.72f, 0.58f, 0.44f); // Feltro linho tostado
            var stitchDark = new Color(0.44f, 0.32f, 0.22f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.12f - 0.06f;
                    float n2 = (Mathf.PerlinNoise(x * 0.4f, y * 0.4f) - 0.5f) * 0.06f;
                    float stipple = ((x + y) % 3 == 0) ? 0.02f : -0.02f;
                    float wash = WatercolorWash(x, y);

                    bool isStitchLine = (x == 20 || x == size - 20 || y == 20 || y == size - 20);
                    bool isStitchDashed = ((x + y) % 18 < 10);

                    if (isStitchLine && isStitchDashed)
                    {
                        tex.SetPixel(x, y, stitchDark);
                    }
                    else
                    {
                        float f = (1.0f + n1 + n2 + stipple) * wash;
                        tex.SetPixel(x, y, new Color(baseHoney.r * f, baseHoney.g * f, baseHoney.b * f, 1f));
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateBookCoversTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            var leatherBlue = new Color(0.32f, 0.44f, 0.56f); // Couro azul ardósia envelhecido
            var leatherBurg = new Color(0.55f, 0.28f, 0.32f); // Couro vinho envelhecido
            var goldFoil = new Color(0.84f, 0.70f, 0.38f);    // Douramento antigo com pátina
            var pagesCream = new Color(0.92f, 0.88f, 0.78f);   // Páginas creme pergaminho
            var pageLines = new Color(0.68f, 0.62f, 0.52f);

            for (int y = 0; y < size; y++)
            {
                bool isTopBook = (y >= 256);
                int localY = isTopBook ? (y - 256) : y;
                var coverCol = isTopBook ? leatherBlue : leatherBurg;

                for (int x = 0; x < size; x++)
                {
                    float wash = WatercolorWash(x, y);

                    if (x > 380)
                    {
                        bool isLine = (y % 4 == 0);
                        tex.SetPixel(x, y, (isLine ? pageLines : pagesCream) * wash);
                    }
                    else
                    {
                        bool isBorder = (localY <= 12 || localY >= 244 || x <= 16 || x == 376);
                        bool isGoldStripe = (localY >= 30 && localY <= 36) || (localY >= 220 && localY <= 226);

                        if (isBorder || isGoldStripe)
                        {
                            tex.SetPixel(x, y, goldFoil * wash);
                        }
                        else
                        {
                            float leather = (Mathf.PerlinNoise(x * 0.15f, y * 0.15f) - 0.5f) * 0.06f;
                            tex.SetPixel(x, y, (coverCol + new Color(leather, leather, leather, 0f)) * wash);
                        }
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateVelvetTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            var deepViolet = new Color(0.44f, 0.32f, 0.48f);   // Ameixa empoeirado
            var brightViolet = new Color(0.58f, 0.44f, 0.62f); // Lavanda suave
            var goldButton = new Color(0.82f, 0.68f, 0.35f);   // Botão de latão antigo

            int diamondSize = 64;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float wash = WatercolorWash(x, y);
                    int d1 = (x + y) % diamondSize;
                    int d2 = (x - y + 1024) % diamondSize;
                    int distToSeam = Mathf.Min(Mathf.Min(d1, diamondSize - d1), Mathf.Min(d2, diamondSize - d2));

                    bool isIntersection = (distToSeam == 0) && (x % diamondSize < 4 || x % diamondSize > diamondSize - 4);

                    if (isIntersection)
                    {
                        tex.SetPixel(x, y, goldButton * wash);
                    }
                    else if (distToSeam <= 2)
                    {
                        tex.SetPixel(x, y, deepViolet * 0.65f);
                    }
                    else
                    {
                        float relief = Mathf.Clamp01((float)distToSeam / (diamondSize * 0.5f));
                        var c = Color.Lerp(deepViolet, brightViolet, relief);
                        tex.SetPixel(x, y, c * wash);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateTrainTrackTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            var darkSleeper = new Color(0.34f, 0.24f, 0.18f); // Madeira envelhecida
            var gravel = new Color(0.20f, 0.18f, 0.20f);
            var brassRail = new Color(0.82f, 0.68f, 0.38f);   // Trilhos de latão envelhecido
            var railShadow = new Color(0.18f, 0.14f, 0.12f);

            for (int y = 0; y < size; y++)
            {
                bool isRail1 = (y >= 140 && y <= 164);
                bool isRail2 = (y >= 348 && y <= 372);
                bool isRailShadow = (y == 138 || y == 139 || y == 346 || y == 347);

                for (int x = 0; x < size; x++)
                {
                    float wash = WatercolorWash(x, y);
                    bool isSleeper = (x % 96) < 56;

                    if (isRail1 || isRail2)
                    {
                        tex.SetPixel(x, y, brassRail * wash);
                    }
                    else if (isRailShadow)
                    {
                        tex.SetPixel(x, y, railShadow);
                    }
                    else if (isSleeper)
                    {
                        float woodNoise = (Mathf.PerlinNoise(x * 0.08f, y * 0.02f) - 0.5f) * 0.08f;
                        tex.SetPixel(x, y, (darkSleeper + new Color(woodNoise, woodNoise, woodNoise, 0f)) * wash);
                    }
                    else
                    {
                        tex.SetPixel(x, y, gravel);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateWallpaperTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            // Paleta canonica exata de BackgroundRenderer.js (drawRoomWall):
            // Fundo #0d0a14, listras verticais #17131d e estrelas douradas sutis #facc15 (alpha ~0.065)
            var wallBase = new Color(13f / 255f, 10f / 255f, 20f / 255f, 1f);   // #0d0a14
            var wallStripe = new Color(23f / 255f, 19f / 255f, 29f / 255f, 1f); // #17131d
            var starGold = new Color(250f / 255f, 204f / 255f, 21f / 255f, 1f); // #facc15

            int stripeW = 64;   // 4 ciclos de listras (128px cada) em 512px
            int cellW = 128;    // Grade regular de estrelas alternadas
            int cellH = 128;

            for (int y = 0; y < size; y++)
            {
                int row = y / cellH;
                int starCenterY = row * cellH + (cellH / 2);

                for (int x = 0; x < size; x++)
                {
                    bool isStripe = ((x / stripeW) % 2 == 0);
                    Color c = isStripe ? wallStripe : wallBase;

                    // Padrao regular alternado (staggered) de estrelas douradas sutis nas listras
                    int col = x / cellW;
                    int starCenterX = col * cellW + ((row % 2 == 0) ? (stripeW / 2) : (stripeW + stripeW / 2));

                    float dx = Mathf.Abs(x - starCenterX);
                    float dy = Mathf.Abs(y - starCenterY);
                    // Losango/estrela de 4 pontas delicada (raio ~5.5px na textura 512x512)
                    float diamondDist = Mathf.Pow(dx, 0.75f) + Mathf.Pow(dy, 0.75f);
                    if (diamondDist <= 4.2f)
                    {
                        float starMask = Mathf.Clamp01((4.2f - diamondDist) / 1.4f);
                        c = Color.Lerp(c, starGold, 0.068f * starMask);
                    }

                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateWallBaseboardTexture()
        {
            int width = 512;
            int height = 128;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            // Paleta canonica do rodape em BackgroundRenderer.js (drawRoomWall):
            // Corpo principal #2b2138, friso superior #3e3152 e sulcos discretos #1e1628
            var bodyCol = new Color(43f / 255f, 33f / 255f, 56f / 255f, 1f);     // #2b2138
            var trimCol = new Color(62f / 255f, 49f / 255f, 82f / 255f, 1f);     // #3e3152
            var trimRim = new Color(77f / 255f, 60f / 255f, 104f / 255f, 1f);    // #4d3c68
            var grooveCol = new Color(26f / 255f, 19f / 255f, 37f / 255f, 1f);   // #1a1325

            for (int y = 0; y < height; y++)
            {
                // normY = 0 no topo do rodape (friso #3e3152), normY = 1 na base junto ao piso
                float normY = 1.0f - (float)y / (height - 1);

                for (int x = 0; x < width; x++)
                {
                    Color c;
                    if (normY <= 0.045f)
                    {
                        c = trimRim;
                    }
                    else if (normY <= 0.18f)
                    {
                        c = trimCol;
                    }
                    else if (normY <= 0.22f)
                    {
                        c = grooveCol;
                    }
                    else if (normY >= 0.92f)
                    {
                        c = grooveCol;
                    }
                    else
                    {
                        // Corpo #2b2138 com discretas juntas verticais de lambril a cada 128px
                        bool isPanelGroove = (x % 128 <= 2);
                        float grain = (Mathf.PerlinNoise(x * 0.04f, y * 0.15f) - 0.5f) * 0.015f;
                        c = isPanelGroove ? grooveCol : new Color(
                            Mathf.Clamp01(bodyCol.r + grain),
                            Mathf.Clamp01(bodyCol.g + grain),
                            Mathf.Clamp01(bodyCol.b + grain),
                            1f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMoonlitWindowTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            // Paleta canonica exata de BackgroundRenderer.js (linhas 285-347)
            var wallOutside = new Color(0.005f, 0.007f, 0.018f, 1f);  // Parede noturna atras do arco superior (sem emissao aparente)
            var frameFill = new Color(0.102f, 0.075f, 0.145f, 1f);    // #1a1325 Moldura interna em tom profundo
            var frameBorder = new Color(0.302f, 0.235f, 0.408f, 1f);  // #4d3c68 Contorno delicado da moldura
            var nightSky = new Color(0.035f, 0.043f, 0.078f, 1f);     // #090b14 Vidro com ceu da meia-noite
            var moonSilver = new Color(0.863f, 0.906f, 0.961f, 1f);   // #dce7f5 Lua crescente prateada
            var moonHalo = new Color(0.55f, 0.68f, 0.90f, 1f);        // Brilho suave ao redor do crescente
            var starWhite = new Color(0.973f, 0.980f, 0.988f, 1f);    // #f8fafc Estrelas cintilando na janela
            var mullionCol = new Color(0.196f, 0.145f, 0.286f, 1f);   // #322549 Divisorias em cruz (2x2)
            var curtainLilac = new Color(0.659f, 0.333f, 0.969f, 1f); // #a855f7 Cortinas translucidas lilases

            // Coordenadas canonicas: janela de 90x130px (px em [0..90], py em [0..130] do topo para a base)
            Vector2 archCenter = new Vector2(45f, 45f);
            Vector2 moonOuterCenter = new Vector2(35f, 35f); // (sx + 35, 115 - 80)
            Vector2 moonCutCenter = new Vector2(39f, 33f);   // (sx + 39, 113 - 80)
            float moonOuterR = 12.0f;
            float moonCutR = 10.0f;

            Vector2 star1 = new Vector2(60f, 30f); // (sx + 60, 110 - 80)
            Vector2 star2 = new Vector2(22f, 65f); // (sx + 22, 145 - 80)
            Vector2 star3 = new Vector2(65f, 80f); // (sx + 65, 160 - 80)

            for (int y = 0; y < size; y++)
            {
                // py = 0 no topo da textura (y = size - 1), py = 130 na base (y = 0)
                float py = (1.0f - (float)y / (size - 1)) * 130.0f;

                for (int x = 0; x < size; x++)
                {
                    float px = ((float)x / (size - 1)) * 90.0f;

                    // 1. Silhueta externa: roundRect(0, 0, 90, 130, [45, 45, 4, 4])
                    float distArch = Vector2.Distance(new Vector2(px, py), archCenter);
                    bool outsideOuterArch = (py < 45f && distArch > 45f);

                    if (outsideOuterArch)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Contorno externo #4d3c68 (espessura ~2.2px canonicos)
                    bool isOuterStroke = (py < 45f)
                        ? (distArch >= 42.6f)
                        : (px <= 2.4f || px >= 87.6f || py >= 127.6f);

                    // 2. Vidro interno: roundRect(6, 6, 78, 118, [39, 39, 2, 2]) -> px em [6..84], py em [6..124]
                    bool insideGlass = (py < 45f)
                        ? (distArch <= 39.0f)
                        : (px >= 6.0f && px <= 84.0f && py <= 124.0f);

                    Color col;

                    if (!insideGlass)
                    {
                        col = isOuterStroke ? frameBorder : frameFill;
                    }
                    else
                    {
                        // Ceu da meia-noite com halo sutil ao redor da lua crescente
                        float distMoon = Vector2.Distance(new Vector2(px, py), moonOuterCenter);
                        float distCut = Vector2.Distance(new Vector2(px, py), moonCutCenter);

                        float halo = Mathf.Exp(-Mathf.Max(0f, distMoon - moonOuterR * 0.6f) * 0.11f) * 0.22f;
                        col = Color.Lerp(nightSky, moonHalo, halo);

                        // Lua crescente prateada (#dce7f5 menos recorte #090b14)
                        if (distMoon <= moonOuterR && distCut >= moonCutR - 0.8f)
                        {
                            float outerAA = Mathf.Clamp01((moonOuterR - distMoon) * 2.5f);
                            float innerAA = Mathf.Clamp01((distCut - (moonCutR - 0.8f)) * 2.5f);
                            float crescentMask = outerAA * innerAA;
                            col = Color.Lerp(col, moonSilver, crescentMask);
                        }

                        // 3 Estrelas cintilantes (#f8fafc)
                        float dS1 = Mathf.Max(Mathf.Abs(px - star1.x), Mathf.Abs(py - star1.y));
                        float dS2 = Mathf.Max(Mathf.Abs(px - star2.x), Mathf.Abs(py - star2.y));
                        float dS3 = Mathf.Max(Mathf.Abs(px - star3.x), Mathf.Abs(py - star3.y));
                        if (dS1 <= 1.25f || dS2 <= 0.95f || dS3 <= 1.25f)
                        {
                            col = starWhite;
                        }

                        // Divisorias em cruz 2x2 da vidraca (#322549): vertical em px=45, horizontal em py=60 (140-80)
                        bool isVertMullion = Mathf.Abs(px - 45.0f) <= 1.15f;
                        bool isHorizMullion = Mathf.Abs(py - 60.0f) <= 1.15f;
                        if (isVertMullion || isHorizMullion)
                        {
                            col = mullionCol;
                        }
                    }

                    // 3. Cortinas translucidas lilases nas duas laterais (rgba(168, 85, 247, 0.20))
                    float t = Mathf.Clamp01(py / 130.0f);
                    // Curva quadratica de (0) -> controle (19) -> base (7)
                    float leftCurtainEdge = (1f - t) * (1f - t) * 1.5f + 2f * (1f - t) * t * 19.5f + t * t * 7.5f;
                    float rightCurtainEdge = 90.0f - leftCurtainEdge;

                    if (px <= leftCurtainEdge || px >= rightCurtainEdge)
                    {
                        float distToCurtainHem = Mathf.Min(Mathf.Abs(px - leftCurtainEdge), Mathf.Abs(px - rightCurtainEdge));
                        float alphaCurtain = (distToCurtainHem < 1.2f) ? 0.36f : 0.24f;
                        col = Color.Lerp(col, curtainLilac, alphaCurtain);
                    }

                    col.a = 1.0f;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateWindowCookieTexture()
        {
            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Vector2 archCenter = new Vector2(45f, 45f);

            for (int y = 0; y < size; y++)
            {
                float py = (1.0f - (float)y / (size - 1)) * 130.0f;

                for (int x = 0; x < size; x++)
                {
                    float px = ((float)x / (size - 1)) * 90.0f;
                    float distArch = Vector2.Distance(new Vector2(px, py), archCenter);

                    bool insideGlass = (py < 45f)
                        ? (distArch <= 39.0f)
                        : (px >= 6.0f && px <= 84.0f && py <= 124.0f);

                    if (!insideGlass)
                    {
                        tex.SetPixel(x, y, Color.black);
                        continue;
                    }

                    // Sombra da divisoria em cruz 2x2 canonica (px=45, py=60)
                    bool isVert = Mathf.Abs(px - 45.0f) <= 1.8f;
                    bool isHoriz = Mathf.Abs(py - 60.0f) <= 1.8f;

                    if (isVert || isHoriz)
                    {
                        tex.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        float distCenter = Vector2.Distance(new Vector2(px, py), new Vector2(45f, 55f)) / 65f;
                        float intensity = Mathf.Clamp01(1.0f - distCenter * 0.45f);
                        tex.SetPixel(x, y, new Color(intensity, intensity, intensity, 1f));
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMoonlightFloorShadowTexture()
        {
            int width = 512;
            int height = 512;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            var moonColor = new Color(0.72f, 0.86f, 1.0f); // Azul claro luar
            var shadowColor = new Color(0.08f, 0.10f, 0.16f); // Sombra da esquadria

            float cx = width * 0.5f;

            for (int y = 0; y < height; y++)
            {
                float normY = (float)y / height; // 0 na frente (perto da camera), 1 no fundo (perto da parede)

                // Perspectiva trapezoidal: mais larga na frente, estreitando em direcao a janela no fundo
                float trapWidth = Mathf.Lerp(width * 0.88f, width * 0.50f, normY);
                float xMin = cx - trapWidth * 0.5f;
                float xMax = cx + trapWidth * 0.5f;

                // Suavidade e decaimento nas bordas externas do lago de luz
                float edgeY = Mathf.SmoothStep(0f, 0.12f, normY) * Mathf.SmoothStep(1f, 0.88f, normY);

                for (int x = 0; x < width; x++)
                {
                    if (x < xMin || x > xMax)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float normXInTrap = (x - xMin) / trapWidth; // 0..1 dentro do feixe
                    float edgeX = Mathf.SmoothStep(0f, 0.10f, normXInTrap) * Mathf.SmoothStep(1f, 0.90f, normXInTrap);
                    float poolAlpha = edgeX * edgeY * 0.70f;

                    // Sombras das travessas da esquadria projetadas no chao:
                    // 2 travessas verticais (dividindo em 3 colunas de luz)
                    bool isMullion1 = Mathf.Abs(normXInTrap - 0.35f) < 0.035f;
                    bool isMullion2 = Mathf.Abs(normXInTrap - 0.65f) < 0.035f;

                    // 3 travessas horizontais (dividindo em 4 fileiras de luz)
                    bool isTransom1 = Mathf.Abs(normY - 0.28f) < 0.025f;
                    bool isTransom2 = Mathf.Abs(normY - 0.55f) < 0.025f;
                    bool isTransom3 = Mathf.Abs(normY - 0.78f) < 0.025f;

                    // Arco superior na extremidade do feixe
                    bool isArchShadow = (normY > 0.85f) && (Mathf.Abs(normXInTrap - 0.5f) > 0.35f);

                    bool isShadow = isMullion1 || isMullion2 || isTransom1 || isTransom2 || isTransom3 || isArchShadow;

                    if (isShadow)
                    {
                        // Sombra escura das barras de madeira da janela sobre a luz
                        tex.SetPixel(x, y, new Color(shadowColor.r, shadowColor.g, shadowColor.b, poolAlpha * 0.25f));
                    }
                    else
                    {
                        // Luz brilhante passando pelos vidros retangulares da janela!
                        float centerBright = Mathf.Clamp01(1.0f - Mathf.Abs(normXInTrap - 0.5f) * 0.8f);
                        float finalAlpha = poolAlpha * (0.65f + centerBright * 0.35f);
                        tex.SetPixel(x, y, new Color(moonColor.r, moonColor.g, moonColor.b, finalAlpha));
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateDreamPortalTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            var goldBorder = new Color(0.88f, 0.72f, 0.38f);
            var cyanPortal = new Color(0.38f, 0.70f, 0.85f);
            var deepSpace = new Color(0.12f, 0.10f, 0.24f);

            float cx = size * 0.5f;
            float cy = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    float angle = Mathf.Atan2(y - cy, x - cx);

                    bool isGoldArch = (dist > 210 && dist < 245);

                    if (isGoldArch)
                    {
                        tex.SetPixel(x, y, goldBorder);
                    }
                    else if (dist <= 210)
                    {
                        float spiral = Mathf.Sin(angle * 4.0f + dist * 0.08f);
                        var c = Color.Lerp(deepSpace, cyanPortal, (spiral + 1f) * 0.5f);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateFalseDoorPosterTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            var frameWood = new Color(0.40f, 0.26f, 0.16f);
            var paintedDoor = new Color(0.36f, 0.48f, 0.62f); // Azul ardósia desbotado
            var posterPaper = new Color(0.92f, 0.88f, 0.78f); // Papel pergaminho antigo
            var goldKnob = new Color(0.82f, 0.68f, 0.35f);    // Maçaneta de latão antigo

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float wash = WatercolorWash(x, y);
                    bool isFrame = (x < 36 || x > size - 36 || y < 36 || y > size - 36);

                    if (isFrame)
                    {
                        tex.SetPixel(x, y, frameWood * wash);
                    }
                    else
                    {
                        bool isPaperBorder = (x < 48 || x > size - 48 || y < 48 || y > size - 48);
                        if (isPaperBorder)
                        {
                            tex.SetPixel(x, y, posterPaper * wash);
                        }
                        else
                        {
                            bool isKnob = Vector2.Distance(new Vector2(x, y), new Vector2(120f, 256f)) < 18f;
                            if (isKnob)
                            {
                                tex.SetPixel(x, y, goldKnob * wash);
                            }
                            else
                            {
                                int panelX = (x - 48) % 104;
                                int panelY = (y - 48) % 104;
                                bool isPanelBorder = (panelX < 8 || panelY < 8);
                                tex.SetPixel(x, y, (isPanelBorder ? paintedDoor * 0.80f : paintedDoor) * wash);
                            }
                        }
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static void ConfigurePostProcessingVolume()
        {
            var profilePath = "Assets/DefaultVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
            }

            // Bloom
            Bloom bloom;
            if (!profile.TryGet(out bloom))
            {
                bloom = profile.Add<Bloom>(true);
            }
            bloom.threshold.Override(0.85f);
            bloom.intensity.Override(1.25f);
            bloom.scatter.Override(0.68f);

            // Vignette
            Vignette vignette;
            if (!profile.TryGet(out vignette))
            {
                vignette = profile.Add<Vignette>(true);
            }
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.45f);
            vignette.color.Override(new Color(0.03f, 0.02f, 0.08f));

            // Color Adjustments
            ColorAdjustments colorAdj;
            if (!profile.TryGet(out colorAdj))
            {
                colorAdj = profile.Add<ColorAdjustments>(true);
            }
            colorAdj.postExposure.Override(0.15f);
            colorAdj.contrast.Override(14f);
            colorAdj.saturation.Override(12f);

            EditorUtility.SetDirty(profile);
        }
    }
}
