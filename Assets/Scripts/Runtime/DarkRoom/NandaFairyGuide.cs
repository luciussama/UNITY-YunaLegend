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
        [SerializeField] private float baseIntensity = 2.5f;
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

        private Vector3 currentVelocity;
        private Vector3 targetPosition;
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
            ApplyMood(NandaMood.Normal);
        }

        private void Update()
        {
            hoverTimer += Time.deltaTime;
            CalculateTargetPosition();
            SmoothFlyToTarget();
        }

        public void SetYuna(Transform target)
        {
            yunaTransform = target;
        }

        private void CalculateTargetPosition()
        {
            if (!isGuidingAhead || yunaTransform == null)
            {
                // Paira ao redor do ombro da Yuna com oscilacao vertical suave
                float bob = Mathf.Sin(hoverTimer * bobbingSpeed) * bobbingAmplitude;
                targetPosition = (yunaTransform != null ? yunaTransform.position : transform.position) + followOffset + new Vector3(0f, bob, 0f);
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
                targetPosition = targetMarker.GetTopCenterWorld() + new Vector3(0f, 1.2f + bob, 0f);
            }
            else
            {
                targetPosition = (yunaTransform != null ? yunaTransform.position : transform.position) + followOffset;
            }
        }

        private void SmoothFlyToTarget()
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, hoverSmoothTime);
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
