using System;
using System.Collections.Generic;

namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Sequenciador puro de beats narrativos sem dependencia de MonoBehaviour ou corrotinas.
    /// </summary>
    public sealed class ToyRoomBeatPlayer
    {
        private IReadOnlyList<ToyRoomNarrativeBeat> activeSequence;
        private int currentIndex = -1;
        private float elapsedInBeat;

        public bool IsPlaying => activeSequence != null && currentIndex >= 0 && currentIndex < activeSequence.Count;
        public ToyRoomNarrativeBeat CurrentBeat => IsPlaying ? activeSequence[currentIndex] : default;

        public event Action<ToyRoomNarrativeBeat> OnBeatEntered;
        public event Action OnSequenceCompleted;

        public void Play(IReadOnlyList<ToyRoomNarrativeBeat> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                activeSequence = null;
                currentIndex = -1;
                OnSequenceCompleted?.Invoke();
                return;
            }

            activeSequence = sequence;
            currentIndex = 0;
            elapsedInBeat = 0f;
            OnBeatEntered?.Invoke(activeSequence[0]);
        }

        public void Tick(float deltaSeconds)
        {
            if (!IsPlaying || deltaSeconds <= 0f)
            {
                return;
            }

            elapsedInBeat += deltaSeconds;
            while (IsPlaying && elapsedInBeat >= activeSequence[currentIndex].DurationSeconds)
            {
                elapsedInBeat -= activeSequence[currentIndex].DurationSeconds;
                Advance();
            }
        }

        public void AdvanceCurrentBeat()
        {
            if (!IsPlaying)
            {
                return;
            }

            elapsedInBeat = 0f;
            Advance();
        }

        public void SkipAll()
        {
            if (!IsPlaying)
            {
                return;
            }

            activeSequence = null;
            currentIndex = -1;
            elapsedInBeat = 0f;
            OnSequenceCompleted?.Invoke();
        }

        private void Advance()
        {
            currentIndex++;
            if (currentIndex >= activeSequence.Count)
            {
                activeSequence = null;
                currentIndex = -1;
                elapsedInBeat = 0f;
                OnSequenceCompleted?.Invoke();
                return;
            }

            OnBeatEntered?.Invoke(activeSequence[currentIndex]);
        }
    }
}
