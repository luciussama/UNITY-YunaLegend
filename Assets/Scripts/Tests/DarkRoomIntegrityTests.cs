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
        public void Test_RampPlatformGenerator_GeneratesFullTrack()
        {
            var ascent = DarkRoomRampPlatformGenerator.GenerateAscentRampPlatforms();
            var returnTrack = DarkRoomRampPlatformGenerator.GenerateReturnChaoticPlatforms();

            Assert.AreEqual(22, ascent.Count, "A subida continua deve ter 22 plataformas (0 a 21)");
            Assert.AreEqual(16, returnTrack.Count, "A volta caotica deve ter 16 plataformas (22 a 37)");
            Assert.AreEqual("21_Poster_Porta_Falsa_Parede", ascent[21].name);
            Assert.AreEqual("37_Verdadeiro_Portal_Sonhos", returnTrack[15].name);
        }
    }
}
