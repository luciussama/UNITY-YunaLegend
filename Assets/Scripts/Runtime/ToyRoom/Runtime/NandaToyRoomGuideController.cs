using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    public enum NandaGuideTargetMode
    {
        FollowPlayer,
        BeaconOverChest,
        HoverOverSword
    }

    /// <summary>
    /// Fadinha guia Nanda (aparencia e luz azulada conforme diretriz do projeto) na Sala de Brinquedos.
    /// </summary>
    public sealed class NandaToyRoomGuideController : MonoBehaviour
    {
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Transform chestTarget;
        [SerializeField] private Transform swordTarget;
        [SerializeField] private Light bluePointLight;
        [SerializeField] private float smoothTime = 0.25f;

        private Vector3 velocity;
        private NandaGuideTargetMode mode = NandaGuideTargetMode.FollowPlayer;

        public NandaGuideTargetMode Mode => mode;

        public void Configure(Transform player, Transform chest, Transform sword, Light pointLight)
        {
            playerTarget = player;
            chestTarget = chest;
            swordTarget = sword;
            bluePointLight = pointLight;

            if (bluePointLight != null)
            {
                bluePointLight.color = new Color(0.36f, 0.88f, 1.0f);
            }
        }

        public void SetMode(NandaGuideTargetMode nextMode)
        {
            mode = nextMode;
        }

        private void LateUpdate()
        {
            Vector3 anchor = ResolveAnchorPosition();
            float hoverY = Mathf.Sin(Time.time * 3.2f) * 0.14f;
            Vector3 desired = anchor + new Vector3(0f, hoverY, 0f);

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        private Vector3 ResolveAnchorPosition()
        {
            switch (mode)
            {
                case NandaGuideTargetMode.BeaconOverChest:
                    return chestTarget != null
                        ? chestTarget.position + new Vector3(0f, 1.85f, 0f)
                        : transform.position;
                case NandaGuideTargetMode.HoverOverSword:
                    return swordTarget != null
                        ? swordTarget.position + new Vector3(0f, 1.45f, 0f)
                        : transform.position;
                default:
                    return playerTarget != null
                        ? playerTarget.position + new Vector3(-0.65f, 1.55f, 0.45f)
                        : transform.position;
            }
        }
    }
}
