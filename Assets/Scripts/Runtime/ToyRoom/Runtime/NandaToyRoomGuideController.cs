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
        private Transform wingUpperL;
        private Transform wingUpperR;
        private Transform wingLowerL;
        private Transform wingLowerR;
        private bool wingsSearched;

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
            AnimateWingFlutter();
        }

        private void AnimateWingFlutter()
        {
            if (!wingsSearched)
            {
                wingsSearched = true;
                var body = transform.Find("Nanda_FairyBody");
                if (body != null)
                {
                    wingUpperL = body.Find("Wing_Upper_L");
                    wingUpperR = body.Find("Wing_Upper_R");
                    wingLowerL = body.Find("Wing_Lower_L");
                    wingLowerR = body.Find("Wing_Lower_R");
                }
            }

            float flap = Mathf.Sin(Time.time * 16.0f) * 24.0f;
            float subFlap = Mathf.Sin(Time.time * 16.0f - 0.6f) * 18.0f;

            if (wingUpperL != null)
            {
                wingUpperL.localRotation = Quaternion.Euler(0f, 22f + flap, 0f);
            }

            if (wingUpperR != null)
            {
                wingUpperR.localRotation = Quaternion.Euler(0f, -22f - flap, 0f);
            }

            if (wingLowerL != null)
            {
                wingLowerL.localRotation = Quaternion.Euler(0f, 28f + subFlap, 0f);
            }

            if (wingLowerR != null)
            {
                wingLowerR.localRotation = Quaternion.Euler(0f, -28f - subFlap, 0f);
            }
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
