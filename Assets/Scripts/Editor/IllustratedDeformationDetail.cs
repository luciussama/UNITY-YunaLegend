using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D;
using Object = UnityEngine.Object;

namespace YunaLegend.EditorScripts
{
    /// <summary>
    /// T02.3 diagnostic only: renders the same painting under the real provisional poses and their limit,
    /// magnified around the character, so the review can see what the deformation layer actually moves.
    /// Edit mode; no play mode, no scene saved, no gameplay touched.
    /// </summary>
    public static class IllustratedDeformationDetail
    {
        const float VisibleHeight = 1.8f;
        const int Width = 1280, Height = 800;

        static string Argument(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }

        [Serializable] class Panel
        {
            public string name, note;
            public float lean, sideBandSwing, headTilt, squashX, squashY, breathe, bodyCompression;
            public float minSoleLocalY, maxLocalY, maxAbsLocalX;
        }

        [Serializable] class Report
        {
            public string method = "Edit-mode renders of the isolated validation scene with a dedicated detail camera (45 deg, 1.8 m visible height, about 2.2x the character height). Parameters are the ones the proof driver applies, not tuned for this image.";
            public string scene, painting;
            public float pixelsPerUnit;
            public float heightMetres;
            public List<Panel> panels = new List<Panel>();
        }

        public static void Run()
        {
            var output = Argument("-detailOutput", "Evidence/25d/T02.3/deformation");
            Directory.CreateDirectory(output);
            EditorSceneManager.OpenScene(IllustratedAnimationProof.ScenePath);
            var skin = Object.FindFirstObjectByType<PaintedMeshDeformer>();
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if (skin == null || player == null)
                throw new InvalidOperationException("Run T02.3 capture first: proof scene or painted skin missing.");
            skin.Build(skin.sprite, skin.material);
            skin.flipX = false;

            var camera = new GameObject("DetailCamera_PROOF", typeof(Camera)).GetComponent<Camera>();
            camera.fieldOfView = 45f;
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.02f, .025f, .04f);
            float distance = VisibleHeight / (2f * Mathf.Tan(22.5f * Mathf.Deg2Rad));
            camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + .85f, -distance);
            camera.transform.rotation = Quaternion.identity;

            var report = new Report
            {
                scene = IllustratedAnimationProof.ScenePath,
                painting = IllustratedYunaProof.TexturePath,
                pixelsPerUnit = skin.sprite != null ? skin.sprite.pixelsPerUnit : 0f,
                heightMetres = skin.Height
            };

            Panel Render(string name, string note, float lean, float band, float tilt, float squashY, float squashX, float breathe, float compression)
            {
                skin.lean = lean; skin.sideBandSwing = band; skin.headTilt = tilt;
                skin.squashY = squashY; skin.squashX = squashX; skin.breathe = breathe; skin.bodyCompression = compression;
                skin.Refresh();
                var image = DarkRoomStartCapture.RenderDisplayImage(camera, Width, Height);
                File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG());
                Object.DestroyImmediate(image);
                var panel = new Panel
                {
                    name = name, note = note, lean = lean, sideBandSwing = band, headTilt = tilt,
                    squashX = squashX, squashY = squashY, breathe = breathe, bodyCompression = compression,
                    minSoleLocalY = skin.MinSoleLocalY, maxLocalY = skin.MaxLocalY, maxAbsLocalX = skin.MaxAbsLocalX
                };
                report.panels.Add(panel);
                return panel;
            }

            // Neutral bind pose: must match the approved painting pixel for pixel before any pose is applied.
            Render("01-bind", "Sem pose: a pintura aprovada, sem deformacao.", 0f, 0f, 0f, 1f, 1f, 0f, 0f);
            // Real run parameters, at the two gait extremes the driver uses.
            Render("02-run-contact", "Corrida, contato: inclinacao 8,2 graus para frente, compressao 5,5 por cento.", 8.2f, 12f, -2.87f, 1f, 1f, .004f, .055f);
            Render("03-run-passing", "Corrida, passagem: inclinacao 5,8 graus, compressao 1,5 por cento, faixa lateral invertida.", 5.8f, -12f, -2.03f, 1f, 1f, -.004f, .015f);
            // Real landing parameters.
            Render("04-land", "Pouso: achatamento 7,5 por cento e alargamento 5 por cento.", 6f, -6f, -2f, .925f, 1.05f, 0f, 0f);
            // Limit of the technique, not a candidate: shows where the painting stops holding together.
            Render("05-limit", "Limite da tecnica (nao e candidata): inclinacao 25 graus, faixa 25, compressao 12 por cento.", 25f, 25f, -8.75f, .9f, 1.06f, 0f, .12f);

            skin.ResetPose();
            Object.DestroyImmediate(camera.gameObject);
            File.WriteAllText(Path.Combine(output, "deformation-detail.json"), JsonUtility.ToJson(report, true));
            Debug.Log("ILLUSTRATED_DEFORMATION_DETAIL_DONE panels=" + report.panels.Count);
        }
    }
}
