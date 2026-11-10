using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    [ExecuteAlways]
    public class DarkRoomBlockoutBuilder : MonoBehaviour
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

        [Header("Estrutura Gerada (3 Etapas CanÃ´nicas)")]
        [SerializeField] private Transform stage1Root;
        [SerializeField] private Transform stage2Root;
        [SerializeField] private Transform stage3Root;
        [SerializeField] private Transform floorRoot;
        [SerializeField] private Transform sceneryRoot;

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
            Material moonbeamShaft = null, Material ambientDust = null, Material floorShadow = null)
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
            float stage2FirstPlatWidth = 2.2f;
            float gapStage1To2 = 2.32f;
            float stage2StartX = lastStage1.x + (lastStage1.width * 0.5f) + gapStage1To2 + (stage2FirstPlatWidth * 0.5f);
            var stage2 = DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms(stage2StartX, lastStage1.y + 0.32f);
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
            float worldY = -9.6f;
            float worldWidth = 170.0f;
            float worldDepth = 12.0f;
            float thickness = 1.0f;

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

        private static Mesh GetPrimitiveSharedMesh(PrimitiveType type)
        {
            if (type == PrimitiveType.Cube && cachedCubeMesh != null) return cachedCubeMesh;
            if (type == PrimitiveType.Sphere && cachedSphereMesh != null) return cachedSphereMesh;
            if (type == PrimitiveType.Cylinder && cachedCylinderMesh != null) return cachedCylinderMesh;

            var temp = GameObject.CreatePrimitive(type);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            if (type == PrimitiveType.Cube) cachedCubeMesh = mesh;
            else if (type == PrimitiveType.Sphere) cachedSphereMesh = mesh;
            else if (type == PrimitiveType.Cylinder) cachedCylinderMesh = mesh;

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

            return go;
        }

        private void BuildBackgroundScenery()
        {
            if (sceneryRoot == null) return;

            // 1. Parede monumental de fundo em azul-indigo noturno (sem qualquer colisor)
            var wall = CreateVisualOnlyObject("Parede_Fundo_Monumental", sceneryRoot, new Vector3(60f, 4.0f, 7.5f), new Vector3(180f, 32f, 1.0f), wallpaperMaterial != null ? wallpaperMaterial : wardrobeMaterial, PrimitiveType.Cube);

            // 2. Guarda-roupa monumental de carvalho (ao fundo da Etapa 1)
            var wardrobe = CreateVisualOnlyObject("GuardaRoupa_Carvalho_Fundo", sceneryRoot, new Vector3(10f, 1.0f, 5.8f), new Vector3(14f, 20f, 3.5f), wardrobeMaterial, PrimitiveType.Cube);

            // Beiral decorativo superior do guarda-roupa
            CreateVisualOnlyObject("Beiral_GuardaRoupa", wardrobe.transform, new Vector3(0f, 0.52f, 0f), new Vector3(1.06f, 0.08f, 1.15f), brassGoldMaterial != null ? brassGoldMaterial : wardrobeMaterial, PrimitiveType.Cube);

            // 3. Janela monumental com luar azulado e grande lua cheia visivel (ao fundo da transicao Etapa 1 -> 2)
            CreateVisualOnlyObject("Janela_Monumental_Luar", sceneryRoot, new Vector3(30f, 3.5f, 5.0f), new Vector3(16f, 22f, 0.4f), windowMaterial != null ? windowMaterial : castleBlocksMaterial, PrimitiveType.Cube);

            // 4. Estante gigante de livros ilustrados (ao fundo da Fuga Acelerada)
            var shelf = CreateVisualOnlyObject("Estante_Gigante_Livros", sceneryRoot, new Vector3(58f, 2.5f, 5.6f), new Vector3(18f, 18f, 3.2f), wardrobeMaterial, PrimitiveType.Cube);

            // Livros gigantes na estante
            CreateVisualOnlyObject("Livro_Gigante_Azul", shelf.transform, new Vector3(-0.35f, 0.15f, -0.3f), new Vector3(0.18f, 0.28f, 0.7f), bookBlueMaterial, PrimitiveType.Cube);
            CreateVisualOnlyObject("Livro_Gigante_Bordo", shelf.transform, new Vector3(-0.15f, 0.18f, -0.3f), new Vector3(0.16f, 0.32f, 0.7f), bookBurgundyMaterial, PrimitiveType.Cube);
            CreateVisualOnlyObject("Livro_Gigante_Verde", shelf.transform, new Vector3(0.08f, 0.14f, -0.3f), new Vector3(0.20f, 0.25f, 0.7f), bookGreenMaterial, PrimitiveType.Cube);

            // 5. Relogio Cuco Monumental de Mogno (proximo a parede do poster)
            var clock = CreateVisualOnlyObject("RelogioCuco_Monumental", sceneryRoot, new Vector3(103f, 3.5f, 5.2f), new Vector3(5.5f, 20f, 2.8f), dresserOakMaterial != null ? dresserOakMaterial : wardrobeMaterial, PrimitiveType.Cube);

            // Mostrador dourado do relogio
            CreateVisualOnlyObject("Mostrador_Dourado", clock.transform, new Vector3(0f, 0.22f, -0.52f), new Vector3(0.45f, 0.04f, 0.45f), brassGoldMaterial, PrimitiveType.Cylinder, Quaternion.Euler(90f, 0f, 0f));

            // 6. Poeira Estelar / Motes Magicos Flutuantes no Quarto
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

        private void BuildPlatformList(List<PlatformSpec> platforms, Transform parent, string prefix)
        {
            foreach (var spec in platforms)
            {
                var platGo = new GameObject($"{prefix}_{spec.index:D2}_{SanitizeName(spec.name)}");
                platGo.transform.SetParent(parent, false);
                platGo.transform.position = new Vector3(spec.x, spec.y, 0f);

                // Malha visual natural 3D
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = "Visual_3D";
                visual.transform.SetParent(platGo.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(spec.width, spec.height, spec.depth);

                // Material estilizado
                var mat = ResolveMaterialForSpec(spec);
                var renderer = visual.GetComponent<MeshRenderer>();
                if (renderer != null && mat != null)
                {
                    renderer.sharedMaterial = mat;
                }

                // Detalhes visuais tematicos em 3D (exclusivamente visuais, sem colisores e fora da linha de corrida Z=0)
                AddPlatformDecorativeDetails(visual, spec, mat);

                // Colisor de caixa solido no topo (intocado para garantir fisica 100% perfeita)
                var collider = visual.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.isTrigger = false;
                }

                platGo.AddComponent<PlatformCollisionDetector>();
                var marker = platGo.AddComponent<DarkRoomPlatformMarker>();
                marker.spec = spec;
            }
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
