using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Animador procedural leve para o modelo 3D da Yuna (passos, balanco do rabo de cavalo,
    /// respiracao em idle e bracos erguidos ao carregar um brinquedo).
    /// </summary>
    public sealed class YunaToyRoomAnimator : MonoBehaviour
    {
        [SerializeField] private ToyRoomPlayerController player;
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform legLeftPivot;
        [SerializeField] private Transform legRightPivot;
        [SerializeField] private Transform armLeftPivot;
        [SerializeField] private Transform armRightPivot;
        [SerializeField] private Transform ponytailPivot;

        private Vector3 lastPosition;
        private float walkCycle;

        public void Configure(
            ToyRoomPlayerController owner,
            Transform body,
            Transform legL,
            Transform legR,
            Transform armL,
            Transform armR,
            Transform ponytail)
        {
            player = owner;
            bodyRoot = body;
            legLeftPivot = legL;
            legRightPivot = legR;
            armLeftPivot = armL;
            armRightPivot = armR;
            ponytailPivot = ponytail;
        }

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            lastPosition = transform.position;

            float speed = delta.magnitude / dt;
            bool isMoving = speed > 0.25f;
            bool isCarrying = player != null && player.CarriedToy != null;

            if (isMoving)
            {
                walkCycle += dt * Mathf.Clamp(speed * 2.6f, 6f, 14f);
            }
            else
            {
                walkCycle = Mathf.Lerp(walkCycle, Mathf.Round(walkCycle / Mathf.PI) * Mathf.PI, dt * 8f);
            }

            float swing = isMoving ? Mathf.Sin(walkCycle) * 32f : 0f;
            float bob = isMoving ? Mathf.Abs(Mathf.Sin(walkCycle)) * 0.045f : Mathf.Sin(Time.time * 2.2f) * 0.012f;

            if (bodyRoot != null)
            {
                bodyRoot.localPosition = new Vector3(0f, bob, 0f);
            }

            if (legLeftPivot != null)
            {
                legLeftPivot.localRotation = Quaternion.Slerp(legLeftPivot.localRotation, Quaternion.Euler(swing, 0f, 0f), dt * 14f);
            }

            if (legRightPivot != null)
            {
                legRightPivot.localRotation = Quaternion.Slerp(legRightPivot.localRotation, Quaternion.Euler(-swing, 0f, 0f), dt * 14f);
            }

            if (armLeftPivot != null)
            {
                Quaternion targetArmL = isCarrying ? Quaternion.Euler(-68f, 0f, 14f) : Quaternion.Euler(-swing * 0.7f, 0f, 12f);
                armLeftPivot.localRotation = Quaternion.Slerp(armLeftPivot.localRotation, targetArmL, dt * 14f);
            }

            if (armRightPivot != null)
            {
                Quaternion targetArmR = isCarrying ? Quaternion.Euler(-68f, 0f, -14f) : Quaternion.Euler(swing * 0.7f, 0f, -12f);
                armRightPivot.localRotation = Quaternion.Slerp(armRightPivot.localRotation, targetArmR, dt * 14f);
            }

            if (ponytailPivot != null)
            {
                float tailPitch = isMoving ? 28f + Mathf.Sin(walkCycle * 2f) * 10f : 22f + Mathf.Sin(Time.time * 2.2f) * 2.5f;
                ponytailPivot.localRotation = Quaternion.Slerp(ponytailPivot.localRotation, Quaternion.Euler(tailPitch, 0f, 0f), dt * 10f);
            }
        }
    }
}
