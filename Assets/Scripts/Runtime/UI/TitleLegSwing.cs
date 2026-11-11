using UnityEngine;

namespace YunaLegend.UI
{
    /// <summary>
    /// Balanco suave da perna da menina na tela de entrada (pendulo curto, como na arte canonica).
    /// A perna e uma camada sobreposta a arte; em repouso coincide pixel a pixel com o desenho.
    /// </summary>
    public sealed class TitleLegSwing : MonoBehaviour
    {
        [SerializeField] private float amplitudeDegrees = 2f;
        [SerializeField] private float periodSeconds = 3.4f;
        [SerializeField] private float phaseDegrees = 0f;

        private RectTransform rect;
        private bool paused;

        public float AmplitudeDegrees => amplitudeDegrees;

        private void Awake()
        {
            rect = transform as RectTransform;
        }

        private void OnEnable()
        {
            paused = false;
        }

        private void Update()
        {
            if (paused || rect == null) return;
            float phase = (Time.time / Mathf.Max(0.1f, periodSeconds)) * Mathf.PI * 2f + phaseDegrees * Mathf.Deg2Rad;
            rect.localRotation = Quaternion.Euler(0f, 0f, amplitudeDegrees * Mathf.Sin(phase));
        }

        /// <summary>Usado pela captura e pelos testes para fixar um angulo.</summary>
        public void SetAngle(float degrees)
        {
            if (rect == null) rect = transform as RectTransform;
            paused = true;
            if (rect != null) rect.localRotation = Quaternion.Euler(0f, 0f, degrees);
        }

        public void Configure(float amplitude, float period)
        {
            amplitudeDegrees = amplitude;
            periodSeconds = period;
        }
    }
}
