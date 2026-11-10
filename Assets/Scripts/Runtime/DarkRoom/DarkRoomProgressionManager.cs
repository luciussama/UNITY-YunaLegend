using System;
using UnityEngine;
using UnityEngine.InputSystem;
using YunaLegend.Camera25D;
using YunaLegend.Dialogue;

namespace YunaLegend.DarkRoom
{
    public enum CutsceneType
    {
        None,
        CastleRest,
        FalseDoorPlotTwist,
        TruePortalVictory
    }

    public class DarkRoomProgressionManager : MonoBehaviour
    {
        [Header("Atores e Modulos")]
        [SerializeField] private Yuna25DPlayerController yuna;
        [SerializeField] private NandaFairyGuide nanda;
        [SerializeField] private DarkRoomOpeningSequence openingSequence;
        [SerializeField] private SideScroll25DCamera cameraRunner;
        [SerializeField] private DarkRoomBlockoutBuilder blockoutBuilder;

        [Header("Configuracao de Dificuldade (Roteiro 00)")]
        [SerializeField] private bool easyMode = false;
        [SerializeField] private float easyCameraSpeedMultiplier = 0.78f;

        [Header("Estado Atual do Quarto")]
        [SerializeField] private DarkRoomPhase currentPhase = DarkRoomPhase.Phase1Ascent;
        [SerializeField] private DarkRoomPhase currentCheckpoint = DarkRoomPhase.Phase1Ascent;
        [SerializeField] private int currentPlatformIndex = -1;
        [SerializeField] private bool isGameOver = false;
        [SerializeField] private bool isPaused = false;

        [Header("Estado de Cutscene e Dialogo Interativo")]
        [SerializeField] private CutsceneType activeCutscene = CutsceneType.None;
        [SerializeField] private int cutsceneStep = 0;
        [SerializeField] private DialogueLine activeDialogue = null;
        [SerializeField] private string activePoeticLine = null;

        [Header("Musica de Fundo (BGM)")]
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioClip bgmMusicClip;

        [Header("Tipografia Narrativa (Fonte Caveat)")]
        [SerializeField] private Font caveatFont;

        public DarkRoomPhase CurrentPhase => currentPhase;
        public DarkRoomPhase CurrentCheckpoint => currentCheckpoint;
        public int CurrentPlatformIndex => currentPlatformIndex;
        public bool EasyMode { get => easyMode; set => easyMode = value; }
        public bool IsGameOver => isGameOver;
        public bool IsPaused => isPaused;
        public CutsceneType ActiveCutscene => activeCutscene;
        public int CutsceneStep => cutsceneStep;
        public DialogueLine ActiveDialogue => activeDialogue;
        public string ActivePoeticLine => activePoeticLine;

