using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using YunaLegend.Arena.Core;
using YunaLegend.Arena.Runtime;

namespace YunaLegend.Editor.Arena
{
    /// <summary>
    /// Constroi a cena da Fase 3 (Arena Imaginaria) em Assets/Scenes/Arena_Gameplay.unity,
    /// fatiando os spritesheets da Yuna, carregando os 144 quadros dos 4 inimigos,
    /// configurando o cenario heroico do quarto com o bau aberto e os 10 efeitos sonoros.
    /// </summary>
    public static class ArenaSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Arena_Gameplay.unity";
        private const string GirlArtDir = "Assets/Art/Arena/Girl";
        private const string EnvArtDir = "Assets/Art/Arena/Environment";
        private const string EnemyArtDir = "Assets/Art/Arena/Enemies";
        private const string AudioDir = "Assets/Audio/Arena";

        [MenuItem("YunaLegend/Fase 3 - Arena Imaginaria/Abrir Cena (Arena_Gameplay)", false, 10)]
        public static void OpenArenaScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[ArenaSceneBuilder] Saia do Play Mode antes de abrir a cena Arena_Gameplay.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.playModeStartScene = null;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        [MenuItem("YunaLegend/Fase 3 - Arena Imaginaria/Build Final Scene", false, 20)]
        public static void BuildArenaScene()
        {
            EditorSceneManager.playModeStartScene = null;
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Arena_Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.04f, 0.07f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            camGo.transform.position = new Vector3(0f, 2.0f, -10f);
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            var camCtrl = camGo.AddComponent<ArenaCameraController>();
            camCtrl.Configure(cam);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            BuildEnvironmentBackdrop();

            var catalogGo = new GameObject("Arena_SpriteCatalog");
            var catalog = catalogGo.AddComponent<ArenaSpriteCatalog>();
            PopulateSpriteCatalog(catalog);

            var playerView = BuildActor("Yuna_Arena", -3.8f, 0f, 10);
            var enemyView = BuildActor("Enemy_Arena", 3.2f, 0.05f, 9);
            var nandaTransform = BuildNandaCompanion();

            var hudGo = new GameObject("Arena_HUD");
            var hudCtrl = hudGo.AddComponent<ArenaHudController>();
            var caveatFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Caveat-Variable.ttf");
            hudCtrl.Configure(caveatFont, catalog.EyesPowerCutin);

            var audioGo = new GameObject("Arena_Audio");
            var audioCtrl = audioGo.AddComponent<ArenaAudioController>();
            ConfigureAudio(audioCtrl);

            var bootstrapGo = new GameObject("Arena_Bootstrap");
            var bootstrap = bootstrapGo.AddComponent<ArenaStageBootstrap>();
            bootstrap.Configure(catalog, playerView, enemyView, camCtrl, hudCtrl, audioCtrl, nandaTransform);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings();
            AssetDatabase.Refresh();
            CaptureArenaPreview(cam, catalog, playerView, enemyView);
            Debug.Log($"[ArenaSceneBuilder] Cena da Arena salva em {ScenePath}");
        }

        private static void CaptureArenaPreview(
            Camera cam,
            ArenaSpriteCatalog catalog,
            ArenaActorView playerView,
            ArenaActorView enemyView)
        {
            if (cam == null)
            {
                return;
            }

            if (playerView != null && catalog != null && catalog.GirlIdle != null && catalog.GirlIdle.Length > 0)
            {
                var sr = playerView.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.sprite = catalog.GirlIdle[0];
            }

            if (enemyView != null && catalog != null && catalog.TryGetEnemySet(ArenaEnemyId.Train, out var trainSet)
                && trainSet.Idle != null && trainSet.Idle.Length > 0)
            {
                var sr = enemyView.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.sprite = trainSet.Idle[0];
            }

            cam.aspect = 16f / 9f;
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);

            Directory.CreateDirectory("Logs");
            File.WriteAllBytes("Logs/arena_preview.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Debug.Log("ARENA_PREVIEW_CAPTURED Logs/arena_preview.png");
        }

