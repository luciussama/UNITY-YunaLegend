using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using YunaLegend.Arena.Core;
using YunaLegend.Qa.Runtime;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Composition Root da Fase 3 (Arena Imaginaria): conecta simulacao 60 Hz,
    /// views 2.5D da menina e dos brinquedos, camera dinamica, HUD, audio e Nanda azul.
    /// </summary>
    public sealed class ArenaStageBootstrap : MonoBehaviour
    {
        [SerializeField] private ArenaSpriteCatalog spriteCatalog;
        [SerializeField] private ArenaActorView playerView;
        [SerializeField] private ArenaActorView enemyView;
        [SerializeField] private ArenaCameraController cameraController;
        [SerializeField] private ArenaHudController hudController;
        [SerializeField] private ArenaAudioController audioController;
        [SerializeField] private Transform nandaFairyTransform;

        private ArenaCombatSimulation simulation;
        private Transform nandaWingL;
        private Transform nandaWingR;
        private Transform nandaHead;
        private bool nandaBonesCached;
        private float jumpBufferTimer;
        private bool pendingAttack;
        private bool pendingSpecial;

        public void Configure(
            ArenaSpriteCatalog catalog,
            ArenaActorView player,
            ArenaActorView enemy,
            ArenaCameraController camCtrl,
            ArenaHudController hudCtrl,
            ArenaAudioController audioCtrl,
            Transform nandaTransform)
        {
            spriteCatalog = catalog;
            playerView = player;
            enemyView = enemy;
            cameraController = camCtrl;
            hudController = hudCtrl;
            audioController = audioCtrl;
            nandaFairyTransform = nandaTransform;
        }

        private void Awake()
        {
            bool easyMode = PlayerPrefs.GetInt("YunaLegend.EasyMode", 0) == 1;
            bool isMobileTouch = Application.isMobilePlatform || Touchscreen.current != null;
            simulation = new ArenaCombatSimulation(easyMode, isMobileTouch);
            if (WebQaRuntime.Current.ArenaFinalWave)
            {
                simulation.PrepareFinalWaveForQa(enemyHp: 2);
                WebQaRuntime.LogEvent("arena_qa_final_wave", $"wave={simulation.WaveIndex + 1}/{simulation.TotalWaves} enemy_hp={simulation.EnemyHp}");
            }

            SubscribeEvents();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            UnsubscribeEvents();
        }

        private void Update()
        {
            if (simulation == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                HandlePauseToggled(hudController == null || !hudController.IsPaused);
                return;
            }

            if (hudController != null && hudController.IsPaused)
            {
                return;
            }

            ArenaInputState rawInput = WebQaRuntime.Current.AutoPlay
                ? ArenaQaPilot.ComputeInput(simulation)
                : ReadInput();
            if (rawInput.JumpPressed)
            {
                jumpBufferTimer = 0.14f;
            }
            else if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
            }

            if (rawInput.AttackPressed) pendingAttack = true;
            if (rawInput.SpecialPressed) pendingSpecial = true;

            var bufferedInput = new ArenaInputState
            {
                MoveX = rawInput.MoveX,
                CrouchHeld = rawInput.CrouchHeld,
                JumpPressed = jumpBufferTimer > 0f,
                AttackPressed = pendingAttack,
                SpecialPressed = pendingSpecial
            };

            int ticks = simulation.Step(Time.deltaTime, bufferedInput);
            if (ticks > 0)
            {
                if (!simulation.PlayerOnGround)
                {
                    jumpBufferTimer = 0f;
                }

                pendingAttack = false;
                pendingSpecial = false;
            }

            if (playerView != null)
            {
                playerView.UpdatePlayerVisual(simulation, spriteCatalog, bufferedInput.MoveX, Time.deltaTime);
            }

            if (enemyView != null)
            {
                enemyView.UpdateEnemyVisual(simulation, spriteCatalog, Time.deltaTime);
            }

            if (cameraController != null)
            {
                cameraController.UpdateFraming(simulation, Time.deltaTime);
            }

            if (hudController != null)
            {
                hudController.Refresh(simulation);
            }

            UpdateNandaHover();
        }

        private ArenaInputState ReadInput()
        {
            float moveX = hudController != null ? hudController.TouchMove.x : 0f;
            bool jump = hudController != null && hudController.ConsumeTouchJump();
            bool crouch = hudController != null && hudController.TouchCrouchHeld;
            bool attack = hudController != null && hudController.ConsumeTouchAttack();
            bool special = hudController != null && hudController.ConsumeTouchSpecial();

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveX -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveX += 1f;
                if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) jump = true;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) crouch = true;
                if (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) attack = true;
                if (kb.rKey.wasPressedThisFrame) special = true;
            }

            var gp = Gamepad.current;
            if (gp != null)
            {
                float dpadX = gp.dpad.x.ReadValue();
                if (Mathf.Abs(dpadX) > 0.1f) moveX = dpadX;
                if (gp.buttonSouth.wasPressedThisFrame) jump = true;
                if (gp.dpad.down.isPressed) crouch = true;
                if (gp.buttonWest.wasPressedThisFrame) attack = true;
                if (gp.buttonNorth.wasPressedThisFrame) special = true;
            }

            return new ArenaInputState
            {
                MoveX = Mathf.Clamp(moveX, -1f, 1f),
                JumpPressed = jump,
                CrouchHeld = crouch,
                AttackPressed = attack,
                SpecialPressed = special
            };
        }

        private void UpdateNandaHover()
        {
            if (nandaFairyTransform == null || simulation == null)
            {
                return;
            }

            Vector3 playerPos = ArenaCoordinateMapper.ToWorldPosition(simulation.PlayerX, simulation.PlayerY);
            Vector3 enemyPos = ArenaCoordinateMapper.ToWorldPosition(simulation.EnemyX, simulation.EnemyY);
            Vector3 desired = playerPos + new Vector3(-simulation.PlayerFacing * 1.15f, 2.05f + Mathf.Sin(Time.time * 3.1f) * 0.12f, 0.45f);
            nandaFairyTransform.position = Vector3.Lerp(nandaFairyTransform.position, desired, Time.deltaTime * 5f);

            Vector3 lookDir = enemyPos - nandaFairyTransform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.002f)
            {
                nandaFairyTransform.rotation = Quaternion.Slerp(
                    nandaFairyTransform.rotation,
                    Quaternion.LookRotation(lookDir.normalized, Vector3.up),
                    Time.deltaTime * 6f);
            }

            if (!nandaBonesCached)
            {
                nandaBonesCached = true;
                var rig = nandaFairyTransform.Find("Nanda_FairyBody/Nanda_GlbMesh/Bone_Torso");
                if (rig != null)
                {
                    nandaWingL = rig.Find("Bone_Wing_L");
                    nandaWingR = rig.Find("Bone_Wing_R");
                    nandaHead = rig.Find("Bone_Head");
                }
            }

            float flap = Mathf.Sin(Time.time * 20f) * 28f;
            if (nandaWingL != null) nandaWingL.localRotation = Quaternion.Euler(0f, 10f + flap, 0f);
            if (nandaWingR != null) nandaWingR.localRotation = Quaternion.Euler(0f, -10f - flap, 0f);
            if (nandaHead != null) nandaHead.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 2.4f) * 8f, 0f);
        }

        private void SubscribeEvents()
        {
            if (simulation != null)
            {
                simulation.OnSwordSwing += HandleSwordSwing;
                simulation.OnSwordHit += HandleSwordHit;
                simulation.OnAttackBlocked += HandleAttackBlocked;
                simulation.OnPlayerDamaged += HandlePlayerDamaged;
                simulation.OnEnemyAttackStarted += HandleEnemyAttackStarted;
                simulation.OnEnemyAttackActive += HandleEnemyAttackActive;
                simulation.OnSpecialPhaseChanged += HandleSpecialPhaseChanged;
                simulation.OnBattleEnded += HandleBattleEnded;
            }

            if (hudController != null)
            {
                hudController.OnPauseToggled += HandlePauseToggled;
                hudController.OnMuteToggled += HandleMuteToggled;
                hudController.OnRestartRequested += HandleRestartRequested;
                hudController.OnReturnToTitleRequested -= HandleReturnToTitleRequested;
                hudController.OnReturnToTitleRequested += HandleReturnToTitleRequested;
                hudController.OnNextStageRequested += HandleReturnToTitleRequested;
            }
        }

        private void UnsubscribeEvents()
        {
            if (simulation != null)
            {
                simulation.OnSwordSwing -= HandleSwordSwing;
                simulation.OnSwordHit -= HandleSwordHit;
                simulation.OnAttackBlocked -= HandleAttackBlocked;
                simulation.OnPlayerDamaged -= HandlePlayerDamaged;
                simulation.OnEnemyAttackStarted -= HandleEnemyAttackStarted;
                simulation.OnEnemyAttackActive -= HandleEnemyAttackActive;
                simulation.OnSpecialPhaseChanged -= HandleSpecialPhaseChanged;
                simulation.OnBattleEnded -= HandleBattleEnded;
            }

            if (hudController != null)
            {
                hudController.OnPauseToggled -= HandlePauseToggled;
                hudController.OnMuteToggled -= HandleMuteToggled;
                hudController.OnRestartRequested -= HandleRestartRequested;
                hudController.OnReturnToTitleRequested -= HandleReturnToTitleRequested;
                hudController.OnNextStageRequested -= HandleReturnToTitleRequested;
            }
        }

        private void HandleSwordSwing()
        {
            playerView?.TriggerSlashArc();
            audioController?.PlaySwordSwing();
        }

        private void HandleSwordHit(bool validHit)
        {
            if (validHit)
            {
                audioController?.StopEnemyAttackAudio();
                enemyView?.TriggerImpactFeedback(new Color(1f, 0.94f, 0.68f));
                cameraController?.AddShake(0.22f, 0.16f);
            }
            else
            {
                enemyView?.TriggerImpactFeedback(new Color(0.68f, 0.92f, 1f));
                audioController?.PlayBlock();
                cameraController?.AddShake(0.14f, 0.12f);
            }
        }

        private void HandleAttackBlocked()
        {
            playerView?.TriggerImpactFeedback(new Color(0.68f, 0.92f, 1f));
            audioController?.PlayBlock();
            cameraController?.AddShake(0.16f, 0.13f);
        }

        private void HandlePlayerDamaged()
        {
            playerView?.TriggerImpactFeedback(new Color(1f, 0.42f, 0.32f));
            cameraController?.AddShake(0.28f, 0.22f);
        }

        private void HandleEnemyAttackStarted(ArenaEnemyId enemyId, ArenaAttackKind kind)
        {
            audioController?.PlayEnemyAttack(enemyId, kind);
        }

        private void HandleEnemyAttackActive(ArenaEnemyId enemyId, ArenaAttackKind kind)
        {
            audioController?.PlayEnemyAttackActive(enemyId, kind);
        }

        private void HandleSpecialPhaseChanged(ArenaSpecialPhase phase)
        {
            if (phase == ArenaSpecialPhase.Launch)
            {
                audioController?.PlaySpecialLaunch();
                cameraController?.AddShake(0.26f, 0.30f);
            }
            else if (phase == ArenaSpecialPhase.Impact)
            {
                enemyView?.TriggerImpactFeedback(Color.white);
                cameraController?.AddShake(0.36f, 0.28f);
            }
        }

        private void HandleBattleEnded(ArenaBattleOutcome outcome)
        {
            if (outcome == ArenaBattleOutcome.Victory)
            {
                WebQaRuntime.LogEvent("arena_victory_cut_to_bearchase", "bubble=false");
                Time.timeScale = 1f;
                SceneManager.LoadScene("BearChase_Gameplay");
                return;
            }

            WebQaRuntime.LogEvent("arena_defeat", $"wave={simulation.WaveIndex + 1}/{simulation.TotalWaves}");
            hudController?.ShowOutcome(outcome);
        }

        private void HandlePauseToggled(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
            hudController?.SetPausedState(paused);
            audioController?.SetPaused(paused);
        }

        private void HandleMuteToggled()
        {
            audioController?.ToggleMute();
        }

        private void HandleRestartRequested()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void HandleReturnToTitleRequested()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("TitleScreen");
        }

    }
}
