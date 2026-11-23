using UnityEngine;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicWheelView : MonoBehaviour
    {
        private readonly Transform[] spokes = new Transform[4];
        private float centerX;
        private float centerY;
        private int index;
        public void Initialize(float x, float y, int wheelIndex)
        {
            centerX = x;
            centerY = y;
            index = wheelIndex;
            for (int i = 0; i < 4; i++)
            {
                spokes[i] = PicnicArt.Create("Wooden spoke", transform, new Vector3(x, y, 0.2f), 1.5f, 0, "wood").transform;
            }
        }
        public void Apply(float time)
        {
            float speed = index == 2 ? -0.36f : index == 3 ? 0.34f : 0.38f;
            float phase = index == 2 ? 0.6f : index == 3 ? 1.2f : 0f;
            for (int i = 0; i < 4; i++)
            {
                float angle = phase + time * speed + i * Mathf.PI * 0.5f;
                Vector3 delta = new Vector3(Mathf.Cos(angle) * 1.5f, -Mathf.Sin(angle) * 1.15f, 0f);
                spokes[i].position = new Vector3(centerX, centerY, 0.2f) + delta * 0.5f;
                spokes[i].rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                spokes[i].localScale = new Vector3(delta.magnitude / PicnicArt.Load("wood").bounds.size.x, 0.045f, 1f);
            }
        }
    }
}
