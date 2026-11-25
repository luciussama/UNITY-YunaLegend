using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;
using Debug = UnityEngine.Debug;

namespace YunaLegend.Picnic.Audit
{
    public sealed class PicnicPlayModePerfAudit
    {
        [UnityTest]
        public IEnumerator MeasureRealPlayModePerformanceAndWormPlacement()
        {
            var load = SceneManager.LoadSceneAsync("Picnic_Gameplay", LoadSceneMode.Single);
            while (!load.isDone) yield return null;
            yield return null;

            var bootstrap = UnityEngine.Object.FindFirstObjectByType<PicnicStageBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            var sim = bootstrap.Simulation;
            sim.SkipStory();
            yield return null;

            int totalObjects = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            int activeRenderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
            int totalRenderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

            var frameTimes = new float[240];
            var simTimes = new double[240];
            var sw = new Stopwatch();
            long gcBefore = GC.GetTotalMemory(false);

            for (int i = 0; i < frameTimes.Length; i++)
            {
                sw.Restart();
                var cmd = PicnicQaPilot.Read(sim);
                sim.Step(1f / 60f, cmd);
                sw.Stop();
                simTimes[i] = sw.Elapsed.TotalMilliseconds;
                frameTimes[i] = Time.unscaledDeltaTime * 1000f;
                yield return null;
            }

            long gcAfter = GC.GetTotalMemory(false);
            Array.Sort(frameTimes);
            Array.Sort(simTimes);

            float p50 = frameTimes[frameTimes.Length / 2];
            float p95 = frameTimes[(int)(frameTimes.Length * 0.95f)];
            float maxMs = frameTimes[frameTimes.Length - 1];
            double simP50 = simTimes[simTimes.Length / 2];
            double simMax = simTimes[simTimes.Length - 1];

            // Inspect first worm position vs ground
            var worm0 = sim.Encounters.Worms[0];
            var wormSup = sim.Level.PlatformById[worm0.SupportId];
            var wormGo = GameObject.Find(worm0.Id);

            Debug.Log($"[PERF_AUDIT] totalTransforms={totalObjects} totalSpriteRenderers={totalRenderers} activeSpriteRenderers={activeRenderers}");
            Debug.Log($"[PERF_AUDIT] frameMs p50={p50:F2} p95={p95:F2} max={maxMs:F2} | simMs p50={simP50:F3} max={simMax:F3} | gcDeltaKB={(gcAfter - gcBefore) / 1024f:F1}");
            Debug.Log($"[WORM_AUDIT] id={worm0.Id} x={worm0.X:F1} y={worm0.Y:F1} supportId={worm0.SupportId} supX={wormSup.X:F1}..{wormSup.X + wormSup.Width:F1} supY={wormSup.Y:F1} goPos={(wormGo != null ? wormGo.transform.position.ToString("F3") : "null")}");

            Assert.That(sim.PlayerX, Is.GreaterThan(600f));
        }
    }
}
