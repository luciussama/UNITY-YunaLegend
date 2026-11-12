using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YunaLegend.DarkRoom;

namespace YunaLegend.RoutePhysics.Tests
{
    public class NandaSeparationTests
    {
        [UnityTest]
        public IEnumerator RunnerCannotCatchFairyDuringJumpReturnOrTeleport()
        {
            var yuna = new GameObject("SeparationTestYuna");
            var controller = yuna.AddComponent<CharacterController>();
            controller.height = 1.4f; controller.radius = .35f; controller.center = Vector3.up * .7f;
            var player = yuna.AddComponent<Yuna25DPlayerController>(); player.enabled = false;
            var fairyObject = new GameObject("SeparationTestFairy");
            var fairy = fairyObject.AddComponent<NandaFairyGuide>();
            var target = new GameObject("SeparationTestTarget");
            var marker = target.AddComponent<DarkRoomPlatformMarker>();
            marker.spec = new PlatformSpec { index = 999, height = 1f };
            fairy.SetYuna(yuna.transform); fairy.ReturnToYuna();
            try
            {
                foreach (float direction in new[] { 1f, -1f })
                {
                    player.RunDirection = direction;
                    fairyObject.transform.position = yuna.transform.position + Vector3.up * .7f;
                    for (int i = 0; i < 75; i++)
                    {
                        // Includes an exact-overlap reset and a moving jump arc.
                        yuna.transform.position += new Vector3(direction * .11f, Mathf.Cos(i * .12f) * .1f, 0f);
                        if (i == 20) { target.transform.position = yuna.transform.position; fairy.SetTargetPlatform(999); }
                        if (i == 55) fairy.ReturnToYuna();
                        if (i == 35) yuna.transform.position = fairyObject.transform.position - Vector3.up * .7f;
                        yield return null;
                        fairy.SendMessage("LateUpdate");
                        Vector3 center = yuna.transform.position + controller.center;
                        Vector3 delta = fairyObject.transform.position - center;
                        float ellipse = delta.x * delta.x / (.95f * .95f) + delta.y * delta.y / (1.2f * 1.2f);
                        Assert.GreaterOrEqual(ellipse, .999f, "Fairy must clear Yuna's silhouette every rendered frame.");
                        Assert.IsNull(fairy.GetComponent<Collider>(), "The guide must never block player physics.");
                    }
                    Assert.Greater(direction * (fairy.transform.position.x - yuna.transform.position.x), .3f);
                }
            }
            finally { Object.Destroy(yuna); Object.Destroy(fairyObject); Object.Destroy(target); }
        }
    }
}
