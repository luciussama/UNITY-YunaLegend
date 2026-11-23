using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

namespace YunaLegend.Editor.Picnic
{
    /// <summary>Small local preview harness for the already-open Editor; accepts only named Picnic operations.</summary>
    [InitializeOnLoad]
    public static class PicnicEditorPreview
    {
        private const string Request = "Evidence/Picnic/editor-request.txt";
        private const string Result = "Evidence/Picnic/editor-result.txt";
        static PicnicEditorPreview()
        {
            EditorApplication.update += Poll;
        }
        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
            string command;
            try
            {
                command = File.ReadAllText(Request).Trim();
                if (string.IsNullOrEmpty(command)) return;
                File.Delete(Request);
            }
            catch (IOException)
            {
                // The producer may still be closing the request file; retry next update.
                return;
            }
            try
            {
                switch (command)
                {
                    case "build": PicnicSceneBuilder.Build(); break;
                    case "play": PicnicSceneBuilder.Play(); break;
                    case "stop": EditorApplication.isPlaying = false; break;
                    case "skip": UnityEngine.Object.FindFirstObjectByType<PicnicStageBootstrap>().Simulation.SkipNarrative(); break;
                    case "capture": ScreenCapture.CaptureScreenshot("Evidence/Picnic/editor-play.png"); break;
                    case "status": WriteRuntimeStatus(); return;
                    case "validate": ValidateScene(); return;
                    case "qa-start":
                        Environment.SetEnvironmentVariable("YUNA_QA_URL", "http://localhost/?qa=1&qa_autoplay=1&qa_scene=Picnic_Gameplay");
                        YunaLegend.Qa.Runtime.WebQaRuntime.Refresh();
                        break;
                    case "qa-stop":
                        Environment.SetEnvironmentVariable("YUNA_QA_URL", null);
                        YunaLegend.Qa.Runtime.WebQaRuntime.Refresh();
                        break;
                    default: throw new InvalidOperationException("Operação de preview desconhecida.");
                }
                File.WriteAllText(Result, "OK " + command);
            }
            catch (Exception error)
            {
                File.WriteAllText(Result, error.ToString());
                Debug.LogException(error);
            }
        }
        private static void WriteRuntimeStatus()
        {
            var stage = UnityEngine.Object.FindFirstObjectByType<PicnicStageBootstrap>();
            if (stage == null || stage.Simulation == null)
            {
                File.WriteAllText(Result, "NO_RUNNING_PICNIC");
                return;
            }
            var sim = stage.Simulation;
            var player = UnityEngine.Object.FindFirstObjectByType<PicnicCharacterController>();
            Vector3 actual = player != null ? player.transform.position : Vector3.zero;
            File.WriteAllText(Result, $"phase={sim.Phase} x={sim.X:F3} y={sim.Y:F3} actualX={actual.x:F3} actualY={actual.y:F3} supports={sim.Stage.Supports.Count} creatures={sim.Creatures.Length} grounded={sim.Grounded} jumps={sim.Jumps} hearts={sim.Hearts}");
        }
        private static void ValidateScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(PicnicSceneBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("Picnic scene is not open.");
            var stage = UnityEngine.Object.FindFirstObjectByType<PicnicStageBootstrap>();
            if (stage == null) throw new InvalidOperationException("Missing composition root.");
            var serialized = new SerializedObject(stage);
            var property = serialized.GetIterator();
            int references = 0;
            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script") continue;
                if (property.objectReferenceValue == null) throw new InvalidOperationException("Missing reference: " + property.name);
                references++;
            }
            int assets = 0;
            foreach (string file in Directory.GetFiles("Assets/Art/Picnic/Resources/Picnic", "*.png"))
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(file.Replace('\\', '/')) == null) throw new InvalidOperationException("Invalid sprite: " + file);
                assets++;
            }
            File.WriteAllText(Result, $"PICNIC_SCENE_VALIDATED references={references} sprites={assets}");
        }
    }
}
