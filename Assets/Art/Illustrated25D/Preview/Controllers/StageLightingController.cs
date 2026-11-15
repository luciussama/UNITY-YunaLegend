using System;
using System.Linq;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Controla a iluminação de penumbra, reflexos no piso e enquadramento de fundo da câmera.
    /// </summary>
    public sealed class StageLightingController : MonoBehaviour
    {
        public PenumbraLightingT054 Lighting { get; private set; }

        public void InstallPenumbra(
            RoomBackgroundT054 room,
            Shader[] requiredShaders,
            DarkRoomPlatformMarker[] platforms,
            float floorY,
            YunaIllustrationVisibilityResolver yunaResolver,
            SpriteRenderer nandaRenderer,
            Func<DarkRoomPlatformMarker, Bounds, Bounds> resolvePaintingBounds)
        {
            Lighting = room.gameObject.AddComponent<PenumbraLightingT054>();
            Lighting.lightingShader = requiredShaders.Single(s => s != null && s.name == "YunaLegend/RoomPenumbraT054");
            Lighting.room = room;
            Lighting.nanda = nandaRenderer.transform;
            Lighting.floorContacts = platforms.Select(p =>
            {
                var b = resolvePaintingBounds(p, StageSceneryController.Bounds(p));
                return new Vector4(b.center.x, floorY, b.size.x * 0.5f, 1f);
            }).ToArray();

            for (int i = 0; i < room.FarPanels.Count; i++)
            {
                Lighting.Bind(room.FarPanels[i], 0, false, false, false, i == 0);
            }

            foreach (var p in platforms)
            {
                foreach (var r in p.GetComponentsInChildren<Renderer>())
                {
                    if (r.enabled)
                    {
                        Lighting.Bind(r, 1);
                    }
                }
            }

            Lighting.Bind(yunaResolver.idleRenderer, 2, true, true);
            Lighting.Bind(yunaResolver.run.painting, 2, true);
            Lighting.Bind(yunaResolver.jump.painting, 2, true);
            Lighting.Bind(yunaResolver.stumble.painting, 2, true, false, true);
            Lighting.Bind(nandaRenderer, 3);
        }

        public void InstallFloorReflections(
            RoomBackgroundT054 room,
            DarkRoomPlatformMarker[] platforms,
            bool includeDresser,
            bool includeBlocks,
            bool includeDrum,
            bool includeDrawers07,
            bool includeCastle09,
            bool includeStage2P10)
        {
            if (includeDresser)
            {
                DresserFloorReceiver.Install(room, Lighting, platforms[3].transform.position.x);
            }
            if (includeBlocks)
            {
                DresserFloorReceiver.Install(room, Lighting, platforms[4].transform.position.x, AbcStackPlatform.FullWidth * 0.5f + 0.2f, "Reflexo_Piso_Blocos_ABC");
            }
            if (includeDrum)
            {
                DresserFloorReceiver.Install(room, Lighting, platforms[5].transform.position.x, Drum05Platform.FullWidth * 0.5f + 0.2f, "Reflexo_Piso_Tambor");
            }
            if (includeDrawers07)
            {
                DresserFloorReceiver.Install(room, Lighting, platforms[6].transform.position.x, Cushion06Platform.FullWidth * 0.5f + 0.2f, "Reflexo_Piso_Almofadao");
                DresserFloorReceiver.Install(room, Lighting, platforms[7].transform.position.x, Drawers07Platform.FullWidth * 0.5f + 0.2f, "Reflexo_Piso_Gavetas");
            }
            if (includeCastle09)
            {
                DresserFloorReceiver.Install(room, Lighting, platforms[8].transform.position.x, MusicBox08Platform.FullWidth * 0.5f + 0.2f, "Reflexo_Piso_Caixa_Musica");
                DresserFloorReceiver.Install(room, Lighting, platforms[9].transform.position.x, Castle09Platform.FullWidth * 0.5f + 0.2f, "Reflexo_Piso_Castelinho");
            }
            if (includeStage2P10)
            {
                DresserFloorReceiver.Install(room, Lighting, platforms[10].transform.position.x, TrainTrestle10Platform.FullWidth * 0.5f + 0.2f, "Reflexo_Piso_Trem_10");
            }
        }

        public void Apply()
        {
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.007f, 0.009f, 0.018f);
            }
            Lighting?.Apply();
        }
    }
}
