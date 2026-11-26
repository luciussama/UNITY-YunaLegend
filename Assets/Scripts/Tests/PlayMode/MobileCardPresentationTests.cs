using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;

namespace YunaLegend.RoutePhysics.Tests
{
    public sealed class MobileCardPresentationTests
    {
        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        private MonoBehaviour collapse;
        private Yuna25DPlayerController player;
        private SideScroll25DCamera camera;
        private DarkRoomPlatformMarker card;
        private readonly List<Vector3> lastPlayerTrace = new List<Vector3>();
        private readonly StringBuilder traceCsv = new StringBuilder("hz,falling,frame,playerX,playerY,velocityY,grounded,support,cameraX,cameraY,cameraZ,anchorY,collapsing,cameraDeltaTime\n");
        private static string Output => Path.Combine(Application.dataPath, "..", "Logs", "mobile-cards");

        private void Capture(string name)
        {
            Directory.CreateDirectory(Output);
            // The physics fixture pauses the opening/bootstrap; refresh only the illustrated views for this frame.
            foreach (var component in player.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string type = component.GetType().Name;
                if (type == "YunaRunIllustration" || type == "YunaJumpIllustration" || type == "YunaStumbleIllustration")
                {
                    var clock = component.GetType().GetField(type == "YunaJumpIllustration" ? "evaluated" : "last", Hidden);
                    clock?.SetValue(component, -1);
                    component.GetType().GetMethod("Refresh").Invoke(component, null);
                }
            }
            foreach (var component in player.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component.GetType().Name == "YunaIllustrationVisibilityResolver")
                {
                    component.GetType().GetField("suppressAll").SetValue(component, false);
                    component.GetType().GetField("showIdle").SetValue(component, false);
                    component.GetType().GetMethod("Resolve").Invoke(component, null);
                }
            }
            foreach (var component in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (component.GetType().Name == "RoomBackgroundT054")
                {
                    component.GetType().GetMethod("Follow").Invoke(component, new object[] { camera.transform.position });
                }
            }
            foreach (var component in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (component.GetType().Name == "PenumbraLightingT054")
                {
                    var fairy = component.GetType().GetField("nanda").GetValue(component) as Transform;
                    if (fairy != null)
                    {
                        fairy.position = player.transform.position + new Vector3(-1f, 1.5f, -.15f);
                    }
                    component.GetType().GetField("nandaStrength").SetValue(component, 1f);
                    component.GetType().GetMethod("Apply").Invoke(component, null);
                }
            }
            var view = camera.GetComponent<Camera>();
            var previous = view.targetTexture;
            var previousActive = RenderTexture.active;
            var render = RenderTexture.GetTemporary(844, 390, 24);
            var texture = new Texture2D(844, 390, TextureFormat.RGB24, false);
            try
            {
                view.targetTexture = render;
                view.aspect = 844f / 390f;
                view.Render();
                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, 844, 390), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(Output, name + ".png"), texture.EncodeToPNG());
                var audit = new StringBuilder("object,enabled,active,worldX,worldY,viewportX,viewportY,material\n");
                foreach (var renderer in player.GetComponentsInChildren<Renderer>(true).Concat(card.GetComponentsInChildren<Renderer>(true)))
                {
                    Vector3 viewport = view.WorldToViewportPoint(renderer.bounds.center);
                    audit.AppendLine(string.Join(",", new[] { renderer.name, renderer.enabled ? "1" : "0", renderer.gameObject.activeInHierarchy ? "1" : "0",
                        renderer.bounds.center.x.ToString("R", CultureInfo.InvariantCulture), renderer.bounds.center.y.ToString("R", CultureInfo.InvariantCulture),
                        viewport.x.ToString("R", CultureInfo.InvariantCulture), viewport.y.ToString("R", CultureInfo.InvariantCulture),
                        renderer.sharedMaterial != null ? renderer.sharedMaterial.name : "null" }));
                }
                File.WriteAllText(Path.Combine(Output, name + "-renderers.csv"), audit.ToString());
            }
            finally
            {
                view.targetTexture = previous;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(render);
                Object.Destroy(texture);
            }
        }

        private IEnumerator Load()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Fase-Jogavel-Completa", new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            yield return null;
            collapse = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(m => m.GetType().Name == "CardCastleCollapseView");
            player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            camera = Object.FindFirstObjectByType<SideScroll25DCamera>();
            card = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(m => m.spec.index == 30);
            foreach (var controller in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                string name = controller.GetType().Name;
                if (name == "IllustratedPreview" || name == "LegacyPlayableStageFlow" || name == "DarkRoomOpeningSequence"
                    || name == "DarkRoomProgressionManager" || name == "NandaFairyGuide")
                {
                    controller.StopAllCoroutines();
                    controller.enabled = false;
                }
            }
            player.enabled = false;
            camera.enabled = false;
            collapse.enabled = false;
            foreach (var marker in Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                marker.gameObject.SetActive(marker.spec.index >= 22);
            }
            Physics.SyncTransforms();
            camera.GetComponent<Camera>().aspect = 844f / 390f;
            camera.MobilePresentationOverride = true;
            collapse.GetType().GetProperty("MobilePresentationOverride").SetValue(collapse, (bool?)true);
        }

        private void CollapseCall(string method)
        {
            collapse.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(collapse, null);
        }

        private List<Vector3> JumpTrace(int hz, bool letCardsFall)
        {
            CollapseCall("ResetCollapseState");
            lastPlayerTrace.Clear();
            Vector3 top = card.GetTakeoffPoint(-1f, .4f);
            player.ResetPlayer(top + Vector3.up * .05f, -1f);
            player.RestoreRouteSupport(30);
            player.SetFloorDeathActive(false);
            player.AutoRunActive = true;
            player.SetRunSpeed(0f);
            Physics.SyncTransforms();
            for (int i = 0; i < hz / 4; i++) player.TickMovement(1f / hz);
            Assert.IsTrue(player.IsGrounded, "The real CharacterController must settle on card support30");
            CollapseCall("Update");
            camera.SetTarget(player.transform);
            camera.SetScrollState(true, -1f, 0f);
            camera.ConfigureIllustratedPilot(true, 7.5f);
            camera.ScreenCrushActive = false;
            camera.SnapToRouteFraming(31);
            player.SetRunSpeed(5.2f);
            if (letCardsFall)
            {
                CollapseCall("ApplyMobilePenumbra");
                Capture($"{hz}hz-before-takeoff");
            }
            typeof(Yuna25DPlayerController).GetField("jumpBufferCounter", Hidden).SetValue(player, .14f);
            var trace = new List<Vector3>();
            for (int i = 0; i < hz * .85f; i++)
            {
                player.TickMovement(1f / hz);
                if (letCardsFall)
                {
                    CollapseCall("Update");
                    collapse.GetType().GetMethod("EvaluateAtCollapseTime").Invoke(collapse, new object[] { i / (float)hz });
                }
                typeof(SideScroll25DCamera).GetMethod("ApplyIllustratedFrame", Hidden).Invoke(camera, new object[] { false });
                trace.Add(camera.transform.position);
                lastPlayerTrace.Add(player.transform.position);
                var anchor = (Vector3)typeof(SideScroll25DCamera).GetField("mobileCardSupport", Hidden).GetValue(camera);
                var p = player.transform.position;
                var c = camera.transform.position;
                traceCsv.AppendLine(string.Join(",", new[] { hz.ToString(), letCardsFall ? "1" : "0", i.ToString(),
                    p.x.ToString("R", CultureInfo.InvariantCulture), p.y.ToString("R", CultureInfo.InvariantCulture),
                    player.Velocity.y.ToString("R", CultureInfo.InvariantCulture), player.IsGrounded ? "1" : "0",
                    player.CurrentSupportIndex.ToString(), c.x.ToString("R", CultureInfo.InvariantCulture),
                    c.y.ToString("R", CultureInfo.InvariantCulture), c.z.ToString("R", CultureInfo.InvariantCulture),
                    anchor.y.ToString("R", CultureInfo.InvariantCulture),
                    (bool)collapse.GetType().GetProperty("IsCollapsing").GetValue(collapse) ? "1" : "0",
                    Time.deltaTime.ToString("R", CultureInfo.InvariantCulture) }));
                if (letCardsFall && i == hz / 3) Capture($"{hz}hz-during-collapse");
                if (i == 0)
                {
                    Assert.Greater(player.Velocity.y, 0f, "The test must execute a real takeoff");
                    Assert.IsFalse(player.IsGrounded);
                }
            }
            if (letCardsFall) Capture($"{hz}hz-after-collapse");
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output, "camera-takeoff-trace.csv"), traceCsv.ToString());
            return trace;
        }

        [UnityTest]
        public IEnumerator RealTakeoffCameraIsIdenticalWithAndWithoutSceneryCollapseAt30And60And120Hz()
        {
            yield return Load();
            float previousCaptureDeltaTime = Time.captureDeltaTime;
            traceCsv.Length = 0;
            traceCsv.AppendLine("hz,falling,frame,playerX,playerY,velocityY,grounded,support,cameraX,cameraY,cameraZ,anchorY,collapsing,cameraDeltaTime");
            try
            {
                foreach (int hz in new[] { 30, 60, 120 })
                {
                    Time.captureDeltaTime = 1f / hz;
                    yield return null;
                    Assert.That(Time.deltaTime, Is.EqualTo(1f / hz).Within(.00001f), "Camera must use the declared time step");
                    var baseline = JumpTrace(hz, false);
                    var baselinePlayer = lastPlayerTrace.ToArray();
                    var falling = JumpTrace(hz, true);
                    Assert.IsTrue((bool)collapse.GetType().GetProperty("IsCollapsing").GetValue(collapse)
                        || (bool)collapse.GetType().GetProperty("CollapseCompleted").GetValue(collapse), "Collapse actually triggered");
                    Assert.IsTrue(card.Support.IsEmpty, "The real collider must disappear during the test");
                    Assert.AreEqual(baseline.Count, falling.Count);
                    float maxDifference = 0f;
                    for (int i = 0; i < baseline.Count; i++)
                    {
                        Assert.Less(Vector3.Distance(baselinePlayer[i], lastPlayerTrace[i]), .002f,
                            $"Physics setup differs between baseline/collapse at {hz}Hz frame{i}");
                        float difference = Vector3.Distance(baseline[i], falling[i]);
                        maxDifference = Mathf.Max(maxDifference, difference);
                        Assert.Less(difference, .002f, $"Scenery changed camera at {hz}Hz frame{i}");
                    }
                    Debug.Log($"[MOBILE_CARD_CAMERA_QA] hz={hz} frames={falling.Count} cameraDt={Time.deltaTime:R} maxCollapseInfluence={maxDifference:R}");
                }
            }
            finally
            {
                Time.captureDeltaTime = previousCaptureDeltaTime;
            }
        }

        [UnityTest]
        public IEnumerator EveryCardLayerStaysInFixedPenumbraAndDesktopMaterialsAreRestored()
        {
            yield return Load();
            CollapseCall("ApplyMobilePenumbra");
            var renderers = card.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.GreaterOrEqual(renderers.Length, 20, "Painting, four wobble tiers and fifteen falling cards");
            foreach (var renderer in renderers)
            {
                Assert.AreEqual(1f, renderer.sharedMaterial.GetFloat("_SceneryPenumbra"), renderer.name);
                Assert.AreEqual("YunaLegend/RoomPenumbraT054", renderer.sharedMaterial.shader.name);
            }
            collapse.GetType().GetProperty("MobilePresentationOverride").SetValue(collapse, (bool?)false);
            CollapseCall("ApplyMobilePenumbra");
            foreach (var renderer in renderers)
            {
                Assert.IsFalse(renderer.sharedMaterial.HasProperty("_SceneryPenumbra")
                    && renderer.sharedMaterial.GetFloat("_SceneryPenumbra") > .5f, "Desktop must retain its original material");
            }
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            var old = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("MobileCardsCleanup");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(old);
        }
    }
}
