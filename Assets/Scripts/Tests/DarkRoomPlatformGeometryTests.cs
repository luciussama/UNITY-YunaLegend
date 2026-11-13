using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Tests
{
    /// <summary>
    /// Garante que cada plataforma tem o seu proprio tamanho canonico (em vez do antigo
    /// 2,6 m x 0,7 m uniforme) e que o modelo da plataforma 00 encosta a superficie de
    /// pouso na face superior do colisor de contato.
    /// </summary>
    [TestFixture]
    public class DarkRoomPlatformGeometryTests
    {
        [Test]
        public void EveryPlatform_HasCanonicalMetersDerivedFromDesignCatalog()
        {
            for (int index = 0; index <= 37; index++)
            {
                Assert.IsTrue(DarkRoomPlatformGeometry.TryGet(index, out var geometry), $"Plataforma {index} sem geometria");
                Assert.Greater(geometry.Width, 1.0f, $"Largura da plataforma {index}");
                Assert.Greater(geometry.Height, 0.4f, $"Altura da plataforma {index}");
                Assert.Greater(geometry.Depth, 0.8f, $"Profundidade da plataforma {index}");
            }

            Assert.AreEqual(0.0318f, DarkRoomPlatformGeometry.MetersPerDesignPixel, 0.00001f,
                "A regua da skill e 44 px de Yuna = 1,40 m");
        }

        [Test]
        public void Platforms_AreNotUniformInWidthHeightOrDepth()
        {
            var widths = new HashSet<float>();
            var heights = new HashSet<float>();
            var depths = new HashSet<float>();

            for (int index = 0; index <= 37; index++)
            {
                var geometry = DarkRoomPlatformGeometry.Get(index);
                widths.Add(geometry.Width);
                heights.Add(geometry.Height);
                depths.Add(geometry.Depth);
            }

            Assert.GreaterOrEqual(widths.Count, 10, "As plataformas devem ter larguras distintas");
            Assert.GreaterOrEqual(heights.Count, 6, "As plataformas devem ter alturas distintas");
            Assert.GreaterOrEqual(depths.Count, 6, "As plataformas devem ter profundidades distintas");
        }

        [Test]
        public void FirstPlatform_IsTheTeddyHeadWithCanonicalProportions()
        {
            var geometry = DarkRoomPlatformGeometry.Get(0);

            Assert.AreEqual(IllustratedBearPlatformProfile.VisibleWidth, geometry.Width, 0.001f);
            Assert.AreEqual(DarkRoomPlatformGeometry.ConceptTeddyHeight, geometry.Height, 0.001f,
                "A altura segue a arte conceitual (cabeca + pe de madeira), nao a estimativa antiga do catalogo");
            Assert.AreEqual(IllustratedBearPlatformProfile.PhysicalDepth, geometry.Depth, 0.001f,
                "A profundidade fisica do urso e propria, nao a profundidade planejada em pixels");
            Assert.AreEqual(PlatformSurfaceKind.SoftCushion, geometry.Surface);
            Assert.IsTrue(geometry.HasModel, "A plataforma 00 deve ter modelo dedicado nesta etapa");
        }

        [Test]
        public void Stage1_FirstPlatformRestsOnFloorWhileOthersKeepTheValidatedTops()
        {
            var stage1 = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms();

            var first = stage1[0];
            Assert.AreEqual(DarkRoomPlatformGeometry.FloorSurfaceY, first.y - first.height * 0.5f, 0.001f,
                "A cabeca do urso deve encostar na face superior do piso");
            Assert.AreEqual(DarkRoomPlatformGeometry.FloorSurfaceY + first.height, first.y + first.height * 0.5f, 0.001f);

            // Topos das demais plataformas continuam os da rota validada.
            var second = stage1[1];
            var secondSupport = DarkRoomPlatformGeometry.GetSupportProfile(second);
            Assert.AreEqual(-7.55f, secondSupport.PointNear(second.x + second.contact.offsetX).y, 0.001f,
                "O topo dos livros e a capa inclinada, nao o centro do AABB");
            var last = stage1[9];
            var lastSupport = DarkRoomPlatformGeometry.GetSupportProfile(last);
            Assert.AreEqual(-3.27f, lastSupport.PointNear(last.x + last.contact.offsetX).y, 0.001f);
        }

        [Test]
        public void Books_DeclareCustomDiagonalContactSurface()
        {
            var contact = DarkRoomPlatformGeometry.GetContact(1);

            Assert.IsTrue(contact.IsCustom, "A pilha de livros nao e uma caixa: o contato e proprio");
            Assert.AreEqual(IllustratedBookStackPlatformProfile.ContactOffsetX, contact.offsetX, 0.0001f);
            Assert.AreEqual(IllustratedBookStackPlatformProfile.SupportTiltDegrees, contact.tiltDegrees, 0.0001f);
            Assert.AreEqual(IllustratedBookStackPlatformProfile.SupportWidth, contact.width, 0.0001f);
            Assert.AreEqual(IllustratedBookStackPlatformProfile.PhysicalDepth, contact.depth, 0.0001f);
            Assert.IsTrue(DarkRoomPlatformGeometry.Get(1).HasModel);
            Assert.AreEqual(DarkRoomPlatformGeometry.BookStackArtKey, DarkRoomPlatformGeometry.Get(1).ArtKey);
            Assert.AreEqual(IllustratedBookStackPlatformProfile.VisibleWidth, DarkRoomPlatformGeometry.Get(1).Width, 0.0001f);
            Assert.AreEqual(IllustratedBookStackPlatformProfile.VisibleHeight, DarkRoomPlatformGeometry.Get(1).Height, 0.0001f);
            Assert.AreEqual(IllustratedBookStackPlatformProfile.PhysicalDepth, DarkRoomPlatformGeometry.Get(1).Depth, 0.0001f);
        }

        [Test]
        public void BearCarriesCrownOffsetAndUnmodeledPlatformsKeepDefaultPlanningContact()
        {
            var contact = DarkRoomPlatformGeometry.GetContact(0);
            Assert.IsTrue(contact.IsCustom, "A coroa pintada e ligeiramente fora do centro da prancha");
            Assert.AreEqual(IllustratedBearPlatformProfile.ContactOffsetX, contact.offsetX, 0.0001f);
            Assert.IsFalse(DarkRoomPlatformGeometry.GetContact(3).IsCustom);
        }

        [Test]
        public void IllustratedBearProfilePreservesTheApprovedPaintingScaleAndCrown()
        {
            var document = IllustratedBearPlatformProfile.Document;
            Assert.AreEqual(1619, document.texture.width);
            Assert.AreEqual(971, document.texture.height);
            Assert.AreEqual(1.10f, document.mapping.crownHeightAboveFloor, 0.0001f);
            Assert.AreEqual(2.017602f, document.mapping.visibleWidth, 0.000001f);
            Assert.AreEqual(0.001403061f, document.mapping.metresPerTexturePixel, 0.000000001f);
            Assert.Greater(document.topProfile.Length, 300, "O perfil fisico deve seguir a silhueta, nao uma caixa");
            Assert.AreEqual(-0.55f, document.mapping.quadBoundsLocal.bottom, 0.0001f);
            Assert.GreaterOrEqual(IllustratedBearPlatformProfile.ContactMesh.bounds.max.y, 0.60f,
                "As orelhas continuam acima da coroa, sem achatar a silhueta");
        }

        [Test]
        public void IllustratedBooksProfilePreservesTheApprovedPaintingScaleAndTopCover()
        {
            var document = IllustratedBookStackPlatformProfile.Document;
            Assert.AreEqual(1620, document.texture.width);
            Assert.AreEqual(971, document.texture.height);
            Assert.AreEqual(1.11f, document.mapping.stackHeight, 0.0001f);
            Assert.AreEqual(1.726526f, document.mapping.visibleWidth, 0.000001f);
            Assert.AreEqual(1.11f, document.mapping.visibleHeight, 0.0001f);
            Assert.AreEqual(0.001268571f, document.mapping.metresPerTexturePixel, 0.000000001f);
            Assert.AreEqual(1.184211f, document.mapping.supportWidth, 0.000001f);
            Assert.AreEqual(-8.739244f, document.mapping.supportTiltDegrees, 0.000001f);
            Assert.AreEqual(0.134786f, document.mapping.contactOffsetX, 0.000001f);
            Assert.AreEqual(0.363763f, document.mapping.supportHeightAboveCentre, 0.000001f);
            Assert.AreEqual(1.15f, document.mapping.physicalDepth, 0.0001f);
            Assert.AreEqual(-0.555f, document.mapping.quadBoundsLocal.bottom, 0.0001f);
            Assert.AreEqual(0.555f, document.mapping.quadBoundsLocal.top, 0.0001f);
        }

        [Test]
        public void PersistedScene_BookStackColliderFollowsTheTiltedTopCover()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            try
            {
                var markers = Object.FindObjectsByType<DarkRoomPlatformMarker>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                var marker = System.Array.Find(markers, m => m.spec.index == 1);

                Assert.IsNotNull(marker, "Plataforma 01 ausente na cena persistida");
                Assert.IsTrue(marker.spec.contact.IsCustom, "O contato da pilha deve ser persistido");

                var contact = marker.transform.Find("Contacto_Fisico");
                Assert.IsNotNull(contact);
                var colliders = contact.GetComponentsInChildren<Collider>();
                Assert.AreEqual(1, colliders.Length, "A capa superior deve ser um unico apoio continuo");
                var collider = colliders[0] as MeshCollider;
                Assert.IsNotNull(collider);
                Assert.IsFalse(collider.convex, "A capa pode manter a silhueta sem virar uma caixa convexa");
                Assert.AreEqual(IllustratedBookStackPlatformProfile.PhysicalDepth, collider.bounds.size.z, 0.01f,
                    "A profundidade fisica deve coincidir com a prancha aprovada");

                var visual = marker.transform.Find("Visual_Ilustrado_Livros");
                Assert.IsNotNull(visual, "A pintura aprovada dos livros nao foi persistida");
                var renderer = visual.GetComponent<MeshRenderer>();
                Assert.IsNotNull(renderer);
                Assert.AreEqual("Books_Platform_v1", renderer.sharedMaterial.mainTexture.name,
                    "A superficie visivel deve usar a prancha aprovada");
                Assert.AreEqual(0f, visual.localPosition.z, 0.0001f,
                    "A projecao visual deve coincidir com o plano de suporte");

                var legacy = marker.transform.Find("Visual_3D_Referencia_Desativada");
                Assert.IsNotNull(legacy);
                Assert.IsFalse(legacy.gameObject.activeSelf, "O antigo visual 3D fica apenas como referencia inativa");

                Physics.SyncTransforms();
                Vector3 expected = marker.GetTopCenterWorld();
                Assert.IsTrue(Physics.Raycast(expected + Vector3.up * 0.2f, Vector3.down, out var topHit, 0.5f),
                    "O tampo da capa precisa oferecer apoio fisico");
                Assert.AreEqual(expected.y, topHit.point.y, 0.01f,
                    "O topo do colisor deve coincidir com a superficie de pouso");
                Assert.AreEqual(marker, topHit.collider.GetComponentInParent<DarkRoomPlatformMarker>());
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            }
        }

        [Test]
        public void CushionMesh_TopMatchesContactPlaneAndNormalsFaceOutward()
        {
            const float width = 2.48f;
            const float height = 0.78f;
            const float depth = 1.92f;

            var mesh = DarkRoomPlatformArt.CushionMesh(width, height, depth, 0.34f);
            Assert.IsNotNull(mesh);
            Assert.AreEqual(height * 0.5f, mesh.bounds.max.y, 0.005f, "O topo do mesh deve coincidir com o plano de contato");
            Assert.AreEqual(-height * 0.5f, mesh.bounds.min.y, 0.005f);
            Assert.AreEqual(width * 0.5f, mesh.bounds.max.x, 0.02f);
            Assert.AreEqual(depth * 0.5f, mesh.bounds.max.z, 0.02f);

            var vertices = mesh.vertices;
            var normals = mesh.normals;
            int outward = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (Vector3.Dot(normals[i], vertices[i]) > 0f) outward++;
            }

            Assert.Greater(outward / (float)vertices.Length, 0.95f, "As normais devem apontar para fora da malha");
        }

        [Test]
        public void PersistedMaterialsUsePaintedArtworkInsteadOfProceduralFallback()
        {
            foreach (var pair in new[] {
                new[] { "M_Bear_Fur", "T_Bear_Fur_Painted" },
                new[] { "M_Book_Spines", "T_Book_Spines_Painted" },
                new[] { "M_Book_Painted_Cover", "T_Book_Cover_Painted" } })
            {
                var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/" + pair[0] + ".mat");
                Assert.IsNotNull(material, pair[0]);
                Assert.IsNotNull(material.GetTexture("_BaseMap"), pair[0]);
                Assert.AreEqual(pair[1], material.GetTexture("_BaseMap").name,
                    "A setup must not silently retain the old fallback when texture import is incomplete.");
            }
        }

        [Test]
        public void RoundedBearTopHasTwoDimensionalTextureCoordinates()
        {
            var mesh = DarkRoomPlatformArt.CushionMesh(2.48f, .78f, 1.65f, .9f, .12f);
            Assert.AreEqual(.78f, mesh.bounds.size.y, .001f);
            var v = mesh.vertices; var uv = mesh.uv; var t = mesh.triangles;
            int tested = 0;
            for (int i = 0; i < t.Length; i += 3)
            {
                int a = t[i], b = t[i+1], c = t[i+2];
                if (v[a].y < .389f || v[b].y < .389f || v[c].y < .389f) continue;
                if (Vector3.Cross(v[b]-v[a], v[c]-v[a]).sqrMagnitude < .00000001f) continue;
                float area = Mathf.Abs((uv[b].x-uv[a].x)*(uv[c].y-uv[a].y)-(uv[c].x-uv[a].x)*(uv[b].y-uv[a].y));
                Assert.Greater(area, .000001f, "The flattened top must not collapse into a texture stripe.");
                tested++;
            }
            Assert.Greater(tested, 10);
        }

        [Test]
        public void PersistedScene_PlatformZeroHasModelAndContactSurfaceOnTop()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            try
            {
                var markers = Object.FindObjectsByType<DarkRoomPlatformMarker>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                var marker = System.Array.Find(markers, m => m.spec.index == 0);

                Assert.IsNotNull(marker, "Plataforma 00 ausente na cena persistida");
                Assert.AreEqual(DarkRoomPlatformGeometry.Get(0).Width, marker.spec.width, 0.001f,
                    "A cena deve persistir a largura canonica da plataforma 00");
            Assert.AreEqual(IllustratedBearPlatformProfile.PhysicalDepth, marker.spec.depth, 0.001f,
                "A cena deve persistir a profundidade fisica do urso");

                var contact = marker.transform.Find("Contacto_Fisico");
                Assert.IsNotNull(contact, "Falta o colisor de contato dedicado");
            var colliders = contact.GetComponentsInChildren<Collider>();
            Assert.AreEqual(1, colliders.Length, "O perfil continuo deve usar um unico collider fisico");
            var collider = colliders[0] as MeshCollider;
                Assert.IsNotNull(collider);
            Assert.IsFalse(collider.convex, "A silhueta pode ser concava sem virar uma caixa convexa");
            Assert.AreEqual(IllustratedBearPlatformProfile.PhysicalDepth, collider.bounds.size.z, 0.01f,
                "A profundidade do collider deve ser a profundidade fisica declarada");

            Physics.SyncTransforms();
            Vector3 crown = marker.GetTopCenterWorld();
            Assert.IsTrue(Physics.Raycast(crown + Vector3.up * 0.2f, Vector3.down, out var crownHit, 0.5f),
                "A coroa visivel precisa oferecer apoio fisico");
            Assert.AreEqual(crown.y, crownHit.point.y, 0.01f,
                "O topo do colisor deve coincidir com a coroa usada pelo salto");
            Assert.AreEqual(marker, crownHit.collider.GetComponentInParent<DarkRoomPlatformMarker>());

            var visual = marker.transform.Find("Visual_Ilustrado_Urso");
            Assert.IsNotNull(visual, "A pintura aprovada do urso nao foi persistida");
            var visualRenderer = visual.GetComponent<MeshRenderer>();
            Assert.IsNotNull(visualRenderer);
            Assert.AreEqual("Bear_Platform_v1", visualRenderer.sharedMaterial.mainTexture.name,
                "A superficie visivel deve usar a prancha aprovada");
            Assert.AreEqual(0f, visual.localPosition.z, 0.0001f,
                "A projecao visual deve coincidir com o plano de suporte");
            var legacy = marker.transform.Find("Visual_3D_Referencia_Desativada");
            Assert.IsNotNull(legacy);
            Assert.IsFalse(legacy.gameObject.activeSelf, "O antigo visual 3D fica apenas como referencia inativa");

            foreach (float z in new[] { -1f, 1f })
            {
                bool hit = Physics.Raycast(marker.GetTopCenterWorld() + new Vector3(0f, .2f, z),
                    Vector3.down, out var rearHit, .5f);
                Assert.IsFalse(hit && rearHit.collider.GetComponentInParent<DarkRoomPlatformMarker>() == marker,
                    "Sem apoio invisivel fora da profundidade fisica do urso");
            }
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            }
        }
    }
}
