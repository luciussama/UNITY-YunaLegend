using System;
using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.Illustrated25D.Furniture;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Controla a instalação das plataformas ilustradas, alinhamentos de pés e consulta
    /// de bounds de pintura sem acoplar detalhes de cada objeto ao Composition Root.
    /// </summary>
    public sealed class StagePlatformController : MonoBehaviour
    {
        private readonly Dictionary<int, IllustratedPlatformView> genericViews = new Dictionary<int, IllustratedPlatformView>();
        private DarkRoomPlatformMarker[] allStageMarkers;

        public YieldingWoodFurniture Furniture { get; private set; }
        public DresserPlatform Dresser { get; private set; }
        public AbcStackPlatform Blocks { get; private set; }
        public Drum05Platform Drum { get; private set; }
        public Cushion06Platform Cushion { get; private set; }
        public Drawers07Platform Drawers { get; private set; }
        public MusicBox08Platform MusicBox { get; private set; }
        public Castle09Platform Castle { get; private set; }
        public TrainTrestle10Platform TrainTrestle10 { get; private set; }

        public void InstallSequence(IllustratedPreview config, DarkRoomPlatformMarker[] platforms, Yuna25DPlayerController player)
        {
            Furniture = platforms[2].GetComponent<YieldingWoodFurniture>();
            platforms[2].gameObject.AddComponent<FurnitureIllustrationCandidate>().Install(config.furnitureBody, config.furnitureWood);

            if (config.includeNextDresser)
            {
                Dresser = platforms[3].gameObject.AddComponent<DresserPlatform>();
                Dresser.Install(config.dresserPainting, platforms[2]);
                AttachFootAlignment(player, Dresser.Top, supportIndex: -1, pinVisibleSole: false);
            }

            if (config.includeNextBlocks)
            {
                if (!config.includeNextDresser) throw new InvalidOperationException("ABC stack requires the dresser step");
                if (config.blocksPainting == null) throw new InvalidOperationException("Approved ABC painting missing");
                Blocks = platforms[4].gameObject.AddComponent<AbcStackPlatform>();
                Blocks.Install(config.blocksPainting);
                AttachFootAlignment(player, Blocks.Top, supportIndex: 4, pinVisibleSole: true);
            }

            if (config.includeNextDrum)
            {
                if (!config.includeNextBlocks) throw new InvalidOperationException("Drum 05 requires the ABC stack step");
                if (config.drumPainting == null) throw new InvalidOperationException("Approved Drum 05 painting missing");
                Drum = platforms[5].gameObject.AddComponent<Drum05Platform>();
                Drum.Install(config.drumPainting);
                AttachFootAlignment(player, Drum.Top, supportIndex: 5, pinVisibleSole: true);
            }

            if (config.includeNextDrawers07)
            {
                if (!config.includeNextDrum) throw new InvalidOperationException("06/07 require the Drum 05 step");
                if (config.cushionPainting == null || config.drawersPainting == null) throw new InvalidOperationException("Approved 06/07 paintings missing");
                Cushion = platforms[6].gameObject.AddComponent<Cushion06Platform>();
                Cushion.Install(config.cushionPainting);
                AttachFootAlignment(player, Cushion.Top, supportIndex: 6, pinVisibleSole: true);

                Drawers = platforms[7].gameObject.AddComponent<Drawers07Platform>();
                Drawers.Install(config.drawersPainting);
                AttachFootAlignment(player, Drawers.Top, supportIndex: 7, pinVisibleSole: true);
            }

            if (config.includeNextCastle09)
            {
                if (!config.includeNextDrawers07) throw new InvalidOperationException("08/09 require the 06/07 step");
                if (config.musicBoxPainting == null || config.castlePainting == null) throw new InvalidOperationException("Approved 08/09 paintings missing");
                MusicBox = platforms[8].gameObject.AddComponent<MusicBox08Platform>();
                MusicBox.Install(config.musicBoxPainting);
                AttachFootAlignment(player, MusicBox.Top, supportIndex: 8, pinVisibleSole: true);

                Castle = platforms[9].gameObject.AddComponent<Castle09Platform>();
                Castle.Install(config.castlePainting, config.castleFrontTowersPainting);
                AttachFootAlignment(player, Castle.Top, supportIndex: 9, pinVisibleSole: true);
            }

            if (config.includeNextStage2Platform10)
            {
                if (!config.includeNextCastle09) throw new InvalidOperationException("Platform 10 requires Castle 09 step");
                if (config.trainTrestle10Painting == null) throw new InvalidOperationException("Approved Train Trestle 10 painting missing");
                TrainTrestle10 = platforms[10].gameObject.AddComponent<TrainTrestle10Platform>();
                TrainTrestle10.Install(config.trainTrestle10Painting);
                AttachFootAlignment(player, TrainTrestle10.Top, supportIndex: 10, pinVisibleSole: true);
            }

            if (config.includeFullStage)
            {
                InstallFullStagePlatforms(config, platforms, player, 37);
            }
        }

        public void InstallFullStagePlatforms(
            IllustratedPreview config,
            DarkRoomPlatformMarker[] platforms,
            Yuna25DPlayerController player,
            int maxIndex)
        {
            allStageMarkers = platforms;
            var specs = DarkRoomFullStageCatalog.BuildSpecsUpTo(maxIndex);
            foreach (var spec in specs)
            {
                if (spec.index < 11 || spec.index >= platforms.Length)
                {
                    continue;
                }

                Texture2D painting = config.GetFullStagePainting(spec.index);
                if (painting == null)
                {
                    throw new InvalidOperationException($"Full stage painting missing for platform {spec.index}");
                }

                InstallGenericPlatform(platforms[spec.index], player, spec, painting);
                if (spec.index >= 22)
                {
                    platforms[spec.index].gameObject.SetActive(false);
                }
            }
        }

        public void SetStage3PlatformsActive(bool stage3Active)
        {
            if (allStageMarkers != null)
            {
                foreach (var marker in allStageMarkers)
                {
                    if (marker == null) continue;
                    int idx = marker.spec.index;
                    marker.gameObject.SetActive(idx >= 22 ? stage3Active : !stage3Active);
                }
                return;
            }

            foreach (var kv in genericViews)
            {
                if (kv.Value == null) continue;
                kv.Value.gameObject.SetActive(kv.Key >= 22 ? stage3Active : !stage3Active);
            }
        }

        public IllustratedPlatformView InstallGenericPlatform(
            DarkRoomPlatformMarker marker,
            Yuna25DPlayerController player,
            IllustratedPlatformSpec spec,
            Texture2D painting,
            Texture2D frontOverlay = null)
        {
            var view = marker.gameObject.AddComponent<IllustratedPlatformView>();
            view.Install(spec, painting, frontOverlay);
            AttachFootAlignment(player, view.Top, spec.index, pinVisibleSole: true);
            genericViews[spec.index] = view;
            return view;
        }

        public IllustratedPlatformView GetGenericView(int index)
        {
            return genericViews.TryGetValue(index, out var view) ? view : null;
        }

        public Bounds GetPaintingBounds(
            DarkRoomPlatformMarker marker,
            Transform bearPainting,
            Transform booksPainting,
            Bounds fallback)
        {
            int idx = marker.spec.index;
            if (genericViews.TryGetValue(idx, out var generic) && generic != null && generic.Painting != null)
            {
                return generic.Painting.bounds;
            }

            switch (idx)
            {
                case 0:
                    return bearPainting.GetComponent<Renderer>().bounds;
                case 1:
                    return booksPainting.GetComponent<Renderer>().bounds;
                case 4:
                    return Blocks != null && Blocks.Painting != null ? Blocks.Painting.bounds : fallback;
                case 5:
                    return Drum != null && Drum.Painting != null ? Drum.Painting.bounds : fallback;
                case 8:
                    return MusicBox != null && MusicBox.Painting != null ? MusicBox.Painting.bounds : fallback;
                case 9:
                    return Castle != null && Castle.Painting != null ? Castle.Painting.bounds : fallback;
                case 10:
                    return TrainTrestle10 != null && TrainTrestle10.Painting != null ? TrainTrestle10.Painting.bounds : fallback;
                default:
                    return fallback;
            }
        }

        public static void AttachFootAlignment(
            Yuna25DPlayerController player,
            BoxCollider surface,
            int supportIndex,
            bool pinVisibleSole)
        {
            var feet = player.gameObject.AddComponent<DresserFootAlignment>();
            feet.player = player;
            feet.surface = surface;
            if (supportIndex >= 0)
            {
                feet.supportIndex = supportIndex;
            }
            feet.pinVisibleSole = pinVisibleSole;
        }
    }
}
