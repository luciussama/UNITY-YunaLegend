using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

namespace YunaLegend.Picnic.Audit
{
    // Focused audit fixture installed only in the isolated validation project.
    public sealed class PicnicPresentationRuntimeTests
    {
        private GameObject owner;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Object.Destroy(owner);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator HeartsAndUnlimitedLivesHaveVisibleMeshes()
        {
            owner = new GameObject("HUD audit");
            var hud = owner.AddComponent<PicnicHudController>();
            hud.Build();
            var sim = new PicnicSimulation();
            sim.SkipNarrative();
            for (int lives = 3; lives >= 1; lives--)
            {
                typeof(PicnicSimulation).GetProperty("Hearts").SetValue(sim, lives);
                hud.Apply(sim, false);
                Canvas.ForceUpdateCanvases();
                yield return null;
                var hearts = Object.FindObjectsByType<PicnicHudSymbol>(FindObjectsSortMode.None)
                    .Where(s => s.Symbol == PicnicSymbol.Heart).ToArray();
                Assert.That(hearts.Length, Is.EqualTo(3));
                Assert.That(hearts.Count(s => s.color.a > 0.5f), Is.EqualTo(lives));
                foreach (var heart in hearts)
                {
                    var mesh = heart.canvasRenderer.GetMesh();
                    Assert.That(mesh.vertexCount, Is.GreaterThan(0));
                    Assert.That(heart.raycastTarget, Is.False);
                }
            }
            var easy = new PicnicSimulation(true);
            easy.SkipNarrative();
            hud.Apply(easy, false);
            Canvas.ForceUpdateCanvases();
            yield return null;
            var symbols = Object.FindObjectsByType<PicnicHudSymbol>(FindObjectsSortMode.None);
            Assert.That(symbols.Count(s => s.Symbol == PicnicSymbol.Heart), Is.EqualTo(1));
            Assert.That(symbols.Count(s => s.Symbol == PicnicSymbol.Infinity), Is.EqualTo(1));
            Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TouchDirectionAndJumpRemainIndependentAndPauseReleasesHold()
        {
            owner = new GameObject("Input audit");
            var hud = owner.AddComponent<PicnicHudController>();
            hud.Build();
            var sim = new PicnicSimulation();
            sim.SkipNarrative();
            hud.Apply(sim, false);
            var right = Object.FindObjectsByType<PicnicHoldButton>(FindObjectsSortMode.None).Single(b => b.name == "Right");
            right.OnPointerDown(null);
            Assert.That(hud.TouchAxis, Is.EqualTo(1f));
            GameObject.Find("Jump").GetComponent<Button>().onClick.Invoke();
            Assert.That(hud.ConsumeJump(), Is.True);
            Assert.That(hud.ConsumeJump(), Is.False);
            Assert.That(hud.TouchAxis, Is.EqualTo(1f));
            right.OnPointerExit(null);
            Assert.That(hud.TouchAxis, Is.Zero);
            right.OnPointerDown(null);
            right.OnPointerUp(null);
            Assert.That(hud.TouchAxis, Is.Zero);
            right.OnPointerDown(null);
            hud.Apply(sim, true);
            Assert.That(hud.TouchAxis, Is.Zero);
            Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OpeningSoleAndPaintedStairCoincideOnActualCanvas()
        {
            owner = new GameObject("Opening audit");
            var story = owner.AddComponent<PicnicNarrativeController>();
            story.Build();
            yield return null;
            Canvas.ForceUpdateCanvases();
            story.Apply(new PicnicSimulation());
            var painting = GameObject.Find("Original actor-free painting").GetComponent<RectTransform>();
            var yuna = GameObject.Find("Current Yuna").GetComponent<RectTransform>();
            Vector3 paintedStep = painting.TransformPoint(new Vector3(
                painting.rect.xMin + painting.rect.width * 450f / 1652f,
                painting.rect.yMax - painting.rect.height * 541f / 952f));
            Vector3 sole = yuna.TransformPoint(new Vector3(
                yuna.rect.xMin + yuna.rect.width * 248f / 512f,
                yuna.rect.yMax - yuna.rect.height * 510f / 512f));
            Assert.That(Vector3.Distance(paintedStep, sole), Is.LessThan(0.01f));
            Object.Destroy(owner);
            yield return null;
        }
    }
}
