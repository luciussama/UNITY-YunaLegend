using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Dialogue;

namespace YunaLegend.Tests
{
    [TestFixture]
    public class DarkRoomIntegrityTests
    {
        [Test]
        public void Test_PersistedPostProcessingChainIncludesCameraVolumeAndRendererResources()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            try
            {
                var camera = Camera.main;
                var data = new UnityEditor.SerializedObject(camera.GetComponent("UniversalAdditionalCameraData"));
                Assert.IsTrue(data.FindProperty("m_RenderPostProcessing").boolValue);
                Assert.IsTrue(camera.allowHDR);
                var volumeGo = GameObject.Find("DarkRoom_GlobalVolume");
                var volume = new UnityEditor.SerializedObject(volumeGo.GetComponent("Volume"));
                var profile = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/DefaultVolumeProfile.asset");
                Assert.AreSame(profile, volume.FindProperty("sharedProfile").objectReferenceValue, "The runtime profile property must not replace a persisted sharedProfile link.");
                Assert.AreNotEqual(0, data.FindProperty("m_VolumeLayerMask").intValue & (1 << volumeGo.layer));
                var renderer = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Settings/DarkRoomRenderer.asset"));
                Assert.IsNotNull(renderer.FindProperty("postProcessData").objectReferenceValue);
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            }
        }

