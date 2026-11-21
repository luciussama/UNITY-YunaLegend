using System;
using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Diretor de Cinematica da abertura da Fase 2 (Quarto de Brinquedos).
    /// Conduz os planos de camera, calibracao de exposicao pictorica,
    /// movimento real 2K das pupilas/iris/catchlight estritamente dentro da cornea (preservando 100% a simetria
    /// e os cilios originais), e atuacao corporal com ternura entre Yuna e Nanda com forceMatrixRecalculationPerRender.
    /// </summary>
    public sealed class ToyRoomIntroCinematicController : MonoBehaviour
    {
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMapProp = Shader.PropertyToID("_BaseMap");

        private Camera mainCam;
        private IsometricToyRoomCamera isoCam;
        private ToyRoomPlayerController player;
        private YunaToyRoomAnimator yunaAnimator;
        private NandaToyRoomGuideController nandaGuide;
        private ToyChestController chest;
        private ToyRoomNarrativeController narrative;
        private ToyRoomAudioController audioController;

        // Esqueleto da Yuna
        private Transform yunaBodyRoot;
        private Transform yunaHips;
        private Transform yunaSpine;
        private Transform yunaChestBone;
        private Transform yunaNeck;
        private Transform yunaHead;
        private Transform yunaUpperArmL;
        private Transform yunaForearmL;
        private Transform yunaHandL;
        private Transform yunaUpperArmR;
        private Transform yunaForearmR;
        private Transform yunaHandR;

        // Esqueleto da Nanda
        private Transform nandaFairyBody;
        private Transform nandaTorso;
        private Transform nandaHead;
        private Transform nandaArmL;
        private Transform nandaArmR;
        private Transform nandaWingL;
        private Transform nandaWingR;
        private Light nandaPointLight;
        private float origNandaLightIntensity = 1.2f;

        // Renderers e texturas clonadas em runtime
        private SkinnedMeshRenderer yunaSmr;
        private Mesh yunaOriginalMesh;
        private Mesh yunaRuntimeMesh;
        private Material yunaOriginalMat;
        private Material yunaCinematicMat;
        private Texture2D yunaRuntimeTex;
        private Color32[] yunaBasePixels;
        private Color32[] yunaWorkPixels;
        private int yunaTexW;
        private int yunaTexH;

        private SkinnedMeshRenderer nandaSmr;
        private Material nandaOriginalMat;
        private Material nandaCinematicMat;
        private Texture2D nandaRuntimeTex;
        private Color32[] nandaBasePixels;
        private Color32[] nandaWorkPixels;
        private int nandaTexW;
        private int nandaTexH;

        private Vector3 yunaAnchorPos;
        private Quaternion yunaAnchorRot;
        private bool active;
        private bool tenderChimePlayed;
        private int lastBeatIndex = -1;

        public bool IsActive => active;
        public Vector2 CurrentYunaEyeGaze { get; private set; }
        public Vector2 CurrentNandaEyeGaze { get; private set; }
        public float CurrentYunaBlink { get; private set; }
        public float CurrentNandaBlink { get; private set; }

        public void Configure(
            Camera cam,
            IsometricToyRoomCamera isometricCam,
            ToyRoomPlayerController playerCtrl,
            NandaToyRoomGuideController nandaCtrl,
            ToyChestController chestCtrl,
            ToyRoomNarrativeController narrativeCtrl,
            ToyRoomAudioController audioCtrl)
        {
            mainCam = cam != null ? cam : Camera.main;
            isoCam = isometricCam;
            player = playerCtrl;
            yunaAnimator = playerCtrl != null ? playerCtrl.GetComponent<YunaToyRoomAnimator>() : null;
            nandaGuide = nandaCtrl;
            chest = chestCtrl;
            narrative = narrativeCtrl;
            audioController = audioCtrl;

            CacheCharacterRigsAndMeshes();
        }

        public void BeginIntroCinematic()
        {
            CacheCharacterRigsAndMeshes();
            ApplyCinematicMaterials(enable: true);

            if (player != null)
            {
                yunaAnchorPos = player.transform.position;
                yunaAnchorRot = Quaternion.Euler(0f, 180f, 0f);
                player.transform.rotation = yunaAnchorRot;
            }

            if (isoCam != null) isoCam.enabled = false;
            if (yunaAnimator != null) yunaAnimator.enabled = false;
            if (nandaGuide != null) nandaGuide.enabled = false;
            if (audioController != null) audioController.PlayExplorationMusic();

            tenderChimePlayed = false;
            lastBeatIndex = -1;
            active = true;
            EvaluateAtBeat(0, 0f);
        }

        public void EndIntroCinematic()
        {
            if (!active)
            {
                return;
            }

            active = false;
            ApplyCinematicMaterials(enable: false);

            if (player != null)
            {
                player.transform.position = yunaAnchorPos;
                player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            if (yunaAnimator != null) yunaAnimator.enabled = true;
            if (nandaGuide != null) nandaGuide.enabled = true;

            if (isoCam != null)
            {
                isoCam.enabled = true;
                isoCam.SnapToTarget();
            }

            if (mainCam != null)
            {
                mainCam.fieldOfView = 35f;
            }
        }

        private void OnDestroy()
        {
            ApplyCinematicMaterials(enable: false);
            if (yunaCinematicMat != null) DestroyImmediate(yunaCinematicMat);
            if (nandaCinematicMat != null) DestroyImmediate(nandaCinematicMat);
            if (yunaRuntimeTex != null) DestroyImmediate(yunaRuntimeTex);
            if (nandaRuntimeTex != null) DestroyImmediate(nandaRuntimeTex);
            if (yunaRuntimeMesh != null) DestroyImmediate(yunaRuntimeMesh);
        }

        private void LateUpdate()
        {
            if (!active)
            {
                return;
            }

            if (narrative == null || !narrative.IsPlaying)
            {
                EndIntroCinematic();
                return;
            }

            int beatIdx = Mathf.Clamp(narrative.CurrentBeatIndex, 0, 6);
            float t = Mathf.Clamp01(narrative.CurrentBeatProgress);

            if (beatIdx == 2 && lastBeatIndex != 2 && !tenderChimePlayed)
            {
                tenderChimePlayed = true;
                audioController?.PlayTenderChime();
            }

            lastBeatIndex = beatIdx;
            EvaluateAtBeat(beatIdx, t);
        }

        /// <summary>
        /// Avalia deterministicamente a pose de camera, movimento simetrico de pupilas/iris nos olhos,
        /// cabeca, bracos/maos e voo de Nanda para qualquer beat (0..6) e progresso normalizado t (0..1).
        /// </summary>
        public void EvaluateAtBeat(int beatIndex, float t)
        {
            CacheCharacterRigsAndMeshes();
            ApplyCinematicMaterials(enable: true);

            t = Mathf.Clamp01(t);
            float s = SmoothStep01(t);

            Vector3 yunaPos = player != null ? yunaAnchorPos : new Vector3(0f, 0.02f, -1.5f);
            if (player != null && yunaAnchorPos.sqrMagnitude < 0.0001f)
            {
                yunaAnchorPos = player.transform.position;
                yunaPos = yunaAnchorPos;
            }

            Vector3 chestPos = chest != null ? chest.transform.position : new Vector3(0f, 0f, 5.2f);

            // gazeScreen.x: -1 = olha para a ESQUERDA da imagem; +1 = olha para a DIREITA da imagem
            // gazeScreen.y: -1 = olha para BAIXO; +1 = olha para CIMA
            Vector2 yunaGazeScreen = Vector2.zero;
            Vector2 nandaGazeScreen = Vector2.zero;
            float yunaBlink = ComputeNaturalBlink(beatIndex, t, isYuna: true);
            float nandaBlink = ComputeNaturalBlink(beatIndex, t, isYuna: false);

            Vector3 yunaHeadEuler = Vector3.zero;
            Vector3 yunaSpineEuler = Vector3.zero;
            Vector3 yunaArmLEuler = new Vector3(0f, 0f, 16f);
            Vector3 yunaForearmLEuler = new Vector3(-10f, 0f, 4f);
            Vector3 yunaHandLEuler = Vector3.zero;
            Vector3 yunaArmREuler = new Vector3(0f, 0f, -16f);
            Vector3 yunaForearmREuler = new Vector3(-10f, 0f, -4f);
            Vector3 yunaHandREuler = Vector3.zero;
            float yunaBodyYaw = 180f;

            Vector3 nandaWorldPos = yunaPos + new Vector3(-0.62f, 1.06f, -0.15f);
            float nandaBodyYaw = 160f;
            Vector3 nandaHeadEuler = Vector3.zero;
            Vector3 nandaTorsoEuler = new Vector3(4f, 0f, 0f);
            Vector3 nandaArmLEuler = new Vector3(0f, 0f, -8f);
            Vector3 nandaArmREuler = new Vector3(0f, 0f, 8f);

            // Camera com headroom generoso abaixo da caixa de dialogo no topo e leve pitch negativo (-3 graus)
            Vector3 camPos = yunaPos + new Vector3(-0.95f, 1.36f, -2.75f);
            Vector3 camLook = yunaPos + new Vector3(-0.22f, 1.24f, 0.0f);
            float camFov = 27f;

            switch (beatIndex)
            {
                case 0:
                    // PLANO 1 (0..2.5s): Despertar na luz matinal
                    // Em t=0.15 (01): Yuna e Nanda movem claramente as pupilas para a ESQUERDA da tela (gazeScreen.x = -1.0)
                    // Em t=0.88 (02): Yuna move as pupilas para a DIREITA da tela (gazeScreen.x = +1.0) e Nanda olha para Yuna (+1.0)
                    yunaBodyYaw = 180f;
                    float scan = Mathf.Lerp(-1.0f, 1.0f, SmoothStep01(Mathf.Clamp01((t - 0.15f) / 0.70f)));
                    yunaGazeScreen = new Vector2(scan, -0.10f);
                    yunaHeadEuler = new Vector3(2f, -scan * 12f, scan * 3f);
                    yunaSpineEuler = new Vector3(Mathf.Sin(t * Mathf.PI) * 2.5f, -scan * 3f, 0f);

                    nandaWorldPos = yunaPos + new Vector3(
                        Mathf.Lerp(-0.68f, -0.58f, s),
                        1.06f + Mathf.Sin(t * Mathf.PI * 2f) * 0.03f,
                        -0.12f);
                    nandaBodyYaw = Mathf.Lerp(175f, 152f, s);
                    nandaGazeScreen = new Vector2(Mathf.Lerp(-1.0f, 1.0f, s), -0.10f);
                    nandaHeadEuler = new Vector3(3f, Mathf.Lerp(-8f, 10f, s), Mathf.Lerp(2f, -5f, s));

                    camPos = Vector3.Lerp(
                        yunaPos + new Vector3(-0.95f, 1.36f, -2.75f),
                        yunaPos + new Vector3(-0.78f, 1.34f, -2.62f),
                        s);
                    camLook = yunaPos + new Vector3(-0.24f, 1.24f, 0.02f);
                    camFov = 27f;
                    break;

                case 1:
                    // PLANO 2 (2.5..6.5s) MENINA: "Aqui também..."
                    // Yuna baixa as pupilas para baixo/direita (olhando os brinquedos no chao) e inclina a cabeca;
                    // Nanda olha diretamente para o rosto de Yuna com empatia.
                    yunaBodyYaw = 178f;
                    yunaGazeScreen = Vector2.Lerp(new Vector2(0.35f, -0.45f), new Vector2(1.0f, -0.90f), s);
                    yunaHeadEuler = new Vector3(Mathf.Lerp(4f, 10f, s), Mathf.Lerp(-4f, -10f, s), Mathf.Lerp(-2f, -5f, s));
                    yunaSpineEuler = new Vector3(Mathf.Lerp(2f, 4.5f, s), -2f, -2f);
                    yunaArmLEuler = new Vector3(12f, 6f, 18f);
                    yunaArmREuler = new Vector3(12f, -6f, -18f);

                    nandaWorldPos = yunaPos + new Vector3(
                        Mathf.Lerp(-0.58f, -0.52f, s),
                        Mathf.Lerp(1.06f, 1.02f, s),
                        -0.10f);
                    nandaBodyYaw = 152f;
                    nandaGazeScreen = new Vector2(1.0f, -0.15f);
                    nandaHeadEuler = new Vector3(5f, 10f, -6f);

                    camPos = Vector3.Lerp(
                        yunaPos + new Vector3(-0.68f, 1.36f, -2.65f),
                        yunaPos + new Vector3(-0.58f, 1.35f, -2.58f),
                        s);
                    camLook = yunaPos + new Vector3(-0.22f, 1.25f, 0.02f);
                    camFov = 26.5f;
                    break;

                case 2:
                    // PLANO 3 (6.5..10.5s) MENINA: "Eu queria espaço pra brincar."
                    // Enquadramento Medium Two-Shot com headroom generoso acima do coque e mostrando bracos e maos!
                    // Em t=0.18 (04): Yuna com bracos abaixados, pupilas olhando para baixo-direita (+0.90, -0.75); Nanda a esquerda.
                    // Em t=0.88 (05): Yuna vira a cabeca, move as pupilas para a ESQUERDA/CIMA (-1.0, +0.65) olhando nos olhos de Nanda
                    // e ERGUE AMBOS OS BRACOS/MAOS num gesto claro de ternura; Nanda voa pertinho do ombro/rosto de Yuna e inclina-se com carinho!
                    float actT = SmoothStep01(Mathf.Clamp01((t - 0.22f) / 0.58f));

                    yunaBodyYaw = Mathf.Lerp(178f, 164f, actT);
                    yunaGazeScreen = Vector2.Lerp(new Vector2(0.90f, -0.75f), new Vector2(-1.0f, 0.65f), actT);
                    yunaHeadEuler = Vector3.Lerp(new Vector3(10f, -12f, -5f), new Vector3(-8f, 22f, 10f), actT);
                    yunaSpineEuler = Vector3.Lerp(new Vector3(5f, -3f, -2f), new Vector3(-4f, 8f, 5f), actT);

                    // Gesto claro de ternura dos bracos/antebracos/maos da Yuna (agora com forceMatrixRecalculationPerRender = true!)
                    yunaArmLEuler = Vector3.Lerp(new Vector3(8f, 0f, 14f), new Vector3(-64f, 28f, 38f), actT);
                    yunaForearmLEuler = Vector3.Lerp(new Vector3(-8f, 0f, 4f), new Vector3(-56f, 16f, 24f), actT);
                    yunaHandLEuler = Vector3.Lerp(Vector3.zero, new Vector3(-18f, 12f, 14f), actT);

                    yunaArmREuler = Vector3.Lerp(new Vector3(8f, 0f, -14f), new Vector3(-58f, -24f, -32f), actT);
                    yunaForearmREuler = Vector3.Lerp(new Vector3(-8f, 0f, -4f), new Vector3(-52f, -14f, -20f), actT);
                    yunaHandREuler = Vector3.Lerp(Vector3.zero, new Vector3(-16f, -10f, -12f), actT);

                    nandaWorldPos = yunaPos + Vector3.Lerp(
                        new Vector3(-0.72f, 1.12f, -0.08f),
                        new Vector3(-0.38f, 0.98f, -0.18f),
                        actT);
                    nandaBodyYaw = Mathf.Lerp(166f, 124f, actT);
                    nandaGazeScreen = Vector2.Lerp(new Vector2(-0.50f, -0.40f), new Vector2(1.0f, 0.55f), actT);
                    nandaHeadEuler = Vector3.Lerp(new Vector3(5f, -6f, 3f), new Vector3(-6f, 12f, -10f), actT);
                    nandaTorsoEuler = Vector3.Lerp(new Vector3(3f, 0f, 0f), new Vector3(14f, 8f, -14f), actT);
                    // Rotacao suave nos bracos da Nanda sem torcer os dedos
                    nandaArmLEuler = Vector3.Lerp(new Vector3(4f, 0f, -6f), new Vector3(-28f, 10f, -16f), actT);
                    nandaArmREuler = Vector3.Lerp(new Vector3(4f, 0f, 6f), new Vector3(-32f, -10f, 18f), actT);

                    camPos = Vector3.Lerp(
                        yunaPos + new Vector3(0.16f, 1.32f, -2.92f),
                        yunaPos + new Vector3(0.08f, 1.30f, -2.78f),
                        actT);
                    camLook = yunaPos + new Vector3(-0.22f, 1.16f, 0.01f);
                    camFov = 27f;
                    break;

                case 3:
                case 4:
                    // PLANO 4 (10.5..14.4s) POETICO: "A sala estava cheia. / Nenhum lugar esperava por ela."
                    float panT = beatIndex == 3 ? t * 0.82f : 0.82f + t * 0.18f;
                    float panS = SmoothStep01(panT);
                    yunaBodyYaw = Mathf.Lerp(168f, 188f, panS);
                    float roomGazeX = Mathf.Lerp(-0.95f, 0.95f, panS);
                    yunaGazeScreen = new Vector2(roomGazeX, -0.10f);
                    yunaHeadEuler = new Vector3(2f, -roomGazeX * 14f, 0f);

                    nandaWorldPos = yunaPos + new Vector3(
                        Mathf.Lerp(-0.52f, -0.60f, panS),
                        1.12f + Mathf.Sin(panT * Mathf.PI * 2f) * 0.04f,
                        -0.12f);
                    nandaBodyYaw = Mathf.Lerp(150f, 175f, panS);
                    nandaGazeScreen = new Vector2(roomGazeX * 0.95f, -0.15f);
                    nandaHeadEuler = new Vector3(3f, -roomGazeX * 10f, 0f);

                    camPos = Vector3.Lerp(
                        yunaPos + new Vector3(-2.10f, 2.05f, -3.85f),
                        yunaPos + new Vector3(1.45f, 1.98f, -3.70f),
                        panS);
                    camLook = Vector3.Lerp(
                        yunaPos + new Vector3(-0.25f, 0.98f, 0.35f),
                        yunaPos + new Vector3(0.20f, 1.00f, 0.45f),
                        panS);
                    camFov = 30f;
                    break;

                case 5:
                    // PLANO 5 (14.4..17.5s) POETICO: "A caixa tinha um fundo. / O resto está vazio."
                    // Posiciona Nanda deslocada ao longo do vetor DIREITO da camera (+0.52, 1.08, -0.22),
                    // 12.4 graus a direita da cabeca da Yuna no centro-esquerdo do quadro, 100% visivel em 3/4!
                    Vector3 toChest = (chestPos - yunaPos).normalized;
                    float chestYaw = Mathf.Atan2(toChest.x, toChest.z) * Mathf.Rad2Deg;

                    yunaBodyYaw = Mathf.Lerp(180f, Mathf.LerpAngle(180f, chestYaw, 0.35f), s);
                    yunaGazeScreen = Vector2.Lerp(new Vector2(0.30f, 0.0f), new Vector2(1.0f, 0.35f), s);
                    yunaHeadEuler = new Vector3(Mathf.Lerp(2f, -4f, s), Mathf.Lerp(0f, -16f, s), 0f);

                    nandaWorldPos = yunaPos + new Vector3(
                        Mathf.Lerp(0.44f, 0.54f, s),
                        Mathf.Lerp(1.04f, 1.10f, s),
                        Mathf.Lerp(-0.26f, -0.18f, s));
                    nandaBodyYaw = 118f; // 3/4 voltada para a direita/bau, mostrando o rosto e as asas
                    nandaGazeScreen = new Vector2(1.0f, 0.25f);
                    nandaHeadEuler = new Vector3(-3f, 8f, -4f);
                    nandaArmLEuler = new Vector3(-18f, 8f, -12f);
                    nandaArmREuler = new Vector3(-28f, -12f, 16f);

                    camPos = Vector3.Lerp(
                        yunaPos + new Vector3(-1.18f, 1.38f, -2.35f),
                        yunaPos + new Vector3(-1.08f, 1.36f, -2.25f),
                        s);
                    camLook = Vector3.Lerp(
                        yunaPos + new Vector3(0.15f, 1.08f, 0.55f),
                        Vector3.Lerp(yunaPos + Vector3.up * 1.08f, chestPos + Vector3.up * 0.78f, 0.28f),
                        s);
                    camFov = 28f;
                    break;

                default:
                    // PLANO 6 (17.5..20.0s) NANDA: "Vamos. Um de cada vez!"
                    yunaBodyYaw = 180f;
                    float arcAngle = t * Mathf.PI;
                    nandaWorldPos = yunaPos + new Vector3(
                        -0.56f,
                        1.06f + Mathf.Sin(arcAngle) * 0.08f,
                        -0.12f + Mathf.Sin(arcAngle) * 0.12f);
                    nandaBodyYaw = 148f + Mathf.Sin(arcAngle) * 14f;
                    nandaGazeScreen = new Vector2(0.95f, -0.25f);
                    nandaHeadEuler = new Vector3(5f, 10f, -6f);
                    nandaArmLEuler = new Vector3(-24f, 8f, -14f);
                    nandaArmREuler = new Vector3(-24f, -8f, 14f);

                    yunaGazeScreen = new Vector2(-0.95f, 0.35f);
                    yunaHeadEuler = new Vector3(-4f, 14f, 4f);
                    float joyWave = Mathf.Sin(t * Mathf.PI * 3f);
                    yunaArmLEuler = new Vector3(-48f, 18f, 28f);
                    yunaForearmLEuler = new Vector3(-46f + joyWave * 10f, 0f, 14f);
                    yunaArmREuler = new Vector3(-48f, -18f, -28f);
                    yunaForearmREuler = new Vector3(-46f - joyWave * 10f, 0f, -14f);

                    Vector3 closePos = yunaPos + new Vector3(-0.62f, 1.36f, -2.72f);
                    Vector3 closeLook = yunaPos + new Vector3(-0.20f, 1.24f, 0.02f);

                    float blendToGameplay = SmoothStep01(Mathf.Clamp01((t - 0.58f) / 0.42f));
                    Vector3 gameplayFocus = yunaPos + new Vector3(0f, 0.95f, 0.15f);
                    Vector3 gameplayPos = gameplayFocus + new Vector3(0f, 4.35f, -9.20f);
                    camPos = Vector3.Lerp(closePos, gameplayPos, blendToGameplay);
                    camLook = Vector3.Lerp(closeLook, gameplayFocus, blendToGameplay);
                    camFov = Mathf.Lerp(26.5f, 35f, blendToGameplay);
                    break;
            }

            if (player != null)
            {
                player.transform.position = yunaPos;
                player.transform.rotation = Quaternion.Euler(0f, yunaBodyYaw, 0f);
            }

            if (yunaSpine != null) yunaSpine.localRotation = Quaternion.Euler(yunaSpineEuler * 0.5f);
            if (yunaChestBone != null) yunaChestBone.localRotation = Quaternion.Euler(yunaSpineEuler * 0.5f);
            if (yunaNeck != null) yunaNeck.localRotation = Quaternion.Euler(yunaHeadEuler * 0.35f);
            if (yunaHead != null) yunaHead.localRotation = Quaternion.Euler(yunaHeadEuler * 0.65f);
            if (yunaUpperArmL != null) yunaUpperArmL.localRotation = Quaternion.Euler(yunaArmLEuler);
            if (yunaForearmL != null) yunaForearmL.localRotation = Quaternion.Euler(yunaForearmLEuler);
            if (yunaHandL != null) yunaHandL.localRotation = Quaternion.Euler(yunaHandLEuler);
            if (yunaUpperArmR != null) yunaUpperArmR.localRotation = Quaternion.Euler(yunaArmREuler);
            if (yunaForearmR != null) yunaForearmR.localRotation = Quaternion.Euler(yunaForearmREuler);
            if (yunaHandR != null) yunaHandR.localRotation = Quaternion.Euler(yunaHandREuler);

            if (nandaGuide != null)
            {
                nandaGuide.transform.position = nandaWorldPos;
                nandaGuide.transform.rotation = Quaternion.Euler(0f, nandaBodyYaw, 0f);
            }

            if (nandaTorso != null) nandaTorso.localRotation = Quaternion.Euler(nandaTorsoEuler);
            if (nandaHead != null) nandaHead.localRotation = Quaternion.Euler(nandaHeadEuler);
            if (nandaArmL != null) nandaArmL.localRotation = Quaternion.Euler(nandaArmLEuler);
            if (nandaArmR != null) nandaArmR.localRotation = Quaternion.Euler(nandaArmREuler);

            float wingFlutter = Mathf.Sin((beatIndex * 3.5f + t * 4.0f) * Mathf.PI * 4f) * 28f;
            if (nandaWingL != null) nandaWingL.localRotation = Quaternion.Euler(0f, 12f + wingFlutter, 0f);
            if (nandaWingR != null) nandaWingR.localRotation = Quaternion.Euler(0f, -12f - wingFlutter, 0f);

            CurrentYunaEyeGaze = yunaGazeScreen;
            CurrentNandaEyeGaze = nandaGazeScreen;
            CurrentYunaBlink = yunaBlink;
            CurrentNandaBlink = nandaBlink;
            UpdateYunaEyeTexture(yunaGazeScreen);
            UpdateNandaEyeTexture(nandaGazeScreen);

            if (mainCam != null)
            {
                float aspect = mainCam.aspect > 0.1f ? mainCam.aspect : (16f / 9f);
                float portraitBackoff = aspect < 1.0f ? Mathf.Clamp(1.0f / Mathf.Max(0.45f, aspect), 1.25f, 1.85f) : 1.0f;
                Vector3 dir = camPos - camLook;
                mainCam.transform.position = camLook + dir * portraitBackoff;
                mainCam.transform.LookAt(camLook);
                mainCam.fieldOfView = camFov;
            }
        }

        private static float ComputeNaturalBlink(int beatIndex, float t, bool isYuna)
        {
            float center = isYuna ? 0.46f : 0.62f;
            if (beatIndex == 2)
            {
                center = isYuna ? 0.50f : 0.55f;
            }

            float dist = Mathf.Abs(t - center);
            if (dist > 0.06f)
            {
                return 0f;
            }

            return Mathf.Cos((dist / 0.06f) * Mathf.PI * 0.5f);
        }

        private void ApplyCinematicMaterials(bool enable)
        {
            if (yunaSmr != null)
            {
                yunaSmr.forceMatrixRecalculationPerRender = enable;
                if (enable && yunaCinematicMat != null)
                {
                    yunaSmr.sharedMaterial = yunaCinematicMat;
                    if (yunaRuntimeMesh != null) yunaSmr.sharedMesh = yunaRuntimeMesh;
                }
                else if (!enable && yunaOriginalMat != null)
                {
                    yunaSmr.sharedMaterial = yunaOriginalMat;
                    if (yunaOriginalMesh != null) yunaSmr.sharedMesh = yunaOriginalMesh;
                }
            }

            if (nandaSmr != null)
            {
                nandaSmr.forceMatrixRecalculationPerRender = enable;
                if (enable && nandaCinematicMat != null)
                {
                    nandaSmr.sharedMaterial = nandaCinematicMat;
                }
                else if (!enable && nandaOriginalMat != null)
                {
                    nandaSmr.sharedMaterial = nandaOriginalMat;
                }
            }

            if (nandaPointLight != null)
            {
                nandaPointLight.intensity = enable ? 0.35f : origNandaLightIntensity;
            }
        }

        private void CacheCharacterRigsAndMeshes()
        {
            if (player == null) player = FindFirstObjectByType<ToyRoomPlayerController>();
            if (nandaGuide == null) nandaGuide = FindFirstObjectByType<NandaToyRoomGuideController>();
            if (chest == null) chest = FindFirstObjectByType<ToyChestController>();
            if (mainCam == null) mainCam = Camera.main;
            if (isoCam == null && mainCam != null) isoCam = mainCam.GetComponent<IsometricToyRoomCamera>();

            if (player != null && yunaHead == null)
            {
                yunaBodyRoot = player.transform.Find("Yuna_BodyRoot");
                if (yunaBodyRoot != null)
                {
                    var rigRoot = yunaBodyRoot.Find("Yuna_RigRoot");
                    if (rigRoot != null)
                    {
                        yunaHips = rigRoot.Find("Hips");
                        yunaSpine = yunaHips != null ? yunaHips.Find("Spine") : null;
                        yunaChestBone = yunaSpine != null ? yunaSpine.Find("Chest") : null;
                        yunaNeck = yunaChestBone != null ? yunaChestBone.Find("Neck") : null;
                        yunaHead = yunaNeck != null ? yunaNeck.Find("Head") : null;
                        yunaUpperArmL = yunaChestBone != null ? yunaChestBone.Find("UpperArm_L") : null;
                        yunaForearmL = yunaUpperArmL != null ? yunaUpperArmL.Find("Forearm_L") : null;
                        yunaHandL = yunaForearmL != null ? yunaForearmL.Find("Hand_L") : null;
                        yunaUpperArmR = yunaChestBone != null ? yunaChestBone.Find("UpperArm_R") : null;
                        yunaForearmR = yunaUpperArmR != null ? yunaUpperArmR.Find("Forearm_R") : null;
                        yunaHandR = yunaForearmR != null ? yunaForearmR.Find("Hand_R") : null;
                    }

                    yunaSmr = yunaBodyRoot.GetComponentInChildren<SkinnedMeshRenderer>(true);
                    if (yunaSmr != null && yunaSmr.sharedMesh != null && yunaRuntimeMesh == null)
                    {
                        yunaSmr.forceMatrixRecalculationPerRender = true;
                        yunaOriginalMesh = yunaSmr.sharedMesh;
                        yunaRuntimeMesh = Instantiate(yunaOriginalMesh);
                        yunaRuntimeMesh.name = "Yuna_SkinnedMesh_IntroClean";
                        CleanYunaMeshAndPreserveSkinning(yunaRuntimeMesh, yunaOriginalMesh);
                        yunaSmr.sharedMesh = yunaRuntimeMesh;

                        yunaOriginalMat = yunaSmr.sharedMaterial;
                        if (yunaOriginalMat != null)
                        {
                            var srcTex = yunaOriginalMat.GetTexture(BaseMapProp);
                            if (srcTex != null)
                            {
                                yunaRuntimeTex = CloneReadableTexture(srcTex, out yunaBasePixels, out yunaWorkPixels, out yunaTexW, out yunaTexH);
                            }

                            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
                            yunaCinematicMat = new Material(unlit != null ? unlit : yunaOriginalMat.shader)
                            {
                                name = "M_Yuna_IntroCinematic_Unlit"
                            };
                            yunaCinematicMat.SetTexture(BaseMapProp, yunaRuntimeTex != null ? yunaRuntimeTex : srcTex);
                            yunaCinematicMat.SetColor(BaseColorProp, new Color(0.94f, 0.91f, 0.88f, 1f));
                        }
                    }
                }
            }

            if (nandaGuide != null && nandaHead == null)
            {
                nandaPointLight = nandaGuide.GetComponentInChildren<Light>(true);
                if (nandaPointLight != null)
                {
                    origNandaLightIntensity = nandaPointLight.intensity;
                }

                nandaFairyBody = nandaGuide.transform.Find("Nanda_FairyBody");
                if (nandaFairyBody != null)
                {
                    var glb = nandaFairyBody.Find("Nanda_GlbMesh");
                    if (glb != null)
                    {
                        nandaTorso = glb.Find("Bone_Torso");
                        if (nandaTorso != null)
                        {
                            nandaHead = nandaTorso.Find("Bone_Head");
                            nandaWingL = nandaTorso.Find("Bone_Wing_L");
                            nandaWingR = nandaTorso.Find("Bone_Wing_R");
                            nandaArmL = nandaTorso.Find("Bone_Arm_L");
                            nandaArmR = nandaTorso.Find("Bone_Arm_R");
                        }

                        nandaSmr = glb.GetComponent<SkinnedMeshRenderer>();
                        if (nandaSmr != null && nandaOriginalMat == null)
                        {
                            nandaSmr.forceMatrixRecalculationPerRender = true;
                            nandaOriginalMat = nandaSmr.sharedMaterial;
                            if (nandaOriginalMat != null)
                            {
                                var srcTex = nandaOriginalMat.GetTexture(BaseMapProp);
                                if (srcTex != null)
                                {
                                    nandaRuntimeTex = CloneReadableTexture(srcTex, out nandaBasePixels, out nandaWorkPixels, out nandaTexW, out nandaTexH);
                                }

                                nandaCinematicMat = new Material(nandaOriginalMat)
                                {
                                    name = "M_Nanda_IntroCinematic_Unlit"
                                };
                                if (nandaRuntimeTex != null)
                                {
                                    nandaCinematicMat.SetTexture(BaseMapProp, nandaRuntimeTex);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove o fragmento orfao flutuante acima do cavalinho (|x|>0.24 em y>0.82),
        /// limpa a mancha em C na tempora direita da Yuna mapeando para o tom de pele da bochecha,
        /// e preserva bindposes e boneWeights para manter o skinning GPU 100% ativo.
        /// </summary>
        private static void CleanYunaMeshAndPreserveSkinning(Mesh mesh, Mesh sourceMesh)
        {
            Vector3[] verts = sourceMesh.vertices;
            Vector2[] uvs = sourceMesh.uv;
            BoneWeight[] weights = sourceMesh.boneWeights;
            Matrix4x4[] bindposes = sourceMesh.bindposes;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 p = verts[i];

                // 1) Elimina pedacinho de malha orfao flutuante (x=+0.54, y=1.06)
                if ((p.y > 0.82f && Mathf.Abs(p.x) > 0.235f) || Mathf.Abs(p.x) > 0.42f)
                {
                    verts[i] = new Vector3(0f, 0.65f, 0f);
                    if (weights != null && i < weights.Length)
                    {
                        weights[i] = new BoneWeight { boneIndex0 = 2, weight0 = 1f };
                    }

                    continue;
                }

                // 2) Limpa a mancha escura em formato de "C" na tempora/bochecha direita da Yuna (x em 0.082..0.130, y em 0.950..1.025, z > 0.055)
                if (p.x >= 0.082f && p.x <= 0.132f && p.y >= 0.950f && p.y <= 1.028f && p.z >= 0.055f)
                {
                    Vector2 uv = uvs[i];
                    if ((uv.x >= 0.732f && uv.x <= 0.775f && uv.y >= 0.645f && uv.y <= 0.688f)
                        || (uv.x >= 0.575f && uv.x <= 0.665f && uv.y >= 0.250f && uv.y <= 0.330f))
                    {
                        // Remapeia para pele limpa da bochecha na mesma ilha UV
                        uvs[i] = new Vector2(0.718f, 0.625f);
                    }
                }

                // 3) Garante que os cachos da bochecha (y >= 0.805) nao sejam puxados pelos ossos do braco
                if (weights != null && i < weights.Length && p.y >= 0.805f)
                {
                    float tHead = Mathf.InverseLerp(0.805f, 0.89f, p.y);
                    weights[i] = new BoneWeight
                    {
                        boneIndex0 = 4,
                        weight0 = tHead,
                        boneIndex1 = 3,
                        weight1 = 1f - tHead
                    };
                }
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            if (weights != null && weights.Length == verts.Length)
            {
                mesh.boneWeights = weights;
            }

            if (bindposes != null && bindposes.Length > 0)
            {
                mesh.bindposes = bindposes;
            }
        }

        private static Texture2D CloneReadableTexture(Texture src, out Color32[] basePixels, out Color32[] workPixels, out int width, out int height)
        {
            width = src.width;
            height = src.height;
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;
            Graphics.Blit(src, rt);
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = src.name + "_RuntimeEyeAnimated",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply(false, false);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            basePixels = tex.GetPixels32();
            workPixels = new Color32[basePixels.Length];
            Array.Copy(basePixels, workPixels, basePixels.Length);
            return tex;
        }

        /// <summary>
        /// Move a pupila escura e o brilho especular branco estritamente DENTRO da iris/cornea (r < 0.0110 UV = 22px),
        /// preservando 100% intocados os cilios, as palpebras e a simetria perfeita dos olhos originais!
        /// </summary>
        private void UpdateYunaEyeTexture(Vector2 gazeScreen)
        {
            if (yunaRuntimeTex == null || yunaBasePixels == null)
            {
                return;
            }

            float gx = Mathf.Clamp(gazeScreen.x, -1f, 1f);
            float gy = Mathf.Clamp(gazeScreen.y, -1f, 1f);

            // Olho Esquerdo da Yuna (x<0 local, direita na tela): center UV (0.8475, 0.7735)
            // Eixos conjugados medidos na malha 3D: eRight = (-0.54, +0.84), eUp = (-0.88, +0.46)
            Vector2 eRightL = new Vector2(-0.54f, 0.84f);
            Vector2 eUpL = new Vector2(-0.88f, 0.46f);
            PaintCleanMovingPupilAndIris(
                yunaBasePixels,
                yunaWorkPixels,
                yunaTexW,
                yunaTexH,
                centerU: 0.8475f,
                centerV: 0.7735f,
                corneaRadiusUV: 0.0112f,
                eRightUV: eRightL,
                eUpUV: eUpL,
                gazeX: gx,
                gazeY: gy,
                irisWarmColor: new Color32(132, 62, 28, 255),
                pupilDarkColor: new Color32(22, 10, 8, 255));

            // Olho Direito da Yuna (x>0 local, esquerda na tela): center UV (0.7355, 0.6485)
            // Eixos conjugados medidos na malha 3D: eRight = (-0.99, +0.14), eUp = (+0.92, -0.38)
            Vector2 eRightR = new Vector2(-0.99f, 0.14f);
            Vector2 eUpR = new Vector2(0.92f, -0.38f);
            PaintCleanMovingPupilAndIris(
                yunaBasePixels,
                yunaWorkPixels,
                yunaTexW,
                yunaTexH,
                centerU: 0.7355f,
                centerV: 0.6485f,
                corneaRadiusUV: 0.0112f,
                eRightUV: eRightR,
                eUpUV: eUpR,
                gazeX: gx,
                gazeY: gy,
                irisWarmColor: new Color32(132, 62, 28, 255),
                pupilDarkColor: new Color32(22, 10, 8, 255));

            yunaRuntimeTex.SetPixels32(yunaWorkPixels);
            yunaRuntimeTex.Apply(false, false);
        }

        private void UpdateNandaEyeTexture(Vector2 gazeScreen)
        {
            if (nandaRuntimeTex == null || nandaBasePixels == null)
            {
                return;
            }

            float gx = Mathf.Clamp(gazeScreen.x, -1f, 1f);
            float gy = Mathf.Clamp(gazeScreen.y, -1f, 1f);

            // Ambos os olhos da Nanda estao na mesma ilha UV frontal: eRight = (-0.27, -0.96), eUp = (-0.90, -0.43)
            Vector2 eRightN = new Vector2(-0.27f, -0.96f);
            Vector2 eUpN = new Vector2(-0.90f, -0.43f);

            PaintCleanMovingPupilAndIris(
                nandaBasePixels,
                nandaWorkPixels,
                nandaTexW,
                nandaTexH,
                centerU: 0.6590f,
                centerV: 0.5930f,
                corneaRadiusUV: 0.0095f,
                eRightUV: eRightN,
                eUpUV: eUpN,
                gazeX: gx,
                gazeY: gy,
                irisWarmColor: new Color32(158, 48, 38, 255),
                pupilDarkColor: new Color32(24, 8, 10, 255));

            PaintCleanMovingPupilAndIris(
                nandaBasePixels,
                nandaWorkPixels,
                nandaTexW,
                nandaTexH,
                centerU: 0.6660f,
                centerV: 0.6495f,
                corneaRadiusUV: 0.0095f,
                eRightUV: eRightN,
                eUpUV: eUpN,
                gazeX: gx,
                gazeY: gy,
                irisWarmColor: new Color32(158, 48, 38, 255),
                pupilDarkColor: new Color32(24, 8, 10, 255));

            nandaRuntimeTex.SetPixels32(nandaWorkPixels);
            nandaRuntimeTex.Apply(false, false);
        }

        /// <summary>
        /// Opera estritamente dentro do disco interno da iris/cornea (radPx ~ 20-23px), sem jamais tocar
        /// nos cilios nem nas costuras UV externas:
        /// 1. Desloca suavemente a iris dentro da cornea (ate 42% do raio) revelando esclera creme do lado oposto;
        /// 2. Desenha a pupila escura e o brilho especular branco nitido deslocados na direcao exata do olhar (gazeX, gazeY),
        ///    substituindo o brilho estatico antigo pela cor ambar da iris para zero duplicacao!
        /// </summary>
        private static void PaintCleanMovingPupilAndIris(
            Color32[] basePixels,
            Color32[] workPixels,
            int texW,
            int texH,
            float centerU,
            float centerV,
            float corneaRadiusUV,
            Vector2 eRightUV,
            Vector2 eUpUV,
            float gazeX,
            float gazeY,
            Color32 irisWarmColor,
            Color32 pupilDarkColor)
        {
            int cx = Mathf.RoundToInt(centerU * texW);
            int cy = Mathf.RoundToInt(centerV * texH);
            int radPx = Mathf.Max(8, Mathf.RoundToInt(corneaRadiusUV * texW));
            float invRad = 1f / radPx;

            // Vetor de deslocamento do olhar em pixels dentro do disco da cornea
            Vector2 gazeUV = eRightUV * gazeX + eUpUV * gazeY;
            float shiftPxX = gazeUV.x * radPx * 0.46f;
            float shiftPxY = gazeUV.y * radPx * 0.46f;

            // Posicao do novo brilho especular branco (catchlight) que se move junto com a pupila
            Vector2 catchOffset = (eRightUV * (gazeX * 0.48f - 0.18f) + eUpUV * (gazeY * 0.48f + 0.22f)) * radPx;

            int xMin = Mathf.Max(1, cx - radPx);
            int xMax = Mathf.Min(texW - 2, cx + radPx);
            int yMin = Mathf.Max(1, cy - radPx);
            int yMax = Mathf.Min(texH - 2, cy + radPx);

            Color32 scleraCream = new Color32(246, 236, 224, 255);
            Color32 catchWhite = new Color32(255, 253, 248, 255);

            for (int y = yMin; y <= yMax; y++)
            {
                float dy = y - cy;
                float ny = dy * invRad;
                int rowOffset = y * texW;
                for (int x = xMin; x <= xMax; x++)
                {
                    float dx = x - cx;
                    float nx = dx * invRad;
                    float r2 = nx * nx + ny * ny;
                    if (r2 >= 1f)
                    {
                        workPixels[rowOffset + x] = basePixels[rowOffset + x];
                        continue;
                    }

                    Color32 orig = basePixels[rowOffset + x];
                    float r = Mathf.Sqrt(r2);
                    // Feather suave na borda da cornea (r entre 0.76 e 1.0) para preservar 100% dos cilios originais
                    float mask = 1f - SmoothRange(0.76f, 1.0f, r);

                    // Coordenada relativa ao novo centro da iris deslocada pelo olhar
                    float ix = (dx - shiftPxX) * invRad;
                    float iy = (dy - shiftPxY) * invRad;
                    float irisR = Mathf.Sqrt(ix * ix + iy * iy);

                    Color32 targetCol;
                    if (irisR > 0.74f)
                    {
                        // Lado oposto ao olhar mostra a esclera clara (branco do olho)
                        float edgeT = SmoothRange(0.68f, 0.80f, irisR);
                        targetCol = LerpColor32(irisWarmColor, scleraCream, edgeT);
                    }
                    else
                    {
                        // Dentro da iris deslocada: anel ambar + pupila escura central
                        float pupilT = 1f - SmoothRange(0.32f, 0.50f, irisR);
                        targetCol = LerpColor32(irisWarmColor, pupilDarkColor, pupilT);
                    }

                    // Brilho especular branco vivo acompanhando o olhar
                    float cdx = dx - catchOffset.x;
                    float cdy = dy - catchOffset.y;
                    float catchDist = Mathf.Sqrt(cdx * cdx + cdy * cdy) / (radPx * 0.22f);
                    if (catchDist < 1f)
                    {
                        float catchT = 1f - SmoothRange(0.45f, 1.0f, catchDist);
                        targetCol = LerpColor32(targetCol, catchWhite, catchT);
                    }

                    workPixels[rowOffset + x] = LerpColor32(orig, targetCol, mask);
                }
            }
        }

        private static float SmoothRange(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static Color32 LerpColor32(Color32 a, Color32 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color32(
                (byte)Mathf.RoundToInt(a.r + (b.r - a.r) * t),
                (byte)Mathf.RoundToInt(a.g + (b.g - a.g) * t),
                (byte)Mathf.RoundToInt(a.b + (b.b - a.b) * t),
                255);
        }

        private static float SmoothStep01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }
}
