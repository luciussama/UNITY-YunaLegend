using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.DarkRoom;
using YunaLegend.Camera25D;

namespace YunaLegend.RoutePhysics.Tests
{
    public class YieldingFurniturePlayModeTests
    {
        Yuna25DPlayerController player;
        YieldingWoodFurniture furniture;
        DarkRoomPlatformMarker marker;
        IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/DarkRoom_Gameplay.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/DarkRoom_Gameplay.unity");
#endif
            yield return null;yield return null;
            var opening=Object.FindFirstObjectByType<DarkRoomOpeningSequence>();opening.StopAllCoroutines();opening.enabled=false;
            var manager=Object.FindFirstObjectByType<DarkRoomProgressionManager>();manager.BindPlayer(null);manager.enabled=false;
            Camera.main.GetComponent<SideScroll25DCamera>().enabled=false;
            Object.FindFirstObjectByType<NandaFairyGuide>().enabled=false;
            player=Object.FindFirstObjectByType<Yuna25DPlayerController>();player.enabled=false;
            furniture=Object.FindFirstObjectByType<YieldingWoodFurniture>();furniture.enabled=false;marker=furniture.GetComponent<DarkRoomPlatformMarker>();
        }
        void Place(float x,int fps)
        {
            furniture.ResetForRetry();player.ResetPlayer(marker.Support.PointNear(x)+Vector3.up*.04f);
            player.RestoreRouteSupport(2);player.SetFloorDeathActive(false);player.AutoRunActive=true;player.SetRunSpeed(0);player.SetJumpHeight(2.4f);
            Physics.SyncTransforms();for(int i=0;i<fps/3;i++)player.TickMovement(1f/fps);
            Assert.IsTrue(player.IsGrounded);player.SetRunSpeed(3.6f);
        }
        void Jump()
        {
            typeof(Yuna25DPlayerController).GetField("jumpBufferCounter",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player,.14f);
            typeof(Yuna25DPlayerController).GetField("jumpHeld",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player,true);
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Time.timeScale=1f;var old=SceneManager.GetActiveScene();var empty=SceneManager.CreateScene("FurnitureCleanup");SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(old);
        }

        [UnityTest]
        public IEnumerator CrossingCracksOnceDelaysExactly80msThenLandsNormallyAtAllFrameRates()
        {
            yield return Load();
            foreach(int fps in new[]{30,60,120})
            {
                Place(marker.GetLandingPoint(1f).x,fps);
                float delay=0,maxSag=0;bool sawPose=false,jumped=false;
                for(int frame=0;frame<fps*5&&!player.IsDead&&player.CurrentSupportIndex<3;frame++)
                {
                    if(!jumped&&player.transform.position.x>=marker.GetTakeoffPoint(1f,.35f).x){Jump();jumped=true;}
                    float before=player.StumbleDelayRemaining;player.TickMovement(1f/fps);
                    delay+=before-player.StumbleDelayRemaining;furniture.Tick(1f/fps);player.SendMessage("LateUpdate");
                    maxSag=Mathf.Max(maxSag,furniture.CurrentSag);
                    var body=player.transform.Find("Body_Visual");
                    var view=body.GetComponent<YunaCharacterView>();
                    float lean=view!=null?view.PoseLeanDegrees:Mathf.DeltaAngle(0,body.localEulerAngles.x);
                    if(player.IsStumbling&&Mathf.Abs(lean)>1f)sawPose=true;
                }
                Assert.IsFalse(player.IsDead,$"death fps{fps}");Assert.AreEqual(3,player.CurrentSupportIndex,$"exit fps{fps}");
                Assert.AreEqual(1,furniture.CrackCount);Assert.AreEqual(.08f,delay,.0001f,$"local delay fps{fps}");
                Assert.IsTrue(sawPose);Assert.Greater(maxSag,.06f);Assert.IsFalse(player.IsStumbling);
                Assert.AreEqual(1f,Time.timeScale);Assert.AreEqual(3.6f,player.RunSpeed);
            }
        }

        [UnityTest]
        public IEnumerator LocalDelayChangesOnlyHorizontalTimeNotGravityOrGlobalTime()
        {
            yield return Load();
            foreach(int fps in new[]{30,60,120})
            {
                Place(marker.GetLandingPoint(1f).x,fps);float start=player.transform.position.x;
                player.BeginStumble(.08f,.28f);
                for(int i=0;i<fps;i++)player.TickMovement(1f/fps);
                Assert.AreEqual(3.6f*(1f-.08f),player.transform.position.x-start,.015f,$"motion fps{fps}");
                Assert.IsTrue(player.IsGrounded);Assert.IsFalse(player.IsStumbling);Assert.AreEqual(1f,Time.timeScale);
            }
        }

        [UnityTest]
        public IEnumerator RetryRestoresBoardsAndAllowsANewSingleCrack()
        {
            yield return Load();Place(marker.transform.position.x-.1f,60);
            furniture.Tick(1f/60);Assert.IsTrue(furniture.HasCracked);Assert.AreEqual(1,furniture.CrackCount);
            for(int i=0;i<45;i++){player.TickMovement(1f/60);furniture.Tick(1f/60);}
            player.PrepareForOpening(DarkRoomStartLayout.Position);furniture.Tick(1f/60);
            Assert.IsFalse(furniture.HasCracked);Assert.AreEqual(0f,furniture.CurrentSag);Assert.IsFalse(player.IsStumbling);
            Place(marker.transform.position.x-.1f,60);furniture.Tick(1f/60);
            Assert.IsTrue(furniture.HasCracked);Assert.AreEqual(1,furniture.CrackCount);
        }
    }
}
