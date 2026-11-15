using System;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Instalador genérico reutilizável para plataformas ilustradas 2.5D:
    /// monta sprite, colisor físico real de apoio e metadados de marcador a partir de IllustratedPlatformSpec.
    /// </summary>
    public sealed class IllustratedPlatformView : MonoBehaviour
    {
        public BoxCollider Top { get; private set; }
        public SpriteRenderer Painting { get; private set; }
        public SpriteRenderer FrontOverlay { get; private set; }
        public IllustratedPlatformSpec Spec { get; private set; }

        private Sprite ownedSprite;
        private Sprite ownedOverlaySprite;
        private Material ownedMaterial;

        public void Install(
            IllustratedPlatformSpec spec,
            Texture2D texture,
            Texture2D frontOverlayTexture = null,
            int frontOverlaySortingOrder = 25)
        {
            if (texture == null || texture.width != 896 || texture.height != 1200)
            {
                throw new InvalidOperationException($"Platform {spec.index} painting dimensions invalid.");
            }

            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != spec.index)
            {
                throw new InvalidOperationException($"Platform {spec.index} requires marker {spec.index}.");
            }

            Spec = spec;
            float x = spec.targetX;
            transform.position = new Vector3(x, spec.floorY + spec.targetHeight * 0.5f, 0f);
            marker.spec.x = x;
            marker.spec.y = transform.position.y;
            marker.spec.width = spec.FullWidth;
            marker.spec.height = spec.targetHeight;
            marker.spec.depth = spec.depth;
            marker.spec.contact.offsetX = spec.SupportOffsetX;

            var contact = transform.Find("Contacto_Fisico");
            if (contact == null)
            {
                contact = new GameObject("Contacto_Fisico").transform;
                contact.SetParent(transform, false);
            }
            foreach (var c in contact.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = false;
            }

            string surfaceName = string.IsNullOrEmpty(spec.surfaceObjectName)
                ? $"Topo_Real_Plataforma_{spec.index:00}"
                : spec.surfaceObjectName;
            var surface = new GameObject(surfaceName);
            surface.transform.SetParent(contact, false);
            surface.transform.position = new Vector3(x + spec.SupportOffsetX, spec.TopY - 0.03f, 0f);
            Top = surface.AddComponent<BoxCollider>();
            Top.size = new Vector3(spec.SupportWidth, 0.06f, spec.depth);

            float pixelsPerMetre = spec.PixelsPerMetre;
            var pivot = new Vector2(spec.centrePixelX / texture.width, (texture.height - spec.feetPixelY) / texture.height);
            ownedMaterial = new Material(Shader.Find("Sprites/Default"));

            string paintingName = string.IsNullOrEmpty(spec.paintingObjectName)
                ? $"Plataforma_{spec.index:00}_Pintura"
                : spec.paintingObjectName;
            var face = new GameObject(paintingName);
            face.transform.SetParent(transform, false);
            face.transform.position = new Vector3(x, spec.floorY, 0f);

            ownedSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                pivot,
                pixelsPerMetre,
                0,
                SpriteMeshType.FullRect);

            Painting = face.AddComponent<SpriteRenderer>();
            Painting.sprite = ownedSprite;
            Painting.sharedMaterial = ownedMaterial;
            Painting.sortingOrder = -1;

            if (frontOverlayTexture != null)
            {
                var overlayGo = new GameObject($"{paintingName}_Frente");
                overlayGo.transform.SetParent(transform, false);
                overlayGo.transform.position = new Vector3(x, spec.floorY, 0f);

                ownedOverlaySprite = Sprite.Create(
                    frontOverlayTexture,
                    new Rect(0, 0, frontOverlayTexture.width, frontOverlayTexture.height),
                    pivot,
                    pixelsPerMetre,
                    0,
                    SpriteMeshType.FullRect);

                FrontOverlay = overlayGo.AddComponent<SpriteRenderer>();
                FrontOverlay.sprite = ownedOverlaySprite;
                FrontOverlay.sharedMaterial = ownedMaterial;
                FrontOverlay.sortingOrder = frontOverlaySortingOrder;
            }
        }

        private void OnDestroy()
        {
            if (ownedSprite != null) Destroy(ownedSprite);
            if (ownedOverlaySprite != null) Destroy(ownedOverlaySprite);
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }
}
