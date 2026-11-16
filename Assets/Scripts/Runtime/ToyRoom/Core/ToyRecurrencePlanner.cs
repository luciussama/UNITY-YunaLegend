using System;
using System.Collections.Generic;

namespace YunaLegend.ToyRoom.Core
{
    public enum ToyPlacementState
    {
        OnFloor,
        Carried,
        Organized,
        PendingReturn
    }

    /// <summary>
    /// Planeja o comportamento normal (brinquedos 1..6) e a recorrencia silenciosa
    /// (brinquedos 7 e 8) fora do enquadramento principal da camera.
    /// </summary>
    public sealed class ToyRecurrencePlanner
    {
        public const float DefaultReturnDelaySeconds = 1.5f;
        public const int RecurrenceRemainingThreshold = 2;
        public const int StrangeDialogueReturnCount = 2;
        public const int InvestigationReturnCount = 4;
        private const float MinPlayerDistanceSq = 3.5f * 3.5f;

        private sealed class ToyEntry
        {
            public ToySpec Spec;
            public ToyPlacementState State;
            public ToyRoomPoint Position;
            public bool HasReturnedOnce;
            public float ReturnTimer;
        }

        private readonly Dictionary<string, ToyEntry> entries = new Dictionary<string, ToyEntry>();
        private readonly List<ToyEntry> orderedEntries = new List<ToyEntry>();
        private readonly IReadOnlyList<ToyRoomPoint> respawnCandidates;
        private int candidateCursor;

        public bool IsRecurrenceActive { get; private set; }
        public int OrganizedCount { get; private set; }
        public int ReturnedStoredCount { get; private set; }
        public bool StrangeDialogueFired { get; private set; }
        public bool InvestigationTriggered { get; private set; }

        public event Action<string, ToyRoomPoint> OnToyRespawned;
        public event Action OnStrangeDialogueReached;
        public event Action OnInvestigationReached;

        public ToyRecurrencePlanner(
            IReadOnlyList<ToySpec> toys = null,
            IReadOnlyList<ToyRoomPoint> customCandidates = null)
        {
            var sourceToys = toys ?? ToyRoomCatalog.Toys;
            respawnCandidates = customCandidates ?? ToyRoomCatalog.PeripheralRespawnCandidates;

            for (int i = 0; i < sourceToys.Count; i++)
            {
                var spec = sourceToys[i];
                var entry = new ToyEntry
                {
                    Spec = spec,
                    State = ToyPlacementState.OnFloor,
                    Position = spec.InitialPosition,
                    HasReturnedOnce = false,
                    ReturnTimer = 0f
                };
                entries[spec.Id] = entry;
                orderedEntries.Add(entry);
            }
        }

        public ToyPlacementState GetState(string toyId)
        {
            return entries[toyId].State;
        }

        public ToyRoomPoint GetPosition(string toyId)
        {
            return entries[toyId].Position;
        }

        public bool TryPickUp(string toyId)
        {
            if (!entries.TryGetValue(toyId, out var entry))
            {
                return false;
            }

            if (entry.State != ToyPlacementState.OnFloor)
            {
                return false;
            }

            entry.State = ToyPlacementState.Carried;
            return true;
        }

        public bool TryStoreInChest(string toyId)
        {
            if (!entries.TryGetValue(toyId, out var entry))
            {
                return false;
            }

            if (entry.State != ToyPlacementState.Carried)
            {
                return false;
            }

            if (!IsRecurrenceActive && CountRemainingUnorganizedExcluding(toyId) <= RecurrenceRemainingThreshold)
            {
                IsRecurrenceActive = true;
            }

            if (entry.HasReturnedOnce)
            {
                ReturnedStoredCount++;
            }

            if (IsRecurrenceActive)
            {
                entry.State = ToyPlacementState.PendingReturn;
                entry.ReturnTimer = DefaultReturnDelaySeconds;
            }
            else
            {
                entry.State = ToyPlacementState.Organized;
                OrganizedCount++;
            }

            EvaluateNarrativeMilestones();
            return true;
        }