        private DarkRoomPlatformMarker FindPlatform(int index)
        {
            foreach (var marker in FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (marker.spec.index == index) return marker;
            return null;
        }

        private Vector3 ReturnStartPosition
        {
            get
            {
                var poster = FindPlatform(21);
                return new Vector3(poster != null ? poster.transform.position.x :
                    DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms()[11].x, DarkRoomReturnLayout.StartY, 0f);
            }
        }

        public bool ReturnJumpCueVisible => currentPhase == DarkRoomPhase.Phase3ChaoticAscent
            && currentPlatformIndex == 21 && !isGameOver && !isPaused && activeCutscene == CutsceneType.None;

        public Vector3 ReturnJumpCuePosition => FindPlatform(22) != null
            ? FindPlatform(22).GetTopCenterWorld() : ReturnStartPosition + Vector3.left * DarkRoomReturnLayout.FirstPlatformOffset;

        public event Action<int> OnPlatformReached;
        public event Action<DarkRoomPhase> OnPhaseChanged;
        public event Action OnPlotTwistTriggered;
        public event Action OnTruePortalReached;
        public event Action<DialogueLine> OnDialogueDisplayed;
        public event Action<string> OnPoeticLineDisplayed;
        public event Action OnDialogueClosed;

        private void Start()
        {
            if (bgmAudioSource != null && bgmMusicClip != null)
            {
                bgmAudioSource.clip = bgmMusicClip;
                bgmAudioSource.loop = true;
                bgmAudioSource.volume = 0.50f;
                bgmAudioSource.spatialBlend = 0f;
                if (!bgmAudioSource.isPlaying) bgmAudioSource.Play();
            }

            if (openingSequence != null)
            {
                openingSequence.OnOpeningCompleted += HandleOpeningCompleted;
            }
            if (yuna != null)
            {
                yuna.OnGameOverTriggered += HandleGameOver;
                yuna.OnPlatformLanded += NotifyPlatformLanded;
                yuna.OnCinematicFallLanded += HandleCinematicFallLanded;
            }
        }

        private void OnDestroy()
        {
            if (openingSequence != null)
            {
                openingSequence.OnOpeningCompleted -= HandleOpeningCompleted;
            }
            if (yuna != null)
            {
                yuna.OnGameOverTriggered -= HandleGameOver;
                yuna.OnPlatformLanded -= NotifyPlatformLanded;
                yuna.OnCinematicFallLanded -= HandleCinematicFallLanded;
            }
        }

        public void BindPlayer(Yuna25DPlayerController player)
        {
            if (this.yuna != null)
            {
                this.yuna.OnGameOverTriggered -= HandleGameOver;
                this.yuna.OnPlatformLanded -= NotifyPlatformLanded;
                this.yuna.OnCinematicFallLanded -= HandleCinematicFallLanded;
            }
            this.yuna = player;
            if (this.yuna != null)
            {
                this.yuna.OnGameOverTriggered += HandleGameOver;
                this.yuna.OnPlatformLanded += NotifyPlatformLanded;
                this.yuna.OnCinematicFallLanded += HandleCinematicFallLanded;
            }
        }

        private void Update()
        {
            // Verificacao de Pausa
            if (CheckPauseInput())
            {
                TogglePause();
                return;
            }

            if (isPaused) return;

            // Avanco de Dialogo/Cutscene por Input do Jogador
            if (activeCutscene != CutsceneType.None)
            {
                if (CheckConfirmInput())
                {
                    AdvanceCutscene();
                }
                return;
            }

            // Retry de Game Over por Input
            if (isGameOver)
            {
                if (CheckConfirmInput())
                {
                    RetryCurrentCheckpoint();
                }
            }
        }

        public void HandleOpeningCompleted()
        {
            // ETAPA 1: Subida calma e acolhedora ate o Castelinho de Blocos (0 a 9)
            currentPhase = DarkRoomPhase.Phase1Ascent;
            currentCheckpoint = DarkRoomPhase.Phase1Ascent;
            currentPlatformIndex = -1;
            isGameOver = false;

            if (yuna != null)
            {
                yuna.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
                yuna.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
                yuna.SetFloorDeathActive(true);
                yuna.SetRunDirection(1f);
                yuna.AutoRunActive = true;
            }

            if (cameraRunner != null)
            {
                cameraRunner.ConfigureSection(DarkRoomStartLayout.CameraY, -16.0f, 45.0f);
                cameraRunner.SnapToTargetX(yuna != null ? yuna.transform.position.x : DarkRoomStartLayout.StartX, DarkRoomStartLayout.CameraOffset);
                cameraRunner.ScreenCrushActive = true; // Morte por atropelamento da camera ativa desde a Fase 1
                cameraRunner.SetScrollState(true, 1f, DarkRoomStartLayout.RunSpeed);
            }

            if (blockoutBuilder == null)
                blockoutBuilder = FindFirstObjectByType<DarkRoomBlockoutBuilder>();
            if (blockoutBuilder != null)
                blockoutBuilder.ShowTemporalSection(currentPhase, currentPlatformIndex);

            if (yuna != null) yuna.AllowFloorLaunch(yuna.transform.position.x);
            if (nanda != null)
            {
                nanda.SetTargetPlatform(0);
            }
            OnPhaseChanged?.Invoke(currentPhase);
        }

        public void NotifyPlatformLanded(int platformIndex)
        {
            currentPlatformIndex = platformIndex;
            OnPlatformReached?.Invoke(platformIndex);

            if (blockoutBuilder != null)
                blockoutBuilder.ShowTemporalSection(currentPhase, currentPlatformIndex);

            if (currentPhase == DarkRoomPhase.Phase1Ascent)
            {
                if (platformIndex < 9)
                {
                    nanda?.SetTargetPlatform(platformIndex + 1);
                }
                else if (platformIndex == 9)
                {
                    // Chegou no Castelinho de Blocos (Plataforma 9): inicia cutscene com dialogos
                    TriggerCastleRestCutscene();
                }
            }
            else if (currentPhase == DarkRoomPhase.Phase2Escape)
            {
                // A cada plataforma da Fuga (10 a 21), a velocidade e o pulo escalam gradativamente
                int escapeLevel = Mathf.Clamp(platformIndex - 9, 1, 12);
                float progress = (escapeLevel - 1) / 11.0f;
                float speed = Mathf.Lerp(4.0f, 6.0f, progress);
                float jumpH = Mathf.Lerp(2.5f, 3.4f, progress);

                if (yuna != null)
                {
                    yuna.SetRunSpeed(speed);
                    yuna.SetJumpHeight(jumpH);
                }
                if (cameraRunner != null)
                {
                    float camSpeed = easyMode ? (speed * easyCameraSpeedMultiplier) : speed;
                    cameraRunner.SetScrollState(true, 1f, camSpeed);
                }

                if (platformIndex < 21)
                {
                    nanda?.SetTargetPlatform(platformIndex + 1);
                }
                else if (platformIndex == 21)
                {
                    // Chegou no Poster da Porta Falsa na parede: aciona queda cinematica real
                    TriggerPlotTwistPoster();
                }
            }
            else if (currentPhase == DarkRoomPhase.Phase3ChaoticAscent)
            {
                // Corrida Reversa (22 a 37): acentuacao gradual extrema de velocidade e pulo
                int chaoticLevel = Mathf.Clamp(platformIndex - 21, 1, 16);
                float progress = (chaoticLevel - 1) / 15.0f;
                float speed = Mathf.Lerp(4.2f, 7.0f, progress);
                float jumpH = Mathf.Lerp(2.6f, 3.9f, progress);

                if (yuna != null)
                {
                    yuna.SetRunSpeed(speed);
                    yuna.SetJumpHeight(jumpH);
                }
                if (cameraRunner != null)
                {
                    float camSpeed = easyMode ? (speed * easyCameraSpeedMultiplier) : speed;
                    cameraRunner.SetScrollState(true, -1f, camSpeed);
                }

                if (platformIndex < 37)
                {
                    nanda?.SetTargetPlatform(platformIndex + 1);
                }
                else if (platformIndex == 37)
                {
                    TriggerTruePortal();
                }
            }
        }

        // =============================================================
        // ETAPA 1 -> 2: CUTSCENE DO CASTELINHO (Plataforma 9)
        // Dialogos exatos Nanda e Menina avancados por input do jogador
        // =============================================================
        private void TriggerCastleRestCutscene()
        {
            if (cameraRunner != null) cameraRunner.SetScrollState(false);
            if (yuna != null) yuna.AutoRunActive = false;

            activeCutscene = CutsceneType.CastleRest;
            cutsceneStep = 0;

            // Passo 1: Narracao poetica + Fala da Nanda
            activePoeticLine = DarkRoomDialogueScript.PoeticCastle;
            activeDialogue = DarkRoomDialogueScript.CastleNanda;
            nanda?.ApplyMood(NandaMood.Normal);

            OnPoeticLineDisplayed?.Invoke(activePoeticLine);
            OnDialogueDisplayed?.Invoke(activeDialogue);
            Debug.Log("[DarkRoomProgression] Cutscene do Castelo iniciada. Aguardando input do jogador...");
        }

        public void AdvanceCutscene()
        {
            if (activeCutscene == CutsceneType.CastleRest)
            {
                if (cutsceneStep == 0)
                {
                    cutsceneStep = 1;
                    // Passo 2: Fala da Menina
                    activePoeticLine = null;
                    activeDialogue = DarkRoomDialogueScript.CastleYuna;
                    OnDialogueDisplayed?.Invoke(activeDialogue);
                }
                else if (cutsceneStep == 1)
                {
                    // Conclusao da cutscene do Castelo -> Inicia Etapa 2 (A Fuga)
                    activeCutscene = CutsceneType.None;
                    activeDialogue = null;
                    activePoeticLine = null;
                    OnDialogueClosed?.Invoke();
                    StartStage2Escape();
                }
            }
            else if (activeCutscene == CutsceneType.FalseDoorPlotTwist)
            {
                if (cutsceneStep == 0)
                {
                    cutsceneStep = 1;
                    // Passo 2 do Plot Twist: Narracao poetica + Fala de frustracao da Nanda
                    activePoeticLine = DarkRoomDialogueScript.PoeticFalseDoor;
                    activeDialogue = DarkRoomDialogueScript.FalseDoorNanda;
                    nanda?.ApplyMood(NandaMood.Annoyed);
                    OnPoeticLineDisplayed?.Invoke(activePoeticLine);
                    OnDialogueDisplayed?.Invoke(activeDialogue);
                }
                else if (cutsceneStep == 1)
                {
                    // Conclusao do Plot Twist -> Inicia Etapa 3 (Subida Caotica Reversa para a esquerda)
                    activeCutscene = CutsceneType.None;
                    activeDialogue = null;
                    activePoeticLine = null;
                    OnDialogueClosed?.Invoke();
                    StartPhase3AfterPlotTwist();
                }
            }
            else if (activeCutscene == CutsceneType.TruePortalVictory)
            {
                activeCutscene = CutsceneType.None;
                activeDialogue = null;
                activePoeticLine = null;
                OnDialogueClosed?.Invoke();
            }
        }

        public void StartStage2Escape()
        {
            currentPhase = DarkRoomPhase.Phase2Escape;
            currentCheckpoint = DarkRoomPhase.Phase2Escape;
            currentPlatformIndex = 9;
            isGameOver = false;

            if (blockoutBuilder != null)
                blockoutBuilder.ShowTemporalSection(currentPhase, currentPlatformIndex);

            if (yuna != null)
            {
                yuna.SetRunSpeed(4.0f);
                yuna.SetJumpHeight(2.5f);
                yuna.SetRunDirection(1f);
                yuna.SetFloorDeathActive(true);
                yuna.AutoRunActive = true;
            }

            if (cameraRunner != null)
            {
                cameraRunner.ConfigureSection(-1.5f, -16.0f, 45.0f);
                cameraRunner.SnapToTargetX(yuna != null ? yuna.transform.position.x : 43.9f, 3.5f);
                cameraRunner.ScreenCrushActive = true;
                float camSpeed = easyMode ? (4.0f * easyCameraSpeedMultiplier) : 4.0f;
                cameraRunner.SetScrollState(true, 1f, camSpeed);
            }

            nanda?.SetTargetPlatform(10);
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log("[DarkRoomProgression] Etapa 2 iniciada: A Fuga Acelerada!");
        }

        // =============================================================
        // ETAPA 2 -> 3: PLOT TWIST DA PORTA FALSA (Plataforma 21)
        // Queda cinematica real (nao letal), dialogos com borda vermelha e inversao
        // =============================================================
        private void TriggerPlotTwistPoster()
        {
            if (cameraRunner != null) cameraRunner.SetScrollState(false);

            if (blockoutBuilder != null)
                blockoutBuilder.ShowTemporalSection(currentPhase, currentPlatformIndex, falling: true);

            OnPlotTwistTriggered?.Invoke();
            Debug.Log("[DarkRoomProgression] Yuna alcancou o Poster da Porta Falsa na parede! Iniciando queda cinematica real...");

            // Inicia queda cinematica real usando gravidade do CharacterController
            if (yuna != null)
            {
                yuna.StartCinematicFall();
            }
            else
            {
                HandleCinematicFallLanded();
            }
        }

        public void HandleCinematicFallLanded()
        {
            Debug.Log("[DarkRoomProgression] Yuna aterrissou no chao do quarto apos a queda da porta falsa!");

            // Nanda desce ate a menina adormecida/sentada no chao
            nanda?.ReturnToYuna();
            nanda?.ApplyMood(NandaMood.Annoyed);

            activeCutscene = CutsceneType.FalseDoorPlotTwist;
            cutsceneStep = 0;

            // Passo 1 do Plot Twist: Fala de espanto da Menina (moldura vermelha)
            activePoeticLine = null;
            activeDialogue = DarkRoomDialogueScript.FalseDoorYuna;
            OnDialogueDisplayed?.Invoke(activeDialogue);
        }

        public void StartPhase3AfterPlotTwist()
        {
            activeCutscene = CutsceneType.None;
            activeDialogue = null;
            activePoeticLine = null;
            currentPhase = DarkRoomPhase.Phase3ChaoticAscent;
            currentCheckpoint = DarkRoomPhase.Phase3ChaoticAscent;
            currentPlatformIndex = 21; // A plataforma 22 ainda precisa ser conquistada
            isGameOver = false;

            if (yuna != null)
            {
                // Inversao estrita para a ESQUERDA
                yuna.SetRunDirection(-1f);
                yuna.SetRunSpeed(DarkRoomReturnLayout.RunSpeed);
                yuna.SetJumpHeight(DarkRoomReturnLayout.JumpHeight);
                yuna.ResetLastPlatformIndex();
                yuna.AllowFloorLaunch(yuna.transform.position.x);
                yuna.SetFloorDeathActive(true); // Quedas voltam a ser letais
                yuna.AutoRunActive = true;
            }

            if (cameraRunner != null)
            {
                cameraRunner.ConfigureSection(DarkRoomReturnLayout.CameraY, -16.0f, 45.0f);
                cameraRunner.SnapToTargetX(yuna != null ? yuna.transform.position.x : ReturnStartPosition.x, DarkRoomReturnLayout.CameraOffset);
                cameraRunner.ScreenCrushActive = true;
                float camSpeed = easyMode ? (DarkRoomReturnLayout.RunSpeed * easyCameraSpeedMultiplier) : DarkRoomReturnLayout.RunSpeed;
                cameraRunner.SetScrollState(true, -1f, camSpeed);
            }

            if (blockoutBuilder != null)
                blockoutBuilder.ShowTemporalSection(currentPhase, currentPlatformIndex);

            nanda?.SetTargetPlatform(22);
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log("[DarkRoomProgression] Etapa 3 iniciada: Subida Caotica Reversa para a esquerda!");
        }

        // =============================================================
        // ETAPA 3 -> FIM: O VERDADEIRO PORTAL DOS SONHOS (Plataforma 37)
        // =============================================================
        private void TriggerTruePortal()
        {
            if (cameraRunner != null) cameraRunner.SetScrollState(false);
            if (yuna != null) yuna.AutoRunActive = false;

            activeCutscene = CutsceneType.TruePortalVictory;
            cutsceneStep = 0;
            activePoeticLine = DarkRoomDialogueScript.PoeticTruePortal;
            activeDialogue = null;
            nanda?.ApplyMood(NandaMood.Happy);

            OnPoeticLineDisplayed?.Invoke(activePoeticLine);
            OnTruePortalReached?.Invoke();
            Debug.Log("[DarkRoomProgression] O Verdadeiro Portal dos Sonhos foi alcancado! Fim do Quarto Escuro.");
        }

        // =============================================================
        // CHECKPOINTS E RETRY
        // =============================================================
        private void HandleGameOver()
        {
            isGameOver = true;
            if (cameraRunner != null) cameraRunner.SetScrollState(false);
            Debug.Log($"[DarkRoomProgression] Game Over acionado na fase {currentPhase}. Checkpoint atual: {currentCheckpoint}");
        }

        public void RetryCurrentCheckpoint()
        {
            isGameOver = false;
            activeCutscene = CutsceneType.None;
            activeDialogue = null;
            activePoeticLine = null;

            if (currentCheckpoint == DarkRoomPhase.Phase1Ascent)
            {
                currentPhase = DarkRoomPhase.Phase1Ascent;
                currentPlatformIndex = -1;

                if (yuna != null)
                {
                    yuna.ResetPlayer(DarkRoomStartLayout.Position, 1f);
                    yuna.AllowFloorLaunch(DarkRoomStartLayout.StartX);
                    yuna.SetRunSpeed(DarkRoomStartLayout.RunSpeed);
                    yuna.SetJumpHeight(DarkRoomStartLayout.JumpHeight);
                    yuna.SetFloorDeathActive(true);
                }

                if (cameraRunner != null)
                {
                    cameraRunner.ConfigureSection(DarkRoomStartLayout.CameraY, -16.0f, 45.0f);
                    cameraRunner.SnapToTargetX(DarkRoomStartLayout.StartX, DarkRoomStartLayout.CameraOffset);
                    cameraRunner.ScreenCrushActive = true;
                    cameraRunner.SetScrollState(true, 1f, DarkRoomStartLayout.RunSpeed);
                }

                nanda?.SetTargetPlatform(0);
            }
            else if (currentCheckpoint == DarkRoomPhase.Phase2Escape)
            {
                currentPhase = DarkRoomPhase.Phase2Escape;
                currentPlatformIndex = 9;

                if (yuna != null)
                {
                    // Retoma sobre o Castelinho de Blocos
                    yuna.ResetPlayer(new Vector3(39.0f, -2.8f, 0f), 1f);
                    yuna.RestoreRouteSupport(9);
                    yuna.SetRunSpeed(4.0f);
                    yuna.SetJumpHeight(2.5f);
                    yuna.SetFloorDeathActive(true);
                }

                if (cameraRunner != null)
                {
                    cameraRunner.ConfigureSection(-1.5f, -16.0f, 45.0f);
                    cameraRunner.SnapToTargetX(39.0f, 3.5f);
                    cameraRunner.ScreenCrushActive = true;
                    float camSpeed = easyMode ? (4.0f * easyCameraSpeedMultiplier) : 4.0f;
                    cameraRunner.SetScrollState(true, 1f, camSpeed);
                }

                nanda?.SetTargetPlatform(10);
            }
            else if (currentCheckpoint == DarkRoomPhase.Phase3ChaoticAscent)
            {
                currentPhase = DarkRoomPhase.Phase3ChaoticAscent;
                currentPlatformIndex = 21;

                if (yuna != null)
                {
                    // Retoma no chao proximo a parede da porta falsa, virada para a esquerda
                    var returnStart = ReturnStartPosition;
                    foreach (var marker in FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                        if (marker.spec.index == 21) returnStart.x = marker.transform.position.x;
                    yuna.ResetPlayer(returnStart, -1f);
                    yuna.SetRunSpeed(DarkRoomReturnLayout.RunSpeed);
                    yuna.SetJumpHeight(DarkRoomReturnLayout.JumpHeight);
                    yuna.AllowFloorLaunch(returnStart.x);
                    yuna.SetFloorDeathActive(true);
                }

                if (cameraRunner != null)
                {
                    cameraRunner.ConfigureSection(DarkRoomReturnLayout.CameraY, -16.0f, 45.0f);
                    cameraRunner.SnapToTargetX(yuna != null ? yuna.transform.position.x : ReturnStartPosition.x, DarkRoomReturnLayout.CameraOffset);
                    cameraRunner.ScreenCrushActive = true;
                    float camSpeed = easyMode ? (DarkRoomReturnLayout.RunSpeed * easyCameraSpeedMultiplier) : DarkRoomReturnLayout.RunSpeed;
                    cameraRunner.SetScrollState(true, -1f, camSpeed);
                }

                nanda?.SetTargetPlatform(22);
            }

            if (blockoutBuilder != null)
                blockoutBuilder.ShowTemporalSection(currentPhase, currentPlatformIndex);

            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log($"[DarkRoomProgression] Retry executado! Reiniciando a partir do checkpoint {currentCheckpoint}");
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            if (bgmAudioSource != null)
            {
                if (isPaused) bgmAudioSource.Pause();
                else bgmAudioSource.UnPause();
            }
            Debug.Log($"[DarkRoomProgression] Pausa: {(isPaused ? "Ativada" : "Desativada")}");
        }

        // =============================================================
        // HELPERS DE ENTRADA (Teclado, Mouse, Toque, Gamepad)
        // =============================================================
        private bool CheckConfirmInput()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.upArrowKey.wasPressedThisFrame ||
                    Keyboard.current.wKey.wasPressedThisFrame)
                {
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        private bool CheckPauseInput()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    return true;
                }
            }

