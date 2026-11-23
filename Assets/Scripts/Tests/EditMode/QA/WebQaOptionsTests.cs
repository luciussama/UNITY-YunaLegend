using NUnit.Framework;
using YunaLegend.Qa.Core;

namespace YunaLegend.Qa.Tests
{
    public sealed class WebQaOptionsTests
    {
        [Test]
        public void ParseFromUrl_WithoutQuery_ReturnsDisabled()
        {
            WebQaOptions options = WebQaOptions.ParseFromUrl("http://127.0.0.1:57601/");

            Assert.IsFalse(options.Enabled);
            Assert.IsFalse(options.AutoPlay);
            Assert.IsFalse(options.ArenaFinalWave);
            Assert.AreEqual(string.Empty, options.StartScene);
        }

        [Test]
        public void ParseFromUrl_WithUnrelatedQuery_ReturnsDisabled()
        {
            WebQaOptions options = WebQaOptions.ParseFromUrl("http://127.0.0.1:57601/?lang=pt&v=2#top");

            Assert.IsFalse(options.Enabled);
            Assert.IsFalse(options.AutoPlay);
            Assert.IsFalse(options.ArenaFinalWave);
        }

        [Test]
        public void ParseFromUrl_WithQaAutoplayAndArenaFinal_EnablesExpectedFlags()
        {
            WebQaOptions options = WebQaOptions.ParseFromUrl(
                "http://127.0.0.1:57601/?qa=1&qa_autoplay=1&qa_arena_final=true&qa_scene=Arena_Gameplay#hash");

            Assert.IsTrue(options.Enabled);
            Assert.IsTrue(options.AutoPlay);
            Assert.IsTrue(options.ArenaFinalWave);
            Assert.AreEqual("Arena_Gameplay", options.StartScene);
        }

        [Test]
        public void ParseFromUrl_WithQaZeroAndNoOtherFlags_ReturnsDisabled()
        {
            WebQaOptions options = WebQaOptions.ParseFromUrl("http://127.0.0.1:57601/?qa=0&qa_autoplay=false");

            Assert.IsFalse(options.Enabled);
            Assert.IsFalse(options.AutoPlay);
            Assert.IsFalse(options.ArenaFinalWave);
        }
    }
}
