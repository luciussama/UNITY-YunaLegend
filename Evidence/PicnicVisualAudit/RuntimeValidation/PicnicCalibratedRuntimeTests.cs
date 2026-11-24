using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;
using YunaLegend.Qa.Runtime;

namespace YunaLegend.Picnic.Audit
{
    // Audit-only assembly copied into the isolated project, never installed in production Assets.
    public sealed class PicnicCalibratedRuntimeTests
    {
        private string previousQaUrl;
        private int previousEasyMode;
        private float previousCapture;
        private int previousVsync;
        private int previousTargetRate;
        private bool previousBackground;

        [Serializable]
        private sealed class Result
        {
            public int hz;
            public string phase;
            public float x;
            public float simulationSeconds;
            public int samples;
            public int falls;
            public int jumps;
            public int hearts;
            public int panics;
            public float maximumBodyErrorMetres;
            public float maximumBodyStepMetres;
        }

        [SetUp]
        public void Setup()
        {
            previousQaUrl = Environment.GetEnvironmentVariable("YUNA_QA_URL");
            previousEasyMode = PlayerPrefs.GetInt("YunaLegend.EasyMode", 0);
            previousCapture = Time.captureDeltaTime;
            previousVsync = QualitySettings.vSyncCount;
            previousTargetRate = Application.targetFrameRate;
            previousBackground = Application.runInBackground;
            Environment.SetEnvironmentVariable("YUNA_QA_URL", "http://audit.local/?qa=1&qa_autoplay=1");
            PlayerPrefs.SetInt("YunaLegend.EasyMode", 0);
            WebQaRuntime.Refresh();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Application.runInBackground = true;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Environment.SetEnvironmentVariable("YUNA_QA_URL", previousQaUrl);
            WebQaRuntime.Refresh();
            PlayerPrefs.SetInt("YunaLegend.EasyMode", previousEasyMode);
            Time.captureDeltaTime = previousCapture;
            QualitySettings.vSyncCount = previousVsync;
            Application.targetFrameRate = previousTargetRate;
            Application.runInBackground = previousBackground;
            yield return SceneManager.LoadSceneAsync("TitleScreen");
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ContinuousCourse30Hz()
        {
            return Run(30);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ContinuousCourse60Hz()
        {
            return Run(60);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ContinuousCourse120Hz()
        {
            return Run(120);
        }

        private IEnumerator Run(int hz)
        {
            Time.captureDeltaTime = 1f / hz;
            yield return SceneManager.LoadSceneAsync("Picnic_Gameplay");
            yield return null;
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<PicnicStageBootstrap>();
            var player = UnityEngine.Object.FindFirstObjectByType<PicnicCharacterController>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            var sim = bootstrap.Simulation;
            Assert.That(sim, Is.Not.Null);
            sim.SkipNarrative();
            var result = new Result { hz = hz };
            sim.Event += name =>
            {
                if (name.StartsWith("panic_started_", StringComparison.Ordinal))
                {
                    result.panics++;
                }
            };
            Vector3 previousPosition = player.transform.position;
            for (int frame = 0; frame < hz * 900 && sim.Phase == PicnicPhase.Playing; frame++)
            {
                yield return null;
                Vector3 position = player.transform.position;
                result.maximumBodyErrorMetres = Mathf.Max(result.maximumBodyErrorMetres,
                    Vector3.Distance(position, new Vector3(sim.X, sim.Y, 0f)));
                result.maximumBodyStepMetres = Mathf.Max(result.maximumBodyStepMetres,
                    Vector3.Distance(position, previousPosition));
                previousPosition = position;
                result.samples++;
            }
            result.phase = sim.Phase.ToString();
            result.x = sim.X;
            result.simulationSeconds = sim.Time;
            result.falls = sim.Falls;
            result.jumps = sim.Jumps;
            result.hearts = sim.Hearts;
            string output = Environment.GetEnvironmentVariable("YUNA_PICNIC_AUDIT_OUTPUT");
            Assert.That(output, Is.Not.Null.And.Not.Empty);
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "runtime-" + hz + "hz.json"), JsonUtility.ToJson(result, true));
            Debug.Log("PICNIC_CALIBRATED_RUNTIME " + JsonUtility.ToJson(result));
            Assert.That(sim.Phase, Is.EqualTo(PicnicPhase.Ending), "Continuous runtime did not reach the ending.");
            Assert.That(result.panics, Is.EqualTo(3));
            Assert.That(sim.Falls, Is.Zero);
            Assert.That(sim.Hearts, Is.EqualTo(3));
            Assert.That(result.maximumBodyErrorMetres, Is.LessThanOrEqualTo(0.035f),
                "Body/Core difference exceeds the predeclared 3.5 cm audit threshold.");
            sim.SkipNarrative();
            Assert.That(sim.Phase, Is.EqualTo(PicnicPhase.Complete));
        }
    }
}
