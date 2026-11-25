using System;
using System.Collections.Generic;

namespace YunaLegend.Picnic.Core
{
    /// <summary>Independent finger ownership; quick taps survive until the next simulation read.</summary>
    public sealed class PicnicTouchInput
    {
        private readonly Dictionary<int, float> directions = new Dictionary<int, float>();
        private readonly HashSet<int> jumps = new HashSet<int>();
        private bool jumpQueued;

        public float Axis
        {
            get
            {
                float sum = 0f;
                foreach (float direction in directions.Values)
                {
                    sum += direction;
                }
                return Math.Sign(sum);
            }
        }

        public void PressDirection(int pointerId, float direction)
        {
            directions[pointerId] = Math.Sign(direction);
        }

        public void PressJump(int pointerId)
        {
            if (jumps.Add(pointerId))
            {
                jumpQueued = true;
            }
        }

        public void Release(int pointerId)
        {
            directions.Remove(pointerId);
            jumps.Remove(pointerId);
        }

        public bool ReadJump()
        {
            bool value = jumpQueued || jumps.Count > 0;
            jumpQueued = false;
            return value;
        }

        public void Clear()
        {
            directions.Clear();
            jumps.Clear();
            jumpQueued = false;
        }
    }
}
