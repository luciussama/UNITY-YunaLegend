using System.IO;
using NUnit.Framework;
using YunaLegend.Illustrated25D.Preview.Controllers;

namespace YunaLegend.Illustrated25D.Tests
{
    public sealed class DarkRoomFullStageCatalogTests
    {
        [Test]
        public void FullStageCatalog_ContainsAll28Platforms_WithValidGeometryAndDirections()
        {
            IllustratedPlatformSpec[] specs = DarkRoomFullStageCatalog.BuildSpecsUpTo(37);
            Assert.AreEqual(28, specs.Length);

            for (int i = 0; i < specs.Length; i++)
            {
                int expectedIndex = 10 + i;
                Assert.AreEqual(expectedIndex, specs[i].index);
                Assert.Greater(specs[i].PixelsPerMetre, 10f, $"Platform {expectedIndex} PPM");
                Assert.Greater(specs[i].SupportWidth, 1.5f, $"Platform {expectedIndex} SupportWidth");
                Assert.Greater(specs[i].FullWidth, 1.5f, $"Platform {expectedIndex} FullWidth");
            }

            // Stage 2 (10..21) moves right and climbs
            for (int i = 1; i <= 11; i++)
            {
                Assert.Greater(specs[i].targetX, specs[i - 1].targetX, $"Stage 2 X {10 + i}");
                Assert.Greater(specs[i].TopY, specs[i - 1].TopY, $"Stage 2 TopY {10 + i}");
            }

            // Stage 3 (22..37) starts at floor and climbs right-to-left
            Assert.AreEqual(DarkRoomFullStageCatalog.RoomFloorY, specs[12].floorY, 0.001f);
            Assert.Less(specs[12].targetX, specs[11].targetX);
            for (int i = 13; i < specs.Length; i++)
            {
                Assert.Less(specs[i].targetX, specs[i - 1].targetX, $"Stage 3 X {10 + i}");
                Assert.Greater(specs[i].TopY, specs[i - 1].TopY, $"Stage 3 TopY {10 + i}");
            }

            Assert.Greater(specs[specs.Length - 1].targetX, -5f, "Portal 37 remains in room bounds");
        }

        [Test]
        public void FullStageCatalog_All28TextureAssetsExistOnDisk()
        {
            for (int index = 10; index <= 37; index++)
            {
                string path = DarkRoomFullStageCatalog.GetTextureAssetPath(index);
                Assert.IsFalse(string.IsNullOrEmpty(path), $"Missing path for {index}");
                Assert.IsTrue(File.Exists(path), $"Texture file missing on disk: {path}");
            }
        }

        [Test]
        public void CardCastleCollapseTimeline_EvaluatesSteppedWobble_AndCascadingCollapseAfterJump()
        {
            Assert.AreEqual(30, CardCastleCollapseTimeline.PlatformIndex);

            // Enquanto Yuna pisa nas cartas, os andares bamboleiam sutilmente sem cair
            CardCastleTierSample wobbleTop = CardCastleCollapseTimeline.EvaluateSteppedWobble(3, 0.15f);
            Assert.AreEqual(1f, wobbleTop.Alpha, 0.001f);
            Assert.Greater(System.Math.Abs(wobbleTop.RotationDegrees), 0.1f);
            Assert.Greater(wobbleTop.OffsetYMetres, -0.10f);

            // Logo apos o pulo (t=0.45s), o topo (Tier 3) cede primeiro seguido pelos andares inferiores
            CardCastleTierSample collapseTop = CardCastleCollapseTimeline.EvaluateCollapseTier(3, 0.45f);
            CardCastleTierSample collapseBase = CardCastleCollapseTimeline.EvaluateCollapseTier(0, 0.45f);
            Assert.Less(collapseTop.OffsetYMetres, collapseBase.OffsetYMetres, "Andar do topo deve cair na frente da base em cascata.");
            Assert.Less(collapseBase.OffsetYMetres, -0.30f, "Base do castelo tambem deve desabar.");

            // Cartas individuais inteiras esvoacam e giram durante o colapso
            CardCastleFlutterCardSample flutter = CardCastleCollapseTimeline.EvaluateFlutterCard(14, 0.35f);
            Assert.Greater(flutter.Alpha, 0.5f);
            Assert.Greater(System.Math.Abs(flutter.RotationDegrees), 15f);
        }
    }
}
