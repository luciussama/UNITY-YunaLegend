using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Dialogue;
using YunaLegend.EditorScripts;
using YunaLegend.YunaIllustration;
using Object = UnityEngine.Object;

namespace YunaLegend.NandaIllustration.Editor
{
    [InitializeOnLoad]
    public static class NandaAccompanimentCapture
    {
        const string Key = "T042.Accompaniment.Active";
        const string Fixture = "Assets/Art/Illustrated25D/Yuna/Joint/YunaT03JointReview.unity";
        const string Flight = "Assets/Art/Illustrated25D/Nanda/Flight";
        static readonly string[] Protected = {
            Fixture, "Assets/Scenes/DarkRoom_Gameplay.unity",
            "Assets/Scripts/Runtime/DarkRoom/NandaFairyGuide.cs",
            "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs",
            "Evidence/25d/T01/nanda-blue-v1.png",
            Flight + "/NandaFlightIllustration.cs", Flight + "/Nanda_Flight_Atlas_v1.png"
        };
        [Serializable] class Source { public string path, before, after; }
        [Serializable] class Sample
        {
            public float t, yunaX, yunaY, nandaX, nandaY, direction, intensity, range, trailWidth, gesture;
            public int pose, trailPoints;
            public string mood;
            public bool flipX, trailEmitting;
        }
        [Serializable] class Proof
        {
            public string method = "Unity PlayMode on isolated T03 flat fixture; existing Yuna controller and NandaFairyGuide. No platform traversal, production camera, mobile or aesthetic acceptance claim.";
            public string unity;
            public int simulationHz, captureFps = 15, frames;
            public bool success, protectedUnchanged = true, blueOnly = true, facingCorrect = true;
            public bool sawRight, sawLeft, sawHappy, sawAnnoyed, resetTrailCleared;
            public float maxIntensity, maxRange, minLeadingDistance = float.MaxValue;
            public List<Source> sources = new List<Source>();
            public List<Sample> samples = new List<Sample>();
            public string error = "";
        }
        static Proof proof;
        static Yuna25DPlayerController player;
        static YunaRunIllustration run;
        static YunaIdleIllustration idle;
        static YunaIllustrationVisibilityResolver resolver;
        static NandaFairyGuide guide;
        static NandaAccompaniment presentation;
        static NandaFlightIllustration flight;
        static Camera game, detail;
        static int tick, lastFrame = -1;
        static string Arg(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, key);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
        }
        static int Hz => int.Parse(Arg("-t042Hz", "60"), CultureInfo.InvariantCulture);
        static string Output => Arg("-t042Output", "Evidence/25d/T04.2/capture-60");
        static string Hash(string path)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
        static NandaAccompanimentCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    try { Initialize(); EditorApplication.update += Tick; }
                    catch (Exception error) { Fail(error); }
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 1));
                }
            };
        }
        public static void Run()
        {
            if (Hz != 30 && Hz != 60 && Hz != 120) throw new ArgumentException("Use 30/60/120 Hz.");
            Directory.CreateDirectory(Output + "/game"); Directory.CreateDirectory(Output + "/detail");
            var sources = new Proof();
            foreach (string path in Protected) sources.sources.Add(new Source { path = path, before = Hash(path) });
            SessionState.SetString(Key + ".Sources", JsonUtility.ToJson(sources));
            EditorSceneManager.OpenScene(Fixture);
            SessionState.SetBool(Key, true); SessionState.SetInt(Key + ".Exit", 1);
            EditorApplication.EnterPlaymode();
        }
        static void Initialize()
        {
            proof = JsonUtility.FromJson<Proof>(SessionState.GetString(Key + ".Sources", ""));
            proof.unity = Application.unityVersion; proof.simulationHz = Hz;
            Time.captureDeltaTime = 1f / Hz; tick = 0; lastFrame = -1;
            player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            run = Object.FindFirstObjectByType<YunaRunIllustration>();
            idle = Object.FindFirstObjectByType<YunaIdleIllustration>();
            resolver = Object.FindFirstObjectByType<YunaIllustrationVisibilityResolver>();
            if (player == null || run == null || idle == null || resolver == null)
                throw new InvalidOperationException("Missing accepted T03 fixture components.");
            game = GameObject.Find("Run_GameCamera").GetComponent<Camera>();
            detail = GameObject.Find("Run_DetailCamera").GetComponent<Camera>();
            player.SetFloorDeathActive(false); player.AutoRunActive = false; player.enabled = true;
            var root = new GameObject("T042_Nanda_ExistingGuide"); root.SetActive(false);
            root.transform.position = player.transform.position + new Vector3(1.65f, 1.4f, .2f);
            var visual = new GameObject("ApprovedT041Painting"); visual.transform.SetParent(root.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>(); renderer.sortingOrder = 20;
            flight = root.AddComponent<NandaFlightIllustration>(); flight.painting = renderer;
            flight.visualRoot = visual.transform; flight.frames = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                flight.frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(Flight + "/Runtime/Frames/NandaFlight" + i + ".asset");
                if (flight.frames[i] == null) throw new InvalidOperationException("Missing approved Nanda pose " + i);
            }
            var light = new GameObject("LocalBlueLight"); light.transform.SetParent(root.transform, false);
            var lamp = light.AddComponent<Light>(); lamp.type = LightType.Point;
            var trail = new GameObject("DiscreteBlueTrail"); trail.transform.SetParent(root.transform, false);
            var ribbon = trail.AddComponent<TrailRenderer>(); ribbon.autodestruct = false;
            guide = root.AddComponent<NandaFairyGuide>();
            var config = new SerializedObject(guide);
            config.FindProperty("yunaTransform").objectReferenceValue = player.transform;
            config.FindProperty("guideLight").objectReferenceValue = lamp;
            config.FindProperty("sparkleTrail").objectReferenceValue = ribbon;
            foreach (string color in new[] { "normalColor", "happyColor", "annoyedColor" })
                config.FindProperty(color).colorValue = new Color(.32f, .58f, 1f);
            config.FindProperty("hoverSmoothTime").floatValue = .01f;
            config.FindProperty("baseIntensity").floatValue = .65f;
            config.FindProperty("lightRange").floatValue = 2.2f;
            config.ApplyModifiedPropertiesWithoutUndo();
            presentation = root.AddComponent<NandaAccompaniment>();
            presentation.guide = guide; presentation.player = player; presentation.flight = flight;
            presentation.blueLight = lamp; presentation.blueTrail = ribbon;
            root.SetActive(true); guide.ReturnToYuna(); presentation.ResetPresentation();
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || Time.frameCount == lastFrame) return;
            lastFrame = Time.frameCount;
            try
            {
                float t = ++tick / (float)Hz;
                bool moving = t >= 1f && t < 9f;
                player.AutoRunActive = moving;
                player.RunDirection = t < 5f ? 1f : -1f;
                guide.ApplyMood(t < 3f ? NandaMood.Normal : t < 5f ? NandaMood.Happy
                    : t < 7f ? NandaMood.Annoyed : NandaMood.Normal);
                resolver.showIdle = !moving; run.Refresh(); idle.Evaluate(t); resolver.Resolve();
                presentation.ApplyPresentation(t);
                if (tick % (Hz / 15) != 0) return;
                var lamp = presentation.blueLight; var ribbon = presentation.blueTrail;
                proof.blueOnly &= lamp.color.b > lamp.color.g && lamp.color.g > lamp.color.r;
                proof.facingCorrect &= flight.FlipX == (player.RunDirection < 0f);
                proof.maxIntensity = Mathf.Max(proof.maxIntensity, lamp.intensity);
                proof.maxRange = Mathf.Max(proof.maxRange, lamp.range);
                // A direction reversal deliberately permits the old guide to cross to the leading side.
                if (t > 1.5f && (t < 5f || t > 6f))
                    proof.minLeadingDistance = Mathf.Min(proof.minLeadingDistance,
                        player.RunDirection * (guide.transform.position.x - player.transform.position.x));
                proof.sawRight |= moving && player.RunDirection > 0f;
                proof.sawLeft |= moving && player.RunDirection < 0f;
                proof.sawHappy |= guide.Mood == NandaMood.Happy;
                proof.sawAnnoyed |= guide.Mood == NandaMood.Annoyed;
                game.transform.position = new Vector3(player.transform.position.x, 2.05f, -9.0533f);
                // Pair detail keeps both identities in view; does not change production camera assets.
                detail.transform.position = new Vector3(player.transform.position.x + player.RunDirection * .8f, 1.2f, -4.8f);
                Save(game, 1280, 720, "game"); Save(detail, 800, 600, "detail");
                proof.samples.Add(new Sample { t = t, yunaX = player.transform.position.x, yunaY = player.transform.position.y,
                    nandaX = guide.transform.position.x, nandaY = guide.transform.position.y,
                    direction = player.RunDirection, pose = flight.Pose, flipX = flight.FlipX,
                    mood = guide.Mood.ToString(), intensity = lamp.intensity, range = lamp.range,
                    trailWidth = presentation.trailWidth, trailPoints = ribbon.positionCount,
                    trailEmitting = ribbon.emitting, gesture = presentation.ExpressionAmount });
                proof.frames++;
                if (proof.frames >= 150)
                {
                    presentation.ResetPresentation(); proof.resetTrailCleared = ribbon.positionCount == 0;
                    Finish();
                }
            }
            catch (Exception error) { Fail(error); }
        }
        static void Save(Camera camera, int width, int height, string group)
        {
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
            try { File.WriteAllBytes(Output + "/" + group + "/" + proof.frames.ToString("D4") + ".png", image.EncodeToPNG()); }
            finally { Object.DestroyImmediate(image); }
        }
        static void Fail(Exception error)
        {
            Debug.LogException(error);
            if (proof == null) proof = new Proof();
            proof.error = error.ToString(); Finish();
        }
        static void Finish()
        {
            EditorApplication.update -= Tick;
            foreach (var source in proof.sources)
            {
                source.after = Hash(source.path); proof.protectedUnchanged &= source.before == source.after;
            }
            proof.success = proof.error.Length == 0 && proof.frames == 150 && proof.protectedUnchanged
                && proof.blueOnly && proof.facingCorrect && proof.sawRight && proof.sawLeft
                && proof.sawHappy && proof.sawAnnoyed && proof.resetTrailCleared
                && proof.maxIntensity <= .8f && proof.maxRange <= 2.5f && proof.minLeadingDistance >= 1.35f;
            File.WriteAllText(Output + "/accompaniment-proof.json", JsonUtility.ToJson(proof, true));
            Time.captureDeltaTime = 0f;
            SessionState.SetInt(Key + ".Exit", proof.success ? 0 : 1);
            EditorApplication.ExitPlaymode();
        }
    }
}
