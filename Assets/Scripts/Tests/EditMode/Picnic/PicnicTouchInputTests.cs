using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

namespace YunaLegend.Picnic.Tests
{
    public sealed class PicnicTouchInputTests
    {
        [Test]
        public void DirectionAndJumpBelongToIndependentFingers()
        {
            var input = new PicnicTouchInput();
            input.PressDirection(10, 1f);
            input.PressJump(20);
            Assert.That(input.Axis, Is.EqualTo(1f));
            Assert.That(input.ReadJump(), Is.True);
            Assert.That(input.ReadJump(), Is.True, "Holding supports the existing variable jump height");
            input.Release(20);
            Assert.That(input.Axis, Is.EqualTo(1f));
            Assert.That(input.ReadJump(), Is.False);
            input.PressJump(21);
            input.Release(10);
            Assert.That(input.Axis, Is.Zero);
            Assert.That(input.ReadJump(), Is.True);
        }

        [Test]
        public void QuickTapBetweenFramesIsNotLost()
        {
            var input = new PicnicTouchInput();
            input.PressJump(5);
            input.Release(5);
            Assert.That(input.ReadJump(), Is.True);
            Assert.That(input.ReadJump(), Is.False);
        }

        [Test]
        public void ReleasingOneOfTwoDirectionFingersKeepsTheOther()
        {
            var input = new PicnicTouchInput();
            input.PressDirection(1, 1f);
            input.PressDirection(2, 1f);
            input.Release(1);
            input.Release(99);
            Assert.That(input.Axis, Is.EqualTo(1f));
            input.PressJump(3);
            input.Clear();
            Assert.That(input.Axis, Is.Zero);
            Assert.That(input.ReadJump(), Is.False);
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void TwoFingersMoveAndJumpInActualSimulation(int hz)
        {
            var input = new PicnicTouchInput();
            var sim = new PicnicSimulation();
            sim.SkipStory();
            sim.Step(1f / hz, new PicnicInput(0f, false));
            float start = sim.PlayerX;
            input.PressDirection(1, 1f);
            input.PressJump(2);
            for (int i = 0; i < hz / 5; i++)
            {
                sim.Step(1f / hz, new PicnicInput(input.Axis, input.ReadJump()));
            }
            Assert.That(sim.Jumps, Is.EqualTo(1));
            Assert.That(sim.PlayerX, Is.GreaterThan(start));
            Assert.That(sim.OnGround, Is.False);
        }

        [Test]
        public void HudPointerEventsStartJumpBeforeReleaseAndKeepDirectionDuringThumbDrift()
        {
            var root = new GameObject("TouchHudTest");
            var hud = root.AddComponent<PicnicNarrativeAndHudController>();
            GameObject events = null;
            try
            {
                hud.Build();
                events = Object.FindFirstObjectByType<EventSystem>().gameObject;
                var sim = new PicnicSimulation();
                sim.SkipStory();
                hud.Apply(sim, false);
                var module = events.GetComponent<InputSystemUIInputModule>();
                Assert.That(module.pointerBehavior, Is.EqualTo(UIPointerBehavior.AllPointersAsIs));
                var right = root.transform.Find("PicnicHudCanvas/SafeArea/Right").GetComponent<PicnicTouchButton>();
                var jump = root.transform.Find("PicnicHudCanvas/SafeArea/Jump").GetComponent<PicnicTouchButton>();
                var first = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = 11 };
                var second = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = 22 };
                right.OnPointerDown(first);
                right.OnPointerExit(first);
                jump.OnPointerDown(second);
                Assert.That(hud.TouchAxis, Is.EqualTo(1f));
                Assert.That(hud.ConsumeTouchJump(), Is.True, "Pulo must fire on press, before finger lift");
                jump.OnPointerUp(second);
                Assert.That(hud.ConsumeTouchJump(), Is.False);
                Assert.That(hud.TouchAxis, Is.EqualTo(1f));
                right.OnPointerUp(second);
                Assert.That(hud.TouchAxis, Is.EqualTo(1f), "Another finger cannot release movement");
                right.gameObject.SetActive(false);
                Assert.That(hud.TouchAxis, Is.Zero, "Disabling a held button must release its fingers");
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (events != null) Object.DestroyImmediate(events);
            }
        }

        [TestCase(320, 568)]
        [TestCase(390, 844)]
        [TestCase(844, 390)]
        public void TouchTargetsAreLargeAndSeparate(int width, int height)
        {
            var root = new GameObject("TouchBoundsTest");
            GameObject events = null;
            try
            {
                var hud = root.AddComponent<PicnicNarrativeAndHudController>();
                hud.Build();
                events = Object.FindFirstObjectByType<EventSystem>().gameObject;
                hud.SetViewportOverride(width, height);
                Canvas.ForceUpdateCanvases();
                hud.SetViewportOverride(width, height);
                Canvas.ForceUpdateCanvases();
                var safe = root.transform.Find("PicnicHudCanvas/SafeArea");
                var canvas = root.transform.Find("PicnicHudCanvas").GetComponent<RectTransform>();
                float scale = canvas.rect.width / width;
                var left = safe.Find("Left").GetComponent<RectTransform>();
                var right = safe.Find("Right").GetComponent<RectTransform>();
                var jump = safe.Find("Jump").GetComponent<RectTransform>();
                Assert.That(left.rect.width / scale, Is.EqualTo(80f).Within(0.1f));
                Assert.That(right.rect.height / scale, Is.EqualTo(80f).Within(0.1f));
                Assert.That(jump.rect.height / scale, Is.EqualTo(80f).Within(0.1f));
                Assert.That(right.anchoredPosition.x / scale + right.rect.width / scale, Is.LessThan(width - 12f - 108f));
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (events != null) Object.DestroyImmediate(events);
            }
        }
    }
}
