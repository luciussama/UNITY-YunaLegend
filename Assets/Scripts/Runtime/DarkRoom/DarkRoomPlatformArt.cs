using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    /// <summary>Ordem dos materiais usados pelos modelos dedicados das plataformas.</summary>
    public enum PlatformArtMaterialIndex
    {
        Fur = 0,
        Muzzle = 1,
        Wood = 2,
        Velvet = 3,
        Button = 4,
        Thread = 5,
        Dark = 6,
        BookBrown = 7,
        BookBurgundy = 8,
        BookGreen = 9,
        BookDark = 10,
        BookPages = 11,
        BookGold = 12,
        BookRibbon = 13,
        BookOchre = 14,
        BookSpine = 15,
        BookCover = 16
    }

    /// <summary>
    /// Modelos dedicados das plataformas jogaveis, construidos por composicao de malhas
    /// nativas (sem assets externos) e fiéis a arte conceitual aprovada.
    /// Cada modelo herda as dimensoes canonicas de <see cref="DarkRoomPlatformGeometry"/>
    /// e encosta a superficie de pouso na face superior do colisor de contato.
    /// </summary>
    public static class DarkRoomPlatformArt
    {
        private static readonly Dictionary<string, Mesh> MeshCache = new Dictionary<string, Mesh>();
        private static readonly Dictionary<PrimitiveType, Mesh> PrimitiveCache = new Dictionary<PrimitiveType, Mesh>();

        public static GameObject Build(PlatformGeometry geometry, PlatformContactSurface contact, Material[] materials, Transform parent)
        {
            if (!geometry.HasModel) return null;

            switch (geometry.ArtKey)
            {
                case DarkRoomPlatformGeometry.TeddyHeadArtKey:
                    return BuildTeddyBear(geometry, materials, parent);
                case DarkRoomPlatformGeometry.BookStackArtKey:
                    return BuildBookStack(geometry, contact, materials, parent);
                default:
                    return null;
            }
        }

        private static Material Mat(Material[] materials, PlatformArtMaterialIndex index)
        {
            return materials != null && (int)index < materials.Length ? materials[(int)index] : null;
        }

        /// <summary>
        /// Plataforma 00 - cabeca do urso de pelucia apoiada em pe cilindrica de madeira,
        /// conforme a arte conceitual: pelucia marrom quente, orelhas redondas com interno
        /// creme, olhos de botao com costura em X, focinho creme costurado, nariz escuro,
        /// sorriso e laco vermelho.
        /// </summary>
        private static GameObject BuildTeddyBear(PlatformGeometry geometry, Material[] materials, Transform parent)
        {
            Material fur = Mat(materials, PlatformArtMaterialIndex.Fur);
            Material muzzle = Mat(materials, PlatformArtMaterialIndex.Muzzle);
            Material wood = Mat(materials, PlatformArtMaterialIndex.Wood);
            Material velvet = Mat(materials, PlatformArtMaterialIndex.Velvet);
            Material button = Mat(materials, PlatformArtMaterialIndex.Button);
            Material thread = Mat(materials, PlatformArtMaterialIndex.Thread);
            Material dark = Mat(materials, PlatformArtMaterialIndex.Dark);

            float w = geometry.Width;
            float h = geometry.Height;
            float d = geometry.Depth;

            float bottom = -h * 0.5f;
            float top = h * 0.5f;

            float plinthHeight = Mathf.Min(0.32f, h * 0.30f);
            float headBottom = bottom + plinthHeight;
            float headHeight = top - headBottom;
            float headCenterY = (headBottom + top) * 0.5f;
            float headDepth = Mathf.Min(d * 0.74f, 1.65f);

            float rx = w * 0.5f;
            float ry = headHeight * 0.5f / .88f; // Match the virtual radius used by the flattened mesh.
            float rz = headDepth * 0.5f;
            const float headSquareness = 0.90f;
            const float headTopFlatten = 0.12f;

            var root = new GameObject("Visual_3D");
            root.transform.SetParent(parent, false);
            var stitches = new StitchBuilder();

            // ---- Banqueta de madeira: mais larga que o urso, topo plano e circular ----
            float plinthRadiusX = w * 0.5f + 0.26f;
            float plinthRadiusZ = Mathf.Min(headDepth * 0.50f, 0.84f);
            float bodyHeight = plinthHeight * 0.80f;
            float rimHeight = plinthHeight - bodyHeight;
            AddPart(root.transform, "Base_Madeira", BeveledPlinthMesh(plinthRadiusX, plinthRadiusZ, bodyHeight, .025f), wood,
                new Vector3(0f, bottom + bodyHeight * 0.5f, 0f), Quaternion.identity, Vector3.one);
            AddPart(root.transform, "Base_Aro", BeveledPlinthMesh(plinthRadiusX + .03f, plinthRadiusZ + .03f, rimHeight, .015f), wood,
                new Vector3(0f, bottom + bodyHeight + rimHeight * 0.5f, 0f), Quaternion.identity, Vector3.one);

            // ---- Cabeca de pelucia: massa redonda com topo plano de pouso ----
            AddPart(root.transform, "Cabeca_Urso", CushionMesh(w, headHeight, headDepth, headSquareness, headTopFlatten), fur,
                new Vector3(0f, headCenterY, 0f), Quaternion.identity, Vector3.one);

            // ---- Orelhas redondas nos cantos superiores ----
            float earRadius = w * 0.16f;
            for (int side = -1; side <= 1; side += 2)
            {
                string suffix = side < 0 ? "Esq" : "Dir";
                var earCenter = new Vector3(side * w * 0.44f, top - earRadius * 0.55f, -0.05f);
                AddPart(root.transform, $"Orelha_{suffix}", PrimitiveMesh(PrimitiveType.Sphere), fur,
                    earCenter, Quaternion.identity,
                    new Vector3(earRadius * 2f, earRadius * 2f, earRadius * 0.85f));
                AddPart(root.transform, $"Orelha_Interna_{suffix}", PrimitiveMesh(PrimitiveType.Sphere), muzzle,
                    earCenter + new Vector3(0f, earRadius * 0.04f, -earRadius * 0.34f), Quaternion.identity,
                    new Vector3(earRadius * 1.05f, earRadius * 1.05f, earRadius * 0.30f));

                // Costura tracejada acompanhando o interno da orelha.
                const int earDashes = 12;
                for (int i = 0; i < earDashes; i++)
                {
                    float a = Mathf.PI * 2f * i / earDashes;
                    float ex = Mathf.Cos(a) * earRadius * .62f;
                    float ey = Mathf.Sin(a) * earRadius * .62f + earRadius * .06f;
                    float ez = -earRadius * .425f * Mathf.Sqrt(Mathf.Max(0f, 1f-(ex*ex+ey*ey)/(earRadius*earRadius)));
                    var normal = new Vector3(ex, ey, ez/(.425f*.425f)).normalized;
                    var p = earCenter + new Vector3(ex,ey,ez) + normal*.007f;
                    stitches.AddQuad(p, Quaternion.FromToRotation(Vector3.back,normal)*Quaternion.Euler(0f,0f,a*Mathf.Rad2Deg+90f), .020f,.055f);
                }
            }

            // ---- Focinho creme volumoso ----
            float muzzleWidth = w * 0.34f;
            float muzzleHeight = headHeight * 0.55f;
            float muzzleZ = -headDepth * 0.5f + 0.055f;
            float muzzleCenterY = headCenterY - headHeight * 0.12f;
            var muzzleCenter = new Vector3(0f, muzzleCenterY, muzzleZ - 0.10f);
            AddPart(root.transform, "Focinho_Urso", CushionMesh(muzzleWidth, muzzleHeight, 0.30f, 0.42f), muzzle,
                muzzleCenter, Quaternion.identity, Vector3.one);

            // Costura tracejada em volta do focinho.
            const int muzzleDashes = 26;
            for (int i = 0; i < muzzleDashes; i++)
            {
                float a = Mathf.PI * 2f * i / muzzleDashes;
                var p = muzzleCenter + new Vector3(
                    Mathf.Cos(a) * muzzleWidth * 0.44f,
                    Mathf.Sin(a) * muzzleHeight * 0.42f,
                    -0.155f);
                stitches.AddQuad(p, Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f), 0.016f, 0.045f);
            }

            // ---- Nariz escuro com brilho ----
            float noseY = muzzleCenterY + muzzleHeight * 0.30f;
            AddPart(root.transform, "Nariz_Urso", CushionMesh(0.26f, 0.16f, 0.13f, 0.55f), dark,
                new Vector3(0f, noseY, muzzleCenter.z - 0.145f),
                Quaternion.identity, Vector3.one);
            AddPart(root.transform, "Brilho_Nariz", PrimitiveMesh(PrimitiveType.Sphere), muzzle,
                new Vector3(0.055f, noseY + 0.045f, muzzleCenter.z - 0.218f),
                Quaternion.identity, new Vector3(0.062f, 0.034f, 0.020f));

            // ---- Olhos de botao com costura em X ----
            float eyeZ = -headDepth * 0.5f - 0.012f;
            for (int side = -1; side <= 1; side += 2)
            {
                string suffix = side < 0 ? "Esq" : "Dir";
                var eye = new Vector3(side * w * 0.175f, headCenterY + headHeight * 0.10f, eyeZ);
                AddPart(root.transform, $"Olho_{suffix}", DiscMesh(0.145f, 28), button,
                    eye, Quaternion.identity, Vector3.one);
            }

            // ---- Sorriso costurado ----
            float smileY = muzzleCenterY + muzzleHeight * 0.02f;
            float smileZ = muzzleCenter.z - 0.152f;
            stitches.AddQuad(new Vector3(0f, smileY - 0.045f, smileZ), Quaternion.identity, 0.026f, 0.065f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    float t = (i + 0.5f) / 5f;
                    float x = side * Mathf.Lerp(0.030f, 0.150f, t);
                    float y = smileY - 0.045f - Mathf.Sin(t * Mathf.PI * 0.5f) * 0.018f + t * 0.062f;
                    float angle = side * Mathf.Lerp(55f, 20f, t);
                    stitches.AddQuad(new Vector3(x, y, smileZ), Quaternion.Euler(0f, 0f, angle), 0.024f, 0.042f);
                }
            }

            // ---- Laco vermelho grande sobre o plinto ----
            float bowY = headBottom + 0.01f;
            float bowZ = Mathf.Min(muzzleCenter.z - 0.06f, -(plinthRadiusZ + 0.08f));
            for (int side = -1; side <= 1; side += 2)
            {
                string suffix = side < 0 ? "Esq" : "Dir";
                AddPart(root.transform, $"Laco_Ala_{suffix}", CushionMesh(w * 0.19f, h * 0.24f, 0.22f, 0.50f), velvet,
                    new Vector3(side * w * 0.12f, bowY + h * 0.03f, bowZ),
                    Quaternion.Euler(0f, 0f, side * -18f), Vector3.one);
            }
            AddPart(root.transform, "Laco_No", CushionMesh(w * 0.072f, h * 0.155f, 0.21f, 0.55f), velvet,
                new Vector3(0f, bowY + h * 0.03f, bowZ - 0.02f), Quaternion.identity, Vector3.one);

            // ---- Emendas costuradas da pelucia ----
            AddSeam(stitches, new Vector2(0f, top - 0.03f), new Vector2(0f, headCenterY + headHeight * 0.30f), headCenterY, rx, ry, rz, headSquareness, 8, true);
            for (int side = -1; side <= 1; side += 2)
            {
                AddSeam(stitches,
                    new Vector2(side * w * 0.335f, top - 0.06f),
                    new Vector2(side * w * 0.335f, headBottom + headHeight * 0.28f),
                    headCenterY, rx, ry, rz, headSquareness, 12, false);
            }
            // Costura superior acompanhando a coroa.
            const int crownDashes = 30;
            for (int i = 0; i <= crownDashes; i++)
            {
                float t = (float)i / crownDashes;
                float x = Mathf.Lerp(-w * 0.42f, w * 0.42f, t);
                float edge = headCenterY + ry * Mathf.Pow(Mathf.Max(0f, 1f-Mathf.Pow(Mathf.Abs(x/rx),2f/headSquareness)), headSquareness*.5f);
                float y = Mathf.Min(top-.055f, edge-.04f);
                AddSurfaceStitch(stitches,x,y,headCenterY,rx,ry,rz,headSquareness,90f,.016f,.05f);
            }

            // ---- Remendo costurado na bochecha esquerda ----
            var patchCenter = new Vector3(-w * 0.255f, headCenterY + headHeight * 0.10f, 0f);
            patchCenter.z = FrontZ(patchCenter.x, patchCenter.y, headCenterY, rx, ry, rz, headSquareness) - 0.006f;
            for (int i=0;i<16;i++)
            {
                float angle=i*Mathf.PI*2f/16;
                AddSurfaceStitch(stitches,patchCenter.x+.15f*Mathf.Cos(angle),patchCenter.y+.13f*Mathf.Sin(angle),
                    headCenterY,rx,ry,rz,headSquareness,angle*Mathf.Rad2Deg,.022f,.055f);
            }

            if (stitches.Count > 0)
            {
                var stitchGo = new GameObject("Costuras");
                stitchGo.transform.SetParent(root.transform, false);
                stitchGo.AddComponent<MeshFilter>().sharedMesh = stitches.Build("costuras-urso");
                var stitchRenderer = stitchGo.AddComponent<MeshRenderer>();
                stitchRenderer.sharedMaterial = thread;
            }

            return root;
        }

        /// <summary>
        /// Plataforma 01 - pilha diagonal de livros.
        /// Cinco volumes empilhados com deslocamento e inclinacao progressivos; o tampo do
        /// livro de cima e a superficie de pouso declarada em PlatformContactSurface.
        /// </summary>
        private static GameObject BuildBookStack(PlatformGeometry geometry, PlatformContactSurface contact, Material[] materials, Transform parent)
        {
            Material brown = Mat(materials, PlatformArtMaterialIndex.BookBrown);
            Material burgundy = Mat(materials, PlatformArtMaterialIndex.BookBurgundy);
            Material green = Mat(materials, PlatformArtMaterialIndex.BookGreen);
            Material dark = Mat(materials, PlatformArtMaterialIndex.BookDark);
            Material pages = Mat(materials, PlatformArtMaterialIndex.BookPages);
            Material gold = Mat(materials, PlatformArtMaterialIndex.BookGold);
            Material ribbon = Mat(materials, PlatformArtMaterialIndex.BookRibbon);
            Material ochre = Mat(materials, PlatformArtMaterialIndex.BookOchre);
            Material spineAtlas = Mat(materials, PlatformArtMaterialIndex.BookSpine);
            Material coverPaint = Mat(materials, PlatformArtMaterialIndex.BookCover);

            float w = geometry.Width;
            float h = geometry.Height;
            float d = Mathf.Min(geometry.Depth, 1.30f);
            float bottom = -h * 0.5f;

            var root = new GameObject("Visual_3D");
            root.transform.SetParent(parent, false);

            var leathers = new[] { dark, burgundy, green, brown, ochre };
            float[] widths = { w * 0.875f, w * 0.91f, w * 0.86f, w * 0.82f, w * 0.77f };
            float[] offsets = { -0.20f, -0.10f, 0.02f, 0.13f, 0.245f };
            float[] tilts = { -0.8f, -1.4f, -2.2f, -3.1f, -4.0f };
            const int books = 5;
            float bookHeight = (h - 0.035f) / books;

            float cursor = bottom;
            for (int i = 0; i < books; i++)
            {
                float centerY = cursor + bookHeight * 0.5f;
                float centerX = offsets[i];

                if (i == books - 1)
                {
                    // O tampo do livro de cima tem de coincidir com a superficie declarada.
                    float angle = -contact.tiltDegrees * Mathf.Deg2Rad;
                    float cx = contact.offsetX - Mathf.Sin(angle) * bookHeight * 0.5f;
                    float cy = h * 0.5f + contact.offsetY - Mathf.Cos(angle) * bookHeight * 0.5f;
                    centerX = cx;
                    centerY = cy;
                }

                var book = new GameObject($"Livro_{i + 1:D2}");
                book.transform.SetParent(root.transform, false);
                book.transform.localPosition = new Vector3(centerX, centerY, 0f);
                book.transform.localRotation = Quaternion.Euler(0f, 0f, tilts[i]);

                var leather = leathers[i] != null ? leathers[i] : brown;
                float bw = widths[i];
                float bh = bookHeight;

                // Miolo de paginas: mais largo que a lombada para aparecer nas laterais.
                AddPart(book.transform, "Miolo", PrimitiveMesh(PrimitiveType.Cube), pages,
                    new Vector3(0f, 0f, 0.012f), Quaternion.identity,
                    new Vector3(bw * 0.985f, bh * 0.66f, d * 0.97f));
                AddPart(book.transform, "Capa_Superior", PrimitiveMesh(PrimitiveType.Cube), leather,
                    new Vector3(0f, bh * 0.445f, 0f), Quaternion.identity,
                    new Vector3(bw, bh * 0.11f, d));
                AddPart(book.transform, "Capa_Inferior", PrimitiveMesh(PrimitiveType.Cube), leather,
                    new Vector3(0f, -bh * 0.445f, 0f), Quaternion.identity,
                    new Vector3(bw, bh * 0.11f, d));
                AddPart(book.transform, "Lombada", PrimitiveMesh(PrimitiveType.Cube), leather,
                    new Vector3(0f, 0f, -d * 0.5f + 0.05f), Quaternion.identity,
                    new Vector3(bw * 0.985f, bh * 0.76f, 0.10f));

                if (coverPaint != null)
                    AddPart(book.transform, "Capa_Dourada", QuadMesh(0f, 1f), coverPaint,
                        new Vector3(0f, bh * .5f + .001f, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(bw, d, 1f));

                // Faixa pintada da lombada (titulo gravado, filetes e ornamentos) vinda do atlas.
                if (spineAtlas != null)
                {
                    float v0 = i * 0.2f + .004f;
                    float v1 = (i + 1) * 0.2f - .004f;
                    AddPart(book.transform, "Lombada_Pintada", QuadMesh(v0, v1), spineAtlas,
                        new Vector3(0f, 0f, -d * 0.5f - 0.003f), Quaternion.identity,
                        new Vector3(bw * 0.985f, bh * 0.76f, 1f));
                }

                cursor += bookHeight;
            }

            // Marcador de fita do livro de cima, caindo pela direita.
            var ribbonRoot = new GameObject("Marcador_Fita");
            ribbonRoot.transform.SetParent(root.transform, false);
            ribbonRoot.transform.localPosition = new Vector3(contact.offsetX + widths[books - 1] * 0.5f + 0.05f, 0.12f, -0.35f);
            ribbonRoot.transform.localRotation = Quaternion.Euler(0f, 0f, -7f);
            AddPart(ribbonRoot.transform, "Fita", PrimitiveMesh(PrimitiveType.Cube), ribbon,
                Vector3.zero, Quaternion.identity, new Vector3(0.14f, 0.82f, 0.02f));
            for (int side = -1; side <= 1; side += 2)
            {
                AddPart(ribbonRoot.transform, side < 0 ? "Ponta_Esq" : "Ponta_Dir", PrimitiveMesh(PrimitiveType.Cube), ribbon,
                    new Vector3(side * 0.05f, -0.45f, 0f), Quaternion.Euler(0f, 0f, side * 24f),
                    new Vector3(0.075f, 0.18f, 0.02f));
            }

            // Pingente de borla na base.
            var tasselRoot = new GameObject("Borla");
            tasselRoot.transform.SetParent(root.transform, false);
            tasselRoot.transform.localPosition = new Vector3(widths[0] * 0.5f - 0.20f, bottom + 0.02f, -0.42f);
            AddPart(tasselRoot.transform, "Cordao", PrimitiveMesh(PrimitiveType.Cube), gold,
                new Vector3(0f, 0.20f, 0f), Quaternion.identity, new Vector3(0.028f, 0.42f, 0.028f));
            AddPart(tasselRoot.transform, "Pincel", PrimitiveMesh(PrimitiveType.Cylinder), gold,
                new Vector3(0f, -0.07f, 0f), Quaternion.identity, new Vector3(0.10f, 0.14f, 0.10f));

            return root;
        }

        /// <summary>Costura reta (tracejada) sobre a superficie frontal da cabeca.</summary>
        private static void AddSeam(StitchBuilder builder, Vector2 from, Vector2 to, float centerY,
            float rx, float ry, float rz, float squareness, int dashes, bool crossStitches)
        {
            for (int i = 0; i < dashes; i++)
            {
                float t = (i + 0.5f) / dashes;
                float x = Mathf.Lerp(from.x, to.x, t);
                float y = Mathf.Lerp(from.y, to.y, t);
                AddSurfaceStitch(builder,x,y,centerY,rx,ry,rz,squareness,0f,.018f,.055f);
            }

            if (!crossStitches) return;

            for (int i = 1; i <= 3; i++)
            {
                float t = i / 4f;
                float x = Mathf.Lerp(from.x, to.x, t);
                float y = Mathf.Lerp(from.y, to.y, t);
                AddSurfaceStitch(builder,x,y,centerY,rx,ry,rz,squareness,0f,.075f,.02f);
                AddSurfaceStitch(builder,x,y,centerY,rx,ry,rz,squareness,90f,.075f,.02f);
            }
        }

        private static void AddSurfaceStitch(StitchBuilder builder, float x, float y, float centerY,
            float rx, float ry, float rz, float exponent, float angle, float width, float height)
        {
            float limit=centerY+ry*Mathf.Pow(Mathf.Max(0f,1f-Mathf.Pow(Mathf.Abs(x/rx),2f/exponent)),exponent*.5f)-.025f;
            y=Mathf.Min(y,limit);
            const float epsilon=.002f;
            float dx=(FrontZ(x+epsilon,y,centerY,rx,ry,rz,exponent)-FrontZ(x-epsilon,y,centerY,rx,ry,rz,exponent))/(2f*epsilon);
            float dy=(FrontZ(x,y+epsilon,centerY,rx,ry,rz,exponent)-FrontZ(x,y-epsilon,centerY,rx,ry,rz,exponent))/(2f*epsilon);
            var normal=new Vector3(dx,dy,-1f).normalized;
            var point=new Vector3(x,y,FrontZ(x,y,centerY,rx,ry,rz,exponent))+normal*.006f;
            builder.AddQuad(point,Quaternion.FromToRotation(Vector3.back,normal)*Quaternion.Euler(0,0,angle),width,height);
        }

        /// <summary>
        /// Superficie frontal do superelipsoide: resolve z para a frente (negativa) da malha
        /// a partir de x/y, para colar costuras e detalhes sem flutuar.
        /// </summary>
        public static float FrontZ(float x, float y, float centerY, float rx, float ry, float rz, float squareness)
        {
            float fx = Mathf.Pow(Mathf.Clamp01(Mathf.Abs(x) / rx), 2f / squareness);
            float fy = Mathf.Pow(Mathf.Clamp01(Mathf.Abs(y - centerY) / ry), 2f / squareness);
            float k = Mathf.Clamp01(1f - fx - fy);
            return -rz * Mathf.Pow(k, squareness * 0.5f);
        }

        private static GameObject AddPart(Transform parent, string name, Mesh mesh, Material material,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = localScale;

            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return go;
        }

        /// <summary>
        /// Superelipsoide com topo plano e cantos macios. O topo e achatado num disco
        /// (superficie de pouso) para comportar os pes da crianca, enquanto a silhueta
        /// permanece redonda. O vertice maximo em Y vale exactly height/2.
        /// </summary>
        public static Mesh CushionMesh(float width, float height, float depth, float squareness, float topFlatten = 0f)
        {
            string key = $"cushion-painted-v2|{width:F2}|{height:F2}|{depth:F2}|{squareness:F2}|{topFlatten:F2}";
            if (MeshCache.TryGetValue(key, out var cached) && cached != null) return cached;

            const int uCount = 48;
            const int vCount = 24;
            float rx = width * 0.5f;
            float ry = height * 0.5f;
            float rz = depth * 0.5f;
            float ryVirtual = ry / Mathf.Max(0.05f, 1f - topFlatten);

            var vertices = new List<Vector3>(uCount * (vCount - 1) + 2);
            var uvs = new List<Vector2>(vertices.Capacity);
            var triangles = new List<int>(uCount * (vCount - 1) * 6);

            Vector2 FrontUv(float x, float y)
            {
                return new Vector2(x / (rx * 2f) + 0.5f, y / (ry * 2f) + 0.5f);
            }

            vertices.Add(new Vector3(0f, -ry, 0f));
            uvs.Add(FrontUv(0f, -ry));
            vertices.Add(new Vector3(0f, ry, 0f));
            uvs.Add(FrontUv(0f, ry));

            for (int j = 1; j < vCount; j++)
            {
                float v = -Mathf.PI * 0.5f + Mathf.PI * j / vCount;
                float ring = PowSigned(Mathf.Cos(v), squareness);
                float y = Mathf.Clamp(ryVirtual * PowSigned(Mathf.Sin(v), squareness), -ry, ry);

                for (int i = 0; i < uCount; i++)
                {
                    float u = 2f * Mathf.PI * i / uCount;
                    float x = rx * ring * PowSigned(Mathf.Cos(u), squareness);
                    float z = rz * ring * PowSigned(Mathf.Sin(u), squareness);
                    vertices.Add(new Vector3(x, y, z));
                    uvs.Add(FrontUv(x, y));
                }
            }

            int Ring(int ringIndex) => 2 + (ringIndex - 1) * uCount;

            for (int i = 0; i < uCount; i++)
            {
                int a = Ring(1) + i;
                int b = Ring(1) + (i + 1) % uCount;
                triangles.Add(0);
                triangles.Add(b);
                triangles.Add(a);
            }

            for (int j = 1; j < vCount - 1; j++)
            {
                for (int i = 0; i < uCount; i++)
                {
                    int a = Ring(j) + i;
                    int b = Ring(j) + (i + 1) % uCount;
                    int c = Ring(j + 1) + i;
                    int d = Ring(j + 1) + (i + 1) % uCount;

                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            for (int i = 0; i < uCount; i++)
            {
                int a = Ring(vCount - 1) + i;
                int b = Ring(vCount - 1) + (i + 1) % uCount;
                triangles.Add(1);
                triangles.Add(b);
                triangles.Add(a);
            }

            EnsureOutwardWinding(vertices, triangles);

            var mesh = new Mesh { name = key };
            if (vertices.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            ApplyFaceProjectedUvs(mesh, width, height, depth);

            MeshCache[key] = mesh;
            return mesh;
        }

        /// <summary>Quad 1x1 voltado para -Z com faixa vertical de UV (atlas de lombadas).</summary>
        public static Mesh QuadMesh(float vMin, float vMax)
        {
            string key = $"quad|{vMin:F3}|{vMax:F3}";
            if (MeshCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var vertices = new List<Vector3>
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            var uvs = new List<Vector2>
            {
                new Vector2(0f, vMin),
                new Vector2(1f, vMin),
                new Vector2(1f, vMax),
                new Vector2(0f, vMax)
            };
            var triangles = new List<int> { 0, 2, 1, 0, 3, 2 };

            var mesh = new Mesh { name = key };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            MeshCache[key] = mesh;
            return mesh;
        }

        /// <summary>Disco voltado para -Z (frente da camera), com UV planar para texturas de botao.</summary>
        public static Mesh DiscMesh(float radius, int segments)
        {
            string key = $"disc|{radius:F3}|{segments}";
            if (MeshCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var vertices = new List<Vector3>(segments + 1);
            var uvs = new List<Vector2>(segments + 1);
            var triangles = new List<int>(segments * 3);

            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i < segments; i++)
            {
                float a = Mathf.PI * 2f * i / segments;
                float x = Mathf.Cos(a) * radius;
                float y = Mathf.Sin(a) * radius;
                vertices.Add(new Vector3(x, y, 0f));
                uvs.Add(new Vector2(x / (radius * 2f) + 0.5f, y / (radius * 2f) + 0.5f));
            }

            for (int i = 0; i < segments; i++)
            {
                triangles.Add(0);
                triangles.Add(1 + (i + 1) % segments);
                triangles.Add(1 + i);
            }

            EnsureOutwardWinding(vertices, triangles);

            var mesh = new Mesh { name = key };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            MeshCache[key] = mesh;
            return mesh;
        }

        public static bool TryBuildShapeContact(int index, GameObject visual, Transform contactRoot)
        {
            if (index != 0) return false;
            // Static collision follows the head, ears and stool instead of a rectangular
            // invisible pad. Decorative stitches, face and bow do not block the runner.
            foreach (string name in new[] { "Cabeca_Urso", "Orelha_Esq", "Orelha_Dir", "Base_Madeira", "Base_Aro" })
            {
                var part = visual.transform.Find(name);
                var go = new GameObject(name);
                go.transform.SetParent(contactRoot, false);
                go.transform.localPosition = part.localPosition;
                go.transform.localRotation = part.localRotation;
                go.transform.localScale = part.localScale;
                go.AddComponent<MeshCollider>().sharedMesh = part.GetComponent<MeshFilter>().sharedMesh;
            }
            return true;
        }

        private static Mesh BeveledPlinthMesh(float rx, float rz, float height, float bevel)
        {
            string key = $"rounded-plinth|{rx:F3}|{rz:F3}|{height:F3}|{bevel:F3}";
            if (MeshCache.TryGetValue(key, out var cached) && cached != null) return cached;
            const int segments = 80;
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
            float[] ys = { -height * .5f, -height * .5f + bevel, height * .5f - bevel, height * .5f };
            for (int ring = 0; ring < 4; ring++)
                for (int i = 0; i <= segments; i++)
                {
                    float a = i * Mathf.PI * 2f / segments;
                    float inset = ring == 0 || ring == 3 ? bevel : 0f;
                    vertices.Add(new Vector3((rx-inset)*Mathf.Cos(a), ys[ring], (rz-inset)*Mathf.Sin(a)));
                    uv.Add(new Vector2(i / (float)segments * 3f, (ys[ring] / height + .5f)));
                }
            for (int r = 0; r < 3; r++) for (int i = 0; i < segments; i++)
            {
                int a=r*(segments+1)+i, b=a+1, c=a+segments+1, d=c+1;
                tris.AddRange(new[] { a,c,b,b,c,d });
            }
            int bottom=vertices.Count; vertices.Add(Vector3.down*height*.5f); uv.Add(new Vector2(.5f,.5f));
            int top=vertices.Count; vertices.Add(Vector3.up*height*.5f); uv.Add(new Vector2(.5f,.5f));
            for(int i=0;i<segments;i++)
            {
                tris.AddRange(new[] {bottom,i,i+1,top,3*(segments+1)+i+1,3*(segments+1)+i});
            }
            EnsureOutwardWinding(vertices,tris);
            var mesh=new Mesh { name=key }; mesh.SetVertices(vertices); mesh.SetUVs(0,uv); mesh.SetTriangles(tris,0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); ApplyFaceProjectedUvs(mesh,rx*2,height,rz*2);
            MeshCache[key]=mesh; return mesh;
        }

        private static void ApplyFaceProjectedUvs(Mesh mesh, float width, float height, float depth)
        {
            // Split UV seams while preserving the original smooth normals. Top faces use
            // X/Z, avoiding the collapsed frontal UV that stretched the fur into stripes.
            var source=mesh.vertices; var normals=mesh.normals; var indices=mesh.triangles;
            var v=new Vector3[indices.Length];var n=new Vector3[indices.Length];var uv=new Vector2[indices.Length];var t=new int[indices.Length];
            for(int i=0;i<indices.Length;i+=3)
            {
                Vector3 normal=Vector3.Cross(source[indices[i+1]]-source[indices[i]],source[indices[i+2]]-source[indices[i]]).normalized;
                for(int j=0;j<3;j++)
                {
                    int k=i+j;var p=source[indices[k]];v[k]=p;n[k]=normals[indices[k]];t[k]=k;
                    uv[k]=Mathf.Abs(normal.y)>Mathf.Max(Mathf.Abs(normal.x),Mathf.Abs(normal.z))
                        ? new Vector2(p.x/width+.5f,p.z/depth+.5f)
                        : Mathf.Abs(normal.x)>Mathf.Abs(normal.z)
                            ? new Vector2(p.z/depth+.5f,p.y/height+.5f)
                            : new Vector2(p.x/width+.5f,p.y/height+.5f);
                }
            }
            mesh.Clear();mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.triangles=t;mesh.RecalculateBounds();
        }

        private static float PowSigned(float value, float exponent)
        {
            return Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), exponent);
        }

        private static void EnsureOutwardWinding(List<Vector3> vertices, List<int> triangles)
        {
            int sample = (triangles.Count / 6) * 3;
            if (sample + 2 >= triangles.Count) return;

            Vector3 a = vertices[triangles[sample]];
            Vector3 b = vertices[triangles[sample + 1]];
            Vector3 c = vertices[triangles[sample + 2]];
            Vector3 normal = Vector3.Cross(b - a, c - a);
            Vector3 center = (a + b + c) / 3f;

            if (Vector3.Dot(normal, center) >= 0f) return;

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int tmp = triangles[i + 1];
                triangles[i + 1] = triangles[i + 2];
                triangles[i + 2] = tmp;
            }
        }

        private static Mesh PrimitiveMesh(PrimitiveType type)
        {
            if (PrimitiveCache.TryGetValue(type, out var cached) && cached != null) return cached;

            var temp = GameObject.CreatePrimitive(type);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            if (Application.isPlaying) Object.Destroy(temp);
            else Object.DestroyImmediate(temp);

            PrimitiveCache[type] = mesh;
            return mesh;
        }

        /// <summary>
        /// Acumula os pontos de costura (quads finos) numa unica malha, economizando
        /// centenas de GameObjects.
        /// </summary>
        private sealed class StitchBuilder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector2> uvs = new List<Vector2>();
            private readonly List<int> triangles = new List<int>();

            public int Count => vertices.Count;

            public void AddQuad(Vector3 center, Quaternion rotation, float width, float height)
            {
                Vector3 right = rotation * new Vector3(width * 0.5f, 0f, 0f);
                Vector3 up = rotation * new Vector3(0f, height * 0.5f, 0f);
                int start = vertices.Count;

                vertices.Add(center - right - up);
                vertices.Add(center + right - up);
                vertices.Add(center + right + up);
                vertices.Add(center - right + up);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));

                // Ordem invertida para a face olhar para -Z (frente da camera).
                triangles.Add(start);
                triangles.Add(start + 2);
                triangles.Add(start + 1);
                triangles.Add(start);
                triangles.Add(start + 3);
                triangles.Add(start + 2);
            }

            public void AddRoundedPatch(Vector3 center, float width, float height, int dashes, float dashWidth, float dashHeight)
            {
                float halfW = width * 0.5f;
                float halfH = height * 0.5f;
                for (int i = 0; i < dashes; i++)
                {
                    float t = (i + 0.5f) / dashes;
                    float perimeter = t * 4f;
                    Vector3 local;
                    Quaternion rotation;
                    if (perimeter < 1f) local = new Vector3(Mathf.Lerp(-halfW, halfW, perimeter), halfH, 0f);
                    else if (perimeter < 2f) local = new Vector3(halfW, Mathf.Lerp(halfH, -halfH, perimeter - 1f), 0f);
                    else if (perimeter < 3f) local = new Vector3(Mathf.Lerp(halfW, -halfW, perimeter - 2f), -halfH, 0f);
                    else local = new Vector3(-halfW, Mathf.Lerp(-halfH, halfH, perimeter - 3f), 0f);

                    rotation = perimeter < 1f || (perimeter >= 2f && perimeter < 3f)
                        ? Quaternion.Euler(0f, 0f, 90f)
                        : Quaternion.identity;
                    AddQuad(center + local, rotation, dashWidth, dashHeight);
                }
            }

            public Mesh Build(string name)
            {
                var mesh = new Mesh { name = name };
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
