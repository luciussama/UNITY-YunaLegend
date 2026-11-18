using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using YunaLegend.Arena.Core;

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
            simulation = new ArenaCombatSimulation(easyMode);
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

            ArenaInputState input = ReadInput();
            simulation.Step(Time.deltaTime, input);

            if (playerView != null)
            {
                playerView.UpdatePlayerVisual(simulation, spriteCatalog, input.MoveX, Time.deltaTime);
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
            Vector3 desired = playerPos + new Vector3(-simulation.PlayerFacing * 1.15f, 2.05f + Mathf.Sin(Time.time * 3.1f) * 0.12f, 0.25f);
            nandaFairyTransform.position = Vector3.Lerp(nandaFairyTransform.position, desired, Time.deltaTime * 5f);
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
                simulation.OnSpecialPhaseChanged += HandleSpecialPhaseChanged;
                simulation.OnBattleEnded += HandleBattleEnded;
            }

            if (hudController != null)
            {
                hudController.OnPauseToggled += HandlePauseToggled;
                hudController.OnMuteToggled += HandleMuteToggled;
                hudController.OnRestartRequested += HandleRestartRequested;
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
                enemyView?.TriggerImpactFeedback(new Color(1f, 0.88f, 0.45f));
                cameraController?.AddShake(0.08f, 0.12f);
            }
            else
            {
                enemyView?.TriggerImpactFeedback(new Color(0.65f, 0.88f, 1f));
                audioController?.PlayBlock();
                cameraController?.AddShake(0.05f, 0.08f);
            }
        }

        private void HandleAttackBlocked()
        {
            playerView?.TriggerImpactFeedback(new Color(0.65f, 0.88f, 1f));
            audioController?.PlayBlock();
            cameraController?.AddShake(0.06f, 0.1f);
        }

        private void HandlePlayerDamaged()
        {
            playerView?.TriggerImpactFeedback(new Color(1f, 0.35f, 0.35f));
            cameraController?.AddShake(0.14f, 0.18f);
        }

        private void HandleEnemyAttackStarted(ArenaEnemyId enemyId, ArenaAttackKind kind)
        {
            audioController?.PlayEnemyAttack(enemyId, kind);
        }

        private void HandleSpecialPhaseChanged(ArenaSpecialPhase phase)
        {
            if (phase == ArenaSpecialPhase.Launch)
            {
                audioController?.PlaySpecialLaunch();
                cameraController?.AddShake(0.18f, 0.35f);
            }
        }

        private void HandleBattleEnded(ArenaBattleOutcome outcome)
        {
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
