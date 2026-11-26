using UnityEngine;
using UnityEngine.UI;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// 16-joint articulated 2D skeletal puppet view rendered natively inside uGUI (MaskableGraphic LBS)
    /// for the BearChase entrance cinematic. Keeps exact Victorian Yuna proportions, waist-level hip
    /// pivots (legs originating inside the skirt), waist Bone_Chest articulation, S-wave ponytail,
    /// and 3 expressive faces (Calm, Focused, Shocked).
    /// </summary>
    public sealed class BearChaseYunaRigUiView : MonoBehaviour
    {
        private const string ResourceRoot = "BearChaseEntrance/YunaRig2D/";

        // Normalized anatomical ratios (total height = 1.00, soles resting cleanly at Y = 0.00):
        // Waist/Hip axis sits at 0.45 inside the Victorian skirt so legs originate from the true hip.
        private const float BaseHipsNormY = 0.45f;

        private RectTransform rigRoot;
        private RectTransform hipsBone;
        private RectTransform torsoBone;
        private RectTransform chestBone;
        private RectTransform headBone;
        private RectTransform ponytailBone;
        private RectTransform armFrontBone;
        private RectTransform armBackBone;
        private RectTransform legFrontBone;
        private RectTransform legBackBone;

        private Image headImage;
        private Sprite headCalmSprite;
        private Sprite headFocusedSprite;
        private Sprite headShockedSprite;

        private DeformableUiPart legFrontPart;
        private DeformableUiPart legBackPart;
        private DeformableUiPart armFrontPart;
        private DeformableUiPart armBackPart;
        private DeformableUiPart torsoPart;
        private DeformableUiPart ponytailPart;

        private float referenceHeightPx = 395f;
        private bool isBuilt;

        public bool IsBuilt => isBuilt;

        public bool TryBuild(RectTransform parent, float targetHeightPx = 395f)
        {
            if (isBuilt)
            {
                return true;
            }

            referenceHeightPx = Mathf.Max(100f, targetHeightPx);

            headCalmSprite = LoadSprite("yuna_bc_head_calm", new Vector2(0.50f, 0.04f));
            headFocusedSprite = LoadSprite("yuna_bc_head_focused", new Vector2(0.50f, 0.04f));
            headShockedSprite = LoadSprite("yuna_bc_head_shocked", new Vector2(0.50f, 0.04f));

            Texture2D ponytailTex = LoadTex("yuna_bc_ponytail");
            Texture2D torsoTex = LoadTex("yuna_bc_torso_dress");
            Texture2D armFrontTex = LoadTex("yuna_bc_arm_front_sword");
            Texture2D armBackTex = LoadTex("yuna_bc_arm_back_reach");
            Texture2D legFrontTex = LoadTex("yuna_bc_leg_front");
            Texture2D legBackTex = LoadTex("yuna_bc_leg_back");

            if (headCalmSprite == null || torsoTex == null || legFrontTex == null || armFrontTex == null)
            {
                return false;
            }

            if (headFocusedSprite == null) headFocusedSprite = headCalmSprite;
            if (headShockedSprite == null) headShockedSprite = headCalmSprite;

            float H = referenceHeightPx;

            rigRoot = CreateRect("YunaBearChaseRig2D", parent, Vector2.zero);
            rigRoot.sizeDelta = new Vector2(H * 0.72f, H);
            rigRoot.pivot = new Vector2(0.5f, 0f);

            // True hip/waist axis inside the Victorian skirt (Y = 0.45 * H)
            hipsBone = CreateRect("Bone_Hips", rigRoot, new Vector2(0f, BaseHipsNormY * H));

            // Back/far arm behind torso (extends forward to reach the bear)
            // Mounted under chestBone below so it inherits waist bend!
            legBackBone = CreateRect("Bone_LegBack", hipsBone, new Vector2(0.022f * H, 0f));
            legBackPart = CreateDeformableUiPart("Part_LegBack", legBackBone, legBackTex ?? legFrontTex,
                new Vector2(0.45f, 0.92f), 0.49f * H, 4, 16);
            legBackPart.color = new Color(0.88f, 0.82f, 0.80f, 1f);

            legFrontBone = CreateRect("Bone_LegFront", hipsBone, new Vector2(-0.022f * H, 0f));
            legFrontPart = CreateDeformableUiPart("Part_LegFront", legFrontBone, legFrontTex,
                new Vector2(0.45f, 0.92f), 0.49f * H, 4, 16);

            // Torso & Victorian skirt (hem sits around Y = 0.22 * H covering upper thighs to knees)
            torsoBone = CreateRect("Bone_Torso", hipsBone, new Vector2(0f, -0.18f * H));

            // Waist-level upper torso pivot (Bone_Chest at dress waistband Y = 0.165 * H inside Bone_Torso)
            chestBone = CreateRect("Bone_Chest", torsoBone, new Vector2(0f, 0.165f * H));

            // Back arm (sibling order BEFORE torsoPart so it renders cleanly behind the dress!)
            var backArmHolder = CreateRect("Bone_ArmBackHolder", torsoBone, new Vector2(0f, 0.165f * H));
            armBackBone = CreateRect("Bone_ArmBack", backArmHolder, new Vector2(0.105f * H, 0.115f * H));
            armBackPart = CreateDeformableUiPart("Part_ArmBack", armBackBone, armBackTex ?? armFrontTex,
                new Vector2(0.50f, 0.85f), 0.30f * H, 4, 14);
            armBackPart.color = new Color(0.90f, 0.84f, 0.82f, 1f);

            // Dress renders in front of legs & back arm
            torsoPart = CreateDeformableUiPart("Part_TorsoDress", torsoBone, torsoTex,
                new Vector2(0.50f, 0.14f), 0.36f * H, 8, 12);

            // Ensure chestBone (carrying ponytail, head, front sword arm) renders IN FRONT of torsoPart
            chestBone.SetAsLastSibling();

            // Neck seated cleanly on the pink ruffled collar (0.165 + 0.148 = 0.313 * H above torsoBone)
            headBone = CreateRect("Bone_Head", chestBone, new Vector2(0.012f * H, 0.148f * H));

            // Single high curly ponytail behind the back-top of the head
            ponytailBone = CreateRect("Bone_Ponytail", headBone, new Vector2(-0.115f * H, 0.265f * H));
            ponytailPart = CreateDeformableUiPart("Part_Ponytail", ponytailBone, ponytailTex,
                new Vector2(0.80f, 0.82f), 0.33f * H, 4, 12);

            // Head (swaps cleanly between Calm, Focused, and Shocked)
            var headGo = new GameObject("Part_Head", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            headGo.transform.SetParent(headBone, false);
            headImage = headGo.GetComponent<Image>();
            headImage.sprite = headCalmSprite;
            headImage.preserveAspect = true;
            headImage.raycastTarget = false;
            var hRt = headGo.GetComponent<RectTransform>();
            float headH = 0.37f * H;
            float headW = headCalmSprite.rect.height > 0f
                ? headH * (headCalmSprite.rect.width / headCalmSprite.rect.height)
                : headH;
            hRt.sizeDelta = new Vector2(headW, headH);
            hRt.pivot = new Vector2(0.50f, 0.04f);
            hRt.anchoredPosition = Vector2.zero;

            // Front arm holding the wooden toy sword (pivotX=0.24 aligns shoulder top since sword extends right)
            armFrontBone = CreateRect("Bone_ArmFront", chestBone, new Vector2(-0.088f * H, 0.118f * H));
            armFrontPart = CreateDeformableUiPart("Part_ArmFrontSword", armFrontBone, armFrontTex,
                new Vector2(0.24f, 0.87f), 0.36f * H, 4, 14);

            isBuilt = true;
            return true;
        }

        public void ApplyPose(
            BearChaseYunaRigPose pose,
            Vector2 normalizedAnchor,
            float extraScaleX = 1f,
            float extraScaleY = 1f,
            float extraTiltDeg = 0f)
        {
            if (!isBuilt || rigRoot == null)
            {
                return;
            }

            float H = referenceHeightPx;
            rigRoot.anchorMin = normalizedAnchor;
            rigRoot.anchorMax = normalizedAnchor;
            rigRoot.anchoredPosition = Vector2.zero;
            rigRoot.localScale = new Vector3(pose.ScaleX * extraScaleX, pose.ScaleY * extraScaleY, 1f);
            rigRoot.localRotation = Quaternion.Euler(0f, 0f, extraTiltDeg);

            hipsBone.anchoredPosition = new Vector2(0f, (BaseHipsNormY + pose.HipsBobY) * H);
            torsoBone.localRotation = Quaternion.Euler(0f, 0f, pose.TorsoLeanDeg);

            float chestRot = pose.ChestBendDeg * 0.75f;
            chestBone.localRotation = Quaternion.Euler(0f, 0f, chestRot);
            if (armBackBone != null && armBackBone.parent != null)
            {
                armBackBone.parent.localRotation = Quaternion.Euler(0f, 0f, chestRot);
            }

            headBone.localRotation = Quaternion.Euler(0f, 0f, pose.HeadTiltDeg);
            ponytailBone.localRotation = Quaternion.Euler(0f, 0f, pose.PonytailRootDeg);
            legFrontBone.localRotation = Quaternion.Euler(0f, 0f, pose.FrontLegDeg);
            legBackBone.localRotation = Quaternion.Euler(0f, 0f, pose.BackLegDeg);
            armFrontBone.localRotation = Quaternion.Euler(0f, 0f, pose.FrontArmDeg);
            armBackBone.localRotation = Quaternion.Euler(0f, 0f, pose.BackArmDeg);

            // Real 2D LBS mesh deformation for knees, ankles, elbows, waist/skirt, and S-wave ponytail
            legFrontPart.DeformThreeBoneChain(kneeV: 0.46f, kneeDeg: pose.FrontKneeDeg, ankleV: 0.21f, ankleDeg: pose.FrontAnkleDeg);
            legBackPart.DeformThreeBoneChain(kneeV: 0.46f, kneeDeg: pose.BackKneeDeg, ankleV: 0.21f, ankleDeg: pose.BackAnkleDeg);
            armFrontPart.DeformTwoBoneChain(jointV: 0.56f, bendDeg: pose.FrontElbowDeg);
            armBackPart.DeformTwoBoneChain(jointV: 0.48f, bendDeg: pose.BackElbowDeg);
            torsoPart.DeformTorsoAndSkirt(pose.ChestBendDeg, pose.SkirtSwayDeg, pose.SkirtFlare);
            ponytailPart.DeformTwoBoneChain(jointV: 0.46f, bendDeg: pose.PonytailTipDeg - pose.PonytailRootDeg);

            if (headImage != null)
            {
                headImage.sprite = pose.Expression == BearChaseYunaExpression.Shocked
                    ? headShockedSprite
                    : (pose.Expression == BearChaseYunaExpression.Focused ? headFocusedSprite : headCalmSprite);
            }
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = anchoredPos;
            return rt;
        }

        private static DeformableUiPart CreateDeformableUiPart(
            string name, RectTransform parent, Texture2D tex, Vector2 pivot, float targetHeightPx, int cols, int rows)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(DeformableUiPart));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            var part = go.GetComponent<DeformableUiPart>();
            part.raycastTarget = false;
            part.Configure(tex, pivot, targetHeightPx, cols, rows);
            return part;
        }

        private static Texture2D LoadTex(string name)
        {
            var tex = Resources.Load<Texture2D>(ResourceRoot + name);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
            }
            return tex;
        }

        private static Sprite LoadSprite(string name, Vector2 pivot)
        {
            var tex = LoadTex(name);
            return tex == null ? null : Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, 100f);
        }

        /// <summary>
        /// Native uGUI MaskableGraphic rendering a subdivided 2D LBS deformable mesh directly inside Canvas.
        /// </summary>
        private sealed class DeformableUiPart : MaskableGraphic
        {
            private Texture2D partTexture;
            private Vector2[] basePos;
            private Vector2[] workPos;
            private Vector2[] uvs;
            private float[] vertexV;
            private int cols;
            private int rows;
            private float widthPx;
            private float heightPx;
            private Vector2 partPivot;

            public override Texture mainTexture => partTexture != null ? partTexture : s_WhiteTexture;

            public void Configure(Texture2D tex, Vector2 pivot, float targetHeightPx, int gridCols, int gridRows)
            {
                partTexture = tex;
                partPivot = pivot;
                cols = gridCols;
                rows = gridRows;
                heightPx = targetHeightPx;
                widthPx = tex != null && tex.height > 0 ? targetHeightPx * (tex.width / (float)tex.height) : targetHeightPx;

                int count = (cols + 1) * (rows + 1);
                basePos = new Vector2[count];
                workPos = new Vector2[count];
                uvs = new Vector2[count];
                vertexV = new float[count];

                int idx = 0;
                for (int y = 0; y <= rows; y++)
                {
                    float v = y / (float)rows;
                    float py = (v - partPivot.y) * heightPx;
                    for (int x = 0; x <= cols; x++)
                    {
                        float u = x / (float)cols;
                        float px = (u - partPivot.x) * widthPx;
                        basePos[idx] = new Vector2(px, py);
                        workPos[idx] = basePos[idx];
                        uvs[idx] = new Vector2(u, v);
                        vertexV[idx] = v;
                        idx++;
                    }
                }
                SetVerticesDirty();
                SetMaterialDirty();
            }

            public void DeformTwoBoneChain(float jointV, float bendDeg)
            {
                if (basePos == null) return;
                float jointY = (jointV - partPivot.y) * heightPx;
                float rad = bendDeg * Mathf.Deg2Rad;
                for (int i = 0; i < basePos.Length; i++)
                {
                    Vector2 p = basePos[i];
                    float w = SmoothStep(jointV + 0.11f, jointV - 0.11f, vertexV[i]);
                    if (w > 0.0001f) p = Rotate2D(p, 0f, jointY, rad * w);
                    workPos[i] = p;
                }
                SetVerticesDirty();
            }

            public void DeformThreeBoneChain(float kneeV, float kneeDeg, float ankleV, float ankleDeg)
            {
                if (basePos == null) return;
                float kneeY = (kneeV - partPivot.y) * heightPx;
                float ankleY = (ankleV - partPivot.y) * heightPx;
                float kneeRad = kneeDeg * Mathf.Deg2Rad;
                float ankleRad = ankleDeg * Mathf.Deg2Rad;
                Vector2 ankleRot = Rotate2D(new Vector2(0f, ankleY), 0f, kneeY, kneeRad);

                for (int i = 0; i < basePos.Length; i++)
                {
                    Vector2 p = basePos[i];
                    float wKnee = SmoothStep(kneeV + 0.10f, kneeV - 0.10f, vertexV[i]);
                    if (wKnee > 0.0001f) p = Rotate2D(p, 0f, kneeY, kneeRad * wKnee);
                    float wAnkle = SmoothStep(ankleV + 0.06f, ankleV - 0.06f, vertexV[i]);
                    if (wAnkle > 0.0001f) p = Rotate2D(p, ankleRot.x, ankleRot.y, ankleRad * wAnkle);
                    workPos[i] = p;
                }
                SetVerticesDirty();
            }

            public void DeformTorsoAndSkirt(float chestBendDeg, float skirtSwayDeg, float skirtFlare)
            {
                if (basePos == null) return;
                const float waistV = 0.56f;
                float waistY = (waistV - partPivot.y) * heightPx;
                float chestRad = chestBendDeg * Mathf.Deg2Rad;
                float swayRad = skirtSwayDeg * Mathf.Deg2Rad;

                for (int i = 0; i < basePos.Length; i++)
                {
                    Vector2 p = basePos[i];
                    float v = vertexV[i];
                    float wChest = SmoothStep(waistV - 0.06f, waistV + 0.18f, v);
                    if (wChest > 0.0001f) p = Rotate2D(p, 0f, waistY, chestRad * wChest);
                    float wSkirt = SmoothStep(waistV + 0.02f, 0.04f, v);
                    if (wSkirt > 0.0001f)
                    {
                        p.x *= 1f + skirtFlare * wSkirt;
                        p = Rotate2D(p, 0f, waistY, swayRad * wSkirt);
                    }
                    workPos[i] = p;
                }
                SetVerticesDirty();
            }

            protected override void OnPopulateMesh(VertexHelper vh)
            {
                vh.Clear();
                if (workPos == null || workPos.Length == 0) return;

                Color32 c = color;
                for (int i = 0; i < workPos.Length; i++)
                {
                    vh.AddVert(new Vector3(workPos[i].x, workPos[i].y, 0f), c, uvs[i]);
                }

                int stride = cols + 1;
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        int a = y * stride + x;
                        int b = a + 1;
                        int d = a + stride;
                        int e = d + 1;
                        vh.AddTriangle(a, d, b);
                        vh.AddTriangle(b, d, e);
                    }
                }
            }

            private static Vector2 Rotate2D(Vector2 p, float cx, float cy, float rad)
            {
                float dx = p.x - cx;
                float dy = p.y - cy;
                float cs = Mathf.Cos(rad);
                float sn = Mathf.Sin(rad);
                return new Vector2(cx + dx * cs - dy * sn, cy + dx * sn + dy * cs);
            }

            private static float SmoothStep(float e0, float e1, float x)
            {
                float t = Mathf.Clamp01((x - e0) / (e1 - e0));
                return t * t * (3f - 2f * t);
            }
        }
    }
}