            if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        public void ConfigureUiFont(Font font)
        {
            caveatFont = font;
        }

        // =============================================================
        // RENDERIZACAO DE INTERFACE EM TEMPO REAL (OnGUI)
        // =============================================================
        private void OnGUI()
        {
            // 1. HUD de Fase e Velocidade no Topo Direito
            DrawStageHud();
            if (currentPlatformIndex < 0 && !isGameOver && activeCutscene == CutsceneType.None)
            {
                var camera = UnityEngine.Camera.main;
                var target = nanda != null ? nanda.transform.position : new Vector3(2f, -5.3f, 0f);
                Vector3 screen = camera != null ? camera.WorldToScreenPoint(target) : new Vector3(Screen.width * 0.5f, 140f, 1f);
                if (screen.z > 0f)
                {
                    float width = Mathf.Min(380f, Screen.width - 20f);
                    var rect = new Rect(Mathf.Clamp(screen.x - width * 0.5f, 10f, Screen.width - width - 10f),
                        Mathf.Clamp(Screen.height - screen.y - 65f, 50f, Screen.height - 90f), width, 82f);
                    var style = new GUIStyle(GUI.skin.box) { wordWrap = true, fontSize = 15, alignment = TextAnchor.MiddleCenter };
                    style.normal.textColor = new Color(1.0f, 0.98f, 0.92f);
                    bool nearTakeoff = yuna != null && yuna.transform.position.x >= DarkRoomStartLayout.FirstPlatformX - 3.4f;
                    string cue = nearTakeoff ? "✦ NANDA: Pule agora! ↓" : "✦ NANDA: Prepare o primeiro salto →";
                    GUI.Box(rect, cue + "\nAperte uma vez: Espaço / W / Clique / Toque.\nO arco do pulo é completo; não precisa segurar.", style);
                }
            }

            if (ReturnJumpCueVisible)
            {
                var camera = UnityEngine.Camera.main;
                Vector3 screen = camera != null ? camera.WorldToScreenPoint(ReturnJumpCuePosition) : Vector3.back;
                if (screen.z > 0f && screen.x >= 0f && screen.x <= Screen.width && screen.y >= 0f && screen.y <= Screen.height)
                {
                    float width = Mathf.Min(360f, Screen.width - 20f);
                    var rect = new Rect(Mathf.Clamp(screen.x - width * 0.5f, 10f, Screen.width - width - 10f),
                        Mathf.Clamp(Screen.height - screen.y - 65f, 50f, Screen.height - 90f), width, 68f);
                    var style = new GUIStyle(GUI.skin.box) { fontSize = 16, wordWrap = true, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                    style.normal.textColor = new Color(1.0f, 0.92f, 0.50f);
                    GUI.Box(rect, "✦ NANDA: " + DarkRoomReturnLayout.JumpCue + "! ✦\nAperte uma vez: Espaço / W / Clique / Toque.\nNão precisa segurar.", style);
                }
            }

            // 2. Caixa de Dialogo vitoriana ou Narracao Poetica
            if (activeCutscene != CutsceneType.None)
            {
                DrawDialogueBox();
            }

            // 3. Tela de Game Over e Retry
            if (isGameOver)
            {
                DrawGameOverScreen();
            }

            // 4. Indicador de Pausa
            if (isPaused)
            {
                DrawPauseBanner();
            }
        }

        private void DrawStageHud()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 13;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleRight;
            style.normal.textColor = new Color(0.96f, 0.94f, 0.88f);

            string hudText = "";
            if (currentPhase == DarkRoomPhase.Phase1Ascent)
            {
                hudText = $"✦ QUARTO ESCURO ✦ Subida ao Castelo • Plat {Mathf.Max(0, currentPlatformIndex)}/9";
            }
            else if (currentPhase == DarkRoomPhase.Phase2Escape)
            {
                int lvl = Mathf.Clamp(currentPlatformIndex - 9, 1, 12);
                hudText = $"⚡ A FUGA ACELERADA • Pulo {lvl}/12 • Plat {currentPlatformIndex}/21";
            }
            else if (currentPhase == DarkRoomPhase.Phase3ChaoticAscent)
            {
                int lvl = Mathf.Clamp(currentPlatformIndex - 21, 1, 16);
                hudText = $"🌪️ SUBIDA CAÓTICA • Nível {lvl}/15 • Plat {currentPlatformIndex}/37 (← Reverso)";
            }

            GUI.Label(new Rect(Screen.width - 460, 15, 445, 30), hudText, style);
        }

