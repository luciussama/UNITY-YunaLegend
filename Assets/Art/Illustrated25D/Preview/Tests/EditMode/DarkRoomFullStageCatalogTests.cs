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
    }
}
