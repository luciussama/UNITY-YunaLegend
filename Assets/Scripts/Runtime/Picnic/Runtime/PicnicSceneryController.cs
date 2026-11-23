using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicSceneryController : MonoBehaviour
    {
        private Camera stageCamera;
        private SpriteRenderer backdrop;
        private PicnicWheelView[] wheels;
        public void Build(Camera camera, PicnicStageSpec stage)
        {
            stageCamera = camera;
            backdrop = PicnicArt.Create("Original panoramic garden", camera.transform, Vector3.zero, 20f, -20, "scenery");
            backdrop.transform.localPosition = new Vector3(0f, 0f, 20f);
            PicnicArt.Create("Home and painted stairs", transform, new Vector3(2.8f, -0.12f, 0.2f), 5f, 2, "house");
            var tree = PicnicArt.Create("Great tree", transform, new Vector3(PicnicStageSpec.Goal - 2.1f, -0.04f, 0.2f), 3.36f, 2, "tree");
            PicnicArt.SetHeight(tree, 3.6f);
            PicnicArt.Create("Lateral picnic with toy friends", transform, new Vector3(PicnicStageSpec.Goal + 2.05f, -0.18f, -0.15f), 4f, 8, "picnic");
            for (int i = 0; i < stage.Supports.Count - 1; i++)
            {
                var a = stage.Supports[i];
                var b = stage.Supports[i + 1];
                if (a.Terrain != PicnicTerrain.Stone) continue;
                float left = a.Left(0f) - 0.9f;
                float right = a.Right(0f) + 0.9f;
                var water = new GameObject("Clear flowing stream", typeof(SpriteRenderer));
                water.transform.SetParent(transform, false);
                water.transform.position = new Vector3((left + right) * 0.5f, -0.8f, 0.05f);
                var renderer = water.GetComponent<SpriteRenderer>();
                renderer.sprite = PicnicArt.Load("soil");
                renderer.color = new Color(0.38f, 0.82f, 0.9f, 0.65f);
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(right - left, 0.55f);
                renderer.sortingOrder = 2;
                PicnicArt.Create("Stream fish", water.transform, new Vector3(a.X, -0.75f, -0.1f), 0.32f, 3, "aquatic" + (1 + i % 3));
                PicnicArt.Create("Aquatic plant", water.transform, new Vector3(a.X - 0.5f, -1.3f, 0.1f), 0.35f, 1, "aquatic4");
            }
        }
        public void Apply(PicnicSimulation sim)
        {
            float height = 6.9f;
            float width = height * stageCamera.aspect;
            stageCamera.orthographicSize = height * 0.5f;
            float cx = Mathf.Clamp(sim.X + width * 0.14f, width * 0.5f, 481.68f - width * 0.5f);
            stageCamera.transform.position = new Vector3(cx, height * 0.31f, -10f);
            float bgHeight = Mathf.Max(height, width * 2f / 3f) * 1.35f;
            PicnicArt.SetHeight(backdrop, bgHeight);
            float bgWidth = bgHeight * 1.5f;
            backdrop.transform.localPosition = new Vector3((bgWidth - width) * (0.5f - sim.X / PicnicStageSpec.Goal), (bgHeight - height) * 0.5f, 20f);
            if (wheels == null) wheels = GetComponentsInChildren<PicnicWheelView>();
            foreach (var wheel in wheels) wheel.Apply(sim.Time);
        }
    }
}
