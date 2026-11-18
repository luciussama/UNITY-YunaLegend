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

        private Transform nearestToyTarget;
        private Light toyFocusLight;
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

        public void SetNearestToyTarget(Transform nearestToy)
        {
            nearestToyTarget = nearestToy;
        }

        private void LateUpdate()
        {
            Vector3 anchor = ResolveAnchorPosition();
            float hoverY = Mathf.Sin(Time.time * 3.2f) * 0.14f;
            Vector3 desired = anchor + new Vector3(0f, hoverY, 0f);

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            UpdateFacing();
            UpdateToyFocusLight();
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
                    if (playerTarget == null)
                    {
                        return transform.position;
                    }

                    if (nearestToyTarget != null)
                    {
                        Vector3 toToy = nearestToyTarget.position - playerTarget.position;
                        toToy.y = 0f;
                        float dist = toToy.magnitude;
                        if (dist > 0.01f)
                        {
                            Vector3 dir = toToy / dist;
                            Vector3 leadPos = playerTarget.position + dir * Mathf.Min(1.45f, dist * 0.55f) + new Vector3(0f, 1.48f, 0f);
                            Vector3 hoverToyPos = nearestToyTarget.position + new Vector3(0f, 1.05f, 0f);
                            float t = 1f - Mathf.InverseLerp(1.6f, 4.8f, dist);
                            return Vector3.Lerp(leadPos, hoverToyPos, t);
                        }
                    }

                    return playerTarget.position + new Vector3(-1.25f, 1.75f, 0.35f);
            }
        }

        private void UpdateFacing()
        {
            Transform lookTarget = mode switch
            {
                NandaGuideTargetMode.BeaconOverChest => chestTarget,
                NandaGuideTargetMode.HoverOverSword => swordTarget,
                _ => nearestToyTarget
            };

            if (lookTarget == null)
            {
                return;
            }

            Vector3 dir = lookTarget.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);
            }
        }

        private void UpdateToyFocusLight()
        {
            bool active = mode == NandaGuideTargetMode.FollowPlayer && nearestToyTarget != null && nearestToyTarget.gameObject.activeInHierarchy;
            if (!active)
            {
                if (toyFocusLight != null)
                {
                    toyFocusLight.enabled = false;
                }

                return;
            }

            if (toyFocusLight == null)
            {
                var lightGo = new GameObject("Nanda_ToyFocusLight");
                toyFocusLight = lightGo.AddComponent<Light>();
                toyFocusLight.type = LightType.Point;
                toyFocusLight.color = new Color(0.42f, 0.90f, 1.0f, 1f);
                toyFocusLight.range = 2.4f;
                toyFocusLight.shadows = LightShadows.None;
            }

            toyFocusLight.enabled = true;
            toyFocusLight.transform.position = nearestToyTarget.position + new Vector3(0f, 0.95f, 0f);
            toyFocusLight.intensity = 1.45f + Mathf.Sin(Time.time * 3.2f) * 0.25f;
        }
    }
}
