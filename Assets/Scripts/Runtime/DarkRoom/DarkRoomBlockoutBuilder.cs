using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    [ExecuteAlways]
    public partial class DarkRoomBlockoutBuilder : MonoBehaviour
    {
        [Header("Configuracao de Escala")]
        [Tooltip("Fator de conversao entre as coordenadas de design 2D do roteiro e unidades 3D da Unity")]
        [SerializeField] private float coordinateScale = 0.02f;

        [Header("Materiais Base e Estilizados")]
        [SerializeField] private Material defaultPlatformMaterial;
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallpaperMaterial;
        [SerializeField] private Material wardrobeMaterial;
        [SerializeField] private Material windowMaterial;
        [SerializeField] private Material teddyPlushMaterial;
        [SerializeField] private Material[] platformArtMaterials;
        [SerializeField] private Material toyBlockRedMaterial;
        [SerializeField] private Material toyBlockYellowMaterial;
        [SerializeField] private Material toyBlockBlueMaterial;
        [SerializeField] private Material toyBlockGreenMaterial;
        [SerializeField] private Material toyBlockAbcMaterial;
        [SerializeField] private Material circusDrumMaterial;
        [SerializeField] private Material royalVelvetMaterial;
        [SerializeField] private Material musicBoxMaterial;
        [SerializeField] private Material brassGoldMaterial;
        [SerializeField] private Material dresserOakMaterial;
        [SerializeField] private Material bookBlueMaterial;
        [SerializeField] private Material bookBurgundyMaterial;
        [SerializeField] private Material bookGreenMaterial;
        [SerializeField] private Material bookPagesMaterial;
        [SerializeField] private Material castleBlocksMaterial;
        [SerializeField] private Material castleTurretMaterial;
        [SerializeField] private Material trainTrackMaterial;
        [SerializeField] private Material falseDoorPosterMaterial;
        [SerializeField] private Material dreamPortalMaterial;
        [SerializeField] private Material moonbeamShaftMaterial;
        [SerializeField] private Material ambientDustMaterial;
        [SerializeField] private Material moonlightFloorShadowMaterial;
        [SerializeField] private Material baseboardMaterial;
        [SerializeField] private Material buntingMaterial;
        [SerializeField] private Material mandalaRugMaterial;
        [SerializeField] private Material stripedRugMaterial;
        [SerializeField] private Material floorPropRedMaterial;
        [SerializeField] private Material floorPropYellowMaterial;
        [SerializeField] private Material floorPropBlueMaterial;
        [SerializeField] private Material floorPropGreenMaterial;
        [SerializeField] private Material floorPropCreamMaterial;
        [SerializeField] private Material floorPropBurgundyMaterial;
        [SerializeField] private Material floorPropPlushMaterial;

        [Header("Estrutura Gerada (3 Etapas CanÃ´nicas)")]
        [SerializeField] private Transform stage1Root;
        [SerializeField] private Transform stage2Root;
        [SerializeField] private Transform stage3Root;
        [SerializeField] private Transform floorRoot;
        [SerializeField] private Transform sceneryRoot;

        [SerializeField] private AudioClip furnitureCrackSfx;
        public void ConfigureFurnitureCrack(AudioClip clip) => furnitureCrackSfx=clip;
        private DarkRoomPlatformMarker[] temporalPlatforms;

        public float CoordinateScale => coordinateScale;

        public void ShowTemporalSection(DarkRoomPhase phase, int platformIndex = -1, bool falling = false)
        {
            if (temporalPlatforms == null)
                temporalPlatforms = GetComponentsInChildren<DarkRoomPlatformMarker>(true);

            // Keep the castle only while it is still the takeoff surface for the escape.
            foreach (var marker in temporalPlatforms)
            {
                if (marker == null) continue;
                int index = marker.spec.index;
                bool visible = !falling && (phase == DarkRoomPhase.Phase1Ascent
                    ? index <= 9
                    : phase == DarkRoomPhase.Phase2Escape
                        ? (index >= 10 && index <= 21) || (index == 9 && platformIndex < 10)
                        : index >= 22);
                marker.gameObject.SetActive(visible);
            }
        }

        public void ConfigureMaterials(
            Material floor, Material wallpaper, Material wardrobe, Material window,
            Material teddyPlush, Material toyRed, Material toyYellow, Material toyBlue, Material toyGreen,
            Material toyAbc, Material drum, Material velvet, Material musicBox, Material brassGold,
            Material dresser, Material bookBlue, Material bookBurgundy, Material bookGreen, Material bookPages,
            Material castleBlocks, Material castleTurret, Material trainTrack, Material falseDoorPoster, Material dreamPortal,
            Material moonbeamShaft = null, Material ambientDust = null, Material floorShadow = null, Material baseboard = null, Material bunting = null,
            Material mandalaRug = null, Material stripedRug = null,
            Material floorPropRed = null, Material floorPropYellow = null, Material floorPropBlue = null,
            Material floorPropGreen = null, Material floorPropCream = null, Material floorPropBurgundy = null, Material floorPropPlush = null,
            Material[] platformArt = null)
        {
            floorMaterial = floor;
            wallpaperMaterial = wallpaper;
            wardrobeMaterial = wardrobe;
            windowMaterial = window;
            teddyPlushMaterial = teddyPlush;
            toyBlockRedMaterial = toyRed;
            toyBlockYellowMaterial = toyYellow;
            toyBlockBlueMaterial = toyBlue;
            toyBlockGreenMaterial = toyGreen;
            toyBlockAbcMaterial = toyAbc;
            circusDrumMaterial = drum;
            royalVelvetMaterial = velvet;
            musicBoxMaterial = musicBox;
            brassGoldMaterial = brassGold;
            dresserOakMaterial = dresser;
            bookBlueMaterial = bookBlue;
            bookBurgundyMaterial = bookBurgundy;
            bookGreenMaterial = bookGreen;
            bookPagesMaterial = bookPages;
            castleBlocksMaterial = castleBlocks;
            castleTurretMaterial = castleTurret;
            trainTrackMaterial = trainTrack;
            falseDoorPosterMaterial = falseDoorPoster;
            dreamPortalMaterial = dreamPortal;
            moonbeamShaftMaterial = moonbeamShaft;
            ambientDustMaterial = ambientDust;
            moonlightFloorShadowMaterial = floorShadow;
            baseboardMaterial = baseboard;
            buntingMaterial = bunting;
            mandalaRugMaterial = mandalaRug;
            stripedRugMaterial = stripedRug;
            floorPropRedMaterial = floorPropRed;
            floorPropYellowMaterial = floorPropYellow;
            floorPropBlueMaterial = floorPropBlue;
            floorPropGreenMaterial = floorPropGreen;
            floorPropCreamMaterial = floorPropCream;
            floorPropBurgundyMaterial = floorPropBurgundy;
            floorPropPlushMaterial = floorPropPlush;
            platformArtMaterials = platformArt;
            defaultPlatformMaterial = dresserOakMaterial;
        }

        [ContextMenu("Construir Blockout Completo")]
        public void BuildBlockout()
        {
            ClearExisting();
            EnsureRoots();

            BuildFloor();
            BuildBackgroundScenery();

            // Etapa 1: Subida calma ate o Castelinho de Blocos (0 a 9)
            var stage1 = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms();
            BuildPlatformList(stage1, stage1Root, "Etapa1_Castelo");

            // Etapa 2: A Fuga Acelerada com pulos crescentes ate o Poster na parede (10 a 21)
            var lastStage1 = stage1[stage1.Count - 1];
            var stage2Origin=DarkRoomThreeStageCircuitGenerator.Stage2Origin(lastStage1);
            var stage2=DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms(stage2Origin.x,stage2Origin.y);
            BuildPlatformList(stage2, stage2Root, "Etapa2_Fuga");

            // Etapa 3: A Subida Caotica Reversa (22 a 37) da direita para a esquerda apos a queda cinematica
            var lastStage2 = stage2[stage2.Count - 1];
            float posterWallX = lastStage2.x;
            var stage3 = DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms(posterWallX, DarkRoomReturnLayout.FloorY);
            BuildPlatformList(stage3, stage3Root, "Etapa3_Caotica");

            temporalPlatforms = null;
            ShowTemporalSection(DarkRoomPhase.Phase1Ascent);

            Debug.Log($"[DarkRoomBlockoutBuilder] Circuito CanÃ´nico em 3 Etapas gerado com sucesso! " +
                      $"Etapa 1: {stage1.Count} | Etapa 2: {stage2.Count} | Etapa 3: {stage3.Count} plataformas.");
        }

        [ContextMenu("Limpar Blockout")]
        public void ClearExisting()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void EnsureRoots()
        {
            floorRoot = CreateGroup("00_Piso_Madeira_Escura");
            stage1Root = CreateGroup("01_Etapa1_SubidaAoCastelo");
            stage2Root = CreateGroup("02_Etapa2_AFugaAcelerada");
            stage3Root = CreateGroup("03_Etapa3_SubidaCaoticaReversa");
            sceneryRoot = CreateGroup("04_Cenografia_Monumental_Quarto");
        }

        private Transform CreateGroup(string groupName)
        {
            var go = new GameObject(groupName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void BuildFloor()
        {
            // Chao escuro da sala: qualquer queda aqui e Game Over imediato (exceto na queda cinematica)
            float thickness = 1.0f;
            float worldY = DarkRoomPlatformGeometry.FloorSurfaceY - thickness * 0.5f;
            float worldWidth = 170.0f;
            float worldDepth = 12.0f;

            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorGo.name = "Chao_Escuro_Sala";
            floorGo.transform.SetParent(floorRoot, false);
            floorGo.transform.position = new Vector3(60.0f, worldY, 0f);
            floorGo.transform.localScale = new Vector3(worldWidth, thickness, worldDepth);

            var floorRenderer = floorGo.GetComponent<MeshRenderer>();
            if (floorRenderer != null && floorMaterial != null)
            {
                floorRenderer.sharedMaterial = floorMaterial;
            }

            // Colisor de chao
            var col = floorGo.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = false;
        }

        private static Mesh cachedCubeMesh;
        private static Mesh cachedSphereMesh;
        private static Mesh cachedCylinderMesh;
        private static Mesh cachedQuadMesh;

        private static Mesh GetPrimitiveSharedMesh(PrimitiveType type)
        {
            if (type == PrimitiveType.Cube && cachedCubeMesh != null) return cachedCubeMesh;
            if (type == PrimitiveType.Sphere && cachedSphereMesh != null) return cachedSphereMesh;
            if (type == PrimitiveType.Cylinder && cachedCylinderMesh != null) return cachedCylinderMesh;
            if (type == PrimitiveType.Quad && cachedQuadMesh != null) return cachedQuadMesh;

            var temp = GameObject.CreatePrimitive(type);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            if (type == PrimitiveType.Cube) cachedCubeMesh = mesh;
            else if (type == PrimitiveType.Sphere) cachedSphereMesh = mesh;
            else if (type == PrimitiveType.Cylinder) cachedCylinderMesh = mesh;
            else if (type == PrimitiveType.Quad) cachedQuadMesh = mesh;

            return mesh;
        }

        private GameObject CreateVisualOnlyObject(string objName, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, PrimitiveType type, Quaternion? localRot = null)
        {
            var go = new GameObject(objName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (localRot.HasValue) go.transform.localRotation = localRot.Value;

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = GetPrimitiveSharedMesh(type);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            if (sceneryRoot != null && parent != null && (parent == sceneryRoot || parent.IsChildOf(sceneryRoot)))
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            return go;
        }

        private void BuildBackgroundScenery()
        {
            if (sceneryRoot == null) return;

            // 1. Parede monumental de fundo em violeta-indigo noturno (#0d0a14 / #17131d), rodape continuo de madeira (#2b2138 / #3e3152) e varal de bandeirinhas no topo
            CreateVisualOnlyObject("Parede_Fundo_Monumental", sceneryRoot, new Vector3(60f, 4.0f, 7.5f), new Vector3(180f, 32f, 1.0f), wallpaperMaterial != null ? wallpaperMaterial : wardrobeMaterial, PrimitiveType.Cube);
            var bbMat = baseboardMaterial != null ? baseboardMaterial : (wardrobeMaterial != null ? wardrobeMaterial : wallpaperMaterial);
            CreateVisualOnlyObject("Rodape_Madeira_Fundo", sceneryRoot, new Vector3(60f, -8.55f, 6.08f), new Vector3(180f, 1.15f, 0.20f), bbMat, PrimitiveType.Cube);
            if (buntingMaterial != null)
            {
                CreateVisualOnlyObject("Varal_Bandeirinhas_Topo", sceneryRoot, new Vector3(60f, 3.55f, 6.45f), new Vector3(180f, 1.55f, 1.0f), buntingMaterial, PrimitiveType.Quad);
            }

            // 2. Duas Janelas Noturnas Canonicas (NIGHT_WINDOWS = [780, 1950] -> X=29.5m e X=73.75m, escala 2.85m x 4.15m)
            var winMat = windowMaterial != null ? windowMaterial : castleBlocksMaterial;
            CreateWindowWithMoonbeam("Janela_Monumental_Luar", 29.5f, winMat);
            CreateWindowWithMoonbeam("Janela_Monumental_Luar_2", 73.75f, winMat);

            BuildWallDecorations();

            // 2b. Cenografia Decorativa de Piso (roomScenery: tapetes e brinquedos em miniatura fora da linha de colisao Z=0)
            BuildFloorScenery();

            // 3. Poeira Estelar / Motes Magicos Flutuantes no Quarto
            if (ambientDustMaterial != null)
            {
                var dustGo = new GameObject("DarkRoom_AmbientDustMotes");
                dustGo.transform.SetParent(sceneryRoot, false);
                dustGo.transform.position = new Vector3(55f, 0f, 1.5f);
                var dustPs = dustGo.AddComponent<ParticleSystem>();
                var dustRend = dustGo.GetComponent<ParticleSystemRenderer>();
                if (dustRend != null) dustRend.sharedMaterial = ambientDustMaterial;

                var dMain = dustPs.main;
                dMain.loop = true;
                dMain.startLifetime = 7.0f;
                dMain.startSpeed = 0.12f;
                dMain.startSize = 0.09f;
                dMain.startColor = new Color(0.85f, 0.92f, 1.0f, 0.45f);
                dMain.maxParticles = 140;

                var dEm = dustPs.emission;
                dEm.rateOverTime = 25f;

                var dShape = dustPs.shape;
                dShape.shapeType = ParticleSystemShapeType.Box;
                dShape.scale = new Vector3(160f, 18f, 6f);
            }
        }

        private void CreateWindowWithMoonbeam(string windowName, float windowX, Material winMat)
        {
            CreateVisualOnlyObject(windowName, sceneryRoot, new Vector3(windowX, 1.35f, 6.2f), new Vector3(2.85f, 4.15f, 0.20f), winMat, PrimitiveType.Cube);

            if (moonbeamShaftMaterial != null)
            {
                // Veu diagonal principal de luar azul-prateado suspenso no ar, extinguindo-se antes do piso (LightingSystem.js linhas 958-979)
                CreateVisualOnlyObject(
                    $"{windowName}_Feixe_Luar",
                    sceneryRoot,
                    new Vector3(windowX + 1.25f, -1.40f, 6.02f),
                    new Vector3(4.00f, 8.20f, 1.0f),
                    moonbeamShaftMaterial,
                    PrimitiveType.Quad,
                    Quaternion.Euler(0f, 0f, 20f)
                );

                // Segunda camada difusa mais larga para o halo atmosferico ao redor e abaixo da janela
                CreateVisualOnlyObject(
                    $"{windowName}_Halo_Luar",
                    sceneryRoot,
                    new Vector3(windowX + 0.70f, -0.40f, 6.06f),
                    new Vector3(5.00f, 6.50f, 1.0f),
                    moonbeamShaftMaterial,
                    PrimitiveType.Quad,
                    Quaternion.Euler(0f, 0f, 14f)
                );
            }
        }

        private void BuildFloorScenery()
        {
            var floorSceneryRoot = new GameObject("Cenografia_Piso_RoomScenery").transform;
            floorSceneryRoot.SetParent(sceneryRoot, false);

            var mandalaMat = mandalaRugMaterial != null ? mandalaRugMaterial : royalVelvetMaterial;
            var stripedMat = stripedRugMaterial != null ? stripedRugMaterial : bookBlueMaterial;
            Quaternion flatOnFloor = Quaternion.Euler(90f, 0f, 0f);

            // 1. Tapetes canonicos (fluffy_rug e striped_rug de config.js / BackgroundRenderer.js) pousados sobre o piso (Y = -9.08m)
            if (mandalaMat != null)
            {
                CreateVisualOnlyObject("Tapete_Mandala_Largada", floorSceneryRoot, new Vector3(-9.5f, -9.08f, 1.4f), new Vector3(4.2f, 2.2f, 1.0f), mandalaMat, PrimitiveType.Quad, flatOnFloor);
                CreateVisualOnlyObject("Tapete_Mandala_Subida_1", floorSceneryRoot, new Vector3(14.0f, -9.08f, 3.5f), new Vector3(4.2f, 2.2f, 1.0f), mandalaMat, PrimitiveType.Quad, flatOnFloor);
                CreateVisualOnlyObject("Tapete_Mandala_Castelo", floorSceneryRoot, new Vector3(34.5f, -9.08f, 3.8f), new Vector3(4.4f, 2.2f, 1.0f), mandalaMat, PrimitiveType.Quad, flatOnFloor);
                CreateVisualOnlyObject("Tapete_Mandala_Fuga_1", floorSceneryRoot, new Vector3(62.0f, -9.08f, 3.8f), new Vector3(4.4f, 2.2f, 1.0f), mandalaMat, PrimitiveType.Quad, flatOnFloor);
                CreateVisualOnlyObject("Tapete_Mandala_Fuga_2", floorSceneryRoot, new Vector3(95.0f, -9.08f, 3.8f), new Vector3(4.4f, 2.2f, 1.0f), mandalaMat, PrimitiveType.Quad, flatOnFloor);
            }

            if (stripedMat != null)
            {
                CreateVisualOnlyObject("Tapete_Listrado_Inicio", floorSceneryRoot, new Vector3(-2.5f, -9.08f, 3.6f), new Vector3(4.8f, 2.0f, 1.0f), stripedMat, PrimitiveType.Quad, flatOnFloor);
                CreateVisualOnlyObject("Tapete_Listrado_Subida_2", floorSceneryRoot, new Vector3(25.5f, -9.08f, 3.8f), new Vector3(5.0f, 2.1f, 1.0f), stripedMat, PrimitiveType.Quad, flatOnFloor);
                CreateVisualOnlyObject("Tapete_Listrado_Castelo", floorSceneryRoot, new Vector3(45.0f, -9.08f, 4.1f), new Vector3(5.2f, 2.1f, 1.0f), stripedMat, PrimitiveType.Quad, flatOnFloor);
                CreateVisualOnlyObject("Tapete_Listrado_Fuga_2", floorSceneryRoot, new Vector3(78.0f, -9.08f, 4.0f), new Vector3(5.2f, 2.1f, 1.0f), stripedMat, PrimitiveType.Quad, flatOnFloor);
            }

            // 2. Brinquedos e objetos espalhados pelo piso (roomScenery em Z = 2.4m .. 4.4m, sem colisor, fora da linha Z=0)
            // Regiao inicial (visivel em start-camera.png)
            CreateOpenStoryBook(floorSceneryRoot, "Livro_Historias_Aberto_1", -13.2f, 2.5f);
            CreatePlushBunny(floorSceneryRoot, "Coelhinho_Pelucia_1", -10.4f, 3.4f);
            CreateSpilledMarbles(floorSceneryRoot, "Bolinhas_Gude_1", -6.2f, 2.4f);
            CreateToyCar(floorSceneryRoot, "Carrinho_Brinquedo_1", -3.8f, 3.3f);
            CreateScatteredBlocks(floorSceneryRoot, "Bloquinhos_Espalhados_1", -0.8f, 4.1f);

            // Regiao intermediaria da Etapa 1
            CreateToyTrain(floorSceneryRoot, "Trenzinho_Miniatura_1", 10.5f, 3.6f);
            CreateRetroRobot(floorSceneryRoot, "Robo_Lata_Retro_1", 18.5f, 4.0f);
            CreateOpenStoryBook(floorSceneryRoot, "Livro_Historias_Aberto_2", 24.0f, 3.5f);

            // Regiao do Castelo e inicio da Fuga (visivel em castle-camera.png em Z = 3.5m .. 4.4m)
            CreateSpilledMarbles(floorSceneryRoot, "Bolinhas_Gude_2", 31.2f, 3.7f);
            CreateToyCar(floorSceneryRoot, "Carrinho_Brinquedo_2", 34.8f, 3.6f);
            CreateRetroRobot(floorSceneryRoot, "Robo_Lata_Retro_2", 39.2f, 4.1f);
            CreateOpenStoryBook(floorSceneryRoot, "Livro_Historias_Aberto_3", 43.2f, 3.8f);
            CreateScatteredBlocks(floorSceneryRoot, "Bloquinhos_Espalhados_2", 47.5f, 4.2f);

            // Regiao final (Etapas 2 e 3)
            CreatePlushBunny(floorSceneryRoot, "Coelhinho_Pelucia_2", 53.5f, 3.8f);
            CreateToyTrain(floorSceneryRoot, "Trenzinho_Miniatura_2", 58.0f, 3.9f);
            CreateToyCar(floorSceneryRoot, "Carrinho_Brinquedo_3", 74.0f, 3.8f);
            CreateRetroRobot(floorSceneryRoot, "Robo_Lata_Retro_3", 88.0f, 4.1f);
            CreateScatteredBlocks(floorSceneryRoot, "Bloquinhos_Espalhados_3", 102.0f, 3.9f);
        }

        private void CreateOpenStoryBook(Transform parent, string baseName, float x, float z)
        {
            var root = new GameObject(baseName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(x, -9.10f, z);

            var coverMat = floorPropBurgundyMaterial != null ? floorPropBurgundyMaterial : (bookBurgundyMaterial != null ? bookBurgundyMaterial : bookBlueMaterial);
            var pageMat = floorPropCreamMaterial != null ? floorPropCreamMaterial : (bookPagesMaterial != null ? bookPagesMaterial : defaultPlatformMaterial);
            var ribbonMat = floorPropRedMaterial != null ? floorPropRedMaterial : (toyBlockRedMaterial != null ? toyBlockRedMaterial : coverMat);

            CreateVisualOnlyObject("Capa_Base", root, new Vector3(0f, 0.04f, 0f), new Vector3(0.96f, 0.06f, 0.66f), coverMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Pagina_Esq", root, new Vector3(-0.22f, 0.09f, 0f), new Vector3(0.42f, 0.05f, 0.58f), pageMat, PrimitiveType.Cube, Quaternion.Euler(0f, 0f, -8f));
            CreateVisualOnlyObject("Pagina_Dir", root, new Vector3(0.22f, 0.09f, 0f), new Vector3(0.42f, 0.05f, 0.58f), pageMat, PrimitiveType.Cube, Quaternion.Euler(0f, 0f, 8f));
            CreateVisualOnlyObject("Fita_Marcadora", root, new Vector3(0.05f, 0.05f, -0.36f), new Vector3(0.08f, 0.03f, 0.28f), ribbonMat, PrimitiveType.Cube, Quaternion.Euler(0f, 18f, 0f));
        }

        private void CreateSpilledMarbles(Transform parent, string baseName, float x, float z)
        {
            var root = new GameObject(baseName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(x, -9.10f, z);

            Material[] mats = new Material[]
            {
                floorPropBlueMaterial != null ? floorPropBlueMaterial : toyBlockBlueMaterial,
                floorPropRedMaterial != null ? floorPropRedMaterial : toyBlockRedMaterial,
                floorPropBurgundyMaterial != null ? floorPropBurgundyMaterial : (royalVelvetMaterial != null ? royalVelvetMaterial : bookBlueMaterial),
                floorPropGreenMaterial != null ? floorPropGreenMaterial : (toyBlockGreenMaterial != null ? toyBlockGreenMaterial : toyBlockBlueMaterial),
                floorPropYellowMaterial != null ? floorPropYellowMaterial : (brassGoldMaterial != null ? brassGoldMaterial : toyBlockYellowMaterial)
            };

            Vector3[] offsets = new Vector3[]
            {
                new Vector3(-0.38f, 0.09f, -0.10f),
                new Vector3(-0.16f, 0.09f, 0.14f),
                new Vector3(0.04f, 0.09f, -0.06f),
                new Vector3(0.24f, 0.09f, 0.12f),
                new Vector3(0.42f, 0.09f, -0.08f)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                CreateVisualOnlyObject($"Bolinha_{i + 1}", root, offsets[i], new Vector3(0.18f, 0.18f, 0.18f), mats[i % mats.Length], PrimitiveType.Sphere);
            }
        }

        private void CreateToyCar(Transform parent, string baseName, float x, float z)
        {
            var root = new GameObject(baseName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(x, -9.10f, z);

            // Carrinho de madeira pastel estilo Legend of Mana (paridade com toy-car.png:
            // carroceria azul-ardosia pastel com faixa vermelha, para-lamas vermelhos, interior de madeira, farois amarelos e rodas escuras com calota vermelha)
            var blueMat = floorPropBlueMaterial != null ? floorPropBlueMaterial : (toyBlockBlueMaterial != null ? toyBlockBlueMaterial : defaultPlatformMaterial);
            var redMat = floorPropRedMaterial != null ? floorPropRedMaterial : (toyBlockRedMaterial != null ? toyBlockRedMaterial : blueMat);
            var woodMat = floorPropCreamMaterial != null ? floorPropCreamMaterial : (dresserOakMaterial != null ? dresserOakMaterial : blueMat);
            var yellowMat = floorPropYellowMaterial != null ? floorPropYellowMaterial : (toyBlockYellowMaterial != null ? toyBlockYellowMaterial : blueMat);
            var wheelMat = floorPropBurgundyMaterial != null ? floorPropBurgundyMaterial : (wardrobeMaterial != null ? wardrobeMaterial : woodMat);

            CreateVisualOnlyObject("Chassi_Azul", root, new Vector3(0f, 0.19f, 0f), new Vector3(0.84f, 0.22f, 0.44f), blueMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Cabine_Arredondada", root, new Vector3(-0.05f, 0.36f, 0f), new Vector3(0.48f, 0.22f, 0.38f), blueMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Moldura_Janela_Madeira", root, new Vector3(-0.04f, 0.35f, -0.195f), new Vector3(0.32f, 0.14f, 0.02f), woodMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Faixa_Central_Vermelha", root, new Vector3(0.02f, 0.475f, 0f), new Vector3(0.44f, 0.02f, 0.12f), redMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Parachoque_Dianteiro", root, new Vector3(0.42f, 0.12f, 0f), new Vector3(0.10f, 0.10f, 0.48f), redMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Paralama_FE", root, new Vector3(0.24f, 0.16f, -0.21f), new Vector3(0.28f, 0.16f, 0.06f), redMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Paralama_TE", root, new Vector3(-0.24f, 0.16f, -0.21f), new Vector3(0.28f, 0.16f, 0.06f), redMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Farol_Esq", root, new Vector3(0.43f, 0.22f, -0.13f), new Vector3(0.08f, 0.10f, 0.10f), yellowMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Farol_Dir", root, new Vector3(0.43f, 0.22f, 0.13f), new Vector3(0.08f, 0.10f, 0.10f), yellowMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Roda_FE", root, new Vector3(0.24f, 0.11f, -0.23f), new Vector3(0.22f, 0.22f, 0.09f), wheelMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Calota_FE", root, new Vector3(0.24f, 0.11f, -0.27f), new Vector3(0.10f, 0.10f, 0.04f), redMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Roda_TE", root, new Vector3(-0.24f, 0.11f, -0.23f), new Vector3(0.22f, 0.22f, 0.09f), wheelMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Calota_TE", root, new Vector3(-0.24f, 0.11f, -0.27f), new Vector3(0.10f, 0.10f, 0.04f), redMat, PrimitiveType.Sphere);
        }

        private void CreateScatteredBlocks(Transform parent, string baseName, float x, float z)
        {
            var root = new GameObject(baseName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(x, -9.10f, z);

            // Torre de blocos de madeira pintada com bordas desgastadas (paridade com stacking-blocks-v1.png:
            // base retangular vermelha/terracota, bloco amarelo/ocre, bloco verde-salvia, telhadinho triangular azul no topo + cubo ABC lateral)
            var redMat = floorPropRedMaterial != null ? floorPropRedMaterial : toyBlockRedMaterial;
            var yellowMat = floorPropYellowMaterial != null ? floorPropYellowMaterial : toyBlockYellowMaterial;
            var greenMat = floorPropGreenMaterial != null ? floorPropGreenMaterial : toyBlockGreenMaterial;
            var blueMat = floorPropBlueMaterial != null ? floorPropBlueMaterial : toyBlockBlueMaterial;
            var abcMat = floorPropCreamMaterial != null ? floorPropCreamMaterial : (toyBlockAbcMaterial != null ? toyBlockAbcMaterial : redMat);
            CreateVisualOnlyObject("Bloco_Base_Terracota", root, new Vector3(0f, 0.12f, 0f), new Vector3(0.56f, 0.24f, 0.38f), redMat, PrimitiveType.Cube, Quaternion.Euler(0f, 6f, 0f));
            CreateVisualOnlyObject("Bloco_Meio_Ocre", root, new Vector3(-0.02f, 0.34f, 0.01f), new Vector3(0.40f, 0.22f, 0.34f), yellowMat, PrimitiveType.Cube, Quaternion.Euler(0f, -8f, 0f));
            CreateVisualOnlyObject("Bloco_Superior_Salvia", root, new Vector3(0.01f, 0.55f, 0f), new Vector3(0.34f, 0.22f, 0.32f), greenMat, PrimitiveType.Cube, Quaternion.Euler(0f, 5f, 0f));
            CreateVisualOnlyObject("Telhado_Prisma_Azul", root, new Vector3(0.01f, 0.72f, 0f), new Vector3(0.28f, 0.28f, 0.30f), blueMat, PrimitiveType.Cube, Quaternion.Euler(0f, 5f, 45f));
            CreateVisualOnlyObject("Bloco_ABC_Lateral", root, new Vector3(-0.46f, 0.16f, -0.06f), new Vector3(0.32f, 0.32f, 0.32f), abcMat, PrimitiveType.Cube, Quaternion.Euler(0f, 18f, 0f));
        }

        private void CreateRetroRobot(Transform parent, string baseName, float x, float z)
        {
            var root = new GameObject(baseName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(x, -9.10f, z);

            // Robo de lata retro estilo Legend of Mana (paridade com retro-robot.png:
            // corpo azul-petroleo com ferrugem/terracota nas bordas, botas terracota, mostrador duplo creme/laranja no peito, olhos redondos amarelos e sirene vermelha)
            var tealMat = floorPropBlueMaterial != null ? floorPropBlueMaterial : (toyBlockBlueMaterial != null ? toyBlockBlueMaterial : defaultPlatformMaterial);
            var dialMat = floorPropCreamMaterial != null ? floorPropCreamMaterial : (bookPagesMaterial != null ? bookPagesMaterial : toyBlockYellowMaterial);
            var yellowMat = floorPropYellowMaterial != null ? floorPropYellowMaterial : (toyBlockYellowMaterial != null ? toyBlockYellowMaterial : brassGoldMaterial);
            var redMat = floorPropRedMaterial != null ? floorPropRedMaterial : (toyBlockRedMaterial != null ? toyBlockRedMaterial : dialMat);

            CreateVisualOnlyObject("Bota_Esq", root, new Vector3(-0.11f, 0.07f, 0f), new Vector3(0.16f, 0.14f, 0.24f), redMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Bota_Dir", root, new Vector3(0.11f, 0.07f, 0f), new Vector3(0.16f, 0.14f, 0.24f), redMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Perna_Esq", root, new Vector3(-0.11f, 0.22f, 0f), new Vector3(0.12f, 0.18f, 0.16f), tealMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Perna_Dir", root, new Vector3(0.11f, 0.22f, 0f), new Vector3(0.12f, 0.18f, 0.16f), tealMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Corpo_Lata", root, new Vector3(0f, 0.46f, 0f), new Vector3(0.42f, 0.34f, 0.28f), tealMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Mostrador_Esq", root, new Vector3(-0.08f, 0.47f, -0.145f), new Vector3(0.13f, 0.20f, 0.03f), dialMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Mostrador_Dir", root, new Vector3(0.08f, 0.47f, -0.145f), new Vector3(0.13f, 0.20f, 0.03f), dialMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Braco_Esq", root, new Vector3(-0.26f, 0.44f, 0f), new Vector3(0.10f, 0.26f, 0.12f), tealMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Braco_Dir", root, new Vector3(0.26f, 0.44f, 0f), new Vector3(0.10f, 0.26f, 0.12f), tealMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Cabeca_Lata", root, new Vector3(0f, 0.74f, 0f), new Vector3(0.30f, 0.22f, 0.24f), tealMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Olho_Esq", root, new Vector3(-0.07f, 0.76f, -0.125f), new Vector3(0.08f, 0.08f, 0.03f), yellowMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Olho_Dir", root, new Vector3(0.07f, 0.76f, -0.125f), new Vector3(0.08f, 0.08f, 0.03f), yellowMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Orelha_Esq", root, new Vector3(-0.165f, 0.74f, 0f), new Vector3(0.05f, 0.09f, 0.09f), redMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Orelha_Dir", root, new Vector3(0.165f, 0.74f, 0f), new Vector3(0.05f, 0.09f, 0.09f), redMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Sirene_Topo", root, new Vector3(0f, 0.89f, 0f), new Vector3(0.12f, 0.10f, 0.12f), redMat, PrimitiveType.Sphere);
        }

        private void CreatePlushBunny(Transform parent, string baseName, float x, float z)
        {
            var root = new GameObject(baseName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(x, -9.10f, z);

            // Coelhinho de algodao costurado pastel (paridade com cotton-bunny-v1.png)
            var plushMat = floorPropPlushMaterial != null ? floorPropPlushMaterial : (teddyPlushMaterial != null ? teddyPlushMaterial : toyBlockRedMaterial);
            var creamMat = floorPropCreamMaterial != null ? floorPropCreamMaterial : (bookPagesMaterial != null ? bookPagesMaterial : toyBlockYellowMaterial);
            var roseMat = floorPropBurgundyMaterial != null ? floorPropBurgundyMaterial : (bookBurgundyMaterial != null ? bookBurgundyMaterial : toyBlockRedMaterial);

            CreateVisualOnlyObject("Corpo_Coelho", root, new Vector3(0f, 0.18f, 0f), new Vector3(0.30f, 0.32f, 0.26f), plushMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Pata_Esq", root, new Vector3(-0.11f, 0.08f, -0.10f), new Vector3(0.13f, 0.12f, 0.14f), creamMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Pata_Dir", root, new Vector3(0.11f, 0.08f, -0.10f), new Vector3(0.13f, 0.12f, 0.14f), creamMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Cabeca_Coelho", root, new Vector3(0f, 0.40f, -0.02f), new Vector3(0.28f, 0.26f, 0.25f), plushMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Focinho_Creme", root, new Vector3(0f, 0.37f, -0.13f), new Vector3(0.14f, 0.11f, 0.08f), creamMat, PrimitiveType.Sphere);
            CreateVisualOnlyObject("Orelha_Esq", root, new Vector3(-0.08f, 0.62f, 0f), new Vector3(0.10f, 0.28f, 0.06f), plushMat, PrimitiveType.Sphere, Quaternion.Euler(0f, 0f, 14f));
            CreateVisualOnlyObject("Orelha_Dir", root, new Vector3(0.08f, 0.62f, 0f), new Vector3(0.10f, 0.28f, 0.06f), plushMat, PrimitiveType.Sphere, Quaternion.Euler(0f, 0f, -14f));
            CreateVisualOnlyObject("Orelha_Interna_Esq", root, new Vector3(-0.08f, 0.61f, -0.025f), new Vector3(0.06f, 0.20f, 0.03f), roseMat, PrimitiveType.Sphere, Quaternion.Euler(0f, 0f, 14f));
            CreateVisualOnlyObject("Orelha_Interna_Dir", root, new Vector3(0.08f, 0.61f, -0.025f), new Vector3(0.06f, 0.20f, 0.03f), roseMat, PrimitiveType.Sphere, Quaternion.Euler(0f, 0f, -14f));
        }

        private void CreateToyTrain(Transform parent, string baseName, float x, float z)
        {
            var root = new GameObject(baseName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(x, -9.10f, z);

            var blueMat = floorPropBlueMaterial != null ? floorPropBlueMaterial : (toyBlockBlueMaterial != null ? toyBlockBlueMaterial : defaultPlatformMaterial);
            var redMat = floorPropRedMaterial != null ? floorPropRedMaterial : (toyBlockRedMaterial != null ? toyBlockRedMaterial : blueMat);
            var goldMat = floorPropYellowMaterial != null ? floorPropYellowMaterial : (brassGoldMaterial != null ? brassGoldMaterial : toyBlockYellowMaterial);

            CreateVisualOnlyObject("Base_Locomotiva", root, new Vector3(0f, 0.16f, 0f), new Vector3(0.86f, 0.20f, 0.40f), redMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Caldeira", root, new Vector3(0.14f, 0.34f, 0f), new Vector3(0.48f, 0.24f, 0.34f), blueMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Cabine_Trem", root, new Vector3(-0.22f, 0.40f, 0f), new Vector3(0.32f, 0.34f, 0.38f), redMat, PrimitiveType.Cube);
            CreateVisualOnlyObject("Chamine_Latao", root, new Vector3(0.26f, 0.54f, 0f), new Vector3(0.12f, 0.22f, 0.12f), goldMat, PrimitiveType.Cylinder);
        }

        private void BuildPlatformList(List<PlatformSpec> platforms, Transform parent, string prefix)
        {
            foreach (var spec in platforms)
            {
                var platGo = new GameObject($"{prefix}_{spec.index:D2}_{SanitizeName(spec.name)}");
                platGo.transform.SetParent(parent, false);
                platGo.transform.position = new Vector3(spec.x, spec.y, 0f);

                var geometry = DarkRoomPlatformGeometry.Get(spec.index);
                // A superficie de contato pertence ao objeto: pode estar deslocada, ser mais
                // estreita ou inclinada. O colisor segue o objeto, nunca o contrario.
                var contact = spec.contact;
                GameObject visual = null;

                if (geometry.HasModel)
                {
                    // Modelo dedicado (malhas nativas): a superficie visivel coincide com a
                    // superficie de contato declarada.
                    visual = DarkRoomPlatformArt.Build(geometry, contact, platformArtMaterials, platGo.transform);
                }

                if (visual == null)
                {
                    // Blockout provisorio: caixa com as dimensoes canonicas do objeto.
                    // A caixa e apenas visual; o colisor de pouso e criado separadamente.
                    visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visual.name = "Visual_3D";
                    visual.transform.SetParent(platGo.transform, false);
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localScale = new Vector3(spec.width, spec.height, spec.depth);

                    var mat = ResolveMaterialForSpec(spec);
                    var renderer = visual.GetComponent<MeshRenderer>();
                    if (renderer != null && mat != null)
                    {
                        renderer.sharedMaterial = mat;
                    }

                    var primitiveCollider = visual.GetComponent<BoxCollider>();
                    if (primitiveCollider != null) DestroySafe(primitiveCollider);

                    // Detalhes visuais tematicos em 3D (exclusivamente visuais, sem colisores)
                    AddPlatformDecorativeDetails(visual, spec, mat);
                }

                // Colisor de contato: mesma superficie de pouso do objeto.
                var contactGo = new GameObject("Contacto_Fisico");
                contactGo.transform.SetParent(platGo.transform, false);
                DarkRoomPlatformArt.BuildContact(spec,contactGo.transform);

                platGo.AddComponent<PlatformCollisionDetector>();
                var marker = platGo.AddComponent<DarkRoomPlatformMarker>();
                var resolved = spec;
                resolved.contact = contact;
                marker.spec = resolved;
                if(spec.index==YieldingWoodFurniture.PlatformIndex)
                    platGo.AddComponent<YieldingWoodFurniture>().Configure(furnitureCrackSfx);
            }
        }

        private static void DestroySafe(Object target)
        {
            if (target == null) return;
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }

        private Material ResolveMaterialForSpec(PlatformSpec spec)
        {
            switch (spec.index)
            {
                // Etapa 1: Subida calma ao Castelo (0 a 9)
                case 0: return teddyPlushMaterial != null ? teddyPlushMaterial : dresserOakMaterial;
                case 1: return bookBlueMaterial != null ? bookBlueMaterial : toyBlockBlueMaterial;
                case 2:
                case 3:
                case 7: return dresserOakMaterial != null ? dresserOakMaterial : wardrobeMaterial;
                case 4: return toyBlockAbcMaterial != null ? toyBlockAbcMaterial : toyBlockRedMaterial;
                case 5: return circusDrumMaterial != null ? circusDrumMaterial : toyBlockRedMaterial;
                case 6: return royalVelvetMaterial != null ? royalVelvetMaterial : toyBlockBlueMaterial;
                case 8: return musicBoxMaterial != null ? musicBoxMaterial : wardrobeMaterial;
                case 9: return castleBlocksMaterial != null ? castleBlocksMaterial : toyBlockBlueMaterial;

                // Etapa 2: A Fuga Acelerada (10 a 21)
                case 10: return trainTrackMaterial != null ? trainTrackMaterial : dresserOakMaterial;
                case 11: return dresserOakMaterial != null ? dresserOakMaterial : wardrobeMaterial;
                case 12: return toyBlockRedMaterial != null ? toyBlockRedMaterial : defaultPlatformMaterial;
                case 13: return toyBlockYellowMaterial != null ? toyBlockYellowMaterial : defaultPlatformMaterial;
                case 14: return toyBlockBlueMaterial != null ? toyBlockBlueMaterial : defaultPlatformMaterial;
                case 15: return toyBlockRedMaterial != null ? toyBlockRedMaterial : defaultPlatformMaterial;
                case 16: return bookBurgundyMaterial != null ? bookBurgundyMaterial : defaultPlatformMaterial;
                case 17:
                case 18: return brassGoldMaterial != null ? brassGoldMaterial : defaultPlatformMaterial;
                case 19: return dresserOakMaterial != null ? dresserOakMaterial : wardrobeMaterial;
                case 20: return wardrobeMaterial != null ? wardrobeMaterial : dresserOakMaterial;
                case 21: return falseDoorPosterMaterial != null ? falseDoorPosterMaterial : wardrobeMaterial;

                // Etapa 3: Subida CaÃ³tica Reversa (22 a 37)
                case 22: return toyBlockAbcMaterial != null ? toyBlockAbcMaterial : toyBlockRedMaterial;
                case 23: return royalVelvetMaterial != null ? royalVelvetMaterial : toyBlockBlueMaterial;
                case 24: return dresserOakMaterial != null ? dresserOakMaterial : wardrobeMaterial;
                case 25: return bookBlueMaterial != null ? bookBlueMaterial : toyBlockBlueMaterial;
                case 26: return circusDrumMaterial != null ? circusDrumMaterial : toyBlockRedMaterial;
                case 27: return teddyPlushMaterial != null ? teddyPlushMaterial : dresserOakMaterial;
                case 28: return toyBlockYellowMaterial != null ? toyBlockYellowMaterial : defaultPlatformMaterial;
                case 29: return trainTrackMaterial != null ? trainTrackMaterial : dresserOakMaterial;
                case 30: return bookBurgundyMaterial != null ? bookBurgundyMaterial : defaultPlatformMaterial;
                case 31: return musicBoxMaterial != null ? musicBoxMaterial : wardrobeMaterial;
                case 32: return castleBlocksMaterial != null ? castleBlocksMaterial : toyBlockBlueMaterial;
                case 33: return toyBlockRedMaterial != null ? toyBlockRedMaterial : defaultPlatformMaterial;
                case 34: return dresserOakMaterial != null ? dresserOakMaterial : wardrobeMaterial;
                case 35: return brassGoldMaterial != null ? brassGoldMaterial : defaultPlatformMaterial;
                case 36: return bookBlueMaterial != null ? bookBlueMaterial : toyBlockBlueMaterial;
                case 37: return dreamPortalMaterial != null ? dreamPortalMaterial : brassGoldMaterial;

                default: return dresserOakMaterial != null ? dresserOakMaterial : defaultPlatformMaterial;
            }
        }

        private void AddPlatformDecorativeDetails(GameObject visual, PlatformSpec spec, Material baseMat)
        {
            // Detalhes sao posicionados no fundo (Z > 0.4f) ou na frente (Z < -0.4f) para deixar Z=0 plano e livre para o CharacterController

            // Plat 0: Cabeca do Urso com orelhas arredondadas no fundo e focinho na frente
            if (spec.index == 0)
            {
                var earMat = teddyPlushMaterial != null ? teddyPlushMaterial : baseMat;
                CreateVisualOnlyObject("Orelha_Esq", visual.transform, new Vector3(-0.35f, 0.38f, 0.45f), new Vector3(0.35f, 0.35f, 0.20f), earMat, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Orelha_Dir", visual.transform, new Vector3(0.35f, 0.38f, 0.45f), new Vector3(0.35f, 0.35f, 0.20f), earMat, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Focinho_Urso", visual.transform, new Vector3(0f, -0.15f, -0.48f), new Vector3(0.40f, 0.26f, 0.25f), dresserOakMaterial != null ? dresserOakMaterial : baseMat, PrimitiveType.Sphere);
            }
            // Plat 1: Pilha de Livros com capas no fundo e na frente
            else if (spec.index == 1)
            {
                CreateVisualOnlyObject("Capa_Bordo", visual.transform, new Vector3(0.04f, -0.25f, -0.2f), new Vector3(1.05f, 0.32f, 0.75f), bookBurgundyMaterial, PrimitiveType.Cube);
                CreateVisualOnlyObject("Capa_Verde", visual.transform, new Vector3(-0.04f, -0.28f, 0.2f), new Vector3(0.95f, 0.30f, 0.75f), bookGreenMaterial, PrimitiveType.Cube);
            }
            // Plat 2 e 3: Moveis e comoda com puxadores na face frontal
            else if (spec.index == 2 || spec.index == 3)
            {
                CreateVisualOnlyObject("Puxador_Esq", visual.transform, new Vector3(-0.25f, -0.12f, -0.52f), new Vector3(0.12f, 0.12f, 0.12f), brassGoldMaterial, PrimitiveType.Sphere);
                CreateVisualOnlyObject("Puxador_Dir", visual.transform, new Vector3(0.25f, -0.12f, -0.52f), new Vector3(0.12f, 0.12f, 0.12f), brassGoldMaterial, PrimitiveType.Sphere);
            }
            // Plat 4: Pilha de Blocos ABC com cubos primarios na frente e atras
            else if (spec.index == 4)
            {
                CreateVisualOnlyObject("Bloco_Amarelo", visual.transform, new Vector3(-0.25f, -0.15f, -0.35f), new Vector3(0.42f, 0.70f, 0.45f), toyBlockYellowMaterial, PrimitiveType.Cube);
                CreateVisualOnlyObject("Bloco_Azul", visual.transform, new Vector3(0.25f, -0.15f, 0.35f), new Vector3(0.42f, 0.70f, 0.45f), toyBlockBlueMaterial, PrimitiveType.Cube);
            }
            // Plat 5: Tamborito de Marcha com aros dourados inferiores
            else if (spec.index == 5)
            {
                CreateVisualOnlyObject("Aro_Inferior", visual.transform, new Vector3(0f, -0.44f, 0f), new Vector3(1.05f, 0.08f, 1.05f), brassGoldMaterial, PrimitiveType.Cube);
            }
            // Plat 6: Almofadao com detalhe na face frontal
            else if (spec.index == 6)
            {
                CreateVisualOnlyObject("Botao_Capitone", visual.transform, new Vector3(0f, -0.1f, -0.48f), new Vector3(0.16f, 0.16f, 0.10f), brassGoldMaterial, PrimitiveType.Sphere);
            }
            // Plat 9: Castelinho com ameias e torres recuadas no fundo (Z = +0.55m)
            else if (spec.index == 9)
            {
                var turretMat = castleTurretMaterial != null ? castleTurretMaterial : toyBlockBlueMaterial;
                CreateVisualOnlyObject("Torre_Fundo_Esq", visual.transform, new Vector3(-0.44f, 0.35f, 0.55f), new Vector3(0.18f, 0.65f, 0.45f), turretMat, PrimitiveType.Cube);
                CreateVisualOnlyObject("Torre_Fundo_Dir", visual.transform, new Vector3(0.44f, 0.35f, 0.55f), new Vector3(0.18f, 0.65f, 0.45f), turretMat, PrimitiveType.Cube);
            }
            // Plat 21: Moldura decorativa no fundo
            else if (spec.index == 21)
            {
                CreateVisualOnlyObject("Moldura_Poster", visual.transform, new Vector3(0f, 0.25f, 0.45f), new Vector3(1.05f, 0.60f, 0.2f), wardrobeMaterial, PrimitiveType.Cube);
            }
            // Plat 37: Verdadeiro Portal dos Sonhos com arcos dourados recuados no fundo
            else if (spec.index == 37)
            {
                var gold = dreamPortalMaterial != null ? dreamPortalMaterial : brassGoldMaterial;
                CreateVisualOnlyObject("Coluna_Portal_Fundo", visual.transform, new Vector3(-0.46f, 1.0f, 0.55f), new Vector3(0.12f, 1.8f, 0.3f), gold, PrimitiveType.Cube);
                CreateVisualOnlyObject("Coluna_Portal_Frente", visual.transform, new Vector3(0.46f, 1.0f, -0.55f), new Vector3(0.12f, 1.8f, 0.3f), gold, PrimitiveType.Cube);
            }
        }

        private static string SanitizeName(string raw)
        {
            return raw.Replace(" ", "_").Replace("/", "_").Replace("(", "").Replace(")", "");
        }
    }
}
