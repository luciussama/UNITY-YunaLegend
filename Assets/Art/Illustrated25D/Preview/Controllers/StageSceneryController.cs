using System;
using System.Linq;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Controla a cenografia de fundo do estágio (painéis do quarto, extensão da base dos livros
    /// até o piso e sombras de contato das plataformas).
    /// </summary>
    public sealed class StageSceneryController : MonoBehaviour
    {
        public RoomBackgroundT054 Room { get; private set; }

        public void InstallRoom(
            DarkRoomPlatformMarker[] platforms,
            Texture2D panorama,
            Texture2D roomExtension,
            bool extendForStage2,
            float floorY,
            Material spriteMaterial)
        {
            float minX = platforms.Take(3).Min(p => Bounds(p).min.x);
            float maxX = platforms.Take(3).Max(p => Bounds(p).max.x);

            Room = new GameObject("Quarto_Ilustrado_Aprovado").AddComponent<RoomBackgroundT054>();
            Room.FollowVertical = extendForStage2;
            var panels = roomExtension != null
                ? (extendForStage2
                    ? new[] { panorama, roomExtension, roomExtension, roomExtension, roomExtension, roomExtension }
                    : new[] { panorama, roomExtension, roomExtension })
                : new[] { panorama };

            Room.InstallPanels(
                panels,
                panorama.width / 32f,
                null,
                null,
                floorY,
                (minX + maxX) * 0.5f,
                spriteMaterial,
                1,
                1,
                RoomBackgroundT054.PanoramaFloorInsetFraction,
                0.35f,
                0);
        }

        public void ExtendBooksBaseToFloor(Transform books, Transform parentPlatform, float floorY)
        {
            float baseY = books.TransformPoint(new Vector3(0f, IllustratedBookStackPlatformProfile.Document.mapping.quadBoundsLocal.bottom, 0f)).y;
            if (baseY <= floorY + 0.001f)
            {
                return;
            }

            var lower = Instantiate(books.gameObject, parentPlatform);
            lower.name = "Livros_Base_T054";
            lower.transform.position = books.position + Vector3.down * (baseY - floorY);
            lower.GetComponent<Renderer>().sortingOrder = books.GetComponent<Renderer>().sortingOrder - 1;
        }

        public void InstallContactShadows(
            DarkRoomPlatformMarker[] platforms,
            float floorY,
            Func<DarkRoomPlatformMarker, Bounds, Bounds> resolvePaintingBounds)
        {
            var shadows = new GameObject(ContactShadowT054.RootName);
            foreach (var p in platforms)
            {
                var colliderBounds = Bounds(p);
                var paintingBounds = resolvePaintingBounds(p, colliderBounds);
                ContactShadowT054.Install(
                    shadows.transform,
                    paintingBounds.center.x,
                    floorY,
                    paintingBounds.size.x,
                    colliderBounds.size.z,
                    0.18f,
                    true);
            }
        }

        public void FollowCamera(Vector3 cameraPosition)
        {
            if (Room != null)
            {
                Room.Follow(cameraPosition);
            }
        }

        public static Bounds Bounds(DarkRoomPlatformMarker marker)
        {
            var colliders = marker.GetComponentsInChildren<Collider>(true).Where(c => c.enabled).ToArray();
            var bounds = colliders[0].bounds;
            foreach (var c in colliders)
            {
                bounds.Encapsulate(c.bounds);
            }
            return bounds;
        }
    }
}
