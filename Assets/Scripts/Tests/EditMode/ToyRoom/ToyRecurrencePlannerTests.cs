using NUnit.Framework;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Tests
{
    public class ToyRecurrencePlannerTests
    {
        [Test]
        public void FirstFiveToysOrganizeNormallyWithoutRecurrence()
        {
            var planner = new ToyRecurrencePlanner();

            for (int i = 0; i < 5; i++)
            {
                string id = ToyRoomCatalog.Toys[i].Id;
                Assert.IsTrue(planner.TryPickUp(id));
                Assert.IsTrue(planner.TryStoreInChest(id));
                Assert.AreEqual(ToyPlacementState.Organized, planner.GetState(id));
            }

            Assert.IsFalse(planner.IsRecurrenceActive);
            Assert.AreEqual(5, planner.OrganizedCount);
        }

        [Test]
        public void SixthAndLastTwoToysActivateSilentRecurrenceAndRespawnOutsideMainFocus()
        {
            var planner = new ToyRecurrencePlanner();

            // 1..5 guardam normalmente (restam 3 na sala)
            for (int i = 0; i < 5; i++)
            {
                string id = ToyRoomCatalog.Toys[i].Id;
                Assert.IsTrue(planner.TryPickUp(id));
                Assert.IsTrue(planner.TryStoreInChest(id));
            }

            // Ao guardar o 6o (restam 2), a recorrencia ativa silenciosamente
            string sixthId = ToyRoomCatalog.Toys[5].Id;
            Assert.IsTrue(planner.TryPickUp(sixthId));
            Assert.IsTrue(planner.TryStoreInChest(sixthId));
            Assert.IsTrue(planner.IsRecurrenceActive);
            Assert.AreEqual(ToyPlacementState.PendingReturn, planner.GetState(sixthId));

            string respawnedId = null;
            ToyRoomPoint respawnedPoint = default;
            planner.OnToyRespawned += (id, pt) =>
            {
                respawnedId = id;
                respawnedPoint = pt;
            };

            // Antes de 1.5s nao reaparece
            planner.Tick(1.0f, new ToyRoomPoint(2.4f, 4.1f), pt => pt.X > -3f && pt.X < 5f && pt.Z > -1f);
            Assert.IsNull(respawnedId);

            // Apos 1.5s reaparece fora do foco principal da camera e longe da Yuna
            planner.Tick(0.6f, new ToyRoomPoint(2.4f, 4.1f), pt => pt.X > -3f && pt.X < 5f && pt.Z > -1f);
            Assert.AreEqual(sixthId, respawnedId);
            Assert.AreEqual(ToyPlacementState.OnFloor, planner.GetState(sixthId));
            Assert.IsFalse(respawnedPoint.X > -3f && respawnedPoint.X < 5f && respawnedPoint.Z > -1f);
        }

        [Test]
        public void StrangeDialogueAndInvestigationTriggerAtTwoAndFourReturnedToys()
        {
            var planner = new ToyRecurrencePlanner();
            bool strangeFired = false;
            bool investigationFired = false;

            planner.OnStrangeDialogueReached += () => strangeFired = true;
            planner.OnInvestigationReached += () => investigationFired = true;

            for (int i = 0; i < 6; i++)
            {
                string id = ToyRoomCatalog.Toys[i].Id;
                planner.TryPickUp(id);
                planner.TryStoreInChest(id);
            }

            string recurringId = ToyRoomCatalog.Toys[5].Id;

            for (int cycle = 1; cycle <= 4; cycle++)
            {
                planner.Tick(2.0f, new ToyRoomPoint(0f, 0f), _ => false);
                Assert.IsTrue(planner.TryPickUp(recurringId));
                Assert.IsTrue(planner.TryStoreInChest(recurringId));

                if (cycle < 2)
                {
                    Assert.IsFalse(strangeFired);
                }
                else
                {
                    Assert.IsTrue(strangeFired);
                }

                if (cycle < 4)
                {
                    Assert.IsFalse(investigationFired);
                }
                else
                {
                    Assert.IsTrue(investigationFired);
                }
            }
        }
    }
}
