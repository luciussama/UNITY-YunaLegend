using UnityEngine;
using YunaLegend.Dialogue;

namespace YunaLegend.DarkRoom
{
    public class NandaFairyGuide : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Transform yunaTransform;
        [SerializeField] private Light guideLight;
        [SerializeField] private TrailRenderer sparkleTrail;

        [Header("Parametros de Iluminacao Noturna")]
        [SerializeField] private Color normalColor = new Color(0.99f, 0.88f, 0.40f); // Dourado-ambar quente
        [SerializeField] private Color annoyedColor = new Color(0.95f, 0.45f, 0.20f);
        [SerializeField] private Color happyColor = new Color(1.0f, 0.95f, 0.60f);
        [SerializeField] private float baseIntensity = 2.2f;
        [SerializeField] private float lightRange = 7.0f;

        [Header("Comportamento de Voo e Mola")]
        [SerializeField] private float hoverSmoothTime = 0.35f;
        [SerializeField] private float bobbingSpeed = 3.5f;
        [SerializeField] private float bobbingAmplitude = 0.15f;
        [SerializeField] private Vector3 followOffset = new Vector3(0.8f, 1.4f, 0.2f);

        [Header("Guia de Plataformas")]
        [SerializeField] private int currentTargetPlatformIndex = 0;
        [SerializeField] private bool isGuidingAhead = true;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip twinkleSfx;

        private DarkRoomPlatformMarker traversalSupport;
        public void SetTraversalSupport(DarkRoomPlatformMarker marker) => traversalSupport=marker;

        private Vector3 currentVelocity;
        private Vector3 targetPosition;
        private CharacterController yunaBody;
        private Yuna25DPlayerController player;
        private float Direction => player != null ? Mathf.Sign(player.RunDirection) : 1f;
        private NandaMood currentMood = NandaMood.Normal;
        private float hoverTimer;

        public NandaMood Mood => currentMood;
        public int CurrentTargetPlatformIndex => currentTargetPlatformIndex;

        public void ConfigureAudio(AudioClip twinkle)
        {
            twinkleSfx = twinkle;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;
        }

        public void PlayTwinkle()
        {
            if (audioSource != null && twinkleSfx != null)
            {
                audioSource.PlayOneShot(twinkleSfx, 0.55f);
            }
        }

        private void Awake()
        {
            if (guideLight == null)
            {
                guideLight = GetComponentInChildren<Light>();
            }
            if (sparkleTrail == null)
            {
                sparkleTrail = GetComponentInChildren<TrailRenderer>();
            }
            SetYuna(yunaTransform);
            ApplyMood(NandaMood.Normal);
        }

        private void LateUpdate()
        {
            hoverTimer += Time.deltaTime;
            CalculateTargetPosition();
            SmoothFlyToTarget();
        }

        public void SetYuna(Transform target)
        {
            yunaTransform = target;
            yunaBody = target != null ? target.GetComponent<CharacterController>() : null;
            player = target != null ? target.GetComponent<Yuna25DPlayerController>() : null;
        }

        private void CalculateTargetPosition()
        {
            if (yunaTransform == null) { targetPosition = transform.position; return; }
            if(traversalSupport!=null)
            {
                var support=traversalSupport.Support;
                targetPosition=support.PointNear(Mathf.Min(yunaTransform.position.x+1.8f,support.MaxX-.35f))+new Vector3(0,1.2f,-.15f);
                return;
            }
            if (!isGuidingAhead || yunaTransform == null)
            {
                // Paira ao redor do ombro da Yuna com oscilacao vertical suave
                float bob = Mathf.Sin(hoverTimer * bobbingSpeed) * bobbingAmplitude;
                targetPosition = (yunaTransform != null ? yunaTransform.position : transform.position) + new Vector3(Direction * (1.65f + .22f * Mathf.Cos(hoverTimer * 1.8f)), followOffset.y + bob, followOffset.z + .12f * Mathf.Sin(hoverTimer * 1.8f));
                return;
            }

            // Procura a plataforma alvo no cenario para pairar exatamente sobre o topo fisico dela
            DarkRoomPlatformMarker[] markers = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsSortMode.None);
            DarkRoomPlatformMarker targetMarker = null;
            foreach (var m in markers)
            {
                if (m.spec.index == currentTargetPlatformIndex)
                {
                    targetMarker = m;
                    break;
                }
            }

