using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.Picnic.Runtime
{
    public static class PicnicArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        public static Sprite Load(string name)
        {
            if (!Cache.TryGetValue(name, out Sprite sprite) || sprite == null)
            {
                sprite = Resources.Load<Sprite>("Picnic/" + name);
                if (sprite == null) Debug.LogError("[Picnic] Missing sprite: " + name);
                Cache[name] = sprite;
            }
            return sprite;
        }

        public static SpriteRenderer Create(string name, Transform parent, Vector3 position,
            float width, int order, string asset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Load(asset);
            renderer.sortingOrder = order;
            SetWidth(renderer, width);
            return renderer;
        }

        public static void SetWidth(SpriteRenderer renderer, float width)
        {
            if (renderer.sprite == null) return;
            float scale = width / renderer.sprite.bounds.size.x;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public static void SetHeight(SpriteRenderer renderer, float height)
        {
            if (renderer.sprite == null) return;
            float scale = height / renderer.sprite.bounds.size.y;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