        private static void BuildEnvironmentBackdrop()
        {
            var envRoot = new GameObject("Arena_Environment").transform;

            // Parede base continua acima de wallBase (+1.90m, correspondente a y=500 no legado)
            CreateColorQuad("Wall_Base", envRoot, new Vector3(0f, 5.1f, 5.2f), new Vector3(30f, 6.4f, 1f), new Color(0.56f, 0.48f, 0.44f));
            // Piso de madeira continuo de wallBase (+1.90m) ate o limite inferior (-3.60m)
            CreateColorQuad("Floor_Base", envRoot, new Vector3(0f, -0.85f, 3.60f), new Vector3(30f, 5.5f, 1f), new Color(0.36f, 0.22f, 0.14f));
            CreateColorQuad("Floor_HorizonShadow", envRoot, new Vector3(0f, 1.82f, 3.55f), new Vector3(30f, 0.24f, 1f), new Color(0.18f, 0.11f, 0.08f));

            for (int i = -4; i <= 4; i++)
            {
                float x = i * 2.55f;
                PlaceProp($"Wallpaper_{i + 4}", envRoot, $"{EnvArtDir}/wallpaper-v1.png", new Vector3(x, 1.90f, 4.8f), new Vector3(1.05f, 2.15f, 1f), -50, new Color(0.75f, 0.72f, 0.82f), bottomPivot: true);
                PlaceProp($"Oak_Trim_{i + 4}", envRoot, $"{EnvArtDir}/oak-trim-v1.png", new Vector3(x, 1.90f, 3.5f), new Vector3(1.05f, 0.42f, 1f), -40, new Color(0.88f, 0.82f, 0.78f), bottomPivot: true);
            }

            // Janela em arco centralizada na parede (y=110..429 no legado -> base em +2.60m)
            PlaceProp("Window_Curtains", envRoot, $"{EnvArtDir}/window-curtains-v1.png", new Vector3(0f, 2.60f, 4.2f), Vector3.one * 2.0f, -45, Color.white, bottomPivot: true);

            // Moveis apoiados no piso sem sobreposicao e com sombra de contato
            PlacePropWithShadow("Bookshelf", envRoot, $"{EnvArtDir}/bookshelf-v1.png", new Vector3(-5.6f, 1.45f, 3.1f), Vector3.one * 1.85f, -35, new Color(0.88f, 0.84f, 0.86f), 3.6f);
            PlacePropWithShadow("Rocking_Horse", envRoot, $"{EnvArtDir}/rocking-horse-v1.png", new Vector3(-2.3f, 1.10f, 2.7f), Vector3.one * 1.25f, -30, new Color(0.88f, 0.84f, 0.84f), 2.1f);
            PlacePropWithShadow("Oak_Cabinet", envRoot, $"{EnvArtDir}/oak-cabinet-v1.png", new Vector3(1.85f, 1.45f, 3.0f), Vector3.one * 1.50f, -34, new Color(0.88f, 0.84f, 0.86f), 2.3f);

            // Tapete oval no chao sob projecao vertical 0.7 (cobrindo a area central de combate)
            PlaceProp("Oval_Rug", envRoot, $"{EnvArtDir}/oval-rug-v1.png", new Vector3(0f, -0.35f, 2.2f), new Vector3(5.2f, 2.25f, 1f), -25, new Color(0.88f, 0.82f, 0.78f));

            // Bau aberto separado do armario, apoiado no chao com sombra de contato
            PlacePropWithShadow("Open_Chest", envRoot, $"{EnvArtDir}/chest-toys-open-v2.png", new Vector3(5.15f, 0.55f, 1.2f), Vector3.one * 0.78f, -10, Color.white, 2.6f);
        }

        private static void PlacePropWithShadow(
            string name,
            Transform parent,
            string assetPath,
            Vector3 pos,
            Vector3 scale,
            int sortingOrder,
            Color tint,
            float shadowWidth)
        {
            var shadowSprite = EnsureContactShadowSprite($"{EnvArtDir}/contact-shadow.png");
            if (shadowSprite != null)
            {
                var shadowGo = new GameObject(name + "_ContactShadow");
                shadowGo.transform.SetParent(parent, false);
                shadowGo.transform.position = new Vector3(pos.x, pos.y + 0.05f, pos.z + 0.05f);
                shadowGo.transform.localScale = new Vector3(shadowWidth, 0.45f, 1f);
                var sr = shadowGo.AddComponent<SpriteRenderer>();
                sr.sprite = shadowSprite;
                sr.color = new Color(0.08f, 0.04f, 0.02f, 0.55f);
                sr.sortingOrder = sortingOrder - 1;
            }

            PlaceProp(name, parent, assetPath, pos, scale, sortingOrder, tint, bottomPivot: true);
        }

