using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Tests
{
    public sealed class BearChaseEndingTimelineTests
    {
        [Test]
        public void Evaluate_ProgressesThroughAllFiveLegacyBeatsWithLiteralTexts()
        {
            BearChaseEndingSample b0 = BearChaseEndingTimeline.Evaluate(1.0f);
            Assert.AreEqual(BearChaseEndingBeat.NandaRelief, b0.Beat);
            Assert.IsTrue(b0.ShowDialogue);
            Assert.AreEqual("NANDA", b0.SpeakerHeader);
            StringAssert.Contains("Ufa, conseguimos.", b0.DialogueText);
            Assert.AreEqual(BearChaseEndingYunaPose.CatchingBreath, b0.YunaPose);
            Assert.AreEqual(BearChaseEndingNandaPose.Happy, b0.NandaPose);

            BearChaseEndingSample b1 = BearChaseEndingTimeline.Evaluate(6.0f);
            Assert.AreEqual(BearChaseEndingBeat.GirlSmiles, b1.Beat);
            Assert.AreEqual("UM QUARTO INTEIRO", b1.SpeakerHeader);
            Assert.AreEqual(BearChaseEndingYunaPose.Smiling, b1.YunaPose);

            BearChaseEndingSample b2Early = BearChaseEndingTimeline.Evaluate(9.5f);
            BearChaseEndingSample b2Late = BearChaseEndingTimeline.Evaluate(12.5f);
            Assert.AreEqual(BearChaseEndingBeat.QuestionGrows, b2Early.Beat);
            Assert.AreEqual(BearChaseEndingYunaPose.Smiling, b2Early.YunaPose);
            Assert.AreEqual(BearChaseEndingYunaPose.Confused, b2Late.YunaPose);

            BearChaseEndingSample b3 = BearChaseEndingTimeline.Evaluate(16.0f);
            Assert.AreEqual(BearChaseEndingBeat.GirlAsks, b3.Beat);
            Assert.AreEqual("MENINA", b3.SpeakerHeader);
            StringAssert.Contains("Vamos pegar os brinquedos de novo?", b3.DialogueText);

            BearChaseEndingSample b4Start = BearChaseEndingTimeline.Evaluate(BearChaseEndingTimeline.Beat3End + 0.1f);
            BearChaseEndingSample b4End = BearChaseEndingTimeline.Evaluate(BearChaseEndingTimeline.Beat4End - 0.1f);
            Assert.AreEqual(BearChaseEndingBeat.NandaFacepalmZoom, b4Start.Beat);
            Assert.IsFalse(b4Start.ShowDialogue);
            Assert.AreEqual(BearChaseEndingNandaPose.Facepalm, b4Start.NandaPose);
            Assert.Greater(b4End.CameraZoom, b4Start.CameraZoom);

            BearChaseEndingSample done = BearChaseEndingTimeline.Evaluate(BearChaseEndingTimeline.TotalDuration);
            Assert.IsTrue(done.IsCompleted);
            Assert.AreEqual(1f, done.FadeAlpha, 0.001f);
        }

        [Test]
        public void GetNextAdvanceTime_AdvancesBeatByBeatMonotonically()
        {
            float t = 0.5f;
            for (int i = 0; i < 6; i++)
            {
                float next = BearChaseEndingTimeline.GetNextAdvanceTime(t);
                Assert.Greater(next, t);
                t = next;
            }

            Assert.AreEqual(BearChaseEndingTimeline.TotalDuration, t, 0.001f);
        }
    }
}
