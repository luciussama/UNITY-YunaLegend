using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.RoutePhysics.Tests
{
    public class ToyRoomPlayModeTests
    {
        [UnityTest]
        public IEnumerator PlayerPicksUpStoresAndTriggersSixthToyRecurrenceInScene()
        {
            yield return LoadToyRoomScene();

            var bootstrap = Object.FindFirstObjectByType<ToyRoomStageBootstrap>();
            var player = Object.FindFirstObjectByType<ToyRoomPlayerController>();
            var chest = Object.FindFirstObjectByType<ToyChestController>();
            var nanda = Object.FindFirstObjectByType<NandaToyRoomGuideController>();

            Assert.IsNotNull(bootstrap);
            Assert.IsNotNull(player);
            Assert.IsNotNull(chest);
            Assert.IsNotNull(nanda);

            bootstrap.SkipActiveCutscene();
            yield return null;
            Assert.AreEqual(ToyRoomPhase.Exploring, bootstrap.StateMachine.CurrentPhase);

            Vector3 startPos = player.transform.position;
            player.ScriptedMoveInput = new Vector2(1f, 0f);
            float moveStart = Time.time;
            while (Time.time - moveStart < 0.25f)
            {
                yield return null;
            }
            player.ScriptedMoveInput = null;
            Assert.Greater(player.transform.position.x, startPos.x + 0.1f);

            var allToys = Object.FindObjectsByType<ToyItemView>(FindObjectsSortMode.None);
            Assert.AreEqual(8, allToys.Length);

            var cc = player.GetComponent<CharacterController>();
            ToyItemView sixthToy = null;
            for (int i = 0; i < 6; i++)
            {
                var toy = allToys[i];
                if (i == 5)
                {
                    sixthToy = toy;
                }

                yield return StoreToy(bootstrap, player, cc, chest, toy, nanda);
            }

            Assert.AreEqual(5, bootstrap.StateMachine.Planner.OrganizedCount);
            Assert.AreEqual(ToyRoomPhase.RecurrenceMystery, bootstrap.StateMachine.CurrentPhase);
            Assert.IsNotNull(sixthToy);
            Assert.IsFalse(sixthToy.gameObject.activeInHierarchy);

            yield return new WaitForSeconds(1.65f);
            Assert.IsTrue(sixthToy.gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator FullMysterySequenceRevealsAndCollectsToySword()
        {
            yield return LoadToyRoomScene();

            var bootstrap = Object.FindFirstObjectByType<ToyRoomStageBootstrap>();
            var player = Object.FindFirstObjectByType<ToyRoomPlayerController>();
            var chest = Object.FindFirstObjectByType<ToyChestController>();
            var nanda = Object.FindFirstObjectByType<NandaToyRoomGuideController>();
            var sword = Object.FindFirstObjectByType<ToySwordView>(FindObjectsInactive.Include);

            Assert.IsNotNull(sword);
            Assert.IsFalse(sword.gameObject.activeInHierarchy);

            bootstrap.SkipActiveCutscene();
            yield return null;

            var allToys = Object.FindObjectsByType<ToyItemView>(FindObjectsSortMode.None);
            var cc = player.GetComponent<CharacterController>();

            for (int i = 0; i < 6; i++)
            {
                yield return StoreToy(bootstrap, player, cc, chest, allToys[i], nanda);
            }

            var recurringToy = allToys[5];
            for (int cycle = 0; cycle < 4; cycle++)
            {
                yield return new WaitForSeconds(1.65f);
                yield return StoreToy(bootstrap, player, cc, chest, recurringToy, nanda);
            }

            Assert.AreEqual(ToyRoomPhase.InvestigationCutscene, bootstrap.StateMachine.CurrentPhase);

            // Pula cutscene de investigacao (revelando a espada no beat 6 ou ao concluir)
            sword.Reveal();
            bootstrap.SkipActiveCutscene();
            yield return null;

            Assert.AreEqual(ToyRoomPhase.SeekingSword, bootstrap.StateMachine.CurrentPhase);
            Assert.IsTrue(sword.gameObject.activeInHierarchy);
            Assert.AreEqual(NandaGuideTargetMode.HoverOverSword, nanda.Mode);

            cc.enabled = false;
            player.transform.position = new Vector3(sword.transform.position.x, 0f, sword.transform.position.z);
            cc.enabled = true;
            yield return null;

            bootstrap.RequestPlayerInteraction();
            Assert.AreEqual(ToyRoomPhase.SwordPickup, bootstrap.StateMachine.CurrentPhase);

            bootstrap.SkipActiveCutscene();
            yield return null;
            Assert.AreEqual(ToyRoomPhase.Completed, bootstrap.StateMachine.CurrentPhase);
        }

        private static IEnumerator LoadToyRoomScene()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/ToyRoom_Gameplay.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/ToyRoom_Gameplay.unity");
#endif
            yield return null;
        }

        private static IEnumerator StoreToy(
            ToyRoomStageBootstrap bootstrap,
            ToyRoomPlayerController player,
            CharacterController cc,
            ToyChestController chest,
            ToyItemView toy,
            NandaToyRoomGuideController nanda)
        {
            cc.enabled = false;
            player.transform.position = new Vector3(toy.transform.position.x, 0f, toy.transform.position.z);
            cc.enabled = true;
            yield return null;

            bootstrap.RequestPlayerInteraction();
            Assert.AreEqual(toy, player.CarriedToy);
            Assert.AreEqual(NandaGuideTargetMode.BeaconOverChest, nanda.Mode);

            cc.enabled = false;
            player.transform.position = new Vector3(chest.transform.position.x, 0f, chest.transform.position.z - 1.3f);
            cc.enabled = true;
            yield return null;

            bootstrap.RequestPlayerInteraction();
            Assert.IsNull(player.CarriedToy);
        }
    }
}
