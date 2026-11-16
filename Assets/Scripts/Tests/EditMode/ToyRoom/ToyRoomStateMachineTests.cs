using NUnit.Framework;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Tests
{
    public class ToyRoomStateMachineTests
    {
        [Test]
        public void FullToyRoomFlowReachesCompletedAfterSwordPickup()
        {
            var machine = new ToyRoomStateMachine();
            Assert.AreEqual(ToyRoomPhase.Intro, machine.CurrentPhase);

            machine.CompleteIntro();
            Assert.AreEqual(ToyRoomPhase.Exploring, machine.CurrentPhase);

            // Guarda 6 brinquedos -> entra em RecurrenceMystery
            for (int i = 0; i < 6; i++)
            {
                string id = ToyRoomCatalog.Toys[i].Id;
                Assert.IsTrue(machine.TryPickUpToy(id));
                Assert.IsTrue(machine.TryStoreToyInChest(id));
            }

            Assert.AreEqual(ToyRoomPhase.RecurrenceMystery, machine.CurrentPhase);

            // Re-guarda brinquedo retornado 4 vezes -> InvestigationCutscene
            string recurringId = ToyRoomCatalog.Toys[5].Id;
            for (int cycle = 0; cycle < 4; cycle++)
            {
                machine.Tick(2.0f, new ToyRoomPoint(0f, 0f), _ => false);
                Assert.IsTrue(machine.TryPickUpToy(recurringId));
                Assert.IsTrue(machine.TryStoreToyInChest(recurringId));
            }

            Assert.AreEqual(ToyRoomPhase.InvestigationCutscene, machine.CurrentPhase);

            machine.CompleteInvestigationCutscene();
            Assert.AreEqual(ToyRoomPhase.SeekingSword, machine.CurrentPhase);

            // Brinquedos comuns nao podem mais ser coletados apos revelar a espada
            Assert.IsFalse(machine.TryPickUpToy(ToyRoomCatalog.Toys[6].Id));

            bool swordStarted = false;
            bool stageCompleted = false;
            machine.OnSwordPickupStarted += () => swordStarted = true;
            machine.OnStageCompleted += () => stageCompleted = true;

            machine.CollectToySword();
            Assert.IsTrue(swordStarted);
            Assert.AreEqual(ToyRoomPhase.SwordPickup, machine.CurrentPhase);

            machine.CompleteSwordPickup();
            Assert.IsTrue(stageCompleted);
            Assert.AreEqual(ToyRoomPhase.Completed, machine.CurrentPhase);
        }
    }
}
