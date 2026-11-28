using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{

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
        private Transform boneTorso;
        private Transform boneHead;
        private Transform boneWingL;
        private Transform boneWingR;
        private Transform boneArmL;
        private Transform boneArmR;
        private Transform boneLegL;
        private Transform boneLegR;
        private float wingPhase;
        private float limbPhase;
        private bool rigSearched;

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
            float speedNormalized = Mathf.Clamp01(velocity.magnitude / 2.2f);
            UpdateFacing();
            UpdateToyFocusLight();
            EnsureRigCached();
            AnimateWingFlutter(speedNormalized);
            AnimateHumanMotion(speedNormalized);
        }

        private void EnsureRigCached()
        {
            if (rigSearched)
            {
                return;
            }

            rigSearched = true;
            var body = transform.Find("Nanda_FairyBody");
            if (body == null)
            {
                return;
            }

            wingUpperL = body.Find("Wing_Upper_L");
            wingUpperR = body.Find("Wing_Upper_R");
            wingLowerL = body.Find("Wing_Lower_L");
            wingLowerR = body.Find("Wing_Lower_R");

            var rig = body.Find("Nanda_GlbMesh");
            if (rig == null)
            {
                return;
            }

            boneTorso = rig.Find("Bone_Torso");
            if (boneTorso != null)
            {
                boneHead = boneTorso.Find("Bone_Head");
                boneWingL = boneTorso.Find("Bone_Wing_L");
                boneWingR = boneTorso.Find("Bone_Wing_R");
                boneArmL = boneTorso.Find("Bone_Arm_L");
                boneArmR = boneTorso.Find("Bone_Arm_R");
                boneLegL = boneTorso.Find("Bone_Leg_L");
                boneLegR = boneTorso.Find("Bone_Leg_R");
            }
        }

        private void AnimateWingFlutter(float speedNormalized)
        {
            wingPhase += Time.deltaTime * Mathf.Lerp(18.0f, 26.0f, speedNormalized);
            float flap = Mathf.Sin(wingPhase) * Mathf.Lerp(26.0f, 34.0f, speedNormalized);
            float roll = Mathf.Cos(wingPhase) * 6.0f;

            if (boneWingL != null)
            {
                boneWingL.localRotation = Quaternion.Euler(0f, 10f + flap, roll);
            }

            if (boneWingR != null)
            {
                boneWingR.localRotation = Quaternion.Euler(0f, -10f - flap, -roll);
            }

            float subFlap = Mathf.Sin(wingPhase - 0.6f) * 18.0f;
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

        private void AnimateHumanMotion(float speedNormalized)
        {
            limbPhase += Time.deltaTime * Mathf.Lerp(2.8f, 8.5f, speedNormalized);

            if (boneTorso != null)
            {
                float leanForward = Mathf.Lerp(4f, 16f, speedNormalized);
                float swayRoll = Mathf.Sin(limbPhase * 0.5f) * 3.5f;
                boneTorso.localRotation = Quaternion.Slerp(
                    boneTorso.localRotation,
                    Quaternion.Euler(leanForward, 0f, swayRoll),
                    Time.deltaTime * 6f);
            }

            Transform lookTarget = ResolveLookTarget();
            if (boneHead != null)
            {
                Quaternion desiredHeadRot = Quaternion.Euler(0f, 0f, Mathf.Sin(limbPhase * 0.45f) * 4f);
                if (lookTarget != null && boneHead.parent != null)
                {
                    Vector3 worldDir = (lookTarget.position + Vector3.up * 0.35f) - boneHead.position;
                    if (worldDir.sqrMagnitude > 0.002f)
                    {
                        Vector3 localDir = boneHead.parent.InverseTransformDirection(worldDir.normalized);
                        float yaw = Mathf.Clamp(Mathf.Atan2(localDir.x, Mathf.Max(0.15f, localDir.z)) * Mathf.Rad2Deg, -42f, 42f);
                        float pitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(localDir.y, -0.7f, 0.7f)) * Mathf.Rad2Deg, -22f, 24f);
                        desiredHeadRot = Quaternion.Euler(pitch, yaw, Mathf.Sin(limbPhase * 0.45f) * 3.5f);
                    }
                }

                boneHead.localRotation = Quaternion.Slerp(boneHead.localRotation, desiredHeadRot, Time.deltaTime * 7f);
            }

            float armSwing = Mathf.Sin(limbPhase) * Mathf.Lerp(7f, 24f, speedNormalized);
            bool isPointing = lookTarget != null && lookTarget != playerTarget;

            if (boneArmL != null)
            {
                Quaternion targetArmL = Quaternion.Euler(-armSwing, 0f, -6f - speedNormalized * 8f);
                boneArmL.localRotation = Quaternion.Slerp(boneArmL.localRotation, targetArmL, Time.deltaTime * 7f);
            }

            if (boneArmR != null)
            {
                float pointPitch = isPointing ? -28f : armSwing;
                float pointYaw = isPointing ? -10f : 0f;
                Quaternion targetArmR = Quaternion.Euler(pointPitch, pointYaw, 6f + speedNormalized * 8f);
                boneArmR.localRotation = Quaternion.Slerp(boneArmR.localRotation, targetArmR, Time.deltaTime * 7f);
            }

            float legKick = Mathf.Sin(limbPhase + 0.4f) * Mathf.Lerp(5f, 20f, speedNormalized);
            float legTrail = Mathf.Lerp(6f, 18f, speedNormalized);

            if (boneLegL != null)
            {
                Quaternion targetLegL = Quaternion.Euler(legTrail + legKick, 0f, -2f);
                boneLegL.localRotation = Quaternion.Slerp(boneLegL.localRotation, targetLegL, Time.deltaTime * 7f);
            }

            if (boneLegR != null)
            {
                Quaternion targetLegR = Quaternion.Euler(legTrail - legKick, 0f, 2f);
                boneLegR.localRotation = Quaternion.Slerp(boneLegR.localRotation, targetLegR, Time.deltaTime * 7f);
            }
        }

        private Transform ResolveLookTarget()
        {
            return mode switch
            {
                NandaGuideTargetMode.BeaconOverChest => chestTarget,
                NandaGuideTargetMode.HoverOverSword => swordTarget,
                _ => nearestToyTarget != null ? nearestToyTarget : playerTarget
            };
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
