using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;

namespace YunaLegend.RoutePhysics.Tests
{
    public class DarkRoomCameraFramingTests
    {
        private IEnumerator Load()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
            yield return null;
            yield return null;
            var opening = Object.FindFirstObjectByType<DarkRoomOpeningSequence>();
            opening.StopAllCoroutines(); opening.enabled = false;
            Object.FindFirstObjectByType<DarkRoomProgressionManager>().enabled = false;
        }

        private DarkRoomPlatformMarker Marker(int index) => Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(m => m.spec.index == index);

        [UnityTearDown]
        public IEnumerator CleanupScene()
        {
            Time.timeScale = 1f;
            var loaded = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("CameraFramingCleanup");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(loaded);
        }

        [UnityTest]
        public IEnumerator IllustratedPilotKeepsScaleAndDoesNotTrackJumpHeight()
        {
            yield return Load();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            player.enabled = false; player.GetComponent<CharacterController>().enabled = false;
            player.AutoRunActive = false;
            player.transform.position = DarkRoomStartLayout.Position;
            var camera = Camera.main; camera.aspect = 16f / 9f;
            var side = camera.GetComponent<SideScroll25DCamera>();
            Assert.IsFalse(side.IllustratedPilotFraming, "Saved Gameplay must keep its existing camera by default.");
            side.SetTarget(player.transform);
            side.ConfigureIllustratedPilot(true, 7.5f);
            var reference = camera.transform.position;
            player.transform.position += Vector3.up * 2f;
            yield return null;
            Assert.AreEqual(reference.y, camera.transform.position.y, .001f, "A jump must not drag the camera up.");
            Assert.AreEqual(reference.z, camera.transform.position.z, .001f);
            camera.aspect = 19.5f / 9f;
            yield return null;
            float pixels = Mathf.Abs(camera.WorldToViewportPoint(player.transform.position + Vector3.up * 1.4f).y - camera.WorldToViewportPoint(player.transform.position).y) * 720f;
            Assert.AreEqual(134.4f, pixels, .01f);
            Assert.AreEqual(reference.z, camera.transform.position.z, .001f, "Landscape aspect changes should reveal more width without zooming.");
            Assert.AreEqual(45f, camera.fieldOfView);
            Assert.Less(Quaternion.Angle(camera.transform.rotation, Quaternion.identity), .01f);
        }

        [UnityTest]
        public IEnumerator CastleOverviewRevealsRouteThenReturnsToCloseFrame()
        {
            yield return Load();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            player.enabled = false; player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = Marker(9).GetTopCenterWorld();
            var camera = Camera.main; camera.aspect = 16f / 9f;
            var side = camera.GetComponent<SideScroll25DCamera>();
            var manager = Object.FindFirstObjectByType<DarkRoomProgressionManager>();
            manager.NotifyPlatformLanded(9);
            yield return new WaitForSeconds(1.7f);
            Assert.IsTrue(side.IsRouteOverview);
            Assert.Greater(side.ViewWidth, 60f);
            Assert.That(camera.WorldToViewportPoint(Marker(9).GetTopCenterWorld()).x, Is.InRange(0.01f, 0.99f));
            Assert.That(camera.WorldToViewportPoint(Marker(21).GetTopCenterWorld()).x, Is.InRange(0.01f, 0.99f));
            Assert.IsTrue(Marker(21).gameObject.activeInHierarchy);
            Assert.AreEqual(21, Object.FindFirstObjectByType<NandaFairyGuide>().CurrentTargetPlatformIndex);
            manager.AdvanceCutscene(); manager.AdvanceCutscene();
            yield return new WaitForSeconds(1f);
            Assert.IsFalse(side.IsRouteOverview);
            Assert.Less(side.ViewWidth, 18f);
            Assert.AreEqual(45f, camera.fieldOfView);
        }

