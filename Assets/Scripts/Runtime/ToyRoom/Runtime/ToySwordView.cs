using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Adaptador visual da Espada de Brinquedo revelada na cutscene de investigacao.
    /// </summary>
    public sealed class ToySwordView : MonoBehaviour
    {
        [SerializeField] private Light auraLight;
        [SerializeField] private float pickupRadius = 1.45f;

        private Vector3 basePosition;
        private bool isRevealed;
        private bool isCollected;

        public bool IsRevealed => isRevealed;
        public bool IsCollected => isCollected;

        public void Configure(Light swordAura)
        {
            auraLight = swordAura;
            basePosition = transform.position;
            Hide();
        }

        public void Hide()
        {
            isRevealed = false;
            isCollected = false;
            gameObject.SetActive(false);
        }

        public void Reveal()
        {
            isRevealed = true;
            isCollected = false;
            basePosition = transform.position;
            gameObject.SetActive(true);

            if (auraLight != null)
            {
                auraLight.intensity = 2.4f;
            }
        }

        public void Collect()
        {
            isCollected = true;
            gameObject.SetActive(false);
        }

        public bool IsWithinPickupRange(Vector3 worldPos)
        {
            if (!isRevealed || isCollected)
            {
                return false;
            }

            Vector3 delta = worldPos - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= pickupRadius * pickupRadius;
        }

        private void Update()
        {
            if (!isRevealed || isCollected)
            {
                return;
            }

            float bob = Mathf.Sin(Time.time * 2.5f) * 0.08f;
            transform.position = basePosition + new Vector3(0f, bob, 0f);
            transform.Rotate(Vector3.up, 38f * Time.deltaTime, Space.World);
        }
    }
}