        private static Sprite EnsureContactShadowSprite(string path)
        {
            if (!File.Exists(path))
            {
                const int w = 64;
                const int h = 32;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                for (int y = 0; y < h; y++)
                {
                    float ny = (y / (float)(h - 1)) * 2f - 1f;
                    for (int x = 0; x < w; x++)
                    {
                        float nx = (x / (float)(w - 1)) * 2f - 1f;
                        float d = Mathf.Sqrt(nx * nx + ny * ny);
                        float alpha = Mathf.Pow(Mathf.Clamp01(1f - d), 1.8f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }

                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            return LoadSingleSprite(path, new Vector2(0.5f, 0.5f), 64f);
        }

        private static void CreateColorQuad(string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }

            var renderer = go.GetComponent<MeshRenderer>();
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (renderer != null && unlit != null)
            {
                var mat = new Material(unlit);
                mat.SetColor("_BaseColor", color);
                renderer.sharedMaterial = mat;
            }
        }

        private static void PlaceProp(
            string name,
            Transform parent,
            string assetPath,
            Vector3 pos,
            Vector3 scale,
            int sortingOrder,
            Color tint,
            bool bottomPivot = false)
        {
            var sprite = LoadSingleSprite(assetPath, bottomPivot ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f), 200f);
            if (sprite == null)
            {
                return;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
            sr.sortingOrder = sortingOrder;
        }

        private static ArenaActorView BuildActor(string name, float startX, float z, int sortingOrder)
        {
            var root = new GameObject(name);
            root.transform.position = new Vector3(startX, 0f, z);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;

            var psGo = new GameObject("Impact_Particles");
            psGo.transform.SetParent(root.transform, false);
            psGo.transform.localPosition = new Vector3(0f, 0.9f, -0.1f);
            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 3.2f;
            main.startSize = 0.14f;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

            var view = root.AddComponent<ArenaActorView>();
            view.Configure(sr, visual.transform, ps, null);
            return view;
        }

        private static Transform BuildNandaCompanion()
        {
            var nandaGo = new GameObject("Nanda_ArenaGuide");
            nandaGo.transform.position = new Vector3(-4.8f, 2.0f, 0.25f);

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Illustrated25D/Nanda/Flight/Runtime/Frames/NandaFlight0.asset");
            if (sprite != null)
            {
                var visual = new GameObject("Nanda_Sprite");
                visual.transform.SetParent(nandaGo.transform, false);
                visual.transform.localScale = Vector3.one * 1.4f;
                var sr = visual.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 11;
            }

            var lightGo = new GameObject("Nanda_BlueLight");
            lightGo.transform.SetParent(nandaGo.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.36f, 0.86f, 1f);
            light.range = 6f;
            light.intensity = 2.0f;

            return nandaGo.transform;
        }

        private static void PopulateSpriteCatalog(ArenaSpriteCatalog catalog)
        {
            // Menina na Arena tem ARENA_PLAYER_HEIGHT = 200 -> 2.00m.
            // As celulas possuem ~260px de altura visivel -> ~130 PPU para 2.00m, com pivo exato na sola do sapato.
            var baseSprites = LoadGridSheet($"{GirlArtDir}/arena-base.png", 4, 3, 130f);
            var actionSprites = LoadGridSheet($"{GirlArtDir}/arena-actions.png", 4, 3, 130f);
            var crouchSprites = LoadGridSheet($"{GirlArtDir}/arena-crouch.png", 2, 2, 130f);
            var guardSprites = LoadGridSheet($"{GirlArtDir}/arena-defense.png", 3, 2, 130f);
            var damageSprites = LoadGridSheet($"{GirlArtDir}/arena-damage.png", 3, 2, 130f);
            var eyesSprite = LoadSingleSprite($"{GirlArtDir}/eyes-power-v1.png", new Vector2(0.5f, 0.5f), 100f);

            var idle = SliceRange(baseSprites, 0, 4);
            var run = SliceRange(baseSprites, 4, 4);
            var jump = SliceRange(actionSprites, 4, 4);
            var attack = SliceRange(actionSprites, 0, 4);

            catalog.ConfigureGirl(idle, run, jump, crouchSprites, attack, guardSprites, damageSprites, eyesSprite);

            // PPU calibrado pela altura canonica de cada inimigo em configuracao-atual.json:
            // Trem=2.25m (139px -> 61.8 PPU), Robo=2.50m (236px -> 94.4 PPU),
            // Dinossauro=2.50m (205px -> 82.0 PPU), Urso=3.25m (177px -> 54.5 PPU).
            var enemySets = new[]
            {
                LoadEnemySet(ArenaEnemyId.Train, "train", "steam-blast", "front-charge", null, 61.8f),
                LoadEnemySet(ArenaEnemyId.Robot, "robot", "rocket-punch", "laser-beam", null, 94.4f),
                LoadEnemySet(ArenaEnemyId.Dinosaur, "dinosaur", "bite", "rush", null, 82.0f),
                LoadEnemySet(ArenaEnemyId.Bear, "bear", "bear-hug", "belly-charge", "toy-leap", 54.5f)
            };
            catalog.ConfigureEnemies(enemySets);
        }

        private static ArenaEnemySpriteSet LoadEnemySet(
            ArenaEnemyId id,
            string folder,
            string primaryAttackDir,
            string secondaryAttackDir,
            string tertiaryAttackDir,
            float ppu)
        {
            string root = $"{EnemyArtDir}/{folder}";
            return new ArenaEnemySpriteSet
            {
                EnemyId = id,
                Idle = LoadFolderFrames($"{root}/idle", ppu),
                Walk = LoadFolderFrames($"{root}/walk", ppu),
                PrimaryAttack = LoadFolderFrames($"{root}/{primaryAttackDir}", ppu),
                SecondaryAttack = LoadFolderFrames($"{root}/{secondaryAttackDir}", ppu),
                TertiaryAttack = string.IsNullOrEmpty(tertiaryAttackDir) ? null : LoadFolderFrames($"{root}/{tertiaryAttackDir}", ppu),
                Enrage = Directory.Exists($"{root}/enrage") ? LoadFolderFrames($"{root}/enrage", ppu) : null,
                Hit = LoadFolderFrames($"{root}/hit", ppu),
                Death = LoadFolderFrames($"{root}/death", ppu)
            };
        }

        private static Sprite[] LoadFolderFrames(string dir, float ppu)
        {
            if (!Directory.Exists(dir))
            {
                return null;
            }

            string[] files = Directory.GetFiles(dir, "*.png").OrderBy(f => f).ToArray();
            var list = new List<Sprite>(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                string assetPath = files[i].Replace('\\', '/');
                float pivotY = DetectOpaqueBottomPivotY(assetPath, 0, 0, 0, 0);
                var sprite = LoadSingleSprite(assetPath, new Vector2(0.5f, pivotY), ppu);
                if (sprite != null)
                {
                    list.Add(sprite);
                }
            }

            return list.ToArray();
        }

        private static float DetectOpaqueBottomPivotY(string path, int rx, int ry, int rw, int rh)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && (!importer.isReadable || importer.npotScale != TextureImporterNPOTScale.None))
            {
                importer.isReadable = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                return 0f;
            }

            int x0 = rx;
            int y0 = ry;
            int w = rw > 0 ? rw : tex.width;
            int h = rh > 0 ? rh : tex.height;
            Color32[] pixels = tex.GetPixels32();
            int texW = tex.width;

            for (int row = 0; row < h; row++)
            {
                int py = y0 + row;
                for (int col = 0; col < w; col++)
                {
                    int px = x0 + col;
                    if (pixels[py * texW + px].a > 24)
                    {
                        return Mathf.Clamp01(row / (float)h);
                    }
                }
            }

            return 0f;
        }

