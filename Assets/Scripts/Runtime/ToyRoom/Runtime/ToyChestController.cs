using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Controla a tampa articulada e o brilho interno dourado do Bau de Brinquedos.
    /// </summary>
    public sealed class ToyChestController : MonoBehaviour
    {
        [SerializeField] private Transform lidPivot;
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private GameObject openVisual;
        [SerializeField] private Light interiorGlowLight;
        [SerializeField] private float deliveryRadius = 2.1f;
        [SerializeField] private float openLidAngle = -68f;
        [SerializeField] private float maxGlowIntensity = 2.8f;

        private bool playerCarryingNear;
        private float pulseTimer;

        public float DeliveryRadius => deliveryRadius;

        public void Configure(Transform lid, Light glowLight)
        {
            lidPivot = lid;
            interiorGlowLight = glowLight;
        }

        public void ConfigureStates(GameObject closedStateVisual, GameObject openStateVisual, Light glowLight)
        {
            closedVisual = closedStateVisual;
            openVisual = openStateVisual;
            interiorGlowLight = glowLight;
            ApplyStateVisuals(false);
        }

        public bool IsWithinDeliveryRange(Vector3 worldPos)
        {
            Vector3 delta = worldPos - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= deliveryRadius * deliveryRadius;
        }

        public void SetPlayerCarryingNear(bool isNear)
        {
            playerCarryingNear = isNear;
        }

        public void TriggerStoreFeedback()
        {
            pulseTimer = 0.45f;
        }

        private void Update()
        {
            if (pulseTimer > 0f)
            {
                pulseTimer = Mathf.Max(0f, pulseTimer - Time.deltaTime);
            }

            bool shouldOpen = playerCarryingNear || pulseTimer > 0f;
            float targetAngle = shouldOpen ? openLidAngle : 0f;
            float targetGlow = shouldOpen ? maxGlowIntensity : 0f;

            ApplyStateVisuals(shouldOpen);

            if (lidPivot != null)
            {
                Quaternion targetRot = Quaternion.Euler(targetAngle, 0f, 0f);
                lidPivot.localRotation = Quaternion.Slerp(lidPivot.localRotation, targetRot, Time.deltaTime * 10f);
            }

            if (interiorGlowLight != null)
            {
                interiorGlowLight.intensity = Mathf.Lerp(interiorGlowLight.intensity, targetGlow, Time.deltaTime * 10f);
            }
        }

        private void ApplyStateVisuals(bool isOpen)
        {
            if (closedVisual != null && closedVisual.activeSelf == isOpen)
            {
                closedVisual.SetActive(!isOpen);
            }

            if (openVisual != null && openVisual.activeSelf != isOpen)
            {
                openVisual.SetActive(isOpen);
            }
        }
    }
}
