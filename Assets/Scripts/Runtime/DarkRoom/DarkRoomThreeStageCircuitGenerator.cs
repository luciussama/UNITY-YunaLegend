using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public static class DarkRoomThreeStageCircuitGenerator
    {
        // Authored contact anchors preserve the approved rhythm while objects keep
        // their own dimensions. Distances/clearances are measured from real support.
        public const float Stage2OpeningGap = 2.115f;
        private static readonly float[] Stage1ContactX = {0.012f,4.070f};
        private static readonly float[] Stage1Gaps = {0f,1.25f,1.35f,1.2f,1.315f,1.185f,1.105f,1.06f,.985f};
        private static readonly float[] Stage1ContactY = {0f,-7.55f,-6.8f,-6.05f,-4.87f,-4.55f,-4.23f,-3.91f,-3.59f,-3.27f};

        public static Vector2 Stage2Origin(PlatformSpec lastStage1)
        {
            var last=DarkRoomPlatformGeometry.GetSupportProfile(lastStage1);
            var first=GenerateStage2Platforms(0f,0f)[0];
            var next=DarkRoomPlatformGeometry.GetSupportProfile(first);
            return new Vector2(last.MaxX+Stage2OpeningGap-next.MinX,
                last.PointNear(lastStage1.x+lastStage1.contact.offsetX).y+.295f-next.PointNear(first.contact.offsetX).y);
        }

        private static float NextRootX(PlatformSpec from,PlatformSpec next,float gap,float direction)
        {
            var a=DarkRoomPlatformGeometry.GetSupportProfile(from);
            var b=DarkRoomPlatformGeometry.GetSupportProfile(next);
            return direction>0f?a.MaxX+gap-b.MinX:a.MinX-gap-b.MaxX;
        }

        // -------------------------------------------------------------
        // ETAPA 1: A Subida ao Castelo (0 a 9) - Calma e Acolhedora
        // Velocidade base: 3.6 m/s, pulo base: 2.4m, gap uniforme: 1.4m.
        // Culmina na Plataforma 09 (Castelinho de Blocos), onde ocorre a cutscene.
        // -------------------------------------------------------------
        public static List<PlatformSpec> GenerateStage1Platforms()
        {
            var list = new List<PlatformSpec>();
            string[] names = new[]
            {
                "00_Cabeca_Urso_Pelucia",
                "01_Pilha_Livros_Ilustrados",
                "02_Penteadeira_Encantada",
                "03_Comoda_Pequena_Carvalho",
                "04_Blocos_ABC_Madeira",
                "05_Tamborito_Marcha",
                "06_Almofadao_Veludo_Roxo",
                "07_Gavetas_Abertas_Armario",
                "08_Caixa_Musica_Bailarina",
                "09_Castelinho_Blocos_Descanso"
            };

            for (int i = 0; i < names.Length; i++)
            {
                var geometry=DarkRoomPlatformGeometry.Get(i);
                var spec=new PlatformSpec(i,names[i],DarkRoomPhase.Phase1Ascent,0,0,
                    geometry.Width,geometry.Depth,geometry.Height,i==9?new Color(.25f,.45f,.75f):Color.HSVToRGB(i*.08f,.5f,.85f),"Etapa 1 - Subida Calma");
                spec.contact=DarkRoomPlatformGeometry.GetContact(i);
                var local=DarkRoomPlatformGeometry.GetSupportProfile(spec);
                Vector3 primary=local.PointNear(spec.contact.offsetX);
                float targetY=i==0?DarkRoomPlatformGeometry.FloorSurfaceY+geometry.Height:Stage1ContactY[i];
                spec.x=i<2?DarkRoomStartLayout.FirstPlatformX+Stage1ContactX[i]-primary.x
                    :NextRootX(list[i-1],spec,Stage1Gaps[i-1],1f);
                spec.y=targetY-primary.y;
                list.Add(spec);
            }

            return list;
        }

        // -------------------------------------------------------------
        // ETAPA 2: A Fuga Acelerada (10 a 21) - Progressao de Velocidade
        // Velocidade escala de 4.0 m/s ate 6.0 m/s. Pulo escala de 2.5m ate 3.4m.
        // Culmina na Plataforma 21: O Poster da Porta Falsa na parede (x ~ 90.5m, y ~ 1.58m).
        // Conexao com Castelo: começa exatamente apos o fim do Castelinho (x=41.10m) + gap 1.6m.
        // -------------------------------------------------------------
        public static List<PlatformSpec> GenerateStage2Platforms(float? startX = null, float? startY = null)
        {
            var list = new List<PlatformSpec>();
            string[] names = new[]
            {
                "10_Pista_Elevada_Trem",
                "11_Prateleira_Brinquedos",
                "12_Abajur_Cogumelo",
                "13_Telhado_Casa_Bonecas",
                "14_Globo_Terrestre",
                "15_Pipa_Encantada_Bambu",
                "16_Livro_Gravuras_No_Ar",
                "17_Lustre_Cristais",
                "18_Varao_Cortina_Estrelada",
                "19_Relogio_Cuco_Vintage",
                "20_Beiral_Guarda_Roupa",
                "21_Poster_Porta_Falsa_Parede"
            };

            if (!startX.HasValue || !startY.HasValue)
            {
                var firstSection=GenerateStage1Platforms();
                var origin=Stage2Origin(firstSection[firstSection.Count-1]);
                startX=startX??origin.x;startY=startY??origin.y;
            }
            float currentX=startX.Value;
            float supportY=startY.Value+.325f; // Authored first contact anchor, not a volume-height module.
            for(int i=0;i<names.Length;i++)
            {
                int index=10+i;float progress=(float)i/(names.Length-1);
                float w=i==11?3.8f:Mathf.Lerp(2.2f,1.8f,progress), h=i==11?1.4f:.65f;
                var spec=new PlatformSpec(index,names[i],DarkRoomPhase.Phase2Escape,0,0,w,1.8f,h,
                    i==11?new Color(.95f,.30f,.25f):Color.HSVToRGB(.1f+progress*.4f,.7f,.9f),"Etapa 2 - Fuga Acelerada");
                spec.contact=DarkRoomPlatformGeometry.GetContact(index);
                var local=DarkRoomPlatformGeometry.GetSupportProfile(spec);
                spec.x=currentX;spec.y=supportY-local.PointNear(spec.contact.offsetX).y;
                list.Add(spec);
                if(i<names.Length-1)
                {
                    float nextProgress=(float)(i+1)/(names.Length-1);
                    var next=new PlatformSpec(index+1,"",spec.phase,0,0,i+1==11?3.8f:Mathf.Lerp(2.2f,1.8f,nextProgress),1.8f,i+1==11?1.4f:.65f,Color.white,"");
                    next.contact=DarkRoomPlatformGeometry.GetContact(index+1);
                    currentX=NextRootX(spec,next,Mathf.Lerp(2.32f,4.75f,progress),1f);
                    supportY+=Mathf.Lerp(.32f,.48f,progress)+(i+1==11?.375f:0f);
                }
            }

            return list;
        }

        // -------------------------------------------------------------
        // ETAPA 3: A Subida Caotica Reversa (22 a 37) - Direita para Esquerda
        // Acentuacao progressiva: plataformas menores (2.2m -> 1.27m), gaps maiores (1.8m -> 4.6m).
        // Velocidade escala de 4.2 ate 6.6 m/s, pulo escala de 2.6m ate 3.9m.
        // Inicia no chao apos a queda da porta falsa (wallX - 3.2m), subindo para a ESQUERDA.
        // Termina na Plataforma 37: O Verdadeiro Portal dos Sonhos (largura 4.5m).
        // -------------------------------------------------------------
        public static List<PlatformSpec> GenerateStage3Platforms(float? wallX = null, float floorDropY = DarkRoomReturnLayout.FloorY)
        {
            var list = new List<PlatformSpec>();
            string[] names = new[]
            {
                "22_Pilha_Blocos_Tombada",
                "23_Boneca_Pano_Desconjuntada",
                "24_Caixa_Giz_Aberta",
                "25_Pilha_Torta_Contos",
                "26_Tamborzinho_Amassado",
                "27_Urso_Pelucia_Desmoronado",
                "28_Xilofone_Colorido_Inclinado",
                "29_Locomotiva_Descarrilada",
                "30_Castelo_Cartas_Bamboleante",
                "31_Caixa_Musica_Desregulada",
                "32_Robo_Lata_Desparafusado",
                "33_Piao_Madeira_Rodopiante",
                "34_Carretel_Fita_Flutuante",
                "35_Mobile_Desequilibrado",
                "36_Livro_Feiticos_No_Vacuo",
                "37_O_Verdadeiro_Portal_Sonhos"
            };

            if (!wallX.HasValue)
            {
                var escape=GenerateStage2Platforms();wallX=escape[escape.Count-1].x;
            }
            float currentX=wallX.Value-DarkRoomReturnLayout.FirstPlatformOffset;
            float supportY=floorDropY+.70f;
            for(int i=0;i<names.Length;i++)
            {
                int index=22+i;float progress=(float)i/(names.Length-1);
                float w=i==15?4.5f:Mathf.Lerp(DarkRoomReturnLayout.FirstPlatformWidth,DarkRoomReturnLayout.EndPlatformWidth,progress),h=i==15?1.4f:.6f;
                var spec=new PlatformSpec(index,names[i],DarkRoomPhase.Phase3ChaoticAscent,0,0,w,1.8f,h,
                    i==15?new Color(1f,.9f,.4f):Color.HSVToRGB(.55f+progress*.4f,.75f,.85f),"Etapa 3 - Subida Caotica Reversa");
                spec.contact=DarkRoomPlatformGeometry.GetContact(index);
                var local=DarkRoomPlatformGeometry.GetSupportProfile(spec);
                spec.x=currentX;spec.y=supportY-local.PointNear(spec.contact.offsetX).y;
                list.Add(spec);
                if(i<names.Length-1)
                {
                    float nextProgress=(float)(i+1)/(names.Length-1);
                    var next=new PlatformSpec(index+1,"",spec.phase,0,0,i+1==15?4.5f:Mathf.Lerp(DarkRoomReturnLayout.FirstPlatformWidth,DarkRoomReturnLayout.EndPlatformWidth,nextProgress),1.8f,i+1==15?1.4f:.6f,Color.white,"");
                    next.contact=DarkRoomPlatformGeometry.GetContact(index+1);
                    currentX=NextRootX(spec,next,Mathf.Lerp(1.90f,4.65f,progress),-1f);
                    supportY+=Mathf.Lerp(.36f,.46f,progress);
                }
            }

            return list;
        }
    }
}
