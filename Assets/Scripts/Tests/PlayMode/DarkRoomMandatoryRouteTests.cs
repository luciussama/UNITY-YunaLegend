using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Tests
{
    public class DarkRoomMandatoryRouteTests
    {
        private GameObject root, playerObject, managerObject;
        private DarkRoomBlockoutBuilder builder;
        private Yuna25DPlayerController player;
        private DarkRoomProgressionManager manager;
        private DarkRoomPlatformMarker[] markers;
        private float Dt = 1f / 120f;

        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("RoutePhysicsFixture");
            builder = root.AddComponent<DarkRoomBlockoutBuilder>();
            builder.BuildBlockout();
            markers = root.GetComponentsInChildren<DarkRoomPlatformMarker>(true);
            playerObject = new GameObject("RoutePlayer");
            var cc = playerObject.AddComponent<CharacterController>();
            cc.height = 1.4f; cc.radius = 0.35f; cc.center = Vector3.up * 0.7f;
            player = playerObject.AddComponent<Yuna25DPlayerController>();
            managerObject = new GameObject("RouteProgression");
            manager = managerObject.AddComponent<DarkRoomProgressionManager>();
            manager.BindPlayer(player);
            Field(manager, "blockoutBuilder", builder);
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(root);
        }

        private DarkRoomPlatformMarker Marker(int index) => markers.Single(m => m.spec.index == index);
        private void Tick(int count = 1)
        {
            Physics.SyncTransforms();
            for (int i = 0; i < count; i++) player.TickMovement(Dt);
        }
        private void Jump()
        {
            Field(player, "jumpHeld", true);
            Field(player, "jumpBufferCounter", 0.14f);
        }

        [Test]
        public void FloorStart_NoJumpDies_AndRetryStartsOnFloor()
        {
            manager.RetryCurrentCheckpoint();
            Assert.AreEqual(-1, manager.CurrentPlatformIndex);
            Assert.Less(player.transform.position.y, Marker(0).GetTopCenterWorld().y);
            Assert.IsTrue(player.AutoRunActive);
            Tick(400);
            Assert.IsTrue(player.IsDead);
            Assert.IsTrue(manager.IsGameOver);
            manager.RetryCurrentCheckpoint();
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(DarkRoomStartLayout.Position, player.transform.position);
            Tick(400);
            Assert.IsTrue(manager.IsGameOver);
        }

        [Test]
        public void ValidFirstJumpLands_ThenMissingNextJumpDies()
        {
            manager.RetryCurrentCheckpoint();
            Tick(300);
            Jump();
            for (int i = 0; i < 160 && manager.CurrentPlatformIndex < 0 && !player.IsDead; i++) Tick();
            Assert.IsFalse(player.IsDead, "Pouso valido nao pode matar");
            Assert.AreEqual(0, manager.CurrentPlatformIndex);
            Tick(300);
            Assert.IsTrue(manager.IsGameOver, "Cair do urso sem saltar deve derrotar");
        }

        [Test]
        public void SkippingPlatformIsFatal()
        {
            manager.RetryCurrentCheckpoint();
            player.ResetPlayer(Marker(1).GetTopCenterWorld() + Vector3.up * 0.15f);
            Tick(40);
            Assert.IsTrue(player.IsDead);
            Assert.AreEqual(-1, manager.CurrentPlatformIndex);
        }

        [Test]
        public void CastleRetryPreservesSupportAndExpectsPlatform10()
        {
            manager.StartStage2Escape();
            player.TriggerDeath("checkpoint test");
            manager.RetryCurrentCheckpoint();
            Tick(48);
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(9, manager.CurrentPlatformIndex);
            Assert.IsTrue(Marker(9).gameObject.activeInHierarchy);
            Assert.IsFalse(Marker(0).gameObject.activeInHierarchy);
            Jump();
            for (int i = 0; i < 150 && manager.CurrentPlatformIndex == 9 && !player.IsDead; i++) Tick();
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(10, manager.CurrentPlatformIndex);
        }

        [Test]
        public void ReturnTransitionAndRetryExpect22()
        {
            player.RestoreRouteSupport(21);
            player.ResetPlayer(new Vector3(Marker(21).transform.position.x, -9f, 0f));
            player.RestoreRouteSupport(21);
            manager.StartPhase3AfterPlotTwist();
            Assert.AreEqual(21, manager.CurrentPlatformIndex);
            Tick(348); Jump();
            for (int i = 0; i < 180 && manager.CurrentPlatformIndex == 21 && !player.IsDead; i++) Tick();
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(22, manager.CurrentPlatformIndex);
            player.TriggerDeath("return retry"); manager.RetryCurrentCheckpoint();
            Tick(348); Jump();
            for (int i = 0; i < 180 && manager.CurrentPlatformIndex == 21 && !player.IsDead; i++) Tick();
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(22, manager.CurrentPlatformIndex);
        }

        [Test]
        public void ReturnWithoutJumpDies()
        {
            manager.StartPhase3AfterPlotTwist(); manager.RetryCurrentCheckpoint();
            Tick(600);
            Assert.IsTrue(manager.IsGameOver);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator PosterFallIsOnlyNonlethalFall()
        {
            manager.StartStage2Escape();
            player.ResetPlayer(Marker(21).GetTopCenterWorld() + Vector3.up * 0.05f);
            player.RestoreRouteSupport(21);
            manager.NotifyPlatformLanded(21);
            Assert.IsTrue(player.IsCinematicFalling);
            Assert.IsTrue(markers.All(m => !m.gameObject.activeInHierarchy));
            for (int i = 0; i < 200 && player.IsCinematicFalling; i++) yield return new WaitForFixedUpdate();
            Assert.IsFalse(player.IsCinematicFalling);
            Assert.IsFalse(player.IsDead);
            Assert.AreEqual(CutsceneType.FalseDoorPlotTwist, manager.ActiveCutscene);
            Assert.Less(player.transform.position.y, -8.9f);
        }

        [Test]
        public void AllMandatoryJumpPairsReachableWithRealCharacterController()
        {
            // Search takeoff positions with the real capsule, collision callbacks and movement code.
            manager.BindPlayer(null);
            foreach (int fps in new[] { 30, 60, 120 })
            for (int target = 1; target <= 37; target++)
            {
                if (target == 22) continue; // Floor launch covered by ReturnTransitionAndRetryExpect22.
                Dt = 1f / fps;
                int source = target - 1;
                var phase = target <= 9 ? DarkRoomPhase.Phase1Ascent : target <= 21 ? DarkRoomPhase.Phase2Escape : DarkRoomPhase.Phase3ChaoticAscent;
                builder.ShowTemporalSection(phase, source);
                float direction = target <= 21 ? 1f : -1f;
                float progress = target <= 21 ? Mathf.Clamp01((source - 10f) / 11f) : Mathf.Clamp01((source - 22f) / 15f);
                float speed = target <= 9 ? 3.6f : target <= 21 ? Mathf.Lerp(4f, 6f, progress) : Mathf.Lerp(4.2f, 6.6f, progress);
                float height = target <= 9 ? 2.4f : target <= 21 ? Mathf.Lerp(2.5f, 3.4f, progress) : Mathf.Lerp(2.6f, 3.9f, progress);
                bool landed = false;
                int consecutive = 0, longestWindow = 0;
                int reached = -1;
                System.Action<int> handler = index => reached = index;
                player.OnPlatformLanded += handler;
                for (float inset = 0.05f; inset <= Marker(source).spec.width; inset += 0.1f)
                {
                    var top = Marker(source).GetTopCenterWorld();
                    top.x += direction * (Marker(source).spec.width * 0.5f - inset);
                    player.ResetPlayer(top + Vector3.up * 0.03f, direction);
                    player.RestoreRouteSupport(source);
                    player.SetRunSpeed(0f); Tick(5);
                    player.SetRunSpeed(speed); player.SetJumpHeight(height);
                    reached = -1; Jump();
                    for (int frame = 0; frame < 220 && reached < 0 && !player.IsDead; frame++) Tick();
                    bool success = reached == target && !player.IsDead;
                    landed |= success;
                    consecutive = success ? consecutive + 1 : 0;
                    longestWindow = Mathf.Max(longestWindow, consecutive);
                }
                player.OnPlatformLanded -= handler;
                Debug.Log($"ROUTE_WINDOW fps={fps} pair={source}->{target} seconds={longestWindow * 0.1f / speed:F3}");
                Assert.GreaterOrEqual(longestWindow * 0.1f / speed, 0.10f, $"Timing window fps={fps} pair={source}->{target}");
                Assert.IsTrue(landed, $"Real CharacterController cannot jump {source} -> {target}; position={player.transform.position}; reached={reached}; speed={speed}; height={height}");
            }
        }

        [Test]
        public void ContinuousEscapeRun_JumpsAllPlatformsWithoutOvershoot()
        {
            // Simula uma corrida humana contínua por toda a Fuga (Plat 9 a 21)
            // Em cada plataforma, Yuna corre e salta antes da borda frontal, comprovando pouso seguro sem overshoot.
            manager.StartStage2Escape();
            Assert.AreEqual(9, manager.CurrentPlatformIndex);

            player.ResetPlayer(new Vector3(39.0f, -2.8f, 0f), 1f);
            player.RestoreRouteSupport(9);

            for (int target = 10; target <= 21; target++)
            {
                int source = target - 1;
                var sourceMarker = Marker(source);
                var targetMarker = Marker(target);

                // Corre ate 0.35m antes da borda frontal da plataforma atual
                float takeoffX = sourceMarker.transform.position.x + (sourceMarker.spec.width * 0.5f) - 0.35f;
                while (player.transform.position.x < takeoffX && !player.IsDead)
                {
                    Tick(1);
                }

                Assert.IsFalse(player.IsDead, $"Yuna morreu antes de decolar da plat {source}");

                // Salta em direcao a proxima plataforma
                Jump();

                // Aguarda o pouso na plataforma de destino
                int timeout = 160;
                while (manager.CurrentPlatformIndex != target && timeout > 0 && !player.IsDead)
                {
                    Tick(1);
                    timeout--;
                }

                Assert.IsFalse(player.IsDead, $"Yuna morreu no salto {source} -> {target}. Posicao: {player.transform.position}, Limites Destino: [{targetMarker.transform.position.x - targetMarker.spec.width * 0.5f} .. {targetMarker.transform.position.x + targetMarker.spec.width * 0.5f}]");
                Assert.AreEqual(target, manager.CurrentPlatformIndex, $"Nao alcancou a plataforma {target} no tempo esperado");

                // Comprova que o ponto de pouso nao ultrapassou a borda frontal (sem overshoot)
                float targetRightEdge = targetMarker.transform.position.x + (targetMarker.spec.width * 0.5f);
                Assert.LessOrEqual(player.transform.position.x, targetRightEdge, $"Overshoot na plat {target}: Yuna pousou alem da borda frontal!");
            }
        }
    }
}
