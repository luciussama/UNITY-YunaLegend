using System;
using System.Collections.Generic;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Catálogo puro (orientado a dados, sem dependência de UnityEngine) das plataformas
    /// ilustradas 2.5D das Etapas 2 (10..21) e 3 (22..37) do Quarto Escuro.
    /// </summary>
    public static class DarkRoomFullStageCatalog
    {
        public const float RoomFloorY = -9.1f;
        public const float Stage2InitialSpeed = 4.0f;
        public const float Stage2MaxSpeed = 6.0f;
        public const float Stage2InitialJumpHeight = 2.55f;
        public const float Stage2MaxJumpHeight = 3.40f;

        public const float Stage3InitialSpeed = 4.2f;
        public const float Stage3MaxSpeed = 6.4f;
        public const float Stage3InitialJumpHeight = 2.60f;
        public const float Stage3MaxJumpHeight = 3.60f;
        public const float Stage3ReturnTakeoffRunway = 10.5f;

        private struct RawArtMetrics
        {
            public int Index;
            public string FileName;
            public string SurfaceName;
            public string PaintingName;
            public float LandingPixelY;
            public float FeetPixelY;
            public float CentrePixelX;
            public float LipLeftPixelX;
            public float LipRightPixelX;
            public float ContentPixelWidth;
            public float VisualHeightMetres;
            public float Depth;
            public bool AnchorToFloor;
        }

        private static readonly RawArtMetrics[] Stage2Metrics =
        {
            // 10: mantem o contrato exato de TrainTrestle10Platform
            new RawArtMetrics
            {
                Index = 10, FileName = "Stage2/10-train-trestle-v1.png",
                SurfaceName = "Topo_Real_Trem_10", PaintingName = "Trem_10_Pintura",
                LandingPixelY = 522f, FeetPixelY = 807f, CentrePixelX = 448f,
                LipLeftPixelX = 22f, LipRightPixelX = 881f, ContentPixelWidth = 865f,
                VisualHeightMetres = 5.45f, Depth = 2.0f, AnchorToFloor = true
            },
            new RawArtMetrics
            {
                Index = 11, FileName = "Stage2/11-wall-shelf-v1.png",
                SurfaceName = "Topo_Real_Prateleira_11", PaintingName = "Prateleira_11_Pintura",
                LandingPixelY = 540f, FeetPixelY = 912f, CentrePixelX = 448f,
                LipLeftPixelX = 90f, LipRightPixelX = 808f, ContentPixelWidth = 844f,
                VisualHeightMetres = 2.40f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 12, FileName = "Stage2/12-mushroom-lamp-v1.png",
                SurfaceName = "Topo_Real_Abajur_12", PaintingName = "Abajur_12_Pintura",
                LandingPixelY = 540f, FeetPixelY = 1116f, CentrePixelX = 448f,
                LipLeftPixelX = 165f, LipRightPixelX = 730f, ContentPixelWidth = 783f,
                VisualHeightMetres = 3.60f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 13, FileName = "Stage2/13-dollhouse-roof-v1.png",
                SurfaceName = "Topo_Real_Casa_Bonecas_13", PaintingName = "Casa_Bonecas_13_Pintura",
                LandingPixelY = 435f, FeetPixelY = 951f, CentrePixelX = 448f,
                LipLeftPixelX = 141f, LipRightPixelX = 793f, ContentPixelWidth = 853f,
                VisualHeightMetres = 3.20f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 14, FileName = "Stage2/14-spinning-globe-v1.png",
                SurfaceName = "Topo_Real_Globo_14", PaintingName = "Globo_14_Pintura",
                LandingPixelY = 822f, FeetPixelY = 1145f, CentrePixelX = 448f,
                LipLeftPixelX = 97f, LipRightPixelX = 801f, ContentPixelWidth = 856f,
                VisualHeightMetres = 2.10f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 15, FileName = "Stage2/15-kite-frame-v1.png",
                SurfaceName = "Topo_Real_Pipa_15", PaintingName = "Pipa_15_Pintura",
                LandingPixelY = 385f, FeetPixelY = 1145f, CentrePixelX = 448f,
                LipLeftPixelX = 148f, LipRightPixelX = 720f, ContentPixelWidth = 718f,
                VisualHeightMetres = 4.60f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 16, FileName = "Stage2/16-floating-books-v1.png",
                SurfaceName = "Topo_Real_Livro_Flutuante_16", PaintingName = "Livro_Flutuante_16_Pintura",
                LandingPixelY = 405f, FeetPixelY = 776f, CentrePixelX = 448f,
                LipLeftPixelX = 135f, LipRightPixelX = 761f, ContentPixelWidth = 849f,
                VisualHeightMetres = 2.25f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 17, FileName = "Stage2/17-chandelier-crystals-v1.png",
                SurfaceName = "Topo_Real_Lustre_17", PaintingName = "Lustre_17_Pintura",
                LandingPixelY = 375f, FeetPixelY = 955f, CentrePixelX = 448f,
                LipLeftPixelX = 46f, LipRightPixelX = 850f, ContentPixelWidth = 868f,
                VisualHeightMetres = 3.10f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 18, FileName = "Stage2/18-curtain-rod-v1.png",
                SurfaceName = "Topo_Real_Varao_18", PaintingName = "Varao_18_Pintura",
                LandingPixelY = 500f, FeetPixelY = 772f, CentrePixelX = 448f,
                LipLeftPixelX = 35f, LipRightPixelX = 860f, ContentPixelWidth = 869f,
                VisualHeightMetres = 1.55f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 19, FileName = "Stage2/19-cuckoo-clock-v1.png",
                SurfaceName = "Topo_Real_Cuco_19", PaintingName = "Cuco_19_Pintura",
                LandingPixelY = 611f, FeetPixelY = 1154f, CentrePixelX = 448f,
                LipLeftPixelX = 86f, LipRightPixelX = 816f, ContentPixelWidth = 832f,
                VisualHeightMetres = 3.10f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 20, FileName = "Stage2/20-wardrobe-ledge-v1.png",
                SurfaceName = "Topo_Real_Guarda_Roupa_20", PaintingName = "Guarda_Roupa_20_Pintura",
                LandingPixelY = 322f, FeetPixelY = 1199f, CentrePixelX = 448f,
                LipLeftPixelX = 54f, LipRightPixelX = 855f, ContentPixelWidth = 852f,
                VisualHeightMetres = 4.90f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 21, FileName = "Stage2/21-false-door-portal-v1.png",
                SurfaceName = "Topo_Real_Porta_Falsa_21", PaintingName = "Porta_Falsa_21_Pintura",
                LandingPixelY = 705f, FeetPixelY = 1029f, CentrePixelX = 448f,
                LipLeftPixelX = 113f, LipRightPixelX = 792f, ContentPixelWidth = 751f,
                VisualHeightMetres = 2.65f, Depth = 2.0f, AnchorToFloor = false
            }
        };

        private static readonly RawArtMetrics[] Stage3Metrics =
        {
            new RawArtMetrics
            {
                Index = 22, FileName = "Stage3/22-fallen-blocks-v1.png",
                SurfaceName = "Topo_Real_Blocos_Tombados_22", PaintingName = "Blocos_Tombados_22_Pintura",
                LandingPixelY = 309f, FeetPixelY = 1127f, CentrePixelX = 448f,
                LipLeftPixelX = 99f, LipRightPixelX = 810f, ContentPixelWidth = 784f,
                VisualHeightMetres = 2.45f, Depth = 1.8f, AnchorToFloor = true
            },
            new RawArtMetrics
            {
                Index = 23, FileName = "Stage3/23-rag-doll-v1.png",
                SurfaceName = "Topo_Real_Boneca_Pano_23", PaintingName = "Boneca_Pano_23_Pintura",
                LandingPixelY = 630f, FeetPixelY = 1007f, CentrePixelX = 448f,
                LipLeftPixelX = 138f, LipRightPixelX = 782f, ContentPixelWidth = 789f,
                VisualHeightMetres = 1.90f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 24, FileName = "Stage3/24-crayon-box-v1.png",
                SurfaceName = "Topo_Real_Giz_Cera_24", PaintingName = "Giz_Cera_24_Pintura",
                LandingPixelY = 480f, FeetPixelY = 889f, CentrePixelX = 448f,
                LipLeftPixelX = 80f, LipRightPixelX = 650f, ContentPixelWidth = 821f,
                VisualHeightMetres = 2.35f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 25, FileName = "Stage3/25-crooked-books-v1.png",
                SurfaceName = "Topo_Real_Contos_Tortos_25", PaintingName = "Contos_Tortos_25_Pintura",
                LandingPixelY = 300f, FeetPixelY = 1103f, CentrePixelX = 448f,
                LipLeftPixelX = 203f, LipRightPixelX = 705f, ContentPixelWidth = 697f,
                VisualHeightMetres = 4.20f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 26, FileName = "Stage3/26-dented-drum-v1.png",
                SurfaceName = "Topo_Real_Tambor_Amassado_26", PaintingName = "Tambor_Amassado_26_Pintura",
                LandingPixelY = 315f, FeetPixelY = 1030f, CentrePixelX = 448f,
                LipLeftPixelX = 160f, LipRightPixelX = 735f, ContentPixelWidth = 731f,
                VisualHeightMetres = 3.80f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 27, FileName = "Stage3/27-collapsed-bear-v1.png",
                SurfaceName = "Topo_Real_Urso_Desmoronado_27", PaintingName = "Urso_Desmoronado_27_Pintura",
                LandingPixelY = 355f, FeetPixelY = 897f, CentrePixelX = 448f,
                LipLeftPixelX = 360f, LipRightPixelX = 635f, ContentPixelWidth = 767f,
                VisualHeightMetres = 4.20f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 28, FileName = "Stage3/28-xylophone-v1.png",
                SurfaceName = "Topo_Real_Xilofone_28", PaintingName = "Xilofone_28_Pintura",
                LandingPixelY = 465f, FeetPixelY = 881f, CentrePixelX = 448f,
                LipLeftPixelX = 121f, LipRightPixelX = 774f, ContentPixelWidth = 771f,
                VisualHeightMetres = 2.05f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 29, FileName = "Stage3/29-derailed-locomotive-v1.png",
                SurfaceName = "Topo_Real_Locomotiva_29", PaintingName = "Locomotiva_29_Pintura",
                LandingPixelY = 405f, FeetPixelY = 966f, CentrePixelX = 448f,
                LipLeftPixelX = 176f, LipRightPixelX = 747f, ContentPixelWidth = 740f,
                VisualHeightMetres = 2.95f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 30, FileName = "Stage3/30-card-castle-v1.png",
                SurfaceName = "Topo_Real_Castelo_Cartas_30", PaintingName = "Castelo_Cartas_30_Pintura",
                LandingPixelY = 191f, FeetPixelY = 1113f, CentrePixelX = 448f,
                LipLeftPixelX = 105f, LipRightPixelX = 803f, ContentPixelWidth = 715f,
                VisualHeightMetres = 4.10f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 31, FileName = "Stage3/31-leaning-music-box-v1.png",
                SurfaceName = "Topo_Real_Caixa_Desregulada_31", PaintingName = "Caixa_Desregulada_31_Pintura",
                LandingPixelY = 501f, FeetPixelY = 969f, CentrePixelX = 448f,
                LipLeftPixelX = 63f, LipRightPixelX = 818f, ContentPixelWidth = 778f,
                VisualHeightMetres = 2.15f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 32, FileName = "Stage3/32-loose-robot-v1.png",
                SurfaceName = "Topo_Real_Robo_32", PaintingName = "Robo_32_Pintura",
                LandingPixelY = 420f, FeetPixelY = 1050f, CentrePixelX = 448f,
                LipLeftPixelX = 90f, LipRightPixelX = 806f, ContentPixelWidth = 716f,
                VisualHeightMetres = 2.85f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 33, FileName = "Stage3/33-spinning-top-v1.png",
                SurfaceName = "Topo_Real_Piao_33", PaintingName = "Piao_33_Pintura",
                LandingPixelY = 340f, FeetPixelY = 1040f, CentrePixelX = 448f,
                LipLeftPixelX = 100f, LipRightPixelX = 796f, ContentPixelWidth = 696f,
                VisualHeightMetres = 3.15f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 34, FileName = "Stage3/34-floating-spool-v1.png",
                SurfaceName = "Topo_Real_Carretel_34", PaintingName = "Carretel_34_Pintura",
                LandingPixelY = 454f, FeetPixelY = 1095f, CentrePixelX = 448f,
                LipLeftPixelX = 121f, LipRightPixelX = 776f, ContentPixelWidth = 666f,
                VisualHeightMetres = 2.95f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 35, FileName = "Stage3/35-unbalanced-mobile-v1.png",
                SurfaceName = "Topo_Real_Mobile_35", PaintingName = "Mobile_35_Pintura",
                LandingPixelY = 360f, FeetPixelY = 980f, CentrePixelX = 448f,
                LipLeftPixelX = 70f, LipRightPixelX = 826f, ContentPixelWidth = 796f,
                VisualHeightMetres = 2.75f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 36, FileName = "Stage3/36-spell-book-v1.png",
                SurfaceName = "Topo_Real_Grimorio_36", PaintingName = "Grimorio_36_Pintura",
                LandingPixelY = 380f, FeetPixelY = 820f, CentrePixelX = 448f,
                LipLeftPixelX = 60f, LipRightPixelX = 836f, ContentPixelWidth = 816f,
                VisualHeightMetres = 1.95f, Depth = 1.8f, AnchorToFloor = false
            },
            new RawArtMetrics
            {
                Index = 37, FileName = "Stage3/37-true-dream-portal-v1.png",
                SurfaceName = "Topo_Real_Portal_Verdadeiro_37", PaintingName = "Portal_Verdadeiro_37_Pintura",
                LandingPixelY = 680f, FeetPixelY = 1060f, CentrePixelX = 448f,
                LipLeftPixelX = 40f, LipRightPixelX = 856f, ContentPixelWidth = 816f,
                VisualHeightMetres = 2.40f, Depth = 2.2f, AnchorToFloor = false
            }
        };

        public static string GetTextureAssetPath(int index)
        {
            const string root = "Assets/Art/Illustrated25D/Environment/";
            foreach (var m in Stage2Metrics)
            {
                if (m.Index == index)
                {
                    return root + m.FileName;
                }
            }
            foreach (var m in Stage3Metrics)
            {
                if (m.Index == index)
                {
                    return root + m.FileName;
                }
            }
            return null;
        }

        public static IllustratedPlatformSpec[] BuildSpecsUpTo(int maxPlatformIndex)
        {
            var result = new List<IllustratedPlatformSpec>();
            if (maxPlatformIndex < 10)
            {
                return result.ToArray();
            }

            // Plataforma 10 (âncora inicial da Etapa 2)
            var spec10 = BuildSpec(Stage2Metrics[0], RoomFloorY, 0f);
            float ppm10 = spec10.PixelsPerMetre;
            float supportOffset10 = spec10.SupportOffsetX;
            float supportWidth10 = spec10.SupportWidth;
            spec10.targetX = 62.07f + supportWidth10 * 0.5f - supportOffset10;
            result.Add(spec10);

            IllustratedPlatformSpec prev = spec10;
            int stage2Count = Stage2Metrics.Length;
            for (int i = 1; i < stage2Count; i++)
            {
                var m = Stage2Metrics[i];
                if (m.Index > maxPlatformIndex)
                {
                    break;
                }

                float prevProgress = (float)(i - 1) / (stage2Count - 2);
                float gap = Lerp(2.35f, 4.15f, prevProgress);
                float stepY = Lerp(0.32f, 0.46f, prevProgress) + (m.Index == 21 ? 0.25f : 0f);
                float nextTopY = prev.TopY + stepY;
                float nextFloorY = nextTopY - m.VisualHeightMetres;

                var temp = BuildSpec(m, nextFloorY, 0f);
                float prevSupportMaxX = prev.targetX + prev.SupportOffsetX + prev.SupportWidth * 0.5f;
                float targetMinSupportX = prevSupportMaxX + gap;
                temp.targetX = targetMinSupportX + temp.SupportWidth * 0.5f - temp.SupportOffsetX;
                result.Add(temp);
                prev = temp;
            }

            if (maxPlatformIndex < 22)
            {
                return result.ToArray();
            }

            // Etapa 3 (22..37): subida caótica reversa da direita para a esquerda
            // Começa após a queda da Porta Falsa (21), com pista de corrida de retorno
            IllustratedPlatformSpec portal21 = prev;
            int stage3Count = Stage3Metrics.Length;
            for (int i = 0; i < stage3Count; i++)
            {
                var m = Stage3Metrics[i];
                if (m.Index > maxPlatformIndex)
                {
                    break;
                }

                if (i == 0)
                {
                    float firstTopY = RoomFloorY + m.VisualHeightMetres;
                    var first = BuildSpec(m, RoomFloorY, 0f);
                    float firstSupportMaxX = portal21.targetX - Stage3ReturnTakeoffRunway;
                    first.targetX = firstSupportMaxX - first.SupportWidth * 0.5f - first.SupportOffsetX;
                    result.Add(first);
                    prev = first;
                }
                else
                {
                    float prevProgress = (float)(i - 1) / (stage3Count - 2);
                    float gap = Lerp(1.95f, 4.05f, prevProgress);
                    float stepY = Lerp(0.36f, 0.45f, prevProgress);
                    float nextTopY = prev.TopY + stepY;
                    float nextFloorY = nextTopY - m.VisualHeightMetres;

                    var temp = BuildSpec(m, nextFloorY, 0f);
                    float prevSupportMinX = prev.targetX + prev.SupportOffsetX - prev.SupportWidth * 0.5f;
                    float targetMaxSupportX = prevSupportMinX - gap;
                    temp.targetX = targetMaxSupportX - temp.SupportWidth * 0.5f - temp.SupportOffsetX;
                    result.Add(temp);
                    prev = temp;
                }
            }

            return result.ToArray();
        }

        private static IllustratedPlatformSpec BuildSpec(RawArtMetrics m, float floorY, float targetX)
        {
            return new IllustratedPlatformSpec
            {
                index = m.Index,
                surfaceObjectName = m.SurfaceName,
                paintingObjectName = m.PaintingName,
                floorY = floorY,
                landingPixelY = m.LandingPixelY,
                feetPixelY = m.FeetPixelY,
                centrePixelX = m.CentrePixelX,
                lipLeftPixelX = m.LipLeftPixelX,
                lipRightPixelX = m.LipRightPixelX,
                contentPixelWidth = m.ContentPixelWidth,
                targetX = targetX,
                targetHeight = m.VisualHeightMetres,
                depth = m.Depth
            };
        }

        private static float Lerp(float a, float b, float t)
        {
            if (t <= 0f) return a;
            if (t >= 1f) return b;
            return a + (b - a) * t;
        }
    }
}
