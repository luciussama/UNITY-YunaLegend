using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.DarkRoom;
using YunaLegend.Camera25D;

namespace YunaLegend.RoutePhysics.Tests
{
    public class PlatformSupportPhysicsTests
    {
        IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
            yield return null;yield return null;
            var opening=Object.FindFirstObjectByType<DarkRoomOpeningSequence>();opening.StopAllCoroutines();opening.enabled=false;
            Object.FindFirstObjectByType<DarkRoomProgressionManager>().enabled=false;
            Camera.main.GetComponent<SideScroll25DCamera>().enabled=false;
            Object.FindFirstObjectByType<NandaFairyGuide>().enabled=false;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Time.timeScale=1f;
            var old=SceneManager.GetActiveScene();var empty=SceneManager.CreateScene("SupportCleanup");SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(old);
        }

        [UnityTest]
        public IEnumerator CameraBackoffIsExactlyFivePercentWithSameFovAndVisibleTargets()
        {
            yield return Load();
            var player=Object.FindFirstObjectByType<Yuna25DPlayerController>();player.enabled=false;player.GetComponent<CharacterController>().enabled=false;
            var camera=Camera.main;camera.aspect=16f/9f;var side=camera.GetComponent<SideScroll25DCamera>();
            var markers=Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            player.transform.position=markers.Single(m=>m.spec.index==0).GetTopCenterWorld();
            side.SetScrollState(true,1f,0f);side.RouteDistanceScale=1f;side.SnapToRouteFraming(1);
            float distance=-camera.transform.position.z,width=side.ViewWidth;
            side.RouteDistanceScale=1.05f;side.SnapToRouteFraming(1);
            Assert.AreEqual(distance*1.05f,-camera.transform.position.z,.001f);
            Assert.AreEqual(width*1.05f,side.ViewWidth,.001f);Assert.AreEqual(45f,camera.fieldOfView);
            foreach(var point in new[]{player.transform.position+Vector3.up*.7f,markers.Single(m=>m.spec.index==1).GetTopCenterWorld()})
            {
                var viewport=camera.WorldToViewportPoint(point);Assert.That(viewport.x,Is.InRange(.02f,.98f));Assert.That(viewport.y,Is.InRange(.02f,.98f));
            }
        }

        [UnityTest]
        public IEnumerator BearAndBooksSupportBothRunningDirectionsAt30And60And120Fps()
        {
            yield return Load();
            var player=Object.FindFirstObjectByType<Yuna25DPlayerController>();player.enabled=false;
            var manager=Object.FindFirstObjectByType<DarkRoomProgressionManager>();manager.BindPlayer(null);
            var markers=Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var marker in markers.Where(m=>m.spec.index<=1))
            foreach(int fps in new[]{30,60,120})
            foreach(float direction in new[]{1f,-1f})
            {
                var main=marker.Support.RegionNear(marker.GetTopCenterWorld().x);
                float x=Mathf.Clamp(marker.GetTopCenterWorld().x-direction*.2f,main.MinX+.4f,main.MaxX-.4f);
                player.ResetPlayer(main.At(x)+Vector3.up*.04f,direction);player.RestoreRouteSupport(marker.spec.index);
                player.SetFloorDeathActive(false);player.AutoRunActive=true;player.SetRunSpeed(0);
                Physics.SyncTransforms();for(int i=0;i<fps/4;i++)player.TickMovement(1f/fps);
                Assert.IsTrue(player.IsGrounded,$"settle {marker.spec.index} fps{fps} dir{direction}");
                player.SetRunSpeed(1f);
                for(int i=0;i<fps/5;i++)player.TickMovement(1f/fps);
                Assert.IsFalse(player.IsDead);Assert.IsTrue(player.IsGrounded,$"walk {marker.spec.index} fps{fps} dir{direction}");
                Assert.Greater(direction*(player.transform.position.x-x),.10f);
            }
        }

        [UnityTest]
        public IEnumerator DisconnectedInclinedSupportsNeverHoldTheCapsuleAcrossTheirGap()
        {
            yield return Load();
            // Put the fixture away from the saved room and use its actual colliders.
            var root=new GameObject("TwoAreas");root.transform.position=new Vector3(-100,0,0);
            var marker=root.AddComponent<DarkRoomPlatformMarker>();marker.spec=new PlatformSpec(999,"Test",DarkRoomPhase.Phase1Ascent,-100,0,6,1,.4f,Color.white,"");
            var contacts=new GameObject("Contacto_Fisico");contacts.transform.SetParent(root.transform,false);
            foreach(float x in new[]{-2f,2f})
            {
                var part=new GameObject("Area");part.transform.SetParent(contacts.transform,false);part.transform.localPosition=new Vector3(x,0,0);
                part.transform.localRotation=Quaternion.Euler(0,0,x<0?-8f:8f);part.AddComponent<BoxCollider>().size=new Vector3(2,.4f,1);
            }
            marker.InvalidateSupport();Assert.AreEqual(2,marker.Support.Regions.Count);Assert.IsFalse(marker.Support.TryGetPoint(-100,out _));
            var player=Object.FindFirstObjectByType<Yuna25DPlayerController>();player.enabled=false;
            Object.FindFirstObjectByType<DarkRoomProgressionManager>().BindPlayer(null);
            foreach(int fps in new[]{30,60,120})
            {
                foreach(float x in new[]{-102f,-98f})
                {
                    player.ResetPlayer(marker.Support.PointNear(x)+Vector3.up*.1f);player.RestoreRouteSupport(999);
                    player.SetFloorDeathActive(false);player.AutoRunActive=true;player.SetRunSpeed(0);Physics.SyncTransforms();
                    for(int i=0;i<fps/2;i++)player.TickMovement(1f/fps);
                    Assert.IsTrue(player.IsGrounded,$"tilted area fps{fps}");
                }
                player.ResetPlayer(new Vector3(-100,1,0));player.RestoreRouteSupport(999);player.SetFloorDeathActive(false);player.AutoRunActive=true;player.SetRunSpeed(0);
                Physics.SyncTransforms();for(int i=0;i<fps;i++)player.TickMovement(1f/fps);
                Assert.IsFalse(player.IsGrounded);Assert.Less(player.transform.position.y,-2f);
            }
        }
    }
}
