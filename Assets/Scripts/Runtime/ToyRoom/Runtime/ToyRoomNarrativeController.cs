using System;
using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Adaptador MonoBehaviour que executa sequencias de ToyRoomNarrativeScript via ToyRoomBeatPlayer.
    /// </summary>
    public sealed class ToyRoomNarrativeController : MonoBehaviour
    {
        [SerializeField] private ToyRoomHudController hud;
        [SerializeField] private ToySwordView swordView;

        private readonly ToyRoomBeatPlayer beatPlayer = new ToyRoomBeatPlayer();
        private Action onCurrentSequenceComplete;

        public bool IsPlaying => beatPlayer.IsPlaying;

        public void Configure(ToyRoomHudController hudController, ToySwordView sword)
        {
            hud = hudController;
            swordView = sword;
        }

        private void Awake()
        {
            beatPlayer.OnBeatEntered += HandleBeatEntered;
            beatPlayer.OnSequenceCompleted += HandleSequenceCompleted;
        }

        private void OnDestroy()
        {
            beatPlayer.OnBeatEntered -= HandleBeatEntered;
            beatPlayer.OnSequenceCompleted -= HandleSequenceCompleted;
        }

        private void Update()
        {
            beatPlayer.Tick(Time.deltaTime);
        }

        public void PlayIntro(Action onComplete)
        {
            onCurrentSequenceComplete = onComplete;
            beatPlayer.Play(ToyRoomNarrativeScript.IntroBeats);
        }

        public void PlayStrangeDialogue()
        {
            onCurrentSequenceComplete = null;
            beatPlayer.Play(ToyRoomNarrativeScript.StrangeDialogueBeats);
        }

        public void PlayInvestigation(Action onComplete)
        {
            onCurrentSequenceComplete = onComplete;
            beatPlayer.Play(ToyRoomNarrativeScript.InvestigationBeats);
        }

        public void PlaySwordPickup(Action onComplete)
        {
            onCurrentSequenceComplete = onComplete;
            beatPlayer.Play(ToyRoomNarrativeScript.SwordPickupBeats);
        }

        public void AdvanceActiveBeat()
        {
            beatPlayer.AdvanceCurrentBeat();
        }

        public void SkipActiveSequence()
        {
            beatPlayer.SkipAll();
        }

        private void HandleBeatEntered(ToyRoomNarrativeBeat beat)
        {
            if (beat.RevealsSword && swordView != null)
            {
                swordView.Reveal();
            }

            if (hud != null)
            {
                hud.ShowBeat(beat);
            }
        }

        private void HandleSequenceCompleted()
        {
            if (hud != null)
            {
                hud.ClearNarrativeOverlays();
            }

            var callback = onCurrentSequenceComplete;
            onCurrentSequenceComplete = null;
            callback?.Invoke();
        }
    }
}
