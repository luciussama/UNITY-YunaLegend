using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Adaptador visual/fisico de um brinquedo interativo na Sala de Brinquedos.
    /// </summary>
    public sealed class ToyItemView : MonoBehaviour
    {
        [SerializeField] private string toyId;
        [SerializeField] private float weightMultiplier = 0.92f;
        [SerializeField] private Collider interactionCollider;

        private Transform carriedAnchor;
        private Vector3 baseLocalOffset;
        private float bouncePhase;

        public string ToyId => toyId;
        public float WeightMultiplier => weightMultiplier;
        public bool IsCarried { get; private set; }
        public bool IsStored { get; private set; }

        public void Configure(string id, float weight)
        {
            toyId = id;
            weightMultiplier = weight;
            if (interactionCollider == null)
            {
                interactionCollider = GetComponent<Collider>();
            }
        }

        public void AttachToCarrier(Transform anchor)
        {
            carriedAnchor = anchor;
            IsCarried = true;
            IsStored = false;
            bouncePhase = 0f;

            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            transform.SetParent(anchor, false);
            baseLocalOffset = Vector3.zero;
            transform.localPosition = baseLocalOffset;
            gameObject.SetActive(true);
        }

        public void StoreInChest()
        {
            carriedAnchor = null;
            IsCarried = false;
            IsStored = true;
            transform.SetParent(null, true);

            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            gameObject.SetActive(false);
        }

        public void RespawnAt(Vector3 worldPosition)
        {
            carriedAnchor = null;
            IsCarried = false;
            IsStored = false;
            transform.SetParent(null, true);
            transform.position = worldPosition;

            if (interactionCollider != null)
            {
                interactionCollider.enabled = true;
            }

            gameObject.SetActive(true);
        }

        public void UpdateCarryBounce(bool isMoving, float deltaTime)
        {
            if (!IsCarried || carriedAnchor == null)
            {
                return;
            }

            if (isMoving)
            {
                bouncePhase += deltaTime * 11f;
                float yOffset = Mathf.Abs(Mathf.Sin(bouncePhase)) * 0.06f;
                transform.localPosition = baseLocalOffset + new Vector3(0f, yOffset, 0f);
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, baseLocalOffset, deltaTime * 12f);
            }
        }
    }
}