        public void Tick(
            float deltaSeconds,
            ToyRoomPoint playerPosition,
            Func<ToyRoomPoint, bool> isInsideMainCameraFocus)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            for (int i = 0; i < orderedEntries.Count; i++)
            {
                var entry = orderedEntries[i];
                if (entry.State != ToyPlacementState.PendingReturn)
                {
                    continue;
                }

                entry.ReturnTimer -= deltaSeconds;
                if (entry.ReturnTimer > 0f)
                {
                    continue;
                }

                var respawnPoint = SelectRespawnPoint(playerPosition, isInsideMainCameraFocus);
                entry.Position = respawnPoint;
                entry.State = ToyPlacementState.OnFloor;
                entry.HasReturnedOnce = true;
                entry.ReturnTimer = 0f;

                OnToyRespawned?.Invoke(entry.Spec.Id, respawnPoint);
            }
        }

        private int CountRemainingUnorganizedExcluding(string excludedToyId)
        {
            int count = 0;
            for (int i = 0; i < orderedEntries.Count; i++)
            {
                var entry = orderedEntries[i];
                if (entry.Spec.Id == excludedToyId)
                {
                    continue;
                }

                if (entry.State == ToyPlacementState.OnFloor || entry.State == ToyPlacementState.Carried)
                {
                    count++;
                }
            }

            return count;
        }

        private ToyRoomPoint SelectRespawnPoint(
            ToyRoomPoint playerPosition,
            Func<ToyRoomPoint, bool> isInsideMainCameraFocus)
        {
            if (respawnCandidates == null || respawnCandidates.Count == 0)
            {
                return new ToyRoomPoint(-6.4f, -4.5f);
            }

            ToyRoomPoint bestOffscreenFar = default;
            bool foundOffscreenFar = false;

            ToyRoomPoint bestOffscreenAny = default;
            bool foundOffscreenAny = false;

            ToyRoomPoint farthestOverall = respawnCandidates[0];
            float maxDistSq = -1f;

            for (int offset = 0; offset < respawnCandidates.Count; offset++)
            {
                int index = (candidateCursor + offset) % respawnCandidates.Count;
                var candidate = respawnCandidates[index];
                float distSq = candidate.DistanceSquaredTo(playerPosition);

                if (distSq > maxDistSq)
                {
                    maxDistSq = distSq;
                    farthestOverall = candidate;
                }

                bool inFocus = isInsideMainCameraFocus != null && isInsideMainCameraFocus(candidate);
                if (inFocus)
                {
                    continue;
                }

                if (!foundOffscreenAny)
                {
                    bestOffscreenAny = candidate;
                    foundOffscreenAny = true;
                }

                if (!foundOffscreenFar && distSq >= MinPlayerDistanceSq)
                {
                    bestOffscreenFar = candidate;
                    foundOffscreenFar = true;
                    candidateCursor = (index + 1) % respawnCandidates.Count;
                    break;
                }
            }

            if (foundOffscreenFar)
            {
                return bestOffscreenFar;
            }

            if (foundOffscreenAny)
            {
                candidateCursor = (candidateCursor + 1) % respawnCandidates.Count;
                return bestOffscreenAny;
            }

            candidateCursor = (candidateCursor + 1) % respawnCandidates.Count;
            return farthestOverall;
        }

        private void EvaluateNarrativeMilestones()
        {
            if (!StrangeDialogueFired && ReturnedStoredCount >= StrangeDialogueReturnCount)
            {
                StrangeDialogueFired = true;
                OnStrangeDialogueReached?.Invoke();
            }

            if (!InvestigationTriggered && ReturnedStoredCount >= InvestigationReturnCount)
            {
                InvestigationTriggered = true;
                OnInvestigationReached?.Invoke();
            }
        }
    }
}
