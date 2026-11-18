using System;
using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Animador procedural para o esqueleto da Yuna (subida/descida na passada,
    /// balanco natural do quadril, contra-rotacao da coluna, joelhos/pes articulados,
    /// bracos relaxados ao lado do corpo e postura de carregar brinquedo).
    /// </summary>
    public sealed class YunaToyRoomAnimator : MonoBehaviour
    {
        [Serializable]
        public struct RigBones
        {
            public Transform Hips;
            public Transform Spine;
            public Transform Chest;
            public Transform Neck;
            public Transform Head;
            public Transform UpperArmL;
            public Transform ForearmL;
            public Transform HandL;
            public Transform UpperArmR;
            public Transform ForearmR;
            public Transform HandR;
            public Transform ThighL;
            public Transform ShinL;
            public Transform FootL;
            public Transform ThighR;
            public Transform ShinR;
            public Transform FootR;

            public bool IsValid => Hips != null && ThighL != null && ThighR != null && UpperArmL != null && UpperArmR != null;
        }

        [SerializeField] private ToyRoomPlayerController player;
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform legLeftPivot;
        [SerializeField] private Transform legRightPivot;
        [SerializeField] private Transform armLeftPivot;
        [SerializeField] private Transform armRightPivot;
        [SerializeField] private Transform ponytailPivot;
        [SerializeField] private RigBones rig;

        private Vector3 lastPosition;
        private Vector3 baseHipsLocalPos;
        private bool hasBaseHipsPos;
        private float walkCycle;
        private ToyChestController cachedChest;
        private ToyItemView[] cachedToys;
        private float nextToyScanTime;

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

        public void ConfigureRig(RigBones bones)
        {
            rig = bones;
            if (rig.Hips != null)
            {
                baseHipsLocalPos = rig.Hips.localPosition;
                hasBaseHipsPos = true;
            }
        }

        private void Start()
        {
            lastPosition = transform.position;
            if (!rig.IsValid && bodyRoot != null)
            {
                Transform rigRoot = bodyRoot.Find("Yuna_RigRoot");
                if (rigRoot != null)
                {
                    Transform hips = rigRoot.Find("Hips");
                    Transform spine = hips != null ? hips.Find("Spine") : null;
                    Transform chest = spine != null ? spine.Find("Chest") : null;
                    Transform neck = chest != null ? chest.Find("Neck") : null;
                    Transform upperArmL = chest != null ? chest.Find("UpperArm_L") : null;
                    Transform forearmL = upperArmL != null ? upperArmL.Find("Forearm_L") : null;
                    Transform upperArmR = chest != null ? chest.Find("UpperArm_R") : null;
                    Transform forearmR = upperArmR != null ? upperArmR.Find("Forearm_R") : null;
                    Transform thighL = hips != null ? hips.Find("Thigh_L") : null;
                    Transform shinL = thighL != null ? thighL.Find("Shin_L") : null;
                    Transform thighR = hips != null ? hips.Find("Thigh_R") : null;
                    Transform shinR = thighR != null ? thighR.Find("Shin_R") : null;

                    rig = new RigBones
                    {
                        Hips = hips,
                        Spine = spine,
                        Chest = chest,
                        Neck = neck,
                        Head = neck != null ? neck.Find("Head") : null,
                        UpperArmL = upperArmL,
                        ForearmL = forearmL,
                        HandL = forearmL != null ? forearmL.Find("Hand_L") : null,
                        UpperArmR = upperArmR,
                        ForearmR = forearmR,
                        HandR = forearmR != null ? forearmR.Find("Hand_R") : null,
                        ThighL = thighL,
                        ShinL = shinL,
                        FootL = shinL != null ? shinL.Find("Foot_L") : null,
                        ThighR = thighR,
                        ShinR = shinR,
                        FootR = shinR != null ? shinR.Find("Foot_R") : null
                    };
                }
            }

            if (rig.Hips != null && !hasBaseHipsPos)
            {
                baseHipsLocalPos = rig.Hips.localPosition;
                hasBaseHipsPos = true;
            }
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

            SimulateWalkStep(dt, isMoving, isCarrying);
        }

        public void SimulateWalkStep(float dt, bool isMoving, bool isCarrying = false)
        {
            if (!hasBaseHipsPos && rig.Hips != null)
            {
                baseHipsLocalPos = rig.Hips.localPosition;
                hasBaseHipsPos = true;
            }

            if (isMoving)
            {
                walkCycle += dt * 10.5f;
            }
            else
            {
                walkCycle = Mathf.Lerp(walkCycle, Mathf.Round(walkCycle / Mathf.PI) * Mathf.PI, dt * 8f);
            }

            if (rig.IsValid)
            {
                AnimateSkinnedRig(dt, isMoving, isCarrying);
                return;
            }

            AnimateLegacyPivots(dt, isMoving, isCarrying);
        }

        private void AnimateSkinnedRig(float dt, bool isMoving, bool isCarrying)
        {
            float sin = Mathf.Sin(walkCycle);
            float cos = Mathf.Cos(walkCycle);

            // Variacao de passada: 2 passos normais + 3o passo com pulinho alegre
            int stepIndex = Mathf.FloorToInt(walkCycle / Mathf.PI);
            bool isHopStep = isMoving && (Mathf.Abs(stepIndex) % 3 == 2);
            float hopBoost = isHopStep ? 1.55f : 1.0f;

            // Inclinacao lateral e abertura de passo ao mudar de direcao
            float turn = Mathf.Clamp(player != null ? player.TurnDeltaDeg : 0f, -55f, 55f);
            float turnNorm = turn / 55f;

            // 1. Quadril sobe e desce na passada + balanco natural da pelve (tronco ereto e equilibrado)
            float bobY = isMoving
                ? Mathf.Abs(sin) * 0.055f * hopBoost - 0.015f
                : Mathf.Sin(Time.time * 2.2f) * 0.008f;
            rig.Hips.localPosition = baseHipsLocalPos + new Vector3(0f, bobY, 0f);

            Quaternion hipsRot = isMoving
                ? Quaternion.Euler(0f, sin * 10.0f + turn * 0.25f, cos * 4.0f - turn * 0.18f)
                : Quaternion.identity;
            rig.Hips.localRotation = Quaternion.Slerp(rig.Hips.localRotation, hipsRot, dt * 16f);

            // 2. Coluna e peito permanecem eretos (zero inclinacao desequilibrada) com contra-rotacao suave
            if (rig.Spine != null)
            {
                Quaternion spineRot = isMoving
                    ? Quaternion.Euler(1f, -sin * 6.0f + turn * 0.15f, -cos * 2.0f - turn * 0.14f)
                    : Quaternion.Euler(Mathf.Sin(Time.time * 2.2f) * 1.2f, 0f, 0f);
                rig.Spine.localRotation = Quaternion.Slerp(rig.Spine.localRotation, spineRot, dt * 16f);
            }

            if (rig.Chest != null)
            {
                Quaternion chestRot = isMoving
                    ? Quaternion.Euler(1f, -sin * 5.0f + turn * 0.18f, -turn * 0.12f)
                    : Quaternion.Euler(Mathf.Sin(Time.time * 2.2f) * 1.5f, 0f, 0f);
                rig.Chest.localRotation = Quaternion.Slerp(rig.Chest.localRotation, chestRot, dt * 16f);
            }

            // 2b. Olhar curioso (Neck 35% + Head 65%) para o brinquedo mais proximo ou para o bau
            ComputeLookTargetAngles(isCarrying, out float lookYaw, out float lookPitch);
            float baseHeadPitch = isMoving ? -2f : Mathf.Sin(Time.time * 1.4f) * 1.5f;
            float baseHeadYaw = isMoving ? sin * 3.0f : 0f;

            if (rig.Neck != null)
            {
                Quaternion neckRot = Quaternion.Euler(
                    (baseHeadPitch + lookPitch) * 0.35f,
                    (baseHeadYaw + lookYaw) * 0.35f,
                    0f);
                rig.Neck.localRotation = Quaternion.Slerp(rig.Neck.localRotation, neckRot, dt * 10f);
            }

            if (rig.Head != null)
            {
                Quaternion headRot = Quaternion.Euler(
                    (baseHeadPitch + lookPitch) * 0.65f,
                    (baseHeadYaw + lookYaw) * 0.65f,
                    0f);
                rig.Head.localRotation = Quaternion.Slerp(rig.Head.localRotation, headRot, dt * 10f);
            }

            // 3. Pernas articuladas sob o vestido: coxa balanca, joelho dobra e passo abre na curva
            float swing = isMoving ? sin * 46f : 0f;
            float liftPhaseL = isMoving ? Mathf.Clamp01(-cos * 0.85f + 0.25f) : 0f;
            float liftPhaseR = isMoving ? Mathf.Clamp01(cos * 0.85f + 0.25f) : 0f;
            float kneeL = isMoving ? liftPhaseL * (isHopStep ? 72f : 64f) : 0f;
            float kneeR = isMoving ? liftPhaseR * (isHopStep ? 72f : 64f) : 0f;

            float stepOpenL = isMoving ? Mathf.Max(0f, -turnNorm) * 14f + Mathf.Abs(turnNorm) * 4f : 0f;
            float stepOpenR = isMoving ? -(Mathf.Max(0f, turnNorm) * 14f + Mathf.Abs(turnNorm) * 4f) : 0f;
            float stepCrossYaw = isMoving ? turn * 0.22f : 0f;

            rig.ThighL.localRotation = Quaternion.Slerp(rig.ThighL.localRotation, Quaternion.Euler(swing, stepCrossYaw, stepOpenL), dt * 18f);
            rig.ThighR.localRotation = Quaternion.Slerp(rig.ThighR.localRotation, Quaternion.Euler(-swing, stepCrossYaw, stepOpenR), dt * 18f);

            if (rig.ShinL != null)
            {
                rig.ShinL.localRotation = Quaternion.Slerp(rig.ShinL.localRotation, Quaternion.Euler(kneeL, 0f, 0f), dt * 18f);
            }

            if (rig.ShinR != null)
            {
                rig.ShinR.localRotation = Quaternion.Slerp(rig.ShinR.localRotation, Quaternion.Euler(kneeR, 0f, 0f), dt * 18f);
            }

            if (rig.FootL != null)
            {
                rig.FootL.localRotation = Quaternion.Slerp(rig.FootL.localRotation, Quaternion.Euler(-swing * 0.40f - kneeL * 0.30f, 0f, 0f), dt * 18f);
            }

            if (rig.FootR != null)
            {
                rig.FootR.localRotation = Quaternion.Slerp(rig.FootR.localRotation, Quaternion.Euler(swing * 0.40f - kneeR * 0.30f, 0f, 0f), dt * 18f);
            }

            // 4. Bracos ao lado do corpo (sem encostar na saia) balancando em oposicao as pernas + hand-flapping alegre
            bool flapActive = !isCarrying && (!isMoving || isHopStep);
            float flapWave = Mathf.Sin(Time.time * (isMoving ? 15f : 11.5f));
            float forearmFlapLift = flapActive ? (isMoving ? -14f : -10f) : 0f;

            Quaternion armTargetL = isCarrying
                ? Quaternion.Euler(-58f, 12f, 16f)
                : Quaternion.Euler(-swing * 0.85f, 0f, 22f + (flapActive ? 4f : 0f));
            Quaternion armTargetR = isCarrying
                ? Quaternion.Euler(-58f, -12f, -16f)
                : Quaternion.Euler(swing * 0.85f, 0f, -22f - (flapActive ? 4f : 0f));

            rig.UpperArmL.localRotation = Quaternion.Slerp(rig.UpperArmL.localRotation, armTargetL, dt * 18f);
            rig.UpperArmR.localRotation = Quaternion.Slerp(rig.UpperArmR.localRotation, armTargetR, dt * 18f);

            if (rig.ForearmL != null)
            {
                float bendL = isCarrying ? -32f : (isMoving ? -22f - Mathf.Max(0f, -sin) * 24f : -14f) + forearmFlapLift;
                rig.ForearmL.localRotation = Quaternion.Slerp(rig.ForearmL.localRotation, Quaternion.Euler(bendL, 0f, 6f), dt * 18f);
            }

            if (rig.ForearmR != null)
            {
                float bendR = isCarrying ? -32f : (isMoving ? -22f - Mathf.Max(0f, sin) * 24f : -14f) + forearmFlapLift;
                rig.ForearmR.localRotation = Quaternion.Slerp(rig.ForearmR.localRotation, Quaternion.Euler(bendR, 0f, -6f), dt * 18f);
            }

            if (rig.HandL != null)
            {
                float handFlapL = flapActive ? flapWave * (isMoving ? 24f : 18f) : 0f;
                rig.HandL.localRotation = Quaternion.Slerp(rig.HandL.localRotation, Quaternion.Euler(handFlapL, 0f, flapActive ? 10f : 0f), dt * 22f);
            }

            if (rig.HandR != null)
            {
                float handFlapR = flapActive ? -flapWave * (isMoving ? 24f : 18f) : 0f;
                rig.HandR.localRotation = Quaternion.Slerp(rig.HandR.localRotation, Quaternion.Euler(handFlapR, 0f, flapActive ? -10f : 0f), dt * 22f);
            }
        }

        private void ComputeLookTargetAngles(bool isCarrying, out float lookYaw, out float lookPitch)
        {
            lookYaw = 0f;
            lookPitch = 0f;

            Vector3? targetPos = null;

            if (isCarrying)
            {
                if (cachedChest == null)
                {
                    cachedChest = FindFirstObjectByType<ToyChestController>();
                }

                if (cachedChest != null)
                {
                    targetPos = cachedChest.transform.position + Vector3.up * 0.45f;
                }
            }
            else
            {
                if (cachedToys == null || Time.time >= nextToyScanTime)
                {
                    cachedToys = FindObjectsByType<ToyItemView>(FindObjectsSortMode.None);
                    nextToyScanTime = Time.time + 1.0f;
                }

                float bestDistSq = 25f;
                Vector3 origin = transform.position;
                for (int i = 0; i < cachedToys.Length; i++)
                {
                    ToyItemView toy = cachedToys[i];
                    if (toy == null || !toy.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    Vector3 delta = toy.transform.position - origin;
                    delta.y = 0f;
                    float distSq = delta.sqrMagnitude;
                    if (distSq < bestDistSq && distSq > 0.04f)
                    {
                        bestDistSq = distSq;
                        targetPos = toy.transform.position + Vector3.up * 0.25f;
                    }
                }
            }

            if (!targetPos.HasValue)
            {
                return;
            }

            Vector3 headOrigin = rig.Head != null ? rig.Head.position : transform.position + Vector3.up * 0.9f;
            Vector3 toTarget = targetPos.Value - headOrigin;
            if (toTarget.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 localDir = transform.InverseTransformDirection(toTarget.normalized);
            lookYaw = Mathf.Clamp(Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg, -42f, 42f);
            lookPitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(localDir.y, -0.9f, 0.9f)) * Mathf.Rad2Deg, -18f, 22f);
        }

        private void AnimateLegacyPivots(float dt, bool isMoving, bool isCarrying)
        {
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
