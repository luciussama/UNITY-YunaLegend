using System.Collections.Generic;
using NUnit.Framework;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Tests
{
    public class ToyRoomCatalogTests
    {
        [Test]
        public void CatalogDefinesEightUniqueToysWithinRoomBounds()
        {
            Assert.AreEqual(8, ToyRoomCatalog.Toys.Count);

            var ids = new HashSet<string>();
            foreach (var toy in ToyRoomCatalog.Toys)
            {
                Assert.IsTrue(ids.Add(toy.Id), $"ID duplicado: {toy.Id}");
                Assert.Greater(toy.WeightMultiplier, 0.5f);
                Assert.LessOrEqual(toy.WeightMultiplier, 1.0f);
                Assert.GreaterOrEqual(toy.InitialPosition.X, -ToyRoomCatalog.HalfWidth);
                Assert.LessOrEqual(toy.InitialPosition.X, ToyRoomCatalog.HalfWidth);
                Assert.GreaterOrEqual(toy.InitialPosition.Z, -ToyRoomCatalog.HalfDepth);
                Assert.LessOrEqual(toy.InitialPosition.Z, ToyRoomCatalog.HalfDepth);
            }
        }

        [Test]
        public void CatalogDefinesSevenFurniturePiecesWithSingleToyChest()
        {
            Assert.AreEqual(7, ToyRoomCatalog.Furniture.Count);

            int chestCount = 0;
            foreach (var furniture in ToyRoomCatalog.Furniture)
            {
                if (furniture.IsToyChest)
                {
                    chestCount++;
                }

                Assert.Greater(furniture.Width, 0f);
                Assert.Greater(furniture.Depth, 0f);
            }

            Assert.AreEqual(1, chestCount);
        }
    }
}
