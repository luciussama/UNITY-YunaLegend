using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Fadinha guia Nanda azul-ciano acompanhando Yuna na cena de entrada e sobre o ombro na corrida.
    /// </summary>
    public sealed class BearChaseNandaController : MonoBehaviour
    {
        [SerializeField] private Transform yunaTransform;
        private Transform[] wings;

        public void Configure(Transform yuna)
        {
            yunaTransform = yuna;
            CacheWings();
        }

        private void Awake()
        {
            CacheWings();
        }

        private void CacheWings()
        {
            var list = new System.Collections.Generic.List<Transform>();
            var all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name.Contains("Wing"))
                {
                    list.Add(all[i]);
                }
            }

            wings = list.ToArray();
        }

        public void UpdateNanda(BearChasePhase phase, float dt)
        {
            if (yunaTransform == null)
            {
                return;
            }

            Vector3 offset = (phase == BearChasePhase.EntranceWalk
                || phase == BearChasePhase.EntranceNudge
                || phase == BearChasePhase.BearWakeUp
                || phase == BearChasePhase.TransitionToChase)
                ? new Vector3(-0.78f, 1.72f, 0.15f)
                : new Vector3(-0.62f, 1.66f, -0.35f);

            float bob = Mathf.Sin(Time.time * 3.8f) * 0.09f;
            float sway = Mathf.Cos(Time.time * 2.4f) * 0.08f;
            Vector3 desired = yunaTransform.position + offset + new Vector3(sway, bob, 0f);

            transform.position = Vector3.Distance(transform.position, desired) > 8f
                ? desired
                : Vector3.Lerp(transform.position, desired, Mathf.Min(1f, dt * 7.5f));
            float hoverPitch = 28f + Mathf.Sin(Time.time * 3.8f) * 5f;
            transform.rotation = yunaTransform.rotation * Quaternion.Euler(hoverPitch, 0f, 0f);

            if (wings != null)
            {
                float flap = Mathf.Sin(Time.time * 22f) * 18f;
                for (int i = 0; i < wings.Length; i++)
                {
                    if (wings[i] != null)
                    {
                        float sign = i % 2 == 0 ? 1f : -1f;
                        wings[i].localRotation = Quaternion.Euler(0f, flap * sign, 0f);
                    }
                }
            }
        }
    }
}