        private static Sprite LoadSingleSprite(string path, Vector2 pivot, float ppu)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }

                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    importer.npotScale = TextureImporterNPOTScale.None;
                    dirty = true;
                }

                if (importer.spritePivot != pivot || !Mathf.Approximately(importer.spritePixelsToUnits, ppu))
                {
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = pivot;
                    settings.spritePixelsPerUnit = ppu;
                    importer.SetTextureSettings(settings);
                    dirty = true;
                }

                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite[] LoadGridSheet(string path, int cols, int rows, float ppu)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool preDirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    preDirty = true;
                }

                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    importer.npotScale = TextureImporterNPOTScale.None;
                    preDirty = true;
                }

                if (!importer.isReadable)
                {
                    importer.isReadable = true;
                    preDirty = true;
                }

                if (preDirty)
                {
                    importer.SaveAndReimport();
                }

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    int cellW = tex.width / cols;
                    int cellH = tex.height / rows;
                    var metas = new List<SpriteMetaData>(cols * rows);
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            int rx = c * cellW;
                            int ry = tex.height - (r + 1) * cellH;
                            float pivotY = DetectOpaqueBottomPivotY(path, rx, ry, cellW, cellH);
                            metas.Add(new SpriteMetaData
                            {
                                name = $"{Path.GetFileNameWithoutExtension(path)}_{r * cols + c:D2}",
                                rect = new Rect(rx, ry, cellW, cellH),
                                alignment = (int)SpriteAlignment.Custom,
                                pivot = new Vector2(0.5f, pivotY)
                            });
                        }
                    }

                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    importer.spritePixelsToUnits = ppu;
#pragma warning disable CS0618
                    importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();
        }

        private static Sprite[] SliceRange(Sprite[] source, int start, int count)
        {
            if (source == null || source.Length == 0)
            {
                return null;
            }

            return source.Skip(start).Take(count).ToArray();
        }

        private static void ConfigureAudio(ArenaAudioController audioCtrl)
        {
            audioCtrl.Configure(
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ToyRoom/Circular Dissonance (1).mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/girl-attack.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/girl-defence.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/girl-special-attack.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/train-horn.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/train-steam-attack.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/robot-noises.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/robot-laser-attack.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/robot-punch.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/dinossaur-attack.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/bear-attack.mp3"));
        }

        private static void RegisterSceneInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Any(s => s.path == ScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