        private void DrawDialogueBox()
        {
            float boxWidth = Mathf.Min(780, Screen.width - 40);
            float boxHeight = 145;
            float boxX = (Screen.width - boxWidth) * 0.5f;
            float boxY = Screen.height - boxHeight - 35;

            // Narracao poetica sobre a cena com a fonte manuscrita Caveat
            if (!string.IsNullOrEmpty(activePoeticLine))
            {
                GUIStyle poeticStyle = new GUIStyle(GUI.skin.label);
                if (caveatFont != null) poeticStyle.font = caveatFont;
                poeticStyle.fontSize = 28;
                poeticStyle.fontStyle = FontStyle.Normal;
                poeticStyle.alignment = TextAnchor.MiddleCenter;
                poeticStyle.normal.textColor = new Color(1.0f, 0.98f, 0.90f);

                // Sombra da narracao para contraste e legibilidade impecavel
                GUIStyle shadowStyle = new GUIStyle(poeticStyle);
                shadowStyle.normal.textColor = new Color(0.04f, 0.03f, 0.08f, 0.85f);
                GUI.Label(new Rect(boxX + 2, boxY - 58, boxWidth, 50), $"\"{activePoeticLine}\"", shadowStyle);
                GUI.Label(new Rect(boxX, boxY - 60, boxWidth, 50), $"\"{activePoeticLine}\"", poeticStyle);
            }

            if (activeDialogue == null) return;

            // Fundo da caixa estilizada
            Color origColor = GUI.color;
            Color borderColor = activeDialogue.isPlotTwistStyle ? new Color(0.85f, 0.25f, 0.25f) : new Color(0.88f, 0.72f, 0.32f);

            // Borda externa dourada de filigrana
            GUI.color = borderColor;
            GUI.Box(new Rect(boxX - 3, boxY - 3, boxWidth + 6, boxHeight + 6), GUIContent.none);

            // Interior aveludado noturno
            GUI.color = new Color(0.06f, 0.05f, 0.12f, 0.94f);
            GUI.Box(new Rect(boxX, boxY, boxWidth, boxHeight), GUIContent.none);
            GUI.color = origColor;

            // Rótulo do personagem com icone tematico
            GUIStyle speakerStyle = new GUIStyle(GUI.skin.label);
            speakerStyle.fontSize = 17;
            speakerStyle.fontStyle = FontStyle.Bold;
            speakerStyle.normal.textColor = activeDialogue.speaker == SpeakerId.Nanda ? new Color(1.0f, 0.88f, 0.35f) : new Color(1.0f, 0.68f, 0.75f);

            string speakerName = activeDialogue.speaker == SpeakerId.Nanda ? "✦ NANDA" : "✿ MENINA";
            GUI.Label(new Rect(boxX + 25, boxY + 16, 220, 26), speakerName, speakerStyle);

            // Texto da fala
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 17;
            textStyle.normal.textColor = new Color(0.98f, 0.97f, 0.92f);
            textStyle.wordWrap = true;
            GUI.Label(new Rect(boxX + 25, boxY + 48, boxWidth - 50, 58), activeDialogue.text, textStyle);

            // Indicador elegante para avancar
            GUIStyle continueStyle = new GUIStyle(GUI.skin.label);
            continueStyle.fontSize = 13;
            continueStyle.fontStyle = FontStyle.Italic;
            continueStyle.alignment = TextAnchor.LowerRight;
            continueStyle.normal.textColor = new Color(0.96f, 0.82f, 0.35f);
            GUI.Label(new Rect(boxX + 25, boxY + boxHeight - 28, boxWidth - 50, 22), "[ Espaço / Clique / Botão A para continuar ▾ ]", continueStyle);
        }

