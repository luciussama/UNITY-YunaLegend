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
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/ToyRoom_Gameplay.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/ToyRoom_Gameplay.unity");
#endif
            yield return null;

            var bootstrap = Object.FindFirstObjectByType<ToyRoomStageBootstrap>();
            var player = Object.FindFirstObjectByType<ToyRoomPlayerController>();
            var chest = Object.FindFirstObjectByType<ToyChestController>();
            var nanda = Object.FindFirstObjectByType<NandaToyRoomGuideController>();

            Assert.IsNotNull(bootstrap);
            Assert.IsNotNull(player);
            Assert.IsNotNull(chest);
            Assert.IsNotNull(nanda);
            Assert.AreEqual(ToyRoomPhase.Exploring, bootstrap.StateMachine.CurrentPhase);

            // Testa movimento 3D livre em X/Z
            Vector3 startPos = player.transform.position;
            player.ScriptedMoveInput = new Vector2(1f, 0f);
            for (int f = 0; f < 10; f++)
            {
                yield return null;
            }
            player.ScriptedMoveInput = null;
            Assert.Greater(player.transform.position.x, startPos.x + 0.05f);

            var allToys = Object.FindObjectsByType<ToyItemView>(FindObjectsSortMode.None);
            Assert.AreEqual(8, allToys.Length);

            var cc = player.GetComponent<CharacterController>();

            // Coleta e guarda os 6 primeiros brinquedos
            ToyItemView sixthToy = null;
            for (int i = 0; i < 6; i++)
            {
                var toy = allToys[i];
                if (i == 5)
                {
                    sixthToy = toy;
                }

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

            Assert.AreEqual(5, bootstrap.StateMachine.Planner.OrganizedCount);
            Assert.AreEqual(ToyRoomPhase.RecurrenceMystery, bootstrap.StateMachine.CurrentPhase);
            Assert.IsNotNull(sixthToy);
            Assert.IsFalse(sixthToy.gameObject.activeInHierarchy);

            // Aguarda 1.6s para o 6o brinquedo reaparecer fora do foco principal
            yield return new WaitForSeconds(1.65f);
            Assert.IsTrue(sixthToy.gameObject.activeInHierarchy);
        }
    }
}
