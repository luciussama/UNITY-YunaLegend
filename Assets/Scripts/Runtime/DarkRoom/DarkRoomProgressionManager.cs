using System;
using UnityEngine;
using YunaLegend.Camera25D;

namespace YunaLegend.DarkRoom
{
    public class DarkRoomProgressionManager : MonoBehaviour
    {
        [Header("Atores e Modulos")]
        [SerializeField] private Yuna25DPlayerController yuna;
        [SerializeField] private NandaFairyGuide nanda;
        [SerializeField] private DarkRoomOpeningSequence openingSequence;
        [SerializeField] private SideScroll25DCamera cameraRunner;

        [Header("Configuracao de Dificuldade (Roteiro 00)")]
        [SerializeField] private bool easyMode = false;
        [SerializeField] private float easyCameraSpeedMultiplier = 0.78f;

        [Header("Estado Atual do Quarto")]
        [SerializeField] private DarkRoomPhase currentPhase = DarkRoomPhase.Phase1Ascent;
        [SerializeField] private int currentPlatformIndex = -1;
        [SerializeField] private bool isEscaping = false;

        public DarkRoomPhase CurrentPhase => currentPhase;
        public int CurrentPlatformIndex => currentPlatformIndex;
        public bool EasyMode { get => easyMode; set => easyMode = value; }

        public event Action<int> OnPlatformReached;
        public event Action<DarkRoomPhase> OnPhaseChanged;
        public event Action OnPlotTwistTriggered;
        public event Action OnTruePortalReached;

        private void Start()
        {
            if (openingSequence != null)
            {
                openingSequence.OnOpeningCompleted += HandleOpeningCompleted;
            }
            if (yuna != null)
            {
                yuna.OnGameOverTriggered += HandleGameOver;
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
            }
        }

        private void HandleOpeningCompleted()
        {
            // A corrida e o avanço da camera comecam imediatamente apos a abertura
            currentPhase = DarkRoomPhase.Phase1Ascent;
            currentPlatformIndex = 0;

            if (yuna != null)
            {
                yuna.AutoRunActive = true;
                yuna.SetRunDirection(1f);
            }

            if (cameraRunner != null)
            {
                cameraRunner.SetScrollState(true, 1f, 5.2f);
            }

            if (nanda != null)
            {
                nanda.SetTargetPlatform(1);
            }
            OnPhaseChanged?.Invoke(currentPhase);
        }

        public void NotifyPlatformLanded(int platformIndex)
        {
            currentPlatformIndex = platformIndex;
            OnPlatformReached?.Invoke(platformIndex);

            if (currentPhase == DarkRoomPhase.Phase1Ascent)
            {
                if (platformIndex < 21)
                {
                    nanda?.SetTargetPlatform(platformIndex + 1);
                }
                else if (platformIndex == 21)
                {
                    // Chegou na Plataforma 21: O Poster na Parede (Porta Falsa)
                    TriggerPlotTwistPoster();
                }
            }
            else if (currentPhase == DarkRoomPhase.Phase3ChaoticAscent)
            {
                if (platformIndex < 37)
                {
                    nanda?.SetTargetPlatform(platformIndex + 1);
                }
                else if (platformIndex == 37)
                {
                    // Chegou no Verdadeiro Portal dos Sonhos
                    TriggerTruePortal();
                }
            }
        }

        private void TriggerPlotTwistPoster()
        {
            // O poster cede, a camera para temporariamente
            if (cameraRunner != null) cameraRunner.SetScrollState(false);
            if (yuna != null) yuna.AutoRunActive = false;

            OnPlotTwistTriggered?.Invoke();
            Debug.Log("[DarkRoomProgression] Yuna bateu no Poster da Porta Falsa na parede e caiu!");

            // Inicia automaticamente a corrida caotica reversa apos a queda
            Invoke(nameof(StartPhase3AfterPlotTwist), 3.5f);
        }

        public void StartPhase3AfterPlotTwist()
        {
            currentPhase = DarkRoomPhase.Phase3ChaoticAscent;
            currentPlatformIndex = 22; // Primeira da volta

            if (yuna != null)
            {
                // Vira para a esquerda e corre em sentido inverso
                yuna.SetRunDirection(-1f);
                yuna.AutoRunActive = true;
            }

            if (cameraRunner != null)
            {
                cameraRunner.SetScrollState(true, -1f, 5.2f);
            }

            nanda?.SetTargetPlatform(22);
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log("[DarkRoomProgression] Corrida caotica reversa iniciada para a esquerda!");
        }

        private void TriggerTruePortal()
        {
            if (cameraRunner != null) cameraRunner.SetScrollState(false);
            if (yuna != null) yuna.AutoRunActive = false;

            OnTruePortalReached?.Invoke();
            Debug.Log("[DarkRoomProgression] O Verdadeiro Portal dos Sonhos foi alcancado! Fim do Quarto Escuro.");
        }

        private void HandleGameOver()
        {
            if (cameraRunner != null) cameraRunner.SetScrollState(false);
            Debug.Log("[DarkRoomProgression] Game Over acionado.");
        }
    }
}
