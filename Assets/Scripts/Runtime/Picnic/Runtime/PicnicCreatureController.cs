using System.Collections.Generic;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicCreatureController : MonoBehaviour
    {
        private readonly List<SpriteRenderer> views = new List<SpriteRenderer>();
        private SpriteRenderer beetle;
        public void Build(PicnicSimulation sim)
        {
            foreach (var creature in sim.Creatures)
            {
                var view = PicnicArt.Create(creature.Spec.Kind.ToString(), transform,
                    new Vector3(creature.X, creature.Y, -0.3f), 0.8f, 11, Asset(creature));
                views.Add(view);
            }
            beetle = PicnicArt.Create("Harmless reassuring beetle", transform, Vector3.zero, 1.1f, 13, "beetle0");
        }
        public void Apply(PicnicSimulation sim)
        {
            for (int i = 0; i < views.Count; i++)
            {
                var creature = sim.Creatures[i];
                var view = views[i];
                bool visible = creature.Active && Mathf.Abs(creature.X - sim.X) < 14f;
                if (creature.Spec.Kind == PicnicCreatureKind.Worm && creature.Y < 0.1f) visible = false;
                view.gameObject.SetActive(visible);
                if (!visible) continue;
                view.sprite = PicnicArt.Load(Asset(creature));
                PicnicArt.SetWidth(view, creature.Spec.Kind == PicnicCreatureKind.Ant ? 0.65f : creature.Spec.Kind == PicnicCreatureKind.Worm ? 0.42f : 0.9f);
                view.transform.position = new Vector3(creature.X, creature.Y + 0.06f, -0.3f);
                view.transform.rotation = Quaternion.Euler(0f, 0f, creature.Warning ? Mathf.Sin(sim.Time * 12f) * 8f : 0f);
            }
            beetle.gameObject.SetActive(sim.Panic);
            beetle.transform.position = new Vector3(sim.X - 1.35f, sim.Y + 0.7f, -0.5f);
            beetle.sprite = PicnicArt.Load(Mathf.Sin(sim.Time * 20f) > 0f ? "beetle0" : "beetle1");
        }
        private static string Asset(PicnicCreatureState creature)
        {
            int frame = Mathf.FloorToInt(creature.Clock * (creature.Health == 1 ? 12f : 7f)) % 2;
            switch (creature.Spec.Kind)
            {
                case PicnicCreatureKind.Ant: return "ant" + frame;
                case PicnicCreatureKind.Caterpillar: return (creature.Health == 1 ? "red-caterpillar" : "caterpillar") + frame;
                case PicnicCreatureKind.Bee: return "bee" + frame;
                default: return "worm";
            }
        }
    }
}
