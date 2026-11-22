using NUnit.Framework;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Tests
{
    public class ToyRoomIntroTimelineTests
    {
        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void HandDrawnExposureCoversEveryDrawingRegardlessOfRenderRate(int renderRate)
        {
            var seen = new bool[ToyRoomHandDrawnTiming.FrameCount];
            int previousFrame = 0;

            for (int tick = 0; tick < renderRate * 25; tick++)
            {
                int frame = ToyRoomHandDrawnTiming.FrameAt(tick / (double)renderRate);
                Assert.That(frame - previousFrame, Is.InRange(0, 1));
                seen[frame] = true;
                previousFrame = frame;
            }

            Assert.That(seen, Is.All.True);
            Assert.That(ToyRoomHandDrawnTiming.FrameAt(25d), Is.EqualTo(625));
            Assert.That(ToyRoomHandDrawnTiming.ShotAt(625), Is.EqualTo(-1));
        }

        [Test]
        public void HandDrawnExposureHoldsDrawingForFortyMilliseconds()
        {
            for (int frame = 0; frame < ToyRoomHandDrawnTiming.FrameCount; frame++)
            {
                double start = frame / 25d;
                Assert.That(ToyRoomHandDrawnTiming.FrameAt(start), Is.EqualTo(frame));
                Assert.That(ToyRoomHandDrawnTiming.FrameAt(start + 0.039999d), Is.EqualTo(frame));
            }
        }

        [TestCase(74, "", "")]
        [TestCase(75, "Yuna", "Aqui também...")]
        [TestCase(124, "Yuna", "Aqui também...")]
        [TestCase(125, "Yuna", "Eu queria espaço pra brincar.")]
        [TestCase(199, "Yuna", "Eu queria espaço pra brincar.")]
        [TestCase(200, "", "")]
        [TestCase(299, "", "")]
        [TestCase(300, "Nanda", "Vamos. Um de cada vez!")]
        [TestCase(399, "Nanda", "Vamos. Um de cada vez!")]
        [TestCase(400, "", "")]
        public void HandDrawnCaptionsRespectDirectorExposureRanges(int frame, string speaker, string text)
        {
            ToyRoomHandDrawnTiming.CaptionAt(frame, out string actualSpeaker, out string actualText);
            Assert.That(actualSpeaker, Is.EqualTo(speaker));
            Assert.That(actualText, Is.EqualTo(text));
        }

        [Test]
        public void TimelineEvaluatesAllSevenStagesAndCompletesAtTotalDuration()
        {
            Assert.AreEqual(23.3f, ToyRoomIntroTimeline.TotalDurationSeconds, 0.001f);

            // TR_002 (0..0.8s): warm curtain fade in
            var s002 = ToyRoomIntroTimeline.Evaluate(0.4f, 960f, 540f);
            Assert.IsTrue(s002.IsActive);
            Assert.AreEqual(ToyRoomIntroStageId.Tr002, s002.StageId);
            Assert.Greater(s002.WarmCurtainAlpha, 0.45f);
            Assert.AreEqual(string.Empty, s002.DialogueText);

            // TR_003 (0.8..3.3s): pose 0, entry walk from x=600 to x=710
            var s003Start = ToyRoomIntroTimeline.Evaluate(0.8f, 960f, 540f);
            var s003End = ToyRoomIntroTimeline.Evaluate(3.0f, 960f, 540f);
            Assert.AreEqual(ToyRoomIntroStageId.Tr003, s003Start.StageId);
            Assert.AreEqual(0, s003Start.GirlPoseIndex);
            Assert.AreEqual(600f, s003Start.GirlX, 0.5f);
            Assert.AreEqual(710f, s003End.GirlX, 0.5f);

            // TR_004 (3.3..7.3s): pose 1, "Aqui também..."
            var s004 = ToyRoomIntroTimeline.Evaluate(5.0f, 960f, 540f);
            Assert.AreEqual(ToyRoomIntroStageId.Tr004, s004.StageId);
            Assert.AreEqual(1, s004.GirlPoseIndex);
            Assert.AreEqual("Aqui também...", s004.DialogueText);
            Assert.AreEqual("Menina", s004.DialogueHeading);

            // TR_005 (7.3..11.3s): pose 1, "Eu queria espaço pra brincar.", Nanda flies right & up
            var s005Mid = ToyRoomIntroTimeline.Evaluate(9.3f, 960f, 540f);
            Assert.AreEqual(ToyRoomIntroStageId.Tr005, s005Mid.StageId);
            Assert.AreEqual(1, s005Mid.GirlPoseIndex);
            Assert.AreEqual("Eu queria espaço pra brincar.", s005Mid.DialogueText);
            Assert.Greater(s005Mid.FairyX, 1140f);

            // TR_006 (11.3..18.3s): pose 2, two poetic lines separated by silence
            var s006First = ToyRoomIntroTimeline.Evaluate(12.5f, 960f, 540f);
            var s006Pause = ToyRoomIntroTimeline.Evaluate(14.8f, 960f, 540f);
            var s006Second = ToyRoomIntroTimeline.Evaluate(16.5f, 960f, 540f);
            Assert.AreEqual(ToyRoomIntroStageId.Tr006, s006First.StageId);
            Assert.AreEqual(2, s006First.GirlPoseIndex);
            Assert.AreEqual(ToyRoomIntroTimeline.PoeticPlayLine, s006First.DialogueText);
            Assert.AreEqual(string.Empty, s006Pause.DialogueText);
            Assert.AreEqual(ToyRoomIntroTimeline.PoeticPlaceLine, s006Second.DialogueText);

            // TR_007 (18.3..20.3s): pose 3, Nanda "Vamos. Um de cada vez!"
            var s007 = ToyRoomIntroTimeline.Evaluate(19.0f, 960f, 540f);
            Assert.AreEqual(ToyRoomIntroStageId.Tr007, s007.StageId);
            Assert.AreEqual(3, s007.GirlPoseIndex);
            Assert.AreEqual("Vamos. Um de cada vez!", s007.DialogueText);
            Assert.AreEqual("Nanda", s007.DialogueHeading);

            // TR_008 (20.3..23.3s): pose 3, music fade in and warm curtain at end
            var s008 = ToyRoomIntroTimeline.Evaluate(23.1f, 960f, 540f);
            Assert.AreEqual(ToyRoomIntroStageId.Tr008, s008.StageId);
            Assert.IsTrue(s008.StartMusic);
            Assert.Greater(s008.MusicFade, 0.85f);
            Assert.Greater(s008.WarmCurtainAlpha, 0.5f);

            // TR_009 (>= 23.3s): inactive
            var s009 = ToyRoomIntroTimeline.Evaluate(23.5f, 960f, 540f);
            Assert.IsFalse(s009.IsActive);
            Assert.AreEqual(ToyRoomIntroStageId.Tr009, s009.StageId);
        }

        [Test]
        public void TryAdvanceDialogueSkipsOnlyActiveDialogueBeatAndRespectsSilentBeats()
        {
            // Silent stage TR_003 refuses dialogue advance
            Assert.IsFalse(ToyRoomIntroTimeline.TryAdvanceDialogue(1.5f, out _));

            // TR_004 advances directly to TR_005 start (7.3s)
            Assert.IsTrue(ToyRoomIntroTimeline.TryAdvanceDialogue(4.0f, out float nextAfterTr004));
            Assert.AreEqual(7.3f, nextAfterTr004, 0.001f);

            // TR_006 first half advances to 3.9s local (11.3 + 3.9 = 15.2s)
            Assert.IsTrue(ToyRoomIntroTimeline.TryAdvanceDialogue(12.0f, out float nextInTr006));
            Assert.AreEqual(15.2f, nextInTr006, 0.001f);

            // TR_006 silent pause (3.1..3.9s local) refuses advance
            Assert.IsFalse(ToyRoomIntroTimeline.TryAdvanceDialogue(14.7f, out _));

            // TR_006 second half advances to TR_007 start (18.3s)
            Assert.IsTrue(ToyRoomIntroTimeline.TryAdvanceDialogue(16.0f, out float nextAfterTr006));
            Assert.AreEqual(18.3f, nextAfterTr006, 0.001f);
        }
    }
}
