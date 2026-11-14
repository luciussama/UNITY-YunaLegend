using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Sombra de contato procedural sob uma plataforma. Sem asset externo: elipse suave
    /// gerada em memoria. Existe porque as plataformas sao sprites unlit e, sem a sombra,
    /// leem como se flutuassem sobre o piso pintado.
    /// </summary>
    public static class ContactShadowT054
    {
        public const string RootName = "Sombras_Contato_T054";
        public const int SoftOrder = -160;
        public const int CoreOrder = -150;

        private static Texture2D s_ellipse;

        /// <summary>Cria duas manchas (larga e nuclear) sob o apoio. offsetX segue a luz da janela.</summary>
        public static void Install(Transform parent, float centreX, float floorY, float width, float depth, float offsetX,
            bool penumbra = false)
        {
            var texture = Ellipse();
            float spriteWidth = texture.width / 64f;
            float spriteHeight = texture.height / 64f;

            var soft = Build(parent, texture, "Sombra_Suave", floorY,
                new Vector3(centreX + offsetX * .5f, 0f, depth * .45f),
                width * 1.15f, depth * 1.25f,
                penumbra ? new Color(0, 0, 0, .65f) : new Color(.05f, .045f, .09f, .24f), SoftOrder);
            soft.transform.localScale = new Vector3(width * 1.15f / spriteWidth, depth * 1.25f / spriteHeight, 1f);

            var core = Build(parent, texture, "Sombra_Nucleo", floorY,
                new Vector3(centreX + offsetX, 0f, depth * .30f),
                width * .82f, depth * .72f,
                penumbra ? new Color(0, 0, 0, .9f) : new Color(.03f, .028f, .06f, .36f), CoreOrder);
            core.transform.localScale = new Vector3(width * .82f / spriteWidth, depth * .72f / spriteHeight, 1f);

            if (penumbra)
            {
                // The accompaniment camera has zero pitch: an XZ sprite is edge-on.
                // Paint the floor projection behind the object, anchored to its base.
                // This is a contact approximation, not a dynamic cast shadow.
                soft.transform.rotation = core.transform.rotation = Quaternion.identity;
                soft.transform.position = new Vector3(centreX + offsetX * .5f, floorY - .045f, .05f);
                core.transform.position = new Vector3(centreX, floorY - .015f, .04f);
                soft.transform.localScale = new Vector3(width * 1.25f / spriteWidth, .28f / spriteHeight, 1f);
                core.transform.localScale = new Vector3(width * .95f / spriteWidth, .12f / spriteHeight, 1f);
            }
        }

        private static SpriteRenderer Build(Transform parent, Texture2D texture, string name, float floorY,
            Vector3 position, float width, float depth, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(.5f, .5f), 64f);
            renderer.color = color;
            renderer.sortingOrder = order;
            go.transform.position = new Vector3(position.x, floorY + .02f, position.z);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            return renderer;
        }

        private static Texture2D Ellipse()
        {
            if (s_ellipse != null) return s_ellipse;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "T054_ContactShadow",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + .5f) / size * 2f - 1f;
                float dy = (y + .5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - distance);
                alpha *= alpha;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            s_ellipse = texture;
            return texture;
        }
    }
}