        private void DrawGameOverScreen()
        {
            float w = 520;
            float h = 200;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            Color origColor = GUI.color;
            // Borda rubi envelhecida
            GUI.color = new Color(0.72f, 0.22f, 0.25f);
            GUI.Box(new Rect(x - 2, y - 2, w + 4, h + 4), GUIContent.none);

            // Fundo escuro aveludado
            GUI.color = new Color(0.04f, 0.03f, 0.09f, 0.94f);
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.color = origColor;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.92f, 0.32f, 0.32f);
            GUI.Label(new Rect(x, y + 18, w, 28), "✦ A PENUMBRA LEVOU YUNA... ✦", titleStyle);

            // Frase poetica de encorajamento com a fonte Caveat
            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            if (caveatFont != null) subStyle.font = caveatFont;
            subStyle.fontSize = 25;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = new Color(0.98f, 0.96f, 0.88f);
            GUI.Label(new Rect(x + 20, y + 54, w - 40, 48), "\"Mas quem não desiste sempre encontra o caminho de volta.\"", subStyle);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 15;
            btnStyle.fontStyle = FontStyle.Bold;
            if (GUI.Button(new Rect(x + (w - 280) * 0.5f, y + 124, 280, 46), "◆ TENTAR DE NOVO (Espaço / A) ◆", btnStyle))
            {
                RetryCurrentCheckpoint();
            }
        }

        private void DrawPauseBanner()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 22;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = new Color(1.0f, 0.88f, 0.35f);
            GUI.Label(new Rect((Screen.width - 260) * 0.5f, 60, 260, 52), "⏸ JOGO PAUSADO [P]", style);
        }
    }
}
