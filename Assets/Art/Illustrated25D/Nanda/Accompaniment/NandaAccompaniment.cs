using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Dialogue;

namespace YunaLegend.NandaIllustration
{
    /// <summary>Presentation only: the existing guide remains sole owner of navigation.</summary>
    [DefaultExecutionOrder(220)]
    public sealed class NandaAccompaniment : MonoBehaviour
    {
        public NandaFairyGuide guide;
        public Yuna25DPlayerController player;
        public NandaFlightIllustration flight;
        public Light blueLight;
        public TrailRenderer blueTrail;
        public float lightIntensity = .65f;
        public float lightRange = 2.2f;
        public float trailLifetime = .32f;
        public float trailWidth = .028f;

        public NandaMood Expression { get; private set; }
        public float ExpressionAmount { get; private set; }
        public float PresentationClock { get; private set; }
        Quaternion restRotation;
        Material trailMaterial;
        bool ownsMaterial;

        void Awake()
        {
            if (guide == null) guide = GetComponent<NandaFairyGuide>();
            if (flight == null) flight = GetComponent<NandaFlightIllustration>();
            if (blueLight == null) blueLight = GetComponentInChildren<Light>();
            if (blueTrail == null) blueTrail = GetComponentInChildren<TrailRenderer>();
            if (flight != null && flight.visualRoot != null) restRotation = flight.visualRoot.localRotation;
            ConfigureTrail();
        }

        void ConfigureTrail()
        {
            if (blueTrail == null) return;
            if (blueTrail.sharedMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader == null) { blueTrail.emitting = false; return; }
                trailMaterial = new Material(shader) { name = "T042_BlueTrail_Runtime" };
                blueTrail.sharedMaterial = trailMaterial;
                ownsMaterial = true;
            }
            blueTrail.time = trailLifetime;
            blueTrail.minVertexDistance = .04f;
            blueTrail.widthCurve = AnimationCurve.Linear(0f, trailWidth, 1f, 0f);
            blueTrail.widthMultiplier = 1f;
            blueTrail.numCapVertices = 3;
            blueTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            blueTrail.receiveShadows = false;
            blueTrail.sortingOrder = 19;
            var gradient = new Gradient();
            gradient.SetKeys(new[] {
                new GradientColorKey(new Color(.34f, .63f, 1f), 0f),
                new GradientColorKey(new Color(.25f, .48f, .92f), 1f)
            }, new[] { new GradientAlphaKey(.24f, 0f), new GradientAlphaKey(0f, 1f) });
            blueTrail.colorGradient = gradient;
        }

        void LateUpdate()
        {
            PresentationClock += Time.deltaTime;
            ApplyPresentation(PresentationClock);
        }

        public void ApplyPresentation(float seconds)
        {
            Expression = guide != null ? guide.Mood : NandaMood.Normal;
            ExpressionAmount = Expression == NandaMood.Happy ? 1f
                : Expression == NandaMood.Annoyed || Expression == NandaMood.Facepalm ? -.6f
                : Expression == NandaMood.Determined ? .45f : 0f;
            float direction = player != null ? player.RunDirection : 1f;
            if (flight != null)
            {
                flight.SetDirection(direction);
                // A small whole-body gesture preserves every painted facial pixel from T04.1.
                if (flight.visualRoot != null && flight.visualRoot != transform)
                    flight.visualRoot.localRotation = restRotation * Quaternion.Euler(0f, 0f,
                        direction * ExpressionAmount * 4f * Mathf.Sin(seconds * 2.4f));
            }
            if (blueLight != null)
            {
                blueLight.color = new Color(.32f, .58f, 1f);
                blueLight.range = Mathf.Clamp(lightRange, .1f, 2.5f);
                blueLight.intensity = Mathf.Clamp(lightIntensity, 0f, .8f)
                    * (1f + .06f * Mathf.Sin(seconds * 2.4f) + .08f * Mathf.Max(0f, ExpressionAmount));
                blueLight.shadows = LightShadows.None;
            }
            if (blueTrail != null) blueTrail.emitting = isActiveAndEnabled;
        }

        public void ResetPresentation()
        {
            PresentationClock = 0f;
            if (blueTrail != null) blueTrail.Clear();
            if (flight != null) flight.Evaluate(0f, player != null ? player.RunDirection : 1f);
            ApplyPresentation(0f);
        }

        void OnDisable()
        {
            if (blueTrail != null) { blueTrail.emitting = false; blueTrail.Clear(); }
            if (flight != null && flight.visualRoot != null) flight.visualRoot.localRotation = restRotation;
        }

        void OnDestroy()
        {
            if (ownsMaterial && trailMaterial != null) Destroy(trailMaterial);
        }
    }
}
