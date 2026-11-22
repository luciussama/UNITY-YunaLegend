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
using YunaLegend.Editor.ToyRoom;

namespace YunaLegend.Editor.Arena
{
    /// <summary>
    /// Constroi a cena 3D Painterly da Fase 3 (Arena Imaginaria) em Assets/Scenes/Arena_Gameplay.unity,
    /// integrando Yuna 3D + Espada, Nanda 3D azul, Trem/Robo/Dinossauro/Urso 3D em escala canonica,
    /// cenario 3D com moveis e bau aberto, corte Arco-Iris e os 10 efeitos sonoros.
    /// </summary>
    public static class ArenaSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Arena_Gameplay.unity";
        private const string GirlArtDir = "Assets/Art/Arena/Girl";
        private const string EnvArtDir = "Assets/Art/Arena/Environment";
        private const string AudioDir = "Assets/Audio/Arena";
        private const string ToyPropsDir = "Assets/Art/ToyRoom/Models/Props";
        private const string ArenaModelsDir = "Assets/Art/Arena/Models";

        private static bool pendingOpenAfterPlayExit;
        private static bool pendingPlayAfterPlayExit;

        [MenuItem("YunaLegend/Fase 3 - Arena Imaginária/Jogar (Play Direto)", false, 10)]
        public static void PlayArenaDirectly()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                pendingOpenAfterPlayExit = true;
                pendingPlayAfterPlayExit = true;
                EditorApplication.playModeStateChanged -= HandlePendingOpenAfterPlayExit;
                EditorApplication.playModeStateChanged += HandlePendingOpenAfterPlayExit;
                EditorApplication.isPlaying = false;
                return;
            }

            OpenAndOptionallyPlayArena(startPlayMode: true);
        }

        [MenuItem("YunaLegend/Fase 3 - Arena Imaginária/Abrir Cena", false, 20)]
        public static void OpenArenaScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                pendingOpenAfterPlayExit = true;
                pendingPlayAfterPlayExit = false;
                EditorApplication.playModeStateChanged -= HandlePendingOpenAfterPlayExit;
                EditorApplication.playModeStateChanged += HandlePendingOpenAfterPlayExit;
                EditorApplication.isPlaying = false;
                return;
            }

            OpenAndOptionallyPlayArena(startPlayMode: false);
        }

        private static void HandlePendingOpenAfterPlayExit(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode || !pendingOpenAfterPlayExit)
            {
                return;
            }

            bool shouldPlay = pendingPlayAfterPlayExit;
            pendingOpenAfterPlayExit = false;
            pendingPlayAfterPlayExit = false;
            EditorApplication.playModeStateChanged -= HandlePendingOpenAfterPlayExit;
            EditorApplication.delayCall += () => OpenAndOptionallyPlayArena(shouldPlay);
        }

        private static void OpenAndOptionallyPlayArena(bool startPlayMode)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.playModeStartScene = null;
            if (!File.Exists(ScenePath))
            {
                BuildArenaScene();
            }
            else
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            EditorSceneManager.playModeStartScene = null;
            if (startPlayMode)
            {
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("YunaLegend/Fase 3 - Arena Imaginária/Reconstruir Cena", false, 30)]
        public static void BuildArenaScene()
        {
            EditorSceneManager.playModeStartScene = null;
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();

            var camGo = new GameObject("Arena_Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = false;
            cam.fieldOfView = 34f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.06f, 0.08f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            camGo.transform.position = new Vector3(0f, 3.0f, -13.5f);
            camGo.transform.LookAt(new Vector3(0f, 1.45f, 0f));
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            var camCtrl = camGo.AddComponent<ArenaCameraController>();
            camCtrl.Configure(cam);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            BuildEnvironment3D();

            var catalogGo = new GameObject("Arena_SpriteCatalog");
            var catalog = catalogGo.AddComponent<ArenaSpriteCatalog>();
            var eyesSprite = LoadSingleSprite($"{GirlArtDir}/eyes-power-v1.png", new Vector2(0.5f, 0.5f), 100f);
            catalog.ConfigureGirl(null, null, null, null, null, null, null, eyesSprite);

            var playerView = BuildPlayer3D(-3.8f);
            var enemyView = BuildEnemies3D(3.2f);
            var nandaTransform = BuildNandaCompanion3D();

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
            CaptureArenaPreview(cam, playerView, enemyView);
            Debug.Log($"[ArenaSceneBuilder] Cena 3D da Arena salva em {ScenePath}");
        }

        private static void ConfigureLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.46f, 0.40f, 0.37f);
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.32f, 0.29f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.14f, 0.11f);

            var keyGo = new GameObject("Sun_KeyLight");
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1.0f, 0.93f, 0.82f);
            key.intensity = 0.92f;
            key.shadows = LightShadows.Soft;
            keyGo.transform.rotation = Quaternion.Euler(34f, -24f, 0f);

            var probeGo = new GameObject("Arena_ReflectionProbe");
            probeGo.transform.position = new Vector3(0f, 2.2f, -1.2f);
            var probe = probeGo.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            probe.size = new Vector3(24f, 12f, 16f);
            probe.intensity = 1.15f;
            probe.resolution = 128;

            var fillGo = new GameObject("Room_FillLight");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.72f, 0.82f, 0.96f);
            fill.intensity = 0.32f;
            fillGo.transform.rotation = Quaternion.Euler(18f, 155f, 0f);
        }

        private static void BuildEnvironment3D()
        {
            var envRoot = new GameObject("Arena_Environment_3D").transform;

            // Piso de madeira texturizado (T_DarkOak_Planks.png da Fase 2) em y = 0
            var floorTex = LoadTexture("Assets/Materials/T_DarkOak_Planks.png");
            CreateTexturedPlane("Floor_Wood", envRoot, new Vector3(0f, 0f, -3.5f), new Vector3(4.2f, 1f, 2.8f),
                new Color(0.76f, 0.70f, 0.62f), floorTex, new Vector2(14f, 9f));

            // Parede vitoriana e rodape atras da arena (z = 3.2m)
            var wallTex = LoadTexture($"{EnvArtDir}/wallpaper-v1.png");
            for (int i = -4; i <= 4; i++)
            {
                float x = i * 3.0f;
                CreateVerticalQuad($"Wall_{i + 4}", envRoot, new Vector3(x, 3.0f, 3.2f), new Vector3(3.05f, 6.0f, 1f),
                    new Color(0.64f, 0.58f, 0.56f), wallTex);
                CreateVerticalQuad($"Baseboard_{i + 4}", envRoot, new Vector3(x, 0.35f, 3.12f), new Vector3(3.05f, 0.72f, 1f),
                    new Color(0.66f, 0.56f, 0.48f), LoadTexture($"{EnvArtDir}/oak-trim-v1.png"));
            }

            // Janela lateral-direita iluminada e desobstruida (igual a janela direita de room-v1.png)
            PlaceSpriteCard("Window_Curtains", envRoot, $"{EnvArtDir}/window-curtains-v1.png",
                new Vector3(3.25f, 3.45f, 3.05f), Vector3.one * 1.95f, Color.white);

            // Quadros infantis emoldurados na parede acima do Bau de Brinquedos (igual a room-v1.png)
            CreateVerticalQuad("WallFrame_Left", envRoot, new Vector3(-4.85f, 3.65f, 3.08f), new Vector3(1.15f, 0.92f, 1f),
                Color.white, LoadTexture("Assets/Materials/T_WallDrawing_Castle.png"));
            CreateVerticalQuad("WallFrame_Oval", envRoot, new Vector3(-6.20f, 3.60f, 3.08f), new Vector3(0.68f, 0.85f, 1f),
                Color.white, LoadTexture("Assets/Materials/T_WallDrawing_Sun.png"));

            // Tapete oval no chao (y = 0.015m)
            var rugTex = LoadTexture($"{EnvArtDir}/oval-rug-v1.png");
            var rugGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rugGo.name = "Oval_Rug_3D";
            rugGo.transform.SetParent(envRoot, false);
            rugGo.transform.position = new Vector3(0f, 0.015f, 0f);
            rugGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rugGo.transform.localScale = new Vector3(8.2f, 4.2f, 1f);
            RemoveCollider(rugGo);
            rugGo.GetComponent<Renderer>().sharedMaterial = CreateUnlitTransparentMat("M_Arena_Rug", rugTex, Color.white);

            // Regressao 3: Ordem, silhueta e posicionamento dos objetos no fundo da Arena mantem igualdade com o Quarto de Brinquedos (room-v1.png + ToyRoomCatalog)
            // Da esquerda para a direita:
            // 1) OpenChest_3D (Bau/Caixa de Brinquedos Aberta): a esquerda (X = -5.30m, Z = 2.02m, 100% visivel e desobstruida!)
            // 2) Bookshelf_3D (Estante Alta de Livros e Brinquedos): ao centro-esquerda (X = -0.85m, Z = 2.28m, estante vertical alta de 3.25m com globo/casinha)
            // 3) Armchair_3D (Poltrona Vermelha Aconchegante): ao centro-direita sob a janela (X = +2.65m, Z = 2.15m)
            // 4) RockingHorse_3D (Cavalinho de Pau): a direita da poltrona (X = +4.65m, Z = 1.85m)
            // 5) Wardrobe_3D (Guarda-Roupa / Armario Alto): na extrema direita (X = +6.75m, Z = 2.25m)
            var openChestGo = BuildOpenToyChestWithOcclusionAndVfx(envRoot, new Vector3(-5.30f, 0f, 2.02f));
            _ = openChestGo;

            BuildTallVictorianBookshelf(envRoot, new Vector3(-0.85f, 0f, 2.28f));

            PlaceNormalizedModel("Armchair_3D", envRoot, $"{ToyPropsDir}/armchair.obj", $"{ToyPropsDir}/armchair_Albedo.png",
                new Vector3(2.65f, 0f, 2.15f), 1.55f, 158f, "M_Arena_Armchair");

            PlaceNormalizedModel("RockingHorse_3D", envRoot, $"{ArenaModelsDir}/rocking_horse_hunyuan.obj", $"{ArenaModelsDir}/T_RockingHorse_Albedo.png",
                new Vector3(4.65f, 0f, 1.85f), 1.38f, -142f, "M_Arena_HorseProp");

            var wardrobeGo = PlaceNormalizedModel("Wardrobe_3D", envRoot, $"{ToyPropsDir}/wardrobe.obj", $"{ToyPropsDir}/wardrobe_Albedo.png",
                new Vector3(6.75f, 0f, 2.25f), 1.65f, 180f, "M_Arena_Wardrobe");
            // Proporcao vertical de armario/estante lateral
            wardrobeGo.transform.localScale = new Vector3(0.88f, 1.55f, 0.95f);
        }

        private static GameObject BuildOpenToyChestWithOcclusionAndVfx(Transform envRoot, Vector3 chestPos)
        {
            var chestGo = PlaceNormalizedModel("OpenChest_3D", envRoot, $"{ToyPropsDir}/toy-chest-open.obj", $"{ToyPropsDir}/toy-chest-open_Albedo.png",
                chestPos, 1.82f, 166f, "M_Arena_OpenChest");

            // Clona a propria malha do bau mas mantendo apenas a metade dianteira (borda frontal do bau em Z < 2.02m)
            // posicionada em zLocal = -0.36m (frente para a camera), garantindo que o brinquedo derrotado (em Z = 2.02m)
            // passe fisicamente POR TRAS da parede frontal amarela/vermelha do bau ao mergulhar dentro da caixa!
            var baseMeshFilter = chestGo.transform.Find("Mesh")?.GetComponent<MeshFilter>();
            var baseMeshRend = chestGo.transform.Find("Mesh")?.GetComponent<MeshRenderer>();
            if (baseMeshFilter != null && baseMeshFilter.sharedMesh != null && baseMeshRend != null)
            {
                var frontLipMesh = CreateFrontHalfOcclusionMesh(baseMeshFilter.sharedMesh);
                var frontLipGo = new GameObject("Chest_FrontOcclusionLip");
                frontLipGo.transform.SetParent(chestGo.transform, false);
                frontLipGo.transform.localPosition = baseMeshFilter.transform.localPosition + new Vector3(0f, 0f, 0.28f);
                frontLipGo.transform.localRotation = baseMeshFilter.transform.localRotation;
                frontLipGo.transform.localScale = baseMeshFilter.transform.localScale;
                var mf = frontLipGo.AddComponent<MeshFilter>();
                mf.sharedMesh = frontLipMesh;
                var mr = frontLipGo.AddComponent<MeshRenderer>();
                mr.sharedMaterial = baseMeshRend.sharedMaterial;
            }

            // Luz dourada interna do bau que brilha forte quando um brinquedo derrotado e depositado de volta
            var glowGo = new GameObject("Chest_InteriorGlow");
            glowGo.transform.SetParent(chestGo.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.95f, -0.10f);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1.0f, 0.86f, 0.38f);
            glow.range = 4.2f;
            glow.intensity = 1.2f;

            // Particulas magicas douradas que saltam da boca do bau no momento em que o brinquedo cai dentro da caixa
            var burstGo = new GameObject("Chest_DepositSparkles");
            burstGo.transform.SetParent(chestGo.transform, false);
            burstGo.transform.localPosition = new Vector3(0f, 0.96f, 0f);
            var ps = burstGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.45f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 3.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
            main.startColor = new Color(1.0f, 0.92f, 0.48f, 0.95f);
            main.gravityModifier = 0.45f;
            var em = ps.emission;
            em.rateOverTime = 0f;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 36f;
            shape.radius = 0.45f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            burstGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = GetSoftSparkVfxMat();

            return chestGo;
        }

        private static Mesh CreateFrontHalfOcclusionMesh(Mesh source)
        {
            Bounds b = source.bounds;
            float maxHeightFrontBody = b.min.y + (b.size.y * 0.54f); // Apenas a caixa inferior (sem a tampa aberta superior)
            float midZ = b.center.z;

            Vector3[] verts = source.vertices;
            int[] tris = source.triangles;
            var keptTris = new System.Collections.Generic.List<int>(tris.Length / 2);

            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v0 = verts[tris[i]];
                Vector3 v1 = verts[tris[i + 1]];
                Vector3 v2 = verts[tris[i + 2]];
                float cy = (v0.y + v1.y + v2.y) / 3f;
                float cz = (v0.z + v1.z + v2.z) / 3f;
                if (cy <= maxHeightFrontBody && cz >= midZ - (b.size.z * 0.08f))
                {
                    keptTris.Add(tris[i]);
                    keptTris.Add(tris[i + 1]);
                    keptTris.Add(tris[i + 2]);
                }
            }

            var mesh = new Mesh
            {
                name = "ToyChest_FrontOcclusionLip",
                vertices = verts,
                normals = source.normals,
                uv = source.uv,
                triangles = keptTris.ToArray()
            };
            mesh.RecalculateBounds();
            return mesh;
        }

        private static GameObject BuildTallVictorianBookshelf(Transform envRoot, Vector3 shelfPos)
        {
            // Constroi a Estante Alta de Livros e Brinquedos (3.25m de altura com 4 prateleiras, livros coloridos,
            // casinha de madeira, globo e trenzinho decorativo, identica a estante central de room-v1.png!)
            var shelfRoot = new GameObject("Bookshelf_3D");
            shelfRoot.transform.SetParent(envRoot, false);
            shelfRoot.transform.localPosition = shelfPos;

            // Andar inferior (0..1.55m)
            var lowerUnit = PlaceNormalizedModel("Bookshelf_Lower", shelfRoot.transform, $"{ToyPropsDir}/bookshelf.obj", $"{ToyPropsDir}/bookshelf_Albedo.png",
                Vector3.zero, 1.58f, 180f, "M_Arena_Bookshelf");
            lowerUnit.transform.localScale = new Vector3(0.76f, 1.0f, 0.92f);

            // Andar superior (1.50..3.08m) formando a estante vertical alta de 4 andares
            var upperUnit = PlaceNormalizedModel("Bookshelf_Upper", shelfRoot.transform, $"{ToyPropsDir}/bookshelf.obj", $"{ToyPropsDir}/bookshelf_Albedo.png",
                new Vector3(0f, 1.50f, 0f), 1.58f, 180f, "M_Arena_Bookshelf");
            upperUnit.transform.localScale = new Vector3(0.76f, 1.0f, 0.92f);

            // Cornija vitoriana esculpida no topo (3.08..3.28m) com pomos laterais e folhagem verde
            var woodMat = CreateLitMat("M_Arena_BookshelfCrown", "Assets/Materials/T_Furniture_Wood_Painted.png", new Color(0.76f, 0.50f, 0.28f));
            var foliageMat = CreateLitMat("M_Arena_BookshelfVine", null, new Color(0.28f, 0.56f, 0.26f));
            CreatePrimitivePart("Crown_Pediment", PrimitiveType.Cube, shelfRoot.transform, new Vector3(0f, 3.12f, 0f), new Vector3(2.28f, 0.16f, 0.86f), woodMat);
            CreatePrimitivePart("Crown_Arch", PrimitiveType.Cylinder, shelfRoot.transform, new Vector3(0f, 3.22f, 0f), new Vector3(0.95f, 0.08f, 0.72f), woodMat);
            CreatePrimitivePart("Crown_FinialL", PrimitiveType.Sphere, shelfRoot.transform, new Vector3(-1.02f, 3.26f, -0.32f), Vector3.one * 0.16f, woodMat);
            CreatePrimitivePart("Crown_FinialR", PrimitiveType.Sphere, shelfRoot.transform, new Vector3(1.02f, 3.26f, -0.32f), Vector3.one * 0.16f, woodMat);

            // Folhagem cascateando no topo da estante alta (como na pintura room-v1.png)
            Vector3[] vineOffsets =
            {
                new Vector3(-0.25f, 3.30f, -0.28f),
                new Vector3(0.05f, 3.34f, -0.30f),
                new Vector3(0.35f, 3.28f, -0.28f),
                new Vector3(0.62f, 3.18f, -0.32f),
                new Vector3(0.78f, 2.95f, -0.34f)
            };
            for (int i = 0; i < vineOffsets.Length; i++)
            {
                CreatePrimitivePart($"Vine_Cluster_{i}", PrimitiveType.Sphere, shelfRoot.transform, vineOffsets[i], new Vector3(0.28f, 0.20f, 0.22f), foliageMat);
            }

            return shelfRoot;
        }

        private static ArenaActorView BuildPlayer3D(float startX)
        {
            var root = new GameObject("Yuna_Arena");
            root.transform.position = new Vector3(startX, 0f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * 1.43f; // 1.40m -> ~2.00m altura canonica da Arena
            visual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            ToyRoomYunaSculptor.BuildYuna3D(
                visual.transform,
                out Transform bodyRoot,
                out Transform legL,
                out Transform legR,
                out Transform armL,
                out Transform armR,
                out Transform ponytail);

            // Garante material dedicado M_Arena_Yuna3D.mat (imune a colisoes do contador M_Painterly_XX)
            var yunaMat = CreateLitMat("M_Arena_Yuna3D", "Assets/Art/ToyRoom/Models/Yuna_ToyRoom_Albedo.png", Color.white);
            foreach (var rend in visual.GetComponentsInChildren<Renderer>(true))
            {
                rend.sharedMaterial = yunaMat;
                if (rend is SkinnedMeshRenderer smr)
                {
                    smr.forceMatrixRecalculationPerRender = true;
                    smr.updateWhenOffscreen = true;
                }
            }

            Transform swordPivot = AttachToySwordTwoHanded(bodyRoot, armR, armL);

            var slashArc = CreateSlashArcObject(root.transform);
            var rainbowBeam = CreateRainbowBeamObject(root.transform);
            var impactPs = CreateImpactParticles(root.transform);
            CreateImpactFlashObject(root.transform);

            var view = root.AddComponent<ArenaActorView>();
            view.ConfigurePlayer3D(visual.transform, bodyRoot, legL, legR, armL, armR, swordPivot, ponytail, slashArc, rainbowBeam, impactPs);
            return view;
        }

        private static Transform AttachToySwordTwoHanded(Transform bodyRoot, Transform armRightPivot, Transform armLeftPivot)
        {
            // Empunhadura canonica com DUAS MAOS a frente do tronco (cotovelos dobrados, maos unidas no cabo)
            if (armRightPivot != null)
            {
                armRightPivot.localRotation = Quaternion.Euler(0f, -100f, 26f);
                var forearmR = armRightPivot.Find("Forearm_R");
                if (forearmR != null)
                {
                    forearmR.localRotation = Quaternion.Euler(0f, -40f, 32f);
                }
            }

            if (armLeftPivot != null)
            {
                armLeftPivot.localRotation = Quaternion.Euler(0f, 100f, -26f);
                var forearmL = armLeftPivot.Find("Forearm_L");
                if (forearmL != null)
                {
                    forearmL.localRotation = Quaternion.Euler(0f, 40f, -32f);
                }
            }

            Transform chest = armRightPivot != null && armRightPivot.parent != null ? armRightPivot.parent : bodyRoot;
            var swordPivot = new GameObject("TwoHandSword_Pivot").transform;
            swordPivot.SetParent(chest, false);
            swordPivot.localPosition = new Vector3(0f, -0.085f, 0.235f);
            swordPivot.localRotation = Quaternion.Euler(52f, 0f, 0f);

            var swordRoot = new GameObject("ToySword_3D").transform;
            swordRoot.SetParent(swordPivot, false);
            swordRoot.localPosition = new Vector3(0f, 0.08f, 0f);
            // Gira 90 graus em Y para exibir a face larga da lamina e a guarda azul alada voltadas para a camera
            swordRoot.localRotation = Quaternion.Euler(0f, 90f, 0f);
            swordRoot.localScale = new Vector3(0.95f, 0.68f, 1.05f);

            // Reutiliza a espada esculpida canonica da Fase 2 (lamina chanfrada + guarda alada + punho torneado)
            ToyRoomPropSculptor.BuildSculptedToySword(swordRoot);

            var bladeMat = CreateMetallicSwordMat(
                "M_Arena_SwordBlade",
                new Color(0.84f, 0.90f, 0.97f),
                new Color(0.18f, 0.24f, 0.36f),
                metallic: 0.96f,
                smoothness: 0.93f,
                reflectionStrength: 1.28f,
                glintIntensity: 1.85f,
                glintPhase: 0.28f);
            var guardMat = CreateMetallicSwordMat(
                "M_Arena_SwordGuard",
                new Color(0.42f, 0.78f, 0.96f),
                new Color(0.14f, 0.28f, 0.44f),
                metallic: 0.86f,
                smoothness: 0.85f,
                reflectionStrength: 1.05f,
                glintIntensity: 1.15f,
                glintPhase: 0.22f);
            var hiltMat = CreateLitMat("M_Arena_SwordHilt", null, new Color(0.76f, 0.28f, 0.26f));

            var bladeRend = swordRoot.Find("Sword_Blade")?.GetComponent<Renderer>();
            if (bladeRend != null) bladeRend.sharedMaterial = bladeMat;
            var guardRend = swordRoot.Find("Sword_Guard")?.GetComponent<Renderer>();
            if (guardRend != null) guardRend.sharedMaterial = guardMat;
            var gripRend = swordRoot.Find("Sword_Grip")?.GetComponent<Renderer>();
            if (gripRend != null) gripRend.sharedMaterial = hiltMat;

            // Alvos de IK centralizados exatamente no meio do cabo vermelho (no pivo das duas maos)
            var gripTargetR = new GameObject("GripTarget_R").transform;
            gripTargetR.SetParent(swordRoot, false);
            gripTargetR.localPosition = new Vector3(0f, -0.07f, 0.022f);

            var gripTargetL = new GameObject("GripTarget_L").transform;
            gripTargetL.SetParent(swordRoot, false);
            gripTargetL.localPosition = new Vector3(0f, -0.15f, -0.022f);

            // Alvo de IK na lateral da lamina para a pose de defesa (mao esquerda escorando a lamina horizontal)
            var guardTargetL = new GameObject("GuardTarget_L").transform;
            guardTargetL.SetParent(swordRoot, false);
            guardTargetL.localPosition = new Vector3(0f, 0.23f, -0.028f);

            CreateSwordGlintFlare(swordRoot);

            return swordPivot;
        }

        private static Material CreateMetallicSwordMat(
            string matName,
            Color baseColor,
            Color shadowTint,
            float metallic,
            float smoothness,
            float reflectionStrength,
            float glintIntensity,
            float glintPhase)
        {
            const string dir = "Assets/Art/Arena/Models";
            Directory.CreateDirectory(dir);
            string matPath = $"{dir}/{matName}.mat";
            var shader = Shader.Find("YunaLegend/Arena_MetallicSword")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_ShadowTint")) mat.SetColor("_ShadowTint", shadowTint);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_ReflectionStrength")) mat.SetFloat("_ReflectionStrength", reflectionStrength);
            if (mat.HasProperty("_GlintIntensity")) mat.SetFloat("_GlintIntensity", glintIntensity);
            if (mat.HasProperty("_GlintPhase")) mat.SetFloat("_GlintPhase", glintPhase);
            if (mat.HasProperty("_GlintSpeed")) mat.SetFloat("_GlintSpeed", 0.44f);
            if (mat.HasProperty("_GlintWidth")) mat.SetFloat("_GlintWidth", 0.12f);
            if (mat.HasProperty("_FresnelStrength")) mat.SetFloat("_FresnelStrength", 0.92f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CreateSwordGlintFlare(Transform swordRoot)
        {
            const string dir = "Assets/Art/Arena/Models";
            Directory.CreateDirectory(dir);
            string texPath = $"{dir}/T_Arena_GlintStar.png";
            if (!File.Exists(texPath))
            {
                const int size = 128;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float half = (size - 1) * 0.5f;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float nx = (x - half) / half;
                        float ny = (y - half) / half;
                        float r = Mathf.Sqrt(nx * nx + ny * ny);

                        // Nucleo luminoso + 4 pontas de reflexo especular (estrela de brilho metalico)
                        float core = Mathf.Exp(-r * r * 14f);
                        float hSpike = Mathf.Exp(-Mathf.Abs(ny) * 26f) * Mathf.Exp(-Mathf.Abs(nx) * 2.8f);
                        float vSpike = Mathf.Exp(-Mathf.Abs(nx) * 26f) * Mathf.Exp(-Mathf.Abs(ny) * 2.8f);
                        float d1 = (nx + ny) * 0.7071f;
                        float d2 = (nx - ny) * 0.7071f;
                        float diagSpike = (Mathf.Exp(-Mathf.Abs(d1) * 34f) * Mathf.Exp(-Mathf.Abs(d2) * 5.2f)
                            + Mathf.Exp(-Mathf.Abs(d2) * 34f) * Mathf.Exp(-Mathf.Abs(d1) * 5.2f)) * 0.45f;

                        float a = Mathf.Clamp01(core + hSpike * 0.95f + vSpike * 0.95f + diagSpike);
                        a *= Mathf.Clamp01(1f - r);
                        tex.SetPixel(x, y, new Color(0.95f, 0.99f, 1f, a));
                    }
                }

                tex.Apply();
                File.WriteAllBytes(texPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            }

            var glintTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            string matPath = $"{dir}/M_Arena_SwordGlint.mat";
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (glintTex != null) mat.SetTexture("_BaseMap", glintTex);
            mat.SetColor("_BaseColor", new Color(1f, 0.99f, 0.95f, 0.95f));
            // Modo transparente aditivo para brilho puro sem quadrado ao redor
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 2f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
            mat.renderQueue = 3100;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_BLENDMODE_ADD");
            EditorUtility.SetDirty(mat);

            var glint = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glint.name = "Sword_ReflectionGlint";
            glint.transform.SetParent(swordRoot, false);
            // Posicionado sobre o chanfro superior-medio da lamina (face frontal voltada para a camera)
            glint.transform.localPosition = new Vector3(-0.06f, 0.62f, 0.04f);
            glint.transform.localRotation = Quaternion.Euler(0f, 90f, 18f);
            glint.transform.localScale = Vector3.one * 0.34f;
            RemoveCollider(glint);
            var rend = glint.GetComponent<Renderer>();
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }

        private static Material CreateProceduralVfxMat(
            string matName,
            string texName,
            int texSize,
            Color tint,
            bool additive,
            System.Func<float, float, Color> pixelFunc)
        {
            Directory.CreateDirectory(ArenaModelsDir);
            string texPath = $"{ArenaModelsDir}/{texName}";
            var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
            for (int y = 0; y < texSize; y++)
            {
                float v = y / (float)(texSize - 1);
                for (int x = 0; x < texSize; x++)
                {
                    float u = x / (float)(texSize - 1);
                    tex.SetPixel(x, y, pixelFunc(u, v));
                }
            }

            tex.Apply();
            File.WriteAllBytes(texPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

            var loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            string matPath = $"{ArenaModelsDir}/{matName}.mat";
            var shader = Shader.Find("YunaLegend/Arena_VfxCard")
                ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (loadedTex != null) mat.SetTexture("_BaseMap", loadedTex);
            mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Intensity")) mat.SetFloat("_Intensity", additive ? 1.55f : 1.15f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            mat.renderQueue = 3100;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material GetSoftSparkVfxMat()
        {
            return CreateProceduralVfxMat(
                "M_Arena_SoftDot",
                "T_Arena_SoftDot.png",
                64,
                Color.white,
                additive: true,
                (u, v) =>
                {
                    float dx = (u - 0.5f) * 2f;
                    float dy = (v - 0.5f) * 2f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(Mathf.Exp(-r * r * 5.2f) * (1f - r));
                    return new Color(1f, 1f, 1f, a);
                });
        }

        private static Material GetSteamPuffVfxMat()
        {
            return CreateProceduralVfxMat(
                "M_Arena_SteamPuff",
                "T_Arena_SteamPuff.png",
                128,
                new Color(1f, 0.96f, 0.90f, 0.88f),
                additive: false,
                (u, v) =>
                {
                    float dx = (u - 0.5f) * 2f;
                    float dy = (v - 0.5f) * 2f;
                    // Multiplas nuvens sobrepostas para textura organica de vapor quente
                    float p1 = Mathf.Exp(-((dx * dx + dy * dy) * 2.6f));
                    float p2 = Mathf.Exp(-(((dx - 0.25f) * (dx - 0.25f) + (dy - 0.12f) * (dy - 0.12f)) * 4.5f)) * 0.7f;
                    float p3 = Mathf.Exp(-(((dx + 0.22f) * (dx + 0.22f) + (dy + 0.16f) * (dy + 0.16f)) * 4.8f)) * 0.65f;
                    float edge = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    float a = Mathf.Clamp01((p1 + p2 + p3) * edge * 1.15f);
                    Color col = Color.Lerp(new Color(0.95f, 0.86f, 0.76f, a), new Color(1f, 0.98f, 0.95f, a), Mathf.Clamp01(p1));
                    return col;
                });
        }

        private static float SmoothRange(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static GameObject CreateSlashArcObject(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Slash_Arc_3D";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(1.15f, 1.02f, -0.40f);
            go.transform.localScale = new Vector3(1.55f, 1.65f, 1f);
            RemoveCollider(go);

            var mat = CreateProceduralVfxMat(
                "M_Arena_SlashArc",
                "T_Arena_SlashStreak.png",
                128,
                new Color(1f, 0.99f, 0.96f, 1f),
                additive: true,
                (u, v) =>
                {
                    // Risco de espada afiado e elegante (nucleo branco-prata fino + halo azul-celeste + pontas afinadas)
                    float dx = (u - 0.5f) * 2f;
                    float dy = (v - 0.5f) * 2f;
                    // Curvatura sutil de corte de lamina
                    float curveOffset = (1f - dy * dy) * 0.14f;
                    float distFromLine = Mathf.Abs(dx - curveOffset);
                    float lengthTaper = Mathf.Clamp01(1f - dy * dy);
                    float width = Mathf.Lerp(0.02f, 0.18f, lengthTaper);
                    float core = Mathf.Exp(-(distFromLine * distFromLine) / (width * width * 0.18f)) * lengthTaper;
                    float glow = Mathf.Exp(-(distFromLine * distFromLine) / (width * width * 1.45f)) * lengthTaper;
                    float a = Mathf.Clamp01(glow * 1.1f);
                    Color c = Color.Lerp(new Color(0.42f, 0.86f, 1f, a), new Color(1f, 0.99f, 0.95f, a), Mathf.Clamp01(core * 1.5f));
                    c.a = a;
                    return c;
                });

            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            go.SetActive(false);
            return go;
        }

        private static GameObject CreateRainbowBeamObject(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Rainbow_Beam_3D";
            go.transform.SetParent(parent, false);
            RemoveCollider(go);

            Color[] rainbow =
            {
                new Color(1.00f, 0.39f, 0.49f),
                new Color(1.00f, 0.64f, 0.31f),
                new Color(1.00f, 0.91f, 0.47f),
                new Color(0.50f, 0.93f, 0.69f),
                new Color(0.45f, 0.86f, 1.00f),
                new Color(0.57f, 0.57f, 1.00f),
                new Color(0.89f, 0.60f, 1.00f)
            };

            var mat = CreateProceduralVfxMat(
                "M_Arena_RainbowBeam",
                "T_Arena_RainbowBeam.png",
                128,
                Color.white,
                additive: true,
                (u, v) =>
                {
                    float bandF = Mathf.Clamp(u * (rainbow.Length - 1), 0f, rainbow.Length - 1.001f);
                    int i0 = Mathf.FloorToInt(bandF);
                    int i1 = Mathf.Min(rainbow.Length - 1, i0 + 1);
                    Color c = Color.Lerp(rainbow[i0], rainbow[i1], bandF - i0);
                    float core = Mathf.Exp(-Mathf.Pow((u - 0.5f) * 3.8f, 2f)) * 0.55f;
                    c = Color.Lerp(c, Color.white, core);
                    c.a = 0.92f;
                    return c;
                });

            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            go.SetActive(false);
            return go;
        }

        private static ArenaActorView BuildEnemies3D(float startX)
        {
            var root = new GameObject("Enemy_Arena");
            root.transform.position = new Vector3(startX, 0f, 0.05f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

            var train = PlaceNormalizedModel("Enemy_Train", visual.transform, $"{ToyPropsDir}/train.obj", $"{ToyPropsDir}/train_Albedo.png",
                Vector3.zero, 2.25f, -90f, "M_Arena_Train");

            var robotMat = CreateLitMat("M_Arena_Robot", $"{ArenaModelsDir}/T_Robot_Albedo.png", Color.white);
            var dinoMat = CreateLitMat("M_Arena_DinoEnemy", $"{ArenaModelsDir}/T_Dinosaur_Albedo.png", Color.white);
            var bearMat = CreateLitMat("M_Arena_BearEnemy", $"{ArenaModelsDir}/T_Bear_Albedo.png", Color.white);

            var robot = ArenaEnemyRigBuilder.BuildRiggedEnemy(
                "Enemy_Robot", ArenaEnemyId.Robot, visual.transform, $"{ArenaModelsDir}/robot_hunyuan.obj", robotMat, 2.50f);
            var dino = ArenaEnemyRigBuilder.BuildRiggedEnemy(
                "Enemy_Dinosaur", ArenaEnemyId.Dinosaur, visual.transform, $"{ArenaModelsDir}/dinosaur_hunyuan.obj", dinoMat, 2.50f);
            var bear = ArenaEnemyRigBuilder.BuildRiggedEnemy(
                "Enemy_Bear", ArenaEnemyId.Bear, visual.transform, $"{ArenaModelsDir}/bear_hunyuan.obj", bearMat, 3.25f);

            robot.SetActive(false);
            dino.SetActive(false);
            bear.SetActive(false);

            var impactPs = CreateImpactParticles(root.transform);
            CreateImpactFlashObject(root.transform);
            var steamMat = GetSteamPuffVfxMat();
            var sparkMat = GetSoftSparkVfxMat();

            // 1) HOT STEAM (SteamBlast): rajada horizontal de nuvens de vapor quente + jato volumetrico
            var steamGo = new GameObject("SteamBlast_VFX");
            steamGo.transform.SetParent(root.transform, false);
            steamGo.transform.localPosition = new Vector3(-1.45f, 0.85f, -0.05f);
            steamGo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            var steamPs = steamGo.AddComponent<ParticleSystem>();
            var sMain = steamPs.main;
            sMain.playOnAwake = false;
            sMain.startLifetime = 0.52f;
            sMain.startSpeed = 6.8f;
            sMain.startSize = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
            sMain.startColor = new Color(1.0f, 0.94f, 0.84f, 0.88f);
            sMain.simulationSpace = ParticleSystemSimulationSpace.World;
            var sEm = steamPs.emission;
            sEm.rateOverTime = 55f;
            var sShape = steamPs.shape;
            sShape.shapeType = ParticleSystemShapeType.Cone;
            sShape.angle = 16f;
            sShape.radius = 0.22f;
            var sSize = steamPs.sizeOverLifetime;
            sSize.enabled = true;
            sSize.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.55f, 1f, 1.85f));
            steamGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = steamMat;

            var steamJet = GameObject.CreatePrimitive(PrimitiveType.Quad);
            steamJet.name = "SteamJet_3D";
            steamJet.transform.SetParent(root.transform, false);
            steamJet.transform.localPosition = new Vector3(-2.55f, 0.82f, -0.18f);
            steamJet.transform.localScale = new Vector3(2.85f, 1.15f, 1f);
            RemoveCollider(steamJet);
            var steamJetMat = CreateProceduralVfxMat(
                "M_Arena_SteamJet",
                "T_Arena_SteamJet.png",
                128,
                new Color(1.0f, 0.96f, 0.90f, 0.96f),
                additive: false,
                (u, v) =>
                {
                    // Nuvem densa de vapor fervente saindo da direita (caldeira u=1) para a esquerda (Yuna u=0)
                    float dy = (v - 0.5f) * 2f;
                    float spread = Mathf.Lerp(0.92f, 0.28f, u);
                    float jetCore = Mathf.Exp(-(dy * dy) / (spread * spread * 0.55f));
                    // Bolsao de nuvens arredondadas (Hot Steam estilizado)
                    float c1 = Mathf.Exp(-(Mathf.Pow((u - 0.25f) * 3.8f, 2f) + Mathf.Pow((v - 0.52f) * 3.4f, 2f)));
                    float c2 = Mathf.Exp(-(Mathf.Pow((u - 0.50f) * 4.2f, 2f) + Mathf.Pow((v - 0.44f) * 4.0f, 2f)));
                    float c3 = Mathf.Exp(-(Mathf.Pow((u - 0.72f) * 4.8f, 2f) + Mathf.Pow((v - 0.54f) * 4.6f, 2f)));
                    float c4 = Mathf.Exp(-(Mathf.Pow((u - 0.16f) * 4.5f, 2f) + Mathf.Pow((v - 0.36f) * 4.2f, 2f))) * 0.8f;
                    float clouds = Mathf.Clamp01(jetCore * 0.85f + (c1 + c2 + c3 + c4) * 0.65f);
                    float fadeLeft = SmoothRange(0.02f, 0.16f, u);
                    float fadeRight = SmoothRange(1.0f, 0.88f, u);
                    float a = Mathf.Clamp01(clouds * fadeLeft * fadeRight * 0.95f);
                    Color hotCore = Color.Lerp(new Color(1.0f, 0.78f, 0.58f, a), new Color(1.0f, 0.99f, 0.96f, a), Mathf.Clamp01(clouds * 0.85f + u * 0.25f));
                    return hotCore;
                });
            var jetRend = steamJet.GetComponent<Renderer>();
            jetRend.sharedMaterial = steamJetMat;
            jetRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            jetRend.receiveShadows = false;
            steamJet.SetActive(false);

            // 2) FRONT CHARGE (Atropelamento): faiscas nas rodas + farol frontal vermelho-ambar
            var sparksGo = new GameObject("ChargeSparks_VFX");
            sparksGo.transform.SetParent(root.transform, false);
            sparksGo.transform.localPosition = new Vector3(-0.45f, 0.22f, -0.08f);
            var sparksPs = sparksGo.AddComponent<ParticleSystem>();
            var spMain = sparksPs.main;
            spMain.playOnAwake = false;
            spMain.startLifetime = 0.35f;
            spMain.startSpeed = 4.6f;
            spMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            spMain.startColor = new Color(1.0f, 0.68f, 0.18f, 0.95f);
            spMain.gravityModifier = 0.65f;
            var spEm = sparksPs.emission;
            spEm.rateOverTime = 45f;
            var spShape = sparksPs.shape;
            spShape.shapeType = ParticleSystemShapeType.Cone;
            spShape.angle = 24f;
            spShape.radius = 0.15f;
            sparksGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = sparkMat;

            var headlightGo = new GameObject("Train_Headlight");
            headlightGo.transform.SetParent(root.transform, false);
            headlightGo.transform.localPosition = new Vector3(-1.65f, 1.05f, -0.25f);
            var headlight = headlightGo.AddComponent<Light>();
            headlight.type = LightType.Point;
            headlight.color = new Color(1.0f, 0.42f, 0.15f);
            headlight.range = 4.2f;
            headlight.intensity = 3.5f;
            headlight.enabled = false;

            // 3) LASER DO ROBO EM 2 ETAPAS:
            // Etapa 1 (Windup): Mira vermelha fina apontando diretamente para a Yuna + ponto de mira
            var laserSightMat = CreateProceduralVfxMat(
                "M_Arena_LaserSight",
                "T_Arena_LaserSight.png",
                64,
                new Color(1.0f, 0.12f, 0.14f, 0.95f),
                additive: true,
                (u, v) =>
                {
                    float dx = (u - 0.5f) * 2f;
                    float core = Mathf.Exp(-dx * dx * 8.0f);
                    Color c = Color.Lerp(new Color(1.0f, 0.08f, 0.10f, core), new Color(1.0f, 0.85f, 0.85f, core), core * 0.65f);
                    c.a = Mathf.Clamp01(core * 1.15f);
                    return c;
                });

            var laserSightGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laserSightGo.name = "LaserSight_3D";
            laserSightGo.transform.SetParent(root.transform, false);
            laserSightGo.transform.localScale = new Vector3(0.016f, 2.5f, 0.016f);
            RemoveCollider(laserSightGo);
            var sightRend = laserSightGo.GetComponent<Renderer>();
            sightRend.sharedMaterial = laserSightMat;
            sightRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sightRend.receiveShadows = false;
            laserSightGo.SetActive(false);

            var laserDotMat = CreateProceduralVfxMat(
                "M_Arena_LaserSightDot",
                "T_Arena_LaserSightDot.png",
                64,
                new Color(1.0f, 0.16f, 0.16f, 1f),
                additive: true,
                (u, v) =>
                {
                    float dx = (u - 0.5f) * 2f;
                    float dy = (v - 0.5f) * 2f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float core = Mathf.Exp(-r * r * 14f);
                    float ring = Mathf.Exp(-Mathf.Pow((r - 0.62f) * 14f, 2f)) * 0.85f;
                    float a = Mathf.Clamp01((core + ring) * Mathf.Clamp01(1f - r));
                    Color c = Color.Lerp(new Color(1.0f, 0.10f, 0.12f, a), Color.white, core * 0.85f);
                    c.a = a;
                    return c;
                });

            var laserDotGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            laserDotGo.name = "LaserSight_Dot";
            laserDotGo.transform.SetParent(root.transform, false);
            laserDotGo.transform.localScale = Vector3.one * 0.22f;
            RemoveCollider(laserDotGo);
            var dotRend = laserDotGo.GetComponent<Renderer>();
            dotRend.sharedMaterial = laserDotMat;
            dotRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dotRend.receiveShadows = false;
            laserDotGo.SetActive(false);

            // Etapa 2 (ActiveAttack): Disparo forte de plasma vermelho-dourado com nucleo branco incandescente
            var laserBeamMat = CreateProceduralVfxMat(
                "M_Arena_LaserBeam",
                "T_Arena_LaserBeam.png",
                128,
                new Color(1.0f, 0.35f, 0.20f, 1f),
                additive: true,
                (u, v) =>
                {
                    float dx = (u - 0.5f) * 2f;
                    float core = Mathf.Exp(-dx * dx * 12f);
                    float aura = Mathf.Exp(-dx * dx * 2.4f);
                    Color c = Color.Lerp(new Color(1.0f, 0.16f, 0.10f, aura), new Color(1.0f, 0.98f, 0.90f, aura), Mathf.Clamp01(core * 1.35f));
                    c.a = Mathf.Clamp01(aura * 1.15f);
                    return c;
                });

            var laserGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laserGo.name = "LaserBeam_3D";
            laserGo.transform.SetParent(root.transform, false);
            laserGo.transform.localScale = new Vector3(0.28f, 4.2f, 0.28f);
            RemoveCollider(laserGo);
            var beamRend = laserGo.GetComponent<Renderer>();
            beamRend.sharedMaterial = laserBeamMat;
            beamRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            beamRend.receiveShadows = false;
            laserGo.SetActive(false);

            var laserLightGo = new GameObject("Laser_PointLight");
            laserLightGo.transform.SetParent(root.transform, false);
            laserLightGo.transform.localPosition = new Vector3(-1.4f, 1.65f, -0.55f);
            var laserLight = laserLightGo.AddComponent<Light>();
            laserLight.type = LightType.Point;
            laserLight.color = new Color(1.0f, 0.25f, 0.15f);
            laserLight.range = 8.0f;
            laserLight.intensity = 0f;
            laserLight.enabled = false;

            // 4) ROCKET PUNCH (Robo — baixo/medio, para pular)
            var punchGo = new GameObject("RocketPunch_3D");
            punchGo.transform.SetParent(root.transform, false);
            var glove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glove.name = "Punch_Glove";
            glove.transform.SetParent(punchGo.transform, false);
            glove.transform.localScale = new Vector3(0.62f, 0.52f, 0.52f);
            RemoveCollider(glove);
            glove.GetComponent<Renderer>().sharedMaterial = CreateLitMat("M_Arena_RocketGlove", null, new Color(0.88f, 0.18f, 0.18f));
            var spring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spring.name = "Punch_Spring";
            spring.transform.SetParent(punchGo.transform, false);
            spring.transform.localPosition = new Vector3(-0.85f, 0f, 0f);
            spring.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            spring.transform.localScale = new Vector3(0.14f, 0.85f, 0.14f);
            RemoveCollider(spring);
            spring.GetComponent<Renderer>().sharedMaterial = CreateMetallicSwordMat(
                "M_Arena_RocketSpring",
                new Color(0.78f, 0.82f, 0.88f),
                new Color(0.22f, 0.26f, 0.32f),
                0.92f, 0.85f, 1.0f, 0.8f, 0.3f);
            punchGo.SetActive(false);

            var enrageGo = new GameObject("Bear_EnrageLight");
            enrageGo.transform.SetParent(root.transform, false);
            enrageGo.transform.localPosition = new Vector3(0f, 2.2f, -0.4f);
            var enrageLight = enrageGo.AddComponent<Light>();
            enrageLight.type = LightType.Point;
            enrageLight.color = new Color(1.0f, 0.32f, 0.18f);
            enrageLight.range = 4.5f;
            enrageLight.intensity = 2.8f;
            enrageLight.enabled = false;

            var view = root.AddComponent<ArenaActorView>();
            view.ConfigureEnemies3D(visual.transform, train, robot, dino, bear, impactPs, steamPs, laserGo, enrageLight);
            return view;
        }

        private static Transform BuildNandaCompanion3D()
        {
            var nandaGo = new GameObject("Nanda_ArenaGuide");
            nandaGo.transform.position = new Vector3(-4.9f, 2.15f, 0.45f);
            ToyRoomYunaSculptor.BuildNandaFairySculpted(nandaGo.transform);

            // Garante material dedicado M_Arena_Nanda3D.mat (imune a colisoes do contador M_Painterly_XX)
            var nandaMat = CreateLitMat("M_Arena_Nanda3D", "Assets/Art/ToyRoom/Models/fairy/T_Nanda_Albedo.png", Color.white);
            foreach (var rend in nandaGo.GetComponentsInChildren<Renderer>(true))
            {
                if (!(rend is ParticleSystemRenderer))
                {
                    rend.sharedMaterial = nandaMat;
                }
            }

            var body = nandaGo.transform.Find("Nanda_FairyBody");
            if (body != null)
            {
                body.localScale = Vector3.one * 0.52f;
            }

            var lightGo = new GameObject("Nanda_BlueLight");
            lightGo.transform.SetParent(nandaGo.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.36f, 0.86f, 1f);
            light.range = 6.5f;
            light.intensity = 2.2f;

            var dustGo = new GameObject("Nanda_WingMagicDust");
            dustGo.transform.SetParent(body != null ? body : nandaGo.transform, false);
            dustGo.transform.localPosition = new Vector3(0f, 0.08f, -0.06f);
            var ps = dustGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.1f;
            main.startSpeed = 0.08f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.032f);
            main.startColor = new Color(0.55f, 0.94f, 1.0f, 0.85f);
            main.gravityModifier = 0.22f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission;
            em.rateOverTime = 20f;
            dustGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = GetSoftSparkVfxMat();

            return nandaGo.transform;
        }

        private static GameObject PlaceNormalizedModel(
            string name,
            Transform parent,
            string objPath,
            string texPath,
            Vector3 worldPos,
            float targetHeight,
            float yawDeg,
            string matName)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = worldPos;
            go.transform.localRotation = Quaternion.Euler(0f, yawDeg, 0f);

            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            if (mesh == null)
            {
                AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
                mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            }

            if (mesh == null)
            {
                return go;
            }

            var meshChild = new GameObject("Mesh");
            meshChild.transform.SetParent(go.transform, false);
            Bounds b = mesh.bounds;
            float h = Mathf.Max(0.001f, b.size.y);
            float scale = targetHeight / h;
            meshChild.transform.localScale = Vector3.one * scale;
            meshChild.transform.localPosition = new Vector3(-b.center.x * scale, -b.min.y * scale, -b.center.z * scale);

            var filter = meshChild.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = meshChild.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateLitMat(matName, texPath, Color.white);
            return go;
        }

        private static Material CreateLitMat(string matName, string texPath, Color tint)
        {
            Directory.CreateDirectory(ArenaModelsDir);
            string matPath = $"{ArenaModelsDir}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var shader = Shader.Find("YunaLegend/ToyRoomPainterly3D") ?? Shader.Find("Universal Render Pipeline/Lit");
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (shader != null) mat.shader = shader;
            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_EmissionColor", Color.black);
            if (!string.IsNullOrEmpty(texPath))
            {
                var tex = LoadTexture(texPath);
                mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            }
            else
            {
                mat.SetTexture("_BaseMap", Texture2D.whiteTexture);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateUnlitTransparentMat(string matName, Texture2D tex, Color tint)
        {
            Directory.CreateDirectory(ArenaModelsDir);
            string matPath = $"{ArenaModelsDir}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (shader != null) mat.shader = shader;
            mat.SetFloat("_Surface", 1f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 2500;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetColor("_BaseColor", tint);
            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Texture2D LoadTexture(string path)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null && File.Exists(path))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            return tex;
        }

        private static ParticleSystem CreateImpactParticles(Transform parent)
        {
            var psGo = new GameObject("Impact_Particles");
            psGo.transform.SetParent(parent, false);
            psGo.transform.localPosition = new Vector3(0f, 1.0f, -0.1f);
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
            psGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = GetSoftSparkVfxMat();
            return ps;
        }

        private static void CreateImpactFlashObject(Transform parent)
        {
            var flashMat = CreateProceduralVfxMat(
                "M_Arena_ImpactBang",
                "T_Arena_ImpactBang.png",
                128,
                new Color(1f, 0.96f, 0.82f, 1f),
                additive: true,
                (u, v) =>
                {
                    // Estrela "BANG" de impacto de espada (nucleo branco quente + 8 raios explosivos + anel de choque)
                    float dx = (u - 0.5f) * 2f;
                    float dy = (v - 0.5f) * 2f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Atan2(dy, dx);

                    float core = Mathf.Exp(-r * r * 9.5f);
                    float rays = Mathf.Pow(Mathf.Abs(Mathf.Cos(ang * 4f)), 6f) * Mathf.Exp(-r * 2.1f);
                    float subRays = Mathf.Pow(Mathf.Abs(Mathf.Cos(ang * 4f + Mathf.PI * 0.25f)), 10f) * Mathf.Exp(-r * 3.0f) * 0.75f;
                    float ring = Mathf.Exp(-Mathf.Pow((r - 0.48f) * 14f, 2f)) * 0.65f;

                    float mask = Mathf.Clamp01((core + rays * 0.95f + subRays + ring) * Mathf.Clamp01(1f - r));
                    Color c = Color.Lerp(new Color(1f, 0.80f, 0.32f, mask), Color.white, Mathf.Clamp01(core * 1.4f + ring * 0.4f));
                    c.a = mask;
                    return c;
                });

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Impact_Flash_3D";
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = new Vector3(-1.15f, 1.08f, -0.55f);
            quad.transform.localScale = Vector3.one * 1.5f;
            RemoveCollider(quad);
            var rend = quad.GetComponent<Renderer>();
            rend.sharedMaterial = flashMat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            quad.SetActive(false);

            var lightGo = new GameObject("Impact_FlashLight");
            lightGo.transform.SetParent(parent, false);
            lightGo.transform.localPosition = new Vector3(-1.15f, 1.15f, -0.85f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.94f, 0.72f);
            light.range = 6.0f;
            light.intensity = 0f;
            light.enabled = false;
        }

        private static void CreateTexturedPlane(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, Texture2D tex, Vector2? tiling = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            RemoveCollider(go);
            var mat = CreateLitMat("M_Arena_" + name, null, color);
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", tiling ?? Vector2.one);
            }

            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateVerticalQuad(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, Texture2D tex)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            RemoveCollider(go);
            var mat = CreateLitMat("M_Arena_Wall_" + (tex != null ? tex.name : "Base"), null, color);
            if (tex != null) mat.SetTexture("_BaseMap", tex);
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreatePrimitivePart(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            RemoveCollider(go);
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void PlaceSpriteCard(string name, Transform parent, string assetPath, Vector3 pos, Vector3 scale, Color tint)
        {
            var sprite = LoadSingleSprite(assetPath, new Vector2(0.5f, 0.5f), 200f);
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
        }

        private static void RemoveCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
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

                if (importer.spritePivot != pivot || !Mathf.Approximately(importer.spritePixelsPerUnit, ppu))
                {
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = pivot;
                    settings.spritePixelsPerUnit = ppu;
                    importer.SetTextureSettings(settings);
                    dirty = true;
                }

                if (dirty) importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void CaptureArenaPreview(Camera cam, ArenaActorView playerView, ArenaActorView enemyView)
        {
            if (cam == null) return;
            cam.aspect = 16f / 9f;
            Directory.CreateDirectory("Logs");

            RenderToPng(cam, 1280, 720, "Logs/arena_3d_mvp.png");

            // Captura close-up da Yuna segurando a espada
            Vector3 prevPos = cam.transform.position;
            Quaternion prevRot = cam.transform.rotation;
            cam.transform.position = new Vector3(-2.75f, 1.35f, -3.1f);
            cam.transform.LookAt(new Vector3(-3.55f, 1.05f, 0f));
            RenderToPng(cam, 640, 640, "Logs/arena_3d_yuna_grip.png");
            cam.transform.SetPositionAndRotation(prevPos, prevRot);

            // Simula gameplay real da Arena para inspecao visual dos golpes (3x3 coreografia + ondas)
            if (playerView != null && enemyView != null)
            {
                CaptureChoreographySheetFrames(cam, playerView, enemyView);
                CaptureCombatGameplayFrames(cam, playerView, enemyView);
            }

            Debug.Log("ARENA_3D_MVP_CAPTURED Logs/arena_3d_mvp.png + Logs/arena_3d_yuna_grip.png + choreography & gameplay frames");
        }

        private static void CaptureChoreographySheetFrames(Camera cam, ArenaActorView playerView, ArenaActorView enemyView)
        {
            // Captura 3 fases-chave (Windup, Strike, Follow-through) de cada um dos 3 golpes do combo em enquadramento focado na Yuna
            var sim = new YunaLegend.Arena.Core.ArenaCombatSimulation(easyMode: false);
            for (int i = 0; i < 65; i++)
            {
                sim.TickFixed(default);
                playerView.UpdatePlayerVisual(sim, null, 0f, 1f / 60f);
                enemyView.UpdateEnemyVisual(sim, null, 1f / 60f);
            }

            Vector3 prevPos = cam.transform.position;
            Quaternion prevRot = cam.transform.rotation;
            float prevAspect = cam.aspect;
            cam.aspect = 1f;

            for (int combo = 0; combo < 3; combo++)
            {
                // Aguarda cooldown zerar e dispara o proximo golpe do combo
                while (sim.SwordCooldown > 0 || sim.SwordTick > 0)
                {
                    sim.TickFixed(default);
                    playerView.UpdatePlayerVisual(sim, null, 0f, 1f / 60f);
                    enemyView.UpdateEnemyVisual(sim, null, 1f / 60f);
                }

                sim.TickFixed(new YunaLegend.Arena.Core.ArenaInputState { AttackPressed = true });
                playerView.TriggerSlashArc();
                playerView.UpdatePlayerVisual(sim, null, 0f, 1f / 60f);
                enemyView.UpdateEnemyVisual(sim, null, 1f / 60f);

                Vector3 pWorld = playerView.transform.position;
                cam.transform.position = new Vector3(pWorld.x + 0.65f, 1.35f, -4.35f);
                cam.transform.LookAt(new Vector3(pWorld.x + 0.45f, 1.05f, 0f));

                // 29 frames = 0.48s: Windup peak (frame 8), Strike peak (frame 14), Follow-through (frame 20)
                for (int f = 1; f <= 26; f++)
                {
                    sim.TickFixed(default);
                    playerView.UpdatePlayerVisual(sim, null, 0f, 1f / 60f);
                    enemyView.UpdateEnemyVisual(sim, null, 1f / 60f);

                    if (f == 8) RenderToPng(cam, 512, 512, $"Logs/arena_choreo_c{combo + 1}_1_windup.png");
                    if (f == 14) RenderToPng(cam, 512, 512, $"Logs/arena_choreo_c{combo + 1}_2_strike.png");
                    if (f == 19) RenderToPng(cam, 512, 512, $"Logs/arena_choreo_c{combo + 1}_3_follow.png");
                }
            }

            // Captura pose de DEFESA (segurando para tras enquanto o Trem prepara ataque)
            int safety = 0;
            while (sim.EnemyState != YunaLegend.Arena.Core.ArenaEnemyState.Windup && safety++ < 260)
            {
                float dist = Mathf.Abs(sim.EnemyX - sim.PlayerX);
                var inp = new YunaLegend.Arena.Core.ArenaInputState { MoveX = dist > 290f ? 1f : 0f };
                sim.TickFixed(inp);
                playerView.UpdatePlayerVisual(sim, null, inp.MoveX, 1f / 60f);
                enemyView.UpdateEnemyVisual(sim, null, 1f / 60f);
            }

            sim.ForceEnemyWindupForTest();
            for (int g = 0; g < 18; g++)
            {
                var guardInp = new YunaLegend.Arena.Core.ArenaInputState { MoveX = -1f };
                sim.TickFixed(guardInp);
                playerView.UpdatePlayerVisual(sim, null, -1f, 1f / 60f);
                enemyView.UpdateEnemyVisual(sim, null, 1f / 60f);
            }

            Vector3 guardWorld = playerView.transform.position;
            cam.transform.position = new Vector3(guardWorld.x + 0.35f, 1.00f, -4.60f);
            cam.transform.LookAt(new Vector3(guardWorld.x + 0.15f, 0.70f, 0f));
            RenderToPng(cam, 512, 512, "Logs/arena_choreo_guard_pose.png");

            cam.aspect = prevAspect;
            cam.transform.SetPositionAndRotation(prevPos, prevRot);
        }

        private static void CaptureCombatGameplayFrames(Camera cam, ArenaActorView playerView, ArenaActorView enemyView)
        {
            var sim = new YunaLegend.Arena.Core.ArenaCombatSimulation(easyMode: false);
            var camCtrl = cam != null ? cam.GetComponent<ArenaCameraController>() : null;
            bool[] capturedCombo = new bool[3];
            bool[] capturedWave = new bool[4];
            bool capturedSteam = false;
            bool capturedImpactBang = false;
            bool capRobotWalk = false, capRobotPunch = false, capRobotHit = false;
            bool capRobotLaserAim = false, capRobotLaserFire = false;
            bool capDinoTell = false, capDinoBite = false, capDinoHit = false;
            bool capBearWalk = false, capBearHug = false, capBearHit = false;

            sim.OnSwordHit += valid =>
            {
                if (valid)
                {
                    enemyView.TriggerImpactFeedback(new Color(1f, 0.94f, 0.68f));
                    camCtrl?.AddShake(0.22f, 0.16f);
                }
            };

            // Joga a batalha completa das 4 ondas (Trem -> Robo -> Dinossauro -> Urso) capturando cada onda e cada golpe do combo
            for (int tick = 0; tick < 9500 && sim.Outcome == YunaLegend.Arena.Core.ArenaBattleOutcome.Ongoing; tick++)
            {
                var input = default(YunaLegend.Arena.Core.ArenaInputState);
                float dist = Mathf.Abs(sim.EnemyX - sim.PlayerX);
                float range = YunaLegend.Arena.Core.ArenaCatalog.GetSwordRange(sim.CurrentEnemySpec, sim.Tuning) - 18f;
                bool enemyThreat = sim.EnemyState == YunaLegend.Arena.Core.ArenaEnemyState.Windup
                    || sim.EnemyState == YunaLegend.Arena.Core.ArenaEnemyState.ActiveAttack;
                bool enemyOpen = sim.EnemyState == YunaLegend.Arena.Core.ArenaEnemyState.Idle
                    || sim.EnemyState == YunaLegend.Arena.Core.ArenaEnemyState.Approaching
                    || sim.EnemyState == YunaLegend.Arena.Core.ArenaEnemyState.Recovery;

                if (enemyThreat)
                {
                    input.MoveX = -sim.PlayerFacing; // Defesa direcional
                }
                else if (dist > range)
                {
                    input.MoveX = sim.PlayerFacing;
                }
                else if (enemyOpen && sim.SwordCooldown == 0 && sim.SwordTick == 0 && sim.BlockRecovery == 0)
                {
                    input.AttackPressed = true;
                    playerView.TriggerSlashArc();
                }

                sim.TickFixed(input);
                playerView.UpdatePlayerVisual(sim, null, input.MoveX, 1f / 60f);
                enemyView.UpdateEnemyVisual(sim, null, 1f / 60f);
                camCtrl?.UpdateFraming(sim, 1f / 60f);

                if (!capturedImpactBang && sim.EnemyState == YunaLegend.Arena.Core.ArenaEnemyState.HitReaction && sim.EnemyStateTick == 2)
                {
                    RenderToPng(cam, 1280, 720, "Logs/arena_gameplay_impact_bang.png");
                    capturedImpactBang = true;
                }

                // Capturas de comprovacao visual dos 3 chefes articulados (Passo, Golpe e Reacao de Dor)
                var eid = sim.CurrentEnemySpec.Id;
                if (eid == ArenaEnemyId.Robot)
                {
                    if (!capRobotWalk && sim.EnemyState == ArenaEnemyState.Approaching && sim.EnemyStateTick == 18)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_robot_walk.png");
                        capRobotWalk = true;
                    }
                    if (!capRobotPunch && sim.EnemyState == ArenaEnemyState.ActiveAttack && sim.ActiveAttackKind == ArenaAttackKind.RocketPunch && sim.EnemyStateTick == 8)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_robot_punch.png");
                        capRobotPunch = true;
                    }
                    if (!capRobotLaserAim && sim.EnemyState == ArenaEnemyState.Windup && sim.ActiveAttackKind == ArenaAttackKind.LaserBeam && sim.EnemyStateTick == 36)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_robot_laser_aim.png");
                        capRobotLaserAim = true;
                    }
                    if (!capRobotLaserFire && sim.EnemyState == ArenaEnemyState.ActiveAttack && sim.ActiveAttackKind == ArenaAttackKind.LaserBeam && sim.EnemyStateTick == 8)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_robot_laser_fire.png");
                        capRobotLaserFire = true;
                    }
                    if (!capRobotHit && sim.EnemyState == ArenaEnemyState.HitReaction && sim.EnemyStateTick == 7)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_robot_hit.png");
                        capRobotHit = true;
                    }
                }
                else if (eid == ArenaEnemyId.Dinosaur)
                {
                    if (!capDinoTell && sim.EnemyState == ArenaEnemyState.Windup && sim.EnemyStateTick == 26)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_dino_tail_tell.png");
                        capDinoTell = true;
                    }
                    if (!capDinoBite && sim.EnemyState == ArenaEnemyState.ActiveAttack && sim.EnemyStateTick == 8)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_dino_bite.png");
                        capDinoBite = true;
                    }
                    if (!capDinoHit && sim.EnemyState == ArenaEnemyState.HitReaction && sim.EnemyStateTick == 7)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_dino_hit.png");
                        capDinoHit = true;
                    }
                }
                else if (eid == ArenaEnemyId.Bear)
                {
                    if (!capBearWalk && sim.EnemyState == ArenaEnemyState.Approaching && sim.EnemyStateTick == 18)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_bear_walk.png");
                        capBearWalk = true;
                    }
                    if (!capBearHug && sim.EnemyState == ArenaEnemyState.ActiveAttack && sim.ActiveAttackKind == ArenaAttackKind.BearHug && sim.EnemyStateTick == 8)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_bear_hug.png");
                        capBearHug = true;
                    }
                    if (!capBearHit && sim.EnemyState == ArenaEnemyState.HitReaction && sim.EnemyStateTick == 7)
                    {
                        RenderToPng(cam, 1280, 720, "Logs/arena_boss_bear_hit.png");
                        capBearHit = true;
                    }
                }

                int wave = sim.WaveIndex;
                if (wave >= 0 && wave < 4 && !capturedWave[wave]
                    && sim.EnemyState != YunaLegend.Arena.Core.ArenaEnemyState.Spawning
                    && sim.EnemyState != YunaLegend.Arena.Core.ArenaEnemyState.Defeated
                    && dist <= range + 95f)
                {
                    RenderToPng(cam, 1280, 720, $"Logs/arena_wave{wave + 1}_{sim.CurrentEnemySpec.Id.ToString().ToLowerInvariant()}.png");
                    capturedWave[wave] = true;
                }

                if (sim.SwordTick == 9 && sim.SwordComboStep >= 0 && sim.SwordComboStep < 3 && !capturedCombo[sim.SwordComboStep])
                {
                    string[] comboNames = { "vertical", "horizontal", "thrust" };
                    RenderToPng(cam, 1280, 720, $"Logs/arena_combo_{sim.SwordComboStep + 1}_{comboNames[sim.SwordComboStep]}.png");
                    capturedCombo[sim.SwordComboStep] = true;
                }

                if (!capturedSteam
                    && sim.CurrentEnemySpec.Id == YunaLegend.Arena.Core.ArenaEnemyId.Train
                    && sim.EnemyState == YunaLegend.Arena.Core.ArenaEnemyState.ActiveAttack
                    && sim.ActiveAttackKind == YunaLegend.Arena.Core.ArenaAttackKind.SteamBlast
                    && sim.EnemyStateTick >= 6)
                {
                    RenderToPng(cam, 1280, 720, "Logs/arena_gameplay_hot_steam.png");
                    capturedSteam = true;
                }
            }
        }

        private static void RenderToPng(Camera cam, int width, int height, string path)
        {
            var rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
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

        [MenuItem("YunaLegend/Build/WebGL - Arena", false, 110)]
        public static void BuildArenaWebGL()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new System.InvalidOperationException("Instale WebGL Build Support para esta versao do Unity pelo Hub.");
            }

            BuildArenaScene();

            string output = "Build/ArenaWebGL";
            Directory.CreateDirectory(output);

            var previousCompression = PlayerSettings.WebGL.compressionFormat;
            var previousBackground = PlayerSettings.runInBackground;
            try
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.runInBackground = true;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.Development
                });

                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    throw new System.InvalidOperationException(
                        $"Build WebGL da Arena falhou: {report.summary.result}, {report.summary.totalErrors} erros.");
                }

                string htmlPath = Path.Combine(output, "index.html");
                if (File.Exists(htmlPath))
                {
                    string text = File.ReadAllText(htmlPath);
                    const string marker = "var canvas = document.querySelector(\"#unity-canvas\");";
                    const string patch =
                        "(function() {\n" +
                        "  const orig = HTMLCanvasElement.prototype.getContext;\n" +
                        "  HTMLCanvasElement.prototype.getContext = function(t, a) {\n" +
                        "    if (t === 'webgl' || t === 'webgl2') a = Object.assign({}, a, { preserveDrawingBuffer: true });\n" +
                        "    return orig.call(this, t, a);\n" +
                        "  };\n" +
                        "})();\n" +
                        "var canvas = document.querySelector(\"#unity-canvas\");";
                    if (!text.Contains("preserveDrawingBuffer: true") && text.Contains(marker))
                    {
                        text = text.Replace(marker, patch);
                        File.WriteAllText(htmlPath, text);
                    }
                }

                Debug.Log($"YUNA_ARENA_WEBGL_OK: {Path.GetFullPath(output)}");
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat = previousCompression;
                PlayerSettings.runInBackground = previousBackground;
            }
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