        [Test]
        public void Test_PersistedSceneStartsOnFloorWithOnlyStage1Visible()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var player = UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
            Assert.AreEqual(DarkRoomStartLayout.Position, player.transform.position);
            var platforms = UnityEngine.Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(38, platforms.Length);
            foreach (var marker in platforms)
                Assert.AreEqual(marker.spec.index <= 9, marker.gameObject.activeInHierarchy, $"Visibility {marker.spec.index}");
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
        }

        [Test]
        public void Test_CameraSnapUsesSignedOffsetOnRetry()
        {
            var go = new GameObject("RetryCamera");
            go.AddComponent<Camera>();
            var camera = go.AddComponent<SideScroll25DCamera>();
            camera.SetScrollState(false, -1f);
            camera.SnapToTargetX(90f, -3.5f);
            Assert.AreEqual(86.5f, go.transform.position.x);
            camera.SnapToTargetX(-2.5f, 3.5f);
            Assert.AreEqual(1f, go.transform.position.x);
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void Test_CatalogPlatformCounts_MatchesRoteiroExactly()
        {
            Assert.AreEqual(10, DarkRoomPlatformCatalog.Phase1Platforms.Length, "Fase 1 deve ter exatamente 10 plataformas (0 a 9)");
            Assert.AreEqual(12, DarkRoomPlatformCatalog.Phase2Platforms.Length, "Fase 2 deve ter exatamente 12 plataformas (10 a 21)");
            Assert.AreEqual(16, DarkRoomPlatformCatalog.Phase3Platforms.Length, "Fase 3 deve ter exatamente 16 plataformas (22 a 37, incluindo portal final)");

            int total = DarkRoomPlatformCatalog.Phase1Platforms.Length
                        + DarkRoomPlatformCatalog.Phase2Platforms.Length
                        + DarkRoomPlatformCatalog.Phase3Platforms.Length;
            Assert.AreEqual(38, total, "O catalogo completo deve mapear os 38 pontos do roteiro");
        }

        [Test]
        public void Test_CatalogPhaseContinuity_IndexesAreSequential()
        {
            int expected = 0;
            foreach (var p in DarkRoomPlatformCatalog.Phase1Platforms)
            {
                Assert.AreEqual(expected++, p.index);
            }
            foreach (var p in DarkRoomPlatformCatalog.Phase2Platforms)
            {
                Assert.AreEqual(expected++, p.index);
            }
            foreach (var p in DarkRoomPlatformCatalog.Phase3Platforms)
            {
                Assert.AreEqual(expected++, p.index);
            }
        }

        [Test]
        public void Test_OpeningDialogueScript_MatchesScriptTexts()
        {
            StringAssert.Contains("Levanta, dorminhoca", DarkRoomDialogueScript.OpeningLine1.text);
            StringAssert.Contains("O quarto era o mesmo", DarkRoomDialogueScript.PoeticOpening1);
            StringAssert.Contains("LEVANTA, SUA PREGUICOSA", DarkRoomDialogueScript.OpeningLine2.text);
            StringAssert.Contains("Mas era ali. Eu tinha certeza", DarkRoomDialogueScript.FalseDoorYuna.text);
            StringAssert.Contains("A madeira caiu", DarkRoomDialogueScript.PoeticFalseDoor);
            StringAssert.Contains("porta que so se abre", DarkRoomDialogueScript.PoeticTruePortal);
        }

        [Test]
        public void Test_AssetsPresence_FontsAndSoundsExistOnDisk()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fontPath = Path.Combine(projectRoot, "Assets", "Fonts", "Caveat.ttf");
            string jumpPath = Path.Combine(projectRoot, "Assets", "Audio", "SFX", "jump.ogg");
            string landPath = Path.Combine(projectRoot, "Assets", "Audio", "SFX", "land.ogg");
            string modelPath = Path.Combine(projectRoot, "Assets", "ThirdParty", "Models", "character-starter.glb");

            Assert.IsTrue(File.Exists(fontPath), "A fonte Caveat.ttf deve existir no projeto");
            Assert.IsTrue(File.Exists(jumpPath), "O SFX jump.ogg deve existir no projeto");
            Assert.IsTrue(File.Exists(landPath), "O SFX land.ogg deve existir no projeto");
            Assert.IsTrue(File.Exists(modelPath), "O modelo de referencia character-starter.glb deve existir");
        }

        [Test]
        public void Test_Progression_InitialStateIsPhase1Ascent()
        {
            var go = new GameObject("TestProgression");
            var progression = go.AddComponent<DarkRoomProgressionManager>();

            Assert.AreEqual(DarkRoomPhase.Phase1Ascent, progression.CurrentPhase);
            Assert.AreEqual(-1, progression.CurrentPlatformIndex);

            progression.NotifyPlatformLanded(0);
            Assert.AreEqual(0, progression.CurrentPlatformIndex);

            // Na subida contínua, Yuna atinge o Poster da Porta Falsa na plataforma 21
            progression.NotifyPlatformLanded(21);
            Assert.AreEqual(21, progression.CurrentPlatformIndex);

            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void Test_ThreeStageCircuitGenerator_MatchesRoteiroSpecifications()
        {
            var stage1 = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms();
            var stage2 = DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms();
            var stage3 = DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms();

            Assert.AreEqual(10, stage1.Count, "Etapa 1 deve ter 10 plataformas (0 a 9)");
            Assert.AreEqual(12, stage2.Count, "Etapa 2 deve ter 12 plataformas (10 a 21)");
            Assert.AreEqual(16, stage3.Count, "Etapa 3 deve ter 16 plataformas (22 a 37)");

            Assert.AreEqual("09_Castelinho_Blocos_Descanso", stage1[9].name);
            Assert.AreEqual("21_Poster_Porta_Falsa_Parede", stage2[11].name);
            Assert.AreEqual("37_O_Verdadeiro_Portal_Sonhos", stage3[15].name);

            // Valida que a Etapa 3 avança para a ESQUERDA (X decrescente)
            Assert.Greater(stage3[0].x, stage3[1].x, "Etapa 3 deve correr no sentido X decrescente (para a esquerda)");
        }

        [Test]
        public void Test_ThreeStageCircuit_NoOverlapBetweenStages()
        {
            var stage1 = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms();
            var lastStage1 = stage1[stage1.Count - 1];
            float stage1RightEdge = lastStage1.x + (lastStage1.width * 0.5f);

            var stage2 = DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms();
            var firstStage2 = stage2[0];
            float stage2LeftEdge = firstStage2.x - (firstStage2.width * 0.5f);

            Assert.Greater(stage2LeftEdge, stage1RightEdge,
                "A Plataforma 10 da Etapa 2 deve comecar APOS a borda direita do Castelinho da Etapa 1 sem sobreposicao");

            var lastStage2 = stage2[stage2.Count - 1];
            float posterX = lastStage2.x;

            var stage3 = DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms(posterX);
            Assert.Less(stage3[0].x, posterX, "A Etapa 3 deve iniciar a esquerda da parede do poster");
        }

        [Test]
        public void Test_PhysicalJumpSimulation_All38PlatformsReachable()
        {
            float g = 28f;
            float fallMult = 1.8f;
            float gFall = g * fallMult;

            // Simula Etapa 1 (0 a 9): v=3.6, jumpH=2.4
            var s1 = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms();
            for (int i = 0; i < s1.Count - 1; i++)
            {
                float vX = 3.6f;
                float jumpH = 2.4f;
                float deltaY = s1[i + 1].y - s1[i].y;
                float gap = (s1[i + 1].x - s1[i + 1].width * 0.5f) - (s1[i].x + s1[i].width * 0.5f);

                float vY0 = Mathf.Sqrt(2f * g * jumpH);
                float tUp = vY0 / g;
                float fallDist = jumpH - deltaY;
                Assert.Greater(fallDist, 0f, $"Pulo da plat {i} deve atingir altura superior a deltaY");

                float tDown = Mathf.Sqrt(2f * fallDist / gFall);
                float reach = vX * (tUp + tDown);
                Assert.Greater(reach, gap, $"Alcance do pulo ({reach:F2}m) deve superar o vao ({gap:F2}m) entre plat {i} e {i+1}");
            }

            // Simula Etapa 2 (10 a 21) com escala de velocidade e pulo
            var s2 = DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms();
            for (int i = 0; i < s2.Count - 1; i++)
            {
                float progress = (float)i / (s2.Count - 1);
                float vX = Mathf.Lerp(4.0f, 6.0f, progress);
                float jumpH = Mathf.Lerp(2.5f, 3.4f, progress);
                float deltaY = s2[i + 1].y - s2[i].y;
                float gap = (s2[i + 1].x - s2[i + 1].width * 0.5f) - (s2[i].x + s2[i].width * 0.5f);

                float vY0 = Mathf.Sqrt(2f * g * jumpH);
                float tUp = vY0 / g;
                float fallDist = jumpH - deltaY;
                Assert.Greater(fallDist, 0f);

                float tDown = Mathf.Sqrt(2f * fallDist / gFall);
                float reach = vX * (tUp + tDown);
                Assert.Greater(reach, gap, $"Alcance do pulo ({reach:F2}m) deve superar o vao ({gap:F2}m) entre plat {10+i} e {10+i+1}");
            }

            // Simula Etapa 3 (22 a 37) no sentido reverso
            var s3 = DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms();
            for (int i = 0; i < s3.Count - 1; i++)
            {
                float progress = (float)i / (s3.Count - 1);
                float vX = Mathf.Lerp(DarkRoomReturnLayout.RunSpeed, DarkRoomReturnLayout.EndRunSpeed, progress);
                float jumpH = Mathf.Lerp(DarkRoomReturnLayout.JumpHeight, DarkRoomReturnLayout.EndJumpHeight, progress);
                float deltaY = (s3[i + 1].y + s3[i + 1].height * 0.5f) - (s3[i].y + s3[i].height * 0.5f);
                float gap = (s3[i].x - s3[i].width * 0.5f) - (s3[i + 1].x + s3[i + 1].width * 0.5f);

                float vY0 = Mathf.Sqrt(2f * g * jumpH);
                float tUp = vY0 / g;
                float fallDist = jumpH - deltaY;
                Assert.Greater(fallDist, 0f);

                float tDown = Mathf.Sqrt(2f * fallDist / gFall);
                float reach = vX * (tUp + tDown);
                Assert.Greater(reach, gap, $"Alcance reverso ({reach:F2}m) deve superar o vao ({gap:F2}m) entre plat {22+i} e {22+i+1}");
            }
        }

        [Test]
        public void Test_CastleCutscene_RequiresInteractiveInputProgression()
        {
            var go = new GameObject("ProgressionTest");
            var progression = go.AddComponent<DarkRoomProgressionManager>();

            // Simula aterrissagem na plataforma 9 (Castelinho)
            progression.NotifyPlatformLanded(9);

            Assert.AreEqual(CutsceneType.CastleRest, progression.ActiveCutscene, "Deve ativar a cutscene do castelo");
            Assert.AreEqual(0, progression.CutsceneStep, "Passo inicial deve ser 0");
            Assert.IsNotNull(progression.ActiveDialogue, "Deve exibir dialogo da Nanda");
            Assert.AreEqual(SpeakerId.Nanda, progression.ActiveDialogue.speaker);
            StringAssert.Contains("vem comigo", progression.ActiveDialogue.text);

            // Jogador pressiona para avancar o dialogo
            progression.AdvanceCutscene();
            Assert.AreEqual(1, progression.CutsceneStep, "Passo deve avancar para 1");
            Assert.AreEqual(SpeakerId.Menina, progression.ActiveDialogue.speaker);
            StringAssert.Contains("Daqui parece perto", progression.ActiveDialogue.text);

            // Jogador confirma o passo 2 -> Inicia a Fase 2 (A Fuga)
            progression.AdvanceCutscene();
            Assert.AreEqual(CutsceneType.None, progression.ActiveCutscene);
            Assert.AreEqual(DarkRoomPhase.Phase2Escape, progression.CurrentPhase);
            Assert.AreEqual(DarkRoomPhase.Phase2Escape, progression.CurrentCheckpoint);

            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void Test_FalseDoorPoster_TriggersCinematicFallAndDialogue()
        {
            var go = new GameObject("ProgressionTest");
            var playerGo = new GameObject("PlayerTest");
            var cc = playerGo.AddComponent<CharacterController>();
            var player = playerGo.AddComponent<Yuna25DPlayerController>();
            var progression = go.AddComponent<DarkRoomProgressionManager>();

            // Vincula player ao progression
            progression.BindPlayer(player);

            // Pula para Fase 2
            progression.StartStage2Escape();

            // Chega na plataforma 21 (Poster)
            progression.NotifyPlatformLanded(21);
            Assert.IsTrue(player.IsCinematicFalling, "Yuna deve iniciar queda cinematica real sob gravidade");

            // Simula aterrissagem no chao
            progression.HandleCinematicFallLanded();

            // Verifica cutscene do Plot Twist
            Assert.AreEqual(CutsceneType.FalseDoorPlotTwist, progression.ActiveCutscene);
            Assert.AreEqual(0, progression.CutsceneStep);
            Assert.AreEqual(SpeakerId.Menina, progression.ActiveDialogue.speaker);
            Assert.IsTrue(progression.ActiveDialogue.isPlotTwistStyle, "Dialogo da Menina no plot twist deve ter estilo especial");

            // Avanca dialogo
            progression.AdvanceCutscene();
            Assert.AreEqual(1, progression.CutsceneStep);
            Assert.AreEqual(SpeakerId.Nanda, progression.ActiveDialogue.speaker);
            StringAssert.Contains("Droga... Vamos olhar por ali", progression.ActiveDialogue.text);

            // Conclui dialogo -> inicia Fase 3 reversa
            progression.AdvanceCutscene();
            Assert.AreEqual(DarkRoomPhase.Phase3ChaoticAscent, progression.CurrentPhase);
            Assert.AreEqual(-1f, player.RunDirection, "Direcao do jogador deve ser invertida para -1 (esquerda)");

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(playerGo);
        }

        [Test]
        public void Test_CheckpointsAndRetry()
        {
            var go = new GameObject("ProgressionTest");
            var playerGo = new GameObject("PlayerTest");
            var cc = playerGo.AddComponent<CharacterController>();
            var player = playerGo.AddComponent<Yuna25DPlayerController>();
            var progression = go.AddComponent<DarkRoomProgressionManager>();

            progression.BindPlayer(player);

            progression.HandleOpeningCompleted();
            Assert.AreEqual(DarkRoomPhase.Phase1Ascent, progression.CurrentCheckpoint);

            // Simula morte na Fase 1
            player.TriggerDeath("Queda teste");
            Assert.IsTrue(player.IsDead);
            Assert.IsTrue(progression.IsGameOver);

            // Executa Retry
            progression.RetryCurrentCheckpoint();
            Assert.IsFalse(player.IsDead);
            Assert.IsFalse(progression.IsGameOver);
            Assert.AreEqual(DarkRoomPhase.Phase1Ascent, progression.CurrentPhase);

            // Avanca para Fase 2
            progression.StartStage2Escape();
            Assert.AreEqual(DarkRoomPhase.Phase2Escape, progression.CurrentCheckpoint);

            player.TriggerDeath("Queda teste fase 2");
            progression.RetryCurrentCheckpoint();
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(DarkRoomPhase.Phase2Escape, progression.CurrentPhase);
            Assert.AreEqual(1f, player.RunDirection);

            // Avanca para Fase 3
            progression.StartPhase3AfterPlotTwist();
            Assert.AreEqual(DarkRoomPhase.Phase3ChaoticAscent, progression.CurrentCheckpoint);

            player.TriggerDeath("Queda teste fase 3");
            progression.RetryCurrentCheckpoint();
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(DarkRoomPhase.Phase3ChaoticAscent, progression.CurrentPhase);
            Assert.AreEqual(-1f, player.RunDirection, "Na Fase 3 o retry deve retomar correndo para a esquerda");

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(playerGo);
        }

        [Test]
        public void Test_CameraSectionFallbackKeepsLensAndRotationStable()
        {
            var camGo = new GameObject("CamTest");
            var cam = camGo.AddComponent<Camera>();
            var sideCam = camGo.AddComponent<SideScroll25DCamera>();

            sideCam.ConfigureSection(-4.5f, -16.0f, 45.0f);
            Assert.AreEqual(-4.5f, sideCam.FixedY);
            Assert.AreEqual(-16.0f, sideCam.FixedZ);
            Assert.AreEqual(45.0f, sideCam.FixedFov);
            Assert.AreEqual(45.0f, cam.fieldOfView);
            Assert.Less(Quaternion.Angle(Quaternion.identity, camGo.transform.rotation), 0.01f);

            sideCam.ConfigureSection(-1.5f, -16.0f, 45.0f);
            Assert.AreEqual(-1.5f, sideCam.FixedY);

            GameObject.DestroyImmediate(camGo);
        }

        [Test]
        public void Test_YunaAutoRunner_CanBeResetAndStartsRunning()
        {
            var go = new GameObject("TestRunner");
            var cc = go.AddComponent<CharacterController>();
            var player = go.AddComponent<Yuna25DPlayerController>();

            player.ResetPlayer(new Vector3(2f, -5f, 0f), 1f);
            Assert.IsTrue(player.AutoRunActive, "AutoRunActive deve ser verdadeiro apos reset");
            Assert.IsFalse(player.IsDead, "Jogador nao deve comecar morto");
            Assert.AreEqual(1f, player.RunDirection, "Direcao inicial deve ser para a direita (+1)");

            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void Test_TemporalSectionVisibility_OnlyCurrentSectionVisible()
        {
            var root = new GameObject("BlockoutRoot");
            var builder = root.AddComponent<DarkRoomBlockoutBuilder>();
            builder.BuildBlockout();

            var markers = root.GetComponentsInChildren<DarkRoomPlatformMarker>(true);
            Assert.AreEqual(38, markers.Length);

            // Na Fase 1 (Subida): apenas 0 a 9 visíveis; 10 a 37 desativadas
            builder.ShowTemporalSection(DarkRoomPhase.Phase1Ascent);
            foreach (var m in markers)
            {
                if (m.spec.index <= 9)
                    Assert.IsTrue(m.gameObject.activeSelf, $"Plataforma {m.spec.index} deve estar visivel na Fase 1");
                else
                    Assert.IsFalse(m.gameObject.activeSelf, $"Plataforma {m.spec.index} DEVE estar invisivel na Fase 1");
            }

            // Na Fase 2 (Fuga): apenas 10 a 21 visíveis (e 9 enquanto ainda no início); 0 a 8 e 22 a 37 desativadas
            builder.ShowTemporalSection(DarkRoomPhase.Phase2Escape, 10);
            foreach (var m in markers)
            {
                if (m.spec.index >= 10 && m.spec.index <= 21)
                    Assert.IsTrue(m.gameObject.activeSelf, $"Plataforma {m.spec.index} deve estar visivel na Fase 2");
                else
                    Assert.IsFalse(m.gameObject.activeSelf, $"Plataforma {m.spec.index} DEVE estar invisivel na Fase 2");
            }

            // Na Queda do Poster: tudo desativado temporariamente
            builder.ShowTemporalSection(DarkRoomPhase.Phase2Escape, 21, falling: true);
            foreach (var m in markers)
            {
                Assert.IsFalse(m.gameObject.activeSelf, $"Plataforma {m.spec.index} deve estar invisivel durante a queda");
            }

            // Na Fase 3 (Volta Caotica): apenas 22 a 37 visíveis; 0 a 21 desativadas
            builder.ShowTemporalSection(DarkRoomPhase.Phase3ChaoticAscent);
            foreach (var m in markers)
            {
                if (m.spec.index >= 22)
                    Assert.IsTrue(m.gameObject.activeSelf, $"Plataforma {m.spec.index} deve estar visivel na Fase 3");
                else
                    Assert.IsFalse(m.gameObject.activeSelf, $"Plataforma {m.spec.index} DEVE estar invisivel na Fase 3");
            }

            GameObject.DestroyImmediate(root);
        }
    }
}
