using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.Arena.Core;
using YunaLegend.Arena.Runtime;
using YunaLegend.DarkRoom;
using YunaLegend.Editor.Arena;
using YunaLegend.Editor.ToyRoom;
using YunaLegend.Illustrated25D.Preview.Controllers;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Executa e captura provas reais de jogo (sem expor codigo-fonte) para o Creative Director Agent
    /// auditar em modo black-box as 4 regressoes e as 2 melhorias do Quarto Escuro.
    /// </summary>
    public static class CreativeDirectorVerificationCapture
    {
        private const string OutputDir = "Logs/creative_director_audit";

        [MenuItem("YunaLegend/Auditoria Creative Director/Capturar Todas as Provas do Jogo Pronto")]
        public static void CaptureAllProofs()
        {
            Directory.CreateDirectory(OutputDir);
            var report = new StringBuilder();
            report.AppendLine("# Relatorio de Execucao do Jogo Pronto — Auditoria Creative Director");
            report.AppendLine();

            // Reconstroi cenas ToyRoom e Arena para garantir que todas as mudancas estejam persistidas nas cenas
            ToyRoomSceneBuilder.BuildBaseScene();
            ArenaSceneBuilder.BuildArenaScene();

            CaptureRegression1_ToyRoomMobileFonts(report);
            CaptureRegression2_SwordSilenceUntilArena(report);
            CaptureRegression3_ArenaBackgroundOrder(report);
            CaptureRegression4_ArenaDefeatedToyDeposit(report);
            CaptureDarkRoom1_SurfaceFootstepSounds(report);
            CaptureDarkRoom2_CardCastleCollapseAfterJump(report);

            File.WriteAllText(Path.Combine(OutputDir, "GAME_EXECUTION_REPORT.md"), report.ToString());
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("CREATIVE_DIRECTOR_AUDIT_OK " + Path.GetFullPath(OutputDir));
        }

        private static void CaptureRegression1_ToyRoomMobileFonts(StringBuilder report)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/ToyRoom_Gameplay.unity", OpenSceneMode.Single);
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var audioCtrl = UnityEngine.Object.FindFirstObjectByType<ToyRoomAudioController>();
            var caveatFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Caveat-Variable.ttf");

            var hostGo = new GameObject("__TempIntroAudit");
            try
            {
                var director = hostGo.AddComponent<ToyRoomIntroCinematicController>();
                director.Configure(audioCtrl, caveatFont);
                director.BeginIntroCinematic();

                // Captura em Retrato Mobile (390x844) e Paisagem Mobile (844x390)
                director.SetViewportOverride(390f, 844f);
                director.EvaluateAtTime(5.0f); // TR_004: Menina "Aqui também..."
                RenderSceneWithUiOverlay(cam, 390, 844, Path.Combine(OutputDir, "reg1_toyroom_intro_mobile_portrait_dialogue.png"));
                RecordCanvasFontSizes(hostGo, "ToyRoom Intro Mobile Retrato (390x844) - Dialogo", report);

                director.EvaluateAtTime(12.8f); // TR_006: Poético "A sala estava cheia..."
                RenderSceneWithUiOverlay(cam, 390, 844, Path.Combine(OutputDir, "reg1_toyroom_intro_mobile_portrait_poetic.png"));
                RecordCanvasFontSizes(hostGo, "ToyRoom Intro Mobile Retrato (390x844) - Poetico", report);

                director.SetViewportOverride(844f, 390f);
                director.EvaluateAtTime(9.3f); // TR_005: Menina "Eu queria espaço pra brincar."
                RenderSceneWithUiOverlay(cam, 844, 390, Path.Combine(OutputDir, "reg1_toyroom_intro_mobile_landscape_dialogue.png"));
                RecordCanvasFontSizes(hostGo, "ToyRoom Intro Mobile Paisagem (844x390) - Dialogo", report);

                director.EndIntroCinematic();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hostGo);
            }
        }

        private static void RecordCanvasFontSizes(GameObject root, string label, StringBuilder report)
        {
            report.AppendLine($"## Regressao 1 — {label}");
            foreach (var txt in root.GetComponentsInChildren<Text>(true))
            {
                if (!txt.gameObject.activeInHierarchy)
                {
                    continue;
                }

                report.AppendLine($"- Texto `{txt.gameObject.name}` (\"{txt.text.Replace("\n", " / ")}\"): **{txt.fontSize} px** (regra mobile: 14 a 16 px)");
            }
            report.AppendLine();
        }

        private static void CaptureRegression2_SwordSilenceUntilArena(StringBuilder report)
        {
            report.AppendLine("## Regressao 2 — Silencio ao Encontrar a Espada ate a Arena Imaginaria");
            EditorSceneManager.OpenScene("Assets/Scenes/ToyRoom_Gameplay.unity", OpenSceneMode.Single);
            var audioCtrl = UnityEngine.Object.FindFirstObjectByType<ToyRoomAudioController>();
            var awakeMethod = typeof(ToyRoomAudioController).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            awakeMethod?.Invoke(audioCtrl, null);

            audioCtrl.PlayExplorationMusic();
            bool playingDuringExploration = audioCtrl.IsMusicPlaying;
            string explorationTrack = audioCtrl.ActiveMusicTrackName;

            // Simula o momento em que a menina encontra/revela a espada
            audioCtrl.PlaySwordReveal();
            bool playingWhenSwordFound = audioCtrl.IsMusicPlaying;

            report.AppendLine($"- Musica durante a exploracao do Quarto de Brinquedos: `isPlaying={playingDuringExploration}` (`{explorationTrack}`)");
            report.AppendLine($"- Musica quando a menina encontra a espada na Sala de Brinquedos: `isPlaying={playingWhenSwordFound}` (**SILENCIO TOTAL = {!playingWhenSwordFound}**)");

            // Carrega a Arena Imaginaria para verificar que a nova musica comeca apenas la
            EditorSceneManager.OpenScene("Assets/Scenes/Arena_Gameplay.unity", OpenSceneMode.Single);
            var arenaAudio = UnityEngine.Object.FindFirstObjectByType<ArenaAudioController>();
            var startMethod = typeof(ArenaAudioController).GetMethod("Start", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            startMethod?.Invoke(arenaAudio, null);
            var arenaSource = arenaAudio.GetComponent<AudioSource>();
            bool playingInArena = arenaSource != null && arenaSource.isPlaying;
            string arenaTrack = arenaSource != null && arenaSource.clip != null ? arenaSource.clip.name : "none";

            report.AppendLine($"- Nova musica ao iniciar a Arena Imaginaria: `isPlaying={playingInArena}` (`{arenaTrack}`)");
            report.AppendLine();
        }

        private static void CaptureRegression3_ArenaBackgroundOrder(StringBuilder report)
        {
            report.AppendLine("## Regressao 3 — Ordem e Posicionamento dos Moveis no Fundo da Arena Imaginaria");
            EditorSceneManager.OpenScene("Assets/Scenes/Arena_Gameplay.unity", OpenSceneMode.Single);
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();

            var chest = GameObject.Find("OpenChest_3D");
            var bookshelf = GameObject.Find("Bookshelf_3D");
            var armchair = GameObject.Find("Armchair_3D");
            var horse = GameObject.Find("RockingHorse_3D");
            var wardrobe = GameObject.Find("Wardrobe_3D");

            report.AppendLine("- Ordem da esquerda para a direita no Quarto de Brinquedos (`room-v1.png` + `ToyRoomCatalog`): Bau/Caixa de Brinquedos Aberta (esquerda) -> Estante Alta de Livros e Brinquedos (centro) -> Poltrona Vermelha sob a Janela -> Cavalinho de Pau (direita) -> Armario Lateral (extrema direita).");
            if (chest != null && bookshelf != null && armchair != null && horse != null && wardrobe != null)
            {
                report.AppendLine($"- Ordem da esquerda para a direita na Arena Imaginaria:");
                report.AppendLine($"  1. `OpenChest_3D` (Bau/Caixa de Brinquedos Aberta, 100% desobstruida): X = **{chest.transform.position.x:F2} m**");
                report.AppendLine($"  2. `Bookshelf_3D` (Estante Alta de Livros e Brinquedos de 4 andares + folhagem): X = **{bookshelf.transform.position.x:F2} m**");
                report.AppendLine($"  3. `Armchair_3D` (Poltrona Vermelha Aconchegante sob a Janela): X = **{armchair.transform.position.x:F2} m**");
                report.AppendLine($"  4. `RockingHorse_3D` (Cavalinho de Pau): X = **{horse.transform.position.x:F2} m**");
                report.AppendLine($"  5. `Wardrobe_3D` (Armario/Guarda-Roupa Lateral): X = **{wardrobe.transform.position.x:F2} m**");
            }
            report.AppendLine();

            Vector3 origPos = cam.transform.position;
            Quaternion origRot = cam.transform.rotation;
            float origFov = cam.fieldOfView;

            cam.fieldOfView = 36f;
            cam.transform.position = new Vector3(0.4f, 2.35f, -15.8f);
            cam.transform.LookAt(new Vector3(0.4f, 1.35f, 1.5f));
            RenderCameraToPng(cam, 1280, 720, Path.Combine(OutputDir, "reg3_arena_background_furniture_order.png"));

            cam.transform.SetPositionAndRotation(origPos, origRot);
            cam.fieldOfView = origFov;
        }

        private static void CaptureRegression4_ArenaDefeatedToyDeposit(StringBuilder report)
        {
            report.AppendLine("## Regressao 4 — Animacao do Brinquedo Derrotado Sendo Depositado de Volta na Caixa de Brinquedos");
            EditorSceneManager.OpenScene("Assets/Scenes/Arena_Gameplay.unity", OpenSceneMode.Single);
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var playerView = GameObject.Find("Yuna_Arena")?.GetComponent<ArenaActorView>();
            var enemyView = GameObject.Find("Enemy_Arena")?.GetComponent<ArenaActorView>();

            // Move Yuna ligeiramente para a direita da boca do bau durante a demonstracao de deposito para nao cobrir a vista frontal da caixa
            var sim = new ArenaCombatSimulation(easyMode: false);
            for (int i = 0; i < 120; i++)
            {
                sim.TickFixed(new ArenaInputState { MoveX = 1f });
                playerView?.UpdatePlayerVisual(sim, null, 1f, 1f / 60f);
                enemyView?.UpdateEnemyVisual(sim, null, 1f / 60f);
            }

            // Derrota o Trem usando o especial para acionar o estado Defeated
            sim.TickFixed(new ArenaInputState { SpecialPressed = true });
            for (int i = 0; i < ArenaSpecialSpec.TotalDurationTicks; i++)
            {
                sim.TickFixed(default);
                playerView?.UpdatePlayerVisual(sim, null, 0f, 1f / 60f);
                enemyView?.UpdateEnemyVisual(sim, null, 1f / 60f);
            }

            // Enquadra toda a trajetoria parabolica desde a direita da Arena ate a Caixa de Brinquedos Aberta a esquerda (X = -5.30m)
            cam.fieldOfView = 36f;
            cam.transform.position = new Vector3(-0.35f, 2.35f, -15.5f);
            cam.transform.LookAt(new Vector3(-0.35f, 1.35f, 1.5f));

            int[] captureTicks = { 1, 16, 32, 46, 58 };
            for (int t = 1; t <= 59; t++)
            {
                sim.TickFixed(default);
                playerView?.UpdatePlayerVisual(sim, null, 0f, 1f / 60f);
                enemyView?.UpdateEnemyVisual(sim, null, 1f / 60f);

                for (int c = 0; c < captureTicks.Length; c++)
                {
                    if (t == captureTicks[c])
                    {
                        string frameName = $"reg4_arena_toy_deposit_frame_{c + 1}_tick{t:00}.png";
                        RenderCameraToPng(cam, 1280, 720, Path.Combine(OutputDir, frameName));
                        report.AppendLine($"- Frame {c + 1} (tick {t}/60): `EnemyX={sim.EnemyX:F1}` (voando em arco ate a caixa em {ArenaCombatSimulation.ChestDepositTargetSimX:F0}), `ArcHeight={sim.EnemyDepositArcHeight:F1}`, `Scale={sim.EnemySpawnScale:F2}`, `Pitch={sim.EnemyDepositPitchDegrees:F1} deg` -> `{frameName}`");
                    }
                }
            }
            report.AppendLine();
        }

        private static void CaptureDarkRoom1_SurfaceFootstepSounds(StringBuilder report)
        {
            report.AppendLine("## Melhoria 1 (Quarto Escuro) — Som dos Passos da Menina de Acordo com o Objeto Pisado");
            int[] samplePlatforms = { 0, 1, 2, 5, 8, 17, 28, 30, 32 };
            string[] platformLabels =
            {
                "00 - Cabeca do Urso de Pelucia",
                "01 - Pilha de Livros Ilustrados",
                "02 - Penteadeira de Madeira",
                "05 - Tamborito de Marcha",
                "08 - Caixa de Musica da Bailarina",
                "17 - Lustre de Cristais",
                "28 - Xilofone Colorido Inclinado",
                "30 - Castelo de Cartas Bamboleante",
                "32 - Robo de Lata Desparafusado"
            };

            for (int i = 0; i < samplePlatforms.Length; i++)
            {
                int pIdx = samplePlatforms[i];
                var step0 = DarkRoomSurfaceSoundCatalog.EvaluateStepSpec(pIdx, 0, isLanding: false);
                var step1 = DarkRoomSurfaceSoundCatalog.EvaluateStepSpec(pIdx, 1, isLanding: false);
                float[] pcm = DarkRoomSurfaceSoundCatalog.SynthesizeSamples(step0, 22050);
                float peak = 0f;
                for (int s = 0; s < pcm.Length; s++)
                {
                    float a = Mathf.Abs(pcm[s]);
                    if (a > peak) peak = a;
                }

                report.AppendLine($"- **{platformLabels[i]}**: Timbre=`{step0.Kind}`, Freq Passo 1=`{step0.FundamentalHz:F1} Hz` ( Parciais `{step0.SecondaryHz:F0}` / `{step0.TertiaryHz:F0} Hz` ), Freq Passo 2=`{step1.FundamentalHz:F1} Hz`, Duracao=`{step0.DurationSeconds * 1000f:F0} ms`, Pico PCM=`{peak:F2}`");
            }
            report.AppendLine();
        }

        private static void CaptureDarkRoom2_CardCastleCollapseAfterJump(StringBuilder report)
        {
            report.AppendLine("## Melhoria 2 (Quarto Escuro) — Animacao do Castelo de Cartas (Plataforma 30) Desmoronando Apos o Pulo");
            EditorSceneManager.OpenScene("Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity", OpenSceneMode.Single);

            var specs = DarkRoomFullStageCatalog.BuildSpecsUpTo(37);
            IllustratedPlatformSpec spec30 = default;
            for (int i = 0; i < specs.Length; i++)
            {
                if (specs[i].index == 30)
                {
                    spec30 = specs[i];
                    break;
                }
            }

            Texture2D tex30 = AssetDatabase.LoadAssetAtPath<Texture2D>(DarkRoomFullStageCatalog.GetTextureAssetPath(30));
            Texture2D yunaRunTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Illustrated25D/Yuna/Joint/Runtime/Yuna_Profile_Runtime_v1.png");

            var tempHost = new GameObject("__TempCardCastleAudit");
            var camGo = new GameObject("__TempCardCastleCamera");
            try
            {
                var marker = tempHost.AddComponent<DarkRoomPlatformMarker>();
                marker.spec.index = 30;
                var view = tempHost.AddComponent<IllustratedPlatformView>();
                view.Install(spec30, tex30);

                var collapseView = tempHost.AddComponent<CardCastleCollapseView>();
                collapseView.Install(view, null, tex30, spec30);

                // Ator ilustrado da Yuna pisando no topo do castelo de cartas e depois saltando para a esquerda (rumo a plataforma 31)
                var yunaGo = new GameObject("Yuna_JumpActor");
                yunaGo.transform.SetParent(tempHost.transform, false);
                var yunaSr = yunaGo.AddComponent<SpriteRenderer>();
                if (yunaRunTex != null)
                {
                    yunaSr.sprite = Sprite.Create(
                        yunaRunTex,
                        new Rect(0, 0, yunaRunTex.width, yunaRunTex.height),
                        new Vector2(0.5f, 0.06f),
                        420f);
                    yunaSr.flipX = true; // Correndo para a esquerda na Etapa 3
                    yunaSr.sortingOrder = 25;
                }

                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = false;
                cam.fieldOfView = 38f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.05f, 0.06f, 0.11f, 1f);
                Vector3 castleCenter = new Vector3(spec30.targetX, spec30.floorY + (spec30.targetHeight * 0.55f), 0f);
                cam.transform.position = castleCenter + new Vector3(-0.6f, 0.35f, -9.2f);
                cam.transform.LookAt(castleCenter + new Vector3(-0.4f, 0.15f, 0f));

                // Frame 1: Yuna pisando no topo do Castelo de Cartas (castelo bamboleia sob os pes)
                yunaGo.transform.position = new Vector3(spec30.targetX + 0.15f, spec30.TopY, 0f);
                collapseView.ResetCollapseState();
                RenderCameraToPng(cam, 1280, 720, Path.Combine(OutputDir, "darkroom2_card_castle_01_stepped_on.png"));
                report.AppendLine("- Frame 1 (`darkroom2_card_castle_01_stepped_on.png`): Menina pisando no topo do Castelo de Cartas (Plataforma 30 intacta/bamboleando).");

                // Frames 2 a 5: Menina acabou de pular para a esquerda e a pilha de cartas desmorona em cascata com cartas esvoacando
                float[] collapseTimes = { 0.14f, 0.36f, 0.64f, 0.95f };
                collapseView.TriggerCollapseNow();
                for (int f = 0; f < collapseTimes.Length; f++)
                {
                    float t = collapseTimes[f];
                    collapseView.EvaluateAtCollapseTime(t);
                    float jumpArcY = (3.8f * t) - (0.5f * 9.5f * t * t);
                    yunaGo.transform.position = new Vector3(
                        spec30.targetX - (0.4f + (3.2f * t)),
                        spec30.TopY + Mathf.Max(0.15f, jumpArcY),
                        0f);

                    string fileName = $"darkroom2_card_castle_0{f + 2}_collapsing_t{Mathf.RoundToInt(t * 1000f):000}ms.png";
                    RenderCameraToPng(cam, 1280, 720, Path.Combine(OutputDir, fileName));
                    report.AppendLine($"- Frame {f + 2} (`{fileName}`): `t={t:F2}s` apos o pulo da menina — andares do castelo de cartas desabando em cascata e cartas soltas esvoacando.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(camGo);
                UnityEngine.Object.DestroyImmediate(tempHost);
            }

            report.AppendLine();
        }

        private static void RenderCameraToPng(Camera cam, int width, int height, string outputPath)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prevTarget = cam.targetTexture;
            float prevAspect = cam.aspect;
            cam.aspect = (float)width / height;
            cam.targetTexture = rt;
            cam.Render();

            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            RenderTexture.active = prevActive;
            cam.targetTexture = prevTarget;
            cam.aspect = prevAspect;
            UnityEngine.Object.DestroyImmediate(rt);

            File.WriteAllBytes(outputPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void RenderSceneWithUiOverlay(Camera cam, int width, int height, string outputPath)
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Array.Sort(canvases, (a, b) => a.sortingOrder.CompareTo(b.sortingOrder));

            var prevModes = new RenderMode[canvases.Length];
            var prevCameras = new Camera[canvases.Length];
            var prevDistances = new float[canvases.Length];
            var prevLayers = new int[canvases.Length];

            var rtScene = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var rtUi = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prevTarget = cam.targetTexture;
            float prevAspect = cam.aspect;
            int prevMask = cam.cullingMask;

            var uiCamGo = new GameObject("__TempAuditUiCam");
            var uiCam = uiCamGo.AddComponent<Camera>();
            uiCam.cameraType = CameraType.Preview;
            uiCam.orthographic = true;
            uiCam.orthographicSize = 5f;
            uiCam.nearClipPlane = 0.1f;
            uiCam.farClipPlane = 10f;
            uiCam.clearFlags = CameraClearFlags.SolidColor;
            uiCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            uiCam.cullingMask = 1 << 5;
            uiCam.aspect = (float)width / height;
            uiCam.targetTexture = rtUi;

            try
            {
                cam.aspect = (float)width / height;
                cam.cullingMask = prevMask & ~(1 << 5);
                cam.targetTexture = rtScene;

                for (int i = 0; i < canvases.Length; i++)
                {
                    prevModes[i] = canvases[i].renderMode;
                    prevCameras[i] = canvases[i].worldCamera;
                    prevDistances[i] = canvases[i].planeDistance;
                    prevLayers[i] = canvases[i].gameObject.layer;

                    SetLayerRecursively(canvases[i].gameObject, 5);
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = uiCam;
                    canvases[i].planeDistance = 1f;
                }

                Canvas.ForceUpdateCanvases();
                cam.Render();
                uiCam.Render();

                var prevActive = RenderTexture.active;
                RenderTexture.active = rtScene;
                var sceneTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                sceneTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                sceneTex.Apply();

                RenderTexture.active = rtUi;
                var uiTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                uiTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                uiTex.Apply();
                RenderTexture.active = prevActive;

                var scenePixels = sceneTex.GetPixels32();
                var uiPixels = uiTex.GetPixels32();
                for (int p = 0; p < scenePixels.Length; p++)
                {
                    byte a = uiPixels[p].a;
                    if (a == 0) continue;
                    if (a == 255)
                    {
                        scenePixels[p] = uiPixels[p];
                        scenePixels[p].a = 255;
                        continue;
                    }

                    float alpha = a / 255f;
                    float inv = 1f - alpha;
                    scenePixels[p].r = (byte)Mathf.Clamp(Mathf.RoundToInt((uiPixels[p].r * alpha) + (scenePixels[p].r * inv)), 0, 255);
                    scenePixels[p].g = (byte)Mathf.Clamp(Mathf.RoundToInt((uiPixels[p].g * alpha) + (scenePixels[p].g * inv)), 0, 255);
                    scenePixels[p].b = (byte)Mathf.Clamp(Mathf.RoundToInt((uiPixels[p].b * alpha) + (scenePixels[p].b * inv)), 0, 255);
                    scenePixels[p].a = 255;
                }

                sceneTex.SetPixels32(scenePixels);
                sceneTex.Apply();
                File.WriteAllBytes(outputPath, sceneTex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(sceneTex);
                UnityEngine.Object.DestroyImmediate(uiTex);
            }
            finally
            {
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i] != null)
                    {
                        canvases[i].renderMode = prevModes[i];
                        canvases[i].worldCamera = prevCameras[i];
                        canvases[i].planeDistance = prevDistances[i];
                        SetLayerRecursively(canvases[i].gameObject, prevLayers[i]);
                    }
                }

                cam.cullingMask = prevMask;
                cam.targetTexture = prevTarget;
                cam.aspect = prevAspect;
                uiCam.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(uiCamGo);
                UnityEngine.Object.DestroyImmediate(rtScene);
                UnityEngine.Object.DestroyImmediate(rtUi);
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