        [UnityTest]
        public IEnumerator CloseFrameShowsNextLandingAndHidesFloorWithoutFollowingJumpArc()
        {
            yield return Load();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            player.enabled = false; player.GetComponent<CharacterController>().enabled = false;
            var camera = Camera.main; camera.aspect = 16f / 9f;
            var side = camera.GetComponent<SideScroll25DCamera>();
            var fairy = Object.FindFirstObjectByType<NandaFairyGuide>(); fairy.enabled = false;
            player.transform.position = DarkRoomStartLayout.Position;
            fairy.SetTargetPlatform(0); side.SetScrollState(true, 1f, 0f); side.ScreenCrushActive = false;
            side.SnapToRouteFraming(0);
            float floorStart = camera.WorldToViewportPoint(new Vector3(player.transform.position.x, DarkRoomReturnLayout.FloorY, 0)).y;
            Assert.That(floorStart, Is.InRange(0f, 1f));

            player.transform.position = Marker(4).GetTopCenterWorld();
            fairy.SetTargetPlatform(5); side.SnapToRouteFraming(5);
            var next = camera.WorldToViewportPoint(Marker(5).GetTopCenterWorld());
            Assert.That(next.x, Is.InRange(0.05f, 0.95f)); Assert.That(next.y, Is.InRange(0.05f, 0.95f));
            Assert.Greater(camera.WorldToViewportPoint(Marker(6).GetTopCenterWorld()).x, 1f, "The following obstacle should remain outside the main view.");
            Assert.Less(camera.WorldToViewportPoint(new Vector3(player.transform.position.x, DarkRoomReturnLayout.FloorY, 0)).y, 0f);
            Assert.Less(side.ViewWidth, 12f);
            float height = camera.transform.position.y;
            player.transform.position += Vector3.up * 2.4f;
            yield return new WaitForSeconds(0.3f);
            Assert.That(camera.transform.position.y, Is.EqualTo(height).Within(0.01f), "Do not chase each jump apex.");
            Assert.That(camera.WorldToViewportPoint(player.transform.position + Vector3.up * 1.4f).y, Is.InRange(0f, 1f));
            Assert.AreEqual(45f, camera.fieldOfView);
        }

        [UnityTest]
        public IEnumerator ReverseFrameLooksLeftAndCinematicFallBringsFloorBack()
        {
            yield return Load();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            player.enabled = false; player.GetComponent<CharacterController>().enabled = false;
            var camera = Camera.main; camera.aspect = 16f / 9f;
            var side = camera.GetComponent<SideScroll25DCamera>();
            var fairy = Object.FindFirstObjectByType<NandaFairyGuide>(); fairy.enabled = false;
            player.transform.position = Marker(30).GetTopCenterWorld();
            fairy.SetTargetPlatform(31); side.SetScrollState(true, -1f, 0f); side.ScreenCrushActive = false;
            side.SnapToRouteFraming(31);
            Assert.Less(camera.transform.position.x, player.transform.position.x);
            Assert.That(camera.WorldToViewportPoint(Marker(31).GetTopCenterWorld()).x, Is.InRange(0.05f, 0.95f));

            player.transform.position = Marker(20).GetTopCenterWorld();
            fairy.SetTargetPlatform(21); side.SetScrollState(true, 1f, 0f); side.SnapToRouteFraming(21);
            float upperY = camera.transform.position.y;
            player.StartCinematicFall(); side.SetScrollState(false);
            player.transform.position = new Vector3(player.transform.position.x, DarkRoomStartLayout.StartY, 0f);
            yield return new WaitForSeconds(1.5f);
            Assert.Less(camera.transform.position.y, upperY - 5f);
            Assert.That(camera.WorldToViewportPoint(new Vector3(player.transform.position.x, DarkRoomReturnLayout.FloorY, 0)).y, Is.InRange(0f, 1f));
            Assert.Less(Quaternion.Angle(Quaternion.identity, camera.transform.rotation), 0.01f);
        }
    }
}
