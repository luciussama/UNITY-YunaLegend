using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicCharacterController : MonoBehaviour
    {
        private CharacterController body;
        private SpriteRenderer painting;
        private float gait;
        private bool initialized;
        public void Build()
        {
            body = gameObject.AddComponent<CharacterController>();
            body.height = 0.85f;
            body.radius = 0.16f;
            body.center = new Vector3(0f, 0.45f, 0f);
            body.skinWidth = 0.015f;
            body.stepOffset = 0.06f;
            painting = PicnicArt.Create("Yuna approved illustration", transform, Vector3.zero, 0.7f, 12, "yuna-run0");
        }
        public void Apply(PicnicSimulation sim, float dt)
        {
            Vector3 desired = new Vector3(sim.X, sim.Y, 0f);
            if (!initialized || (transform.position - desired).sqrMagnitude > 9f || sim.Phase == PicnicPhase.Rescuing)
            {
                body.enabled = false;
                transform.position = desired;
                body.enabled = true;
                initialized = true;
            }
            else
            {
                body.Move(desired - transform.position);
                // Resolve only the controller's skin offset against the analytic top contract.
                if ((transform.position - desired).sqrMagnitude < 0.025f) transform.position = desired;
            }
            gait += Mathf.Abs(sim.VelocityX) * dt;
            bool running = Mathf.Abs(sim.VelocityX) > 0.05f;
            painting.sprite = PicnicArt.Load(running || !sim.Grounded ? "yuna-run" + (Mathf.FloorToInt(gait * 5f) % 8) : "yuna-pose1");
            PicnicArt.SetHeight(painting, running || !sim.Grounded ? 1.02f : 0.99f);
            painting.transform.localPosition = new Vector3(0f, -0.085f, -0.2f);
            painting.flipX = sim.VelocityX < -0.05f || (sim.Panic && Mathf.Sin(sim.Time * 3f) > 0.6f);
            painting.transform.localRotation = Quaternion.Euler(0f, 0f,
                sim.Terrain == PicnicTerrain.Gravel ? Mathf.Sin(sim.Time * 11f) * 2.6f : sim.Panic ? 5f : 0f);
            painting.color = new Color(1f, 1f, 1f, sim.Immunity > 0f && Mathf.Sin(sim.Time * 22f) < 0f ? 0.4f : 1f);
        }
    }
}