            if (targetMarker != null)
            {
                float bob = Mathf.Sin(hoverTimer * bobbingSpeed) * bobbingAmplitude;
                targetPosition = targetMarker.GetTopCenterWorld() + new Vector3(.22f * Mathf.Cos(hoverTimer * 1.8f), 1.2f + bob, -.15f);
            }
            else
            {
                targetPosition = (yunaTransform != null ? yunaTransform.position : transform.position) + followOffset;
            }
        }

        private void SmoothFlyToTarget()
        {
            // Anticipate the runner before it reaches the fairy, then enforce separation
            // after locomotion. No physical collider: Nanda never blocks a jump.
            if (yunaTransform != null)
                targetPosition.x = Direction * Mathf.Max(Direction * targetPosition.x, Direction * yunaTransform.position.x + 1.4f);
            targetPosition = SeparateFromYuna(targetPosition);
            Vector3 candidate = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, hoverSmoothTime);
            Vector3 safe = SeparateFromYuna(candidate);
            if ((safe - candidate).sqrMagnitude > .000001f) currentVelocity = Vector3.zero;
            transform.position = safe;
        }

        // A conservative screen-plane ellipse encloses the capsule, ribbon and
        // fairy radius plus an air gap; Z alone cannot hide a visual overlap.
        private Vector3 SeparateFromYuna(Vector3 position)
        {
            if (yunaTransform == null) return position;
            Vector3 center = yunaBody != null ? yunaTransform.TransformPoint(yunaBody.center) : yunaTransform.position + Vector3.up * .7f;
            float scaleX = Mathf.Abs(yunaTransform.lossyScale.x);
            float scaleY = Mathf.Abs(yunaTransform.lossyScale.y);
            float rx = (yunaBody != null ? yunaBody.radius * scaleX : .35f) + .6f;
            float ry = (yunaBody != null ? yunaBody.height * scaleY * .5f : .7f) + .5f;
            float dy = (position.y - center.y) / ry;
            float dx = (position.x - center.x) / rx;
            if (Mathf.Abs(dy) < 1f && Mathf.Abs(position.x - center.x) < 2.5f)
            {
                float leadingEdge = Mathf.Sqrt(Mathf.Max(0f, 1f - dy * dy));
                if (Direction * dx < leadingEdge)
                    position.x = center.x + Direction * rx * leadingEdge;
            }
            return position;
        }

        public void AdvanceToNextPlatform()
        {
            currentTargetPlatformIndex++;
            ApplyMood(NandaMood.Happy);
            PlayTwinkle();
        }

        public void SetTargetPlatform(int index)
        {
            currentTargetPlatformIndex = index;
            isGuidingAhead = true;
            PlayTwinkle();
        }

        public void ReturnToYuna(string dialogueMessage = null)
        {
            isGuidingAhead = false;
            ApplyMood(NandaMood.Normal);
            if (!string.IsNullOrEmpty(dialogueMessage))
            {
                Debug.Log($"[Nanda]: {dialogueMessage}");
            }
        }

        public void ApplyMood(NandaMood mood)
        {
            currentMood = mood;
            if (guideLight == null) return;

            guideLight.range = lightRange;
            switch (mood)
            {
                case NandaMood.Annoyed:
                    guideLight.color = annoyedColor;
                    guideLight.intensity = baseIntensity * 1.3f;
                    break;
                case NandaMood.Happy:
                    guideLight.color = happyColor;
                    guideLight.intensity = baseIntensity * 1.2f;
                    break;
                default:
                    guideLight.color = normalColor;
                    guideLight.intensity = baseIntensity;
                    break;
            }
        }
    }
}
