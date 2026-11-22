using System;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Presents actual drawings at discrete exposures without transforming the artwork.
    /// </summary>
    public sealed class HandDrawnExposurePlayer : MonoBehaviour
    {
        private Image surface;
        private Sprite[] drawings;
        private double elapsedSeconds;
        private int currentFrame = -1;
        private bool isPlaying;

        public int MissedExposures { get; private set; }
        public event Action<int> OnDrawingPresented;
        public event Action OnCompleted;

        public void Configure(Image target, Sprite[] frames)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (frames == null || frames.Length == 0)
            {
                throw new ArgumentException("A sequence must contain drawings.", nameof(frames));
            }

            foreach (Sprite drawing in frames)
            {
                if (drawing == null)
                {
                    throw new ArgumentException("An exposure cannot have a missing drawing.", nameof(frames));
                }
            }

            surface = target;
            drawings = frames;
            isPlaying = false;
        }

        public void Play()
        {
            if (surface == null || drawings == null)
            {
                throw new InvalidOperationException("Configure the sequence before playback.");
            }

            elapsedSeconds = 0d;
            currentFrame = -1;
            MissedExposures = 0;
            isPlaying = true;
            Present(0);
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            int frame = ToyRoomHandDrawnTiming.FrameAt(elapsedSeconds);
            if (frame >= drawings.Length)
            {
                MissedExposures += Math.Max(0, drawings.Length - currentFrame - 1);
                isPlaying = false;
                OnCompleted?.Invoke();
                return;
            }

            Present(frame);
        }

        private void Present(int frame)
        {
            if (frame == currentFrame)
            {
                return;
            }

            MissedExposures += Math.Max(0, frame - currentFrame - 1);
            currentFrame = frame;
            surface.sprite = drawings[frame];
            OnDrawingPresented?.Invoke(frame);
        }
    }
}
