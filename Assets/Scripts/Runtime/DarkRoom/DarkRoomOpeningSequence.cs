using System;
using System.Collections;
using UnityEngine;
using YunaLegend.Dialogue;

namespace YunaLegend.DarkRoom
{
    public class DarkRoomOpeningSequence : MonoBehaviour
    {
        [Header("Atores da Cena")]
        [SerializeField] private Yuna25DPlayerController yuna;
        [SerializeField] private NandaFairyGuide nanda;

        [Header("Parametros da Abertura (42.5 segundos)")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool allowSkip = true;

        [Header("Eventos e Callbacks")]
        public Action OnOpeningStarted;
        public Action<string> OnPoeticLineDisplayed;
        public Action<DialogueLine> OnDialogueDisplayed;
        public Action OnOpeningCompleted;

        private Coroutine sequenceCoroutine;
        private bool isOpeningFinished;

        public bool IsOpeningFinished => isOpeningFinished;

        private void Start()
        {
            if (autoStart)
            {
                StartSequence();
            }
        }

        public void StartSequence()
        {
            if (sequenceCoroutine != null)
            {
                StopCoroutine(sequenceCoroutine);
            }
            sequenceCoroutine = StartCoroutine(RunOpeningTimeline());
        }

        public void SkipSequence()
        {
            if (!allowSkip || isOpeningFinished) return;

            if (sequenceCoroutine != null)
            {
                StopCoroutine(sequenceCoroutine);
            }
            FinishOpening();
        }

        private IEnumerator RunOpeningTimeline()
        {
            OnOpeningStarted?.Invoke();

            // Bloquear controles do jogador durante a cutscene
            if (yuna != null)
            {
                yuna.AutoRunActive = false;
                // Posicao inicial na Plataforma 0 (Cabeca do Urso em X = 2.0, Y = -6.5)
                yuna.transform.position = new Vector3(2.0f, -6.5f, 0f);
            }

            // 1. 0.0s a 4.0s - Fade-in na penumbra e chegada da luz
            if (nanda != null)
            {
                nanda.ApplyMood(NandaMood.Normal);
            }
            yield return new WaitForSeconds(4.0f);

            // 2. 4.0s a 9.0s - Primeira chamada da fada
            OnDialogueDisplayed?.Invoke(DarkRoomDialogueScript.OpeningLine1);
            yield return new WaitForSeconds(5.0f);

            // 3. 10.0s a 13.0s - Narracao Poetica 1
            OnPoeticLineDisplayed?.Invoke(DarkRoomDialogueScript.PoeticOpening1);
            yield return new WaitForSeconds(4.0f);

            // 4. 14.0s a 20.0s - Narracao Poetica 2
            OnPoeticLineDisplayed?.Invoke(DarkRoomDialogueScript.PoeticOpening2);
            yield return new WaitForSeconds(6.0f);

            // 5. 21.0s a 24.0s - Nanda perde a paciencia
            if (nanda != null) nanda.ApplyMood(NandaMood.Annoyed);
            OnDialogueDisplayed?.Invoke(DarkRoomDialogueScript.OpeningLine2);
            yield return new WaitForSeconds(4.0f);

            // 6. 25.0s a 29.0s - O chamado urgente
            OnDialogueDisplayed?.Invoke(DarkRoomDialogueScript.OpeningLine3);
            yield return new WaitForSeconds(4.0f);

            // 7. 29.0s a 36.0s - Yuna se levanta (aos 33.5s entra a musica tema)
            if (nanda != null) nanda.ApplyMood(NandaMood.Normal);
            yield return new WaitForSeconds(7.0f);

            // 8. 36.0s a 41.0s - Cartao de Capitulo: "CAPITULO 1 - Quarto de brinquedos"
            yield return new WaitForSeconds(5.0f);

            // 9. 41.0s a 42.5s - Transicao para jogabilidade
            yield return new WaitForSeconds(1.5f);

            FinishOpening();
        }

        private void FinishOpening()
        {
            isOpeningFinished = true;

            // Nanda posiciona-se sobre a Plataforma 0 (Cabeca do Urso de Pelucia)
            if (nanda != null)
            {
                nanda.SetTargetPlatform(0);
                nanda.ApplyMood(NandaMood.Normal);
            }

            // Ativar a corrida automatica da Yuna
            if (yuna != null)
            {
                yuna.AutoRunActive = true;
                yuna.SetRunDirection(1f);
            }

            OnOpeningCompleted?.Invoke();
            Debug.Log("[DarkRoomOpeningSequence] Abertura concluida. Jogabilidade liberada na Plataforma 0.");
        }
    }
}
