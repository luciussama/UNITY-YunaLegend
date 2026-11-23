using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicNandaController : MonoBehaviour
    {
        private SpriteRenderer painting;
        private Light glow;
        public void Build()
        {
            painting = PicnicArt.Create("Nanda blue flight", transform, Vector3.zero, 0.65f, 14, "nanda0");
            var lightObject = new GameObject("Blue emitted light");
            lightObject.transform.SetParent(transform, false);
            glow = lightObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.3f, 0.7f, 1f);
            glow.range = 2.4f;
            glow.intensity = 0.5f;
        }
        public void Apply(PicnicSimulation sim)
        {
            transform.position = new Vector3(sim.X + (sim.Phase == PicnicPhase.Rescuing ? 0f : 0.85f),
                sim.Y + (sim.Phase == PicnicPhase.Rescuing ? 0.65f : 1.2f) + Mathf.Sin(sim.Time * 4f) * 0.08f, -0.4f);
            painting.sprite = PicnicArt.Load("nanda" + (Mathf.FloorToInt(sim.Time * 12f) % 8));
            PicnicArt.SetHeight(painting, 0.64f);
            painting.transform.localPosition = Vector3.zero;
        }
    }
}
