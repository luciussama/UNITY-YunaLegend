using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controla visibilidade de entrada ilustrada 2D versus corredor 3D e atores 3D por fase.
    /// </summary>
    public sealed class BearChaseSceneryController : MonoBehaviour
    {
        [SerializeField] private GameObject chaseCorridorRoot;
        [SerializeField] private GameObject yuna3DActor;
        [SerializeField] private GameObject bear3DActor;
        [SerializeField] private GameObject nanda3DActor;

        public GameObject ChaseCorridorRoot => chaseCorridorRoot;

        public void Configure(
            GameObject corridorRoot,
            GameObject yunaActor,
            GameObject bearActor,
            GameObject nandaActor)
        {
            chaseCorridorRoot = corridorRoot;
            yuna3DActor = yunaActor;
            bear3DActor = bearActor;
            nanda3DActor = nandaActor;
        }

        public void ApplyPhaseVisibility(BearChasePhase phase)
        {
            bool isChaseActive = !phase.IsEntrance();
            if (chaseCorridorRoot != null && chaseCorridorRoot.activeSelf != isChaseActive)
            {
                chaseCorridorRoot.SetActive(isChaseActive);
            }

            if (yuna3DActor != null && yuna3DActor.activeSelf != isChaseActive)
            {
                yuna3DActor.SetActive(isChaseActive);
            }

            if (bear3DActor != null && bear3DActor.activeSelf != isChaseActive)
            {
                bear3DActor.SetActive(isChaseActive);
            }

            if (nanda3DActor != null && nanda3DActor.activeSelf != isChaseActive)
            {
                nanda3DActor.SetActive(isChaseActive);
            }
        }
    }
}
