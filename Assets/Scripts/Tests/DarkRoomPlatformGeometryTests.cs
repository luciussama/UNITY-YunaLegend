using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using YunaLegend.DarkRoom;

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

            Assert.AreEqual(DarkRoomPlatformGeometry.PixelsToMeters(78f), geometry.Width, 0.001f);
            Assert.AreEqual(DarkRoomPlatformGeometry.ConceptTeddyHeight, geometry.Height, 0.001f,
                "A altura segue a arte conceitual (cabeca + pe de madeira), nao a estimativa antiga do catalogo");
            Assert.AreEqual(DarkRoomPlatformGeometry.PixelsToMeters(70f), geometry.Depth, 0.001f);
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
            Assert.AreEqual(-7.55f, second.y + second.height * 0.5f, 0.001f);
            var last = stage1[9];
            Assert.AreEqual(-3.27f, last.y + last.height * 0.5f, 0.001f);
        }

        [Test]
        public void Books_DeclareCustomDiagonalContactSurface()
        {
            var contact = DarkRoomPlatformGeometry.GetContact(1);

            Assert.IsTrue(contact.IsCustom, "A pilha de livros nao e uma caixa: o contato e proprio");
            Assert.AreEqual(0.24f, contact.offsetX, 0.001f);
            Assert.AreEqual(-4f, contact.tiltDegrees, 0.001f);
            Assert.AreEqual(1.85f, contact.width, 0.001f);
            Assert.IsTrue(DarkRoomPlatformGeometry.Get(1).HasModel);
            Assert.AreEqual(DarkRoomPlatformGeometry.BookStackArtKey, DarkRoomPlatformGeometry.Get(1).ArtKey);
        }

        [Test]
        public void UnmodeledPlatformsKeepDefaultPlanningContact()
        {
            var contact = DarkRoomPlatformGeometry.GetContact(0);
            Assert.IsFalse(contact.IsCustom, "A plataforma 00 usa o topo da propria massa");
            Assert.IsFalse(DarkRoomPlatformGeometry.GetContact(2).IsCustom);
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
                var collider = contact.GetComponent<BoxCollider>();
                Assert.IsNotNull(collider);
                Assert.AreEqual(-4f, Mathf.DeltaAngle(0f, collider.transform.localEulerAngles.z), 0.5f,
                    "A laje de contato deve acompanhar a inclinacao da pilha");

                Vector3 topFace = collider.transform.position +
                    collider.transform.rotation * new Vector3(0f, collider.size.y * 0.5f, 0f);
                Vector3 expected = marker.GetTopCenterWorld();
                Assert.AreEqual(expected.x, topFace.x, 0.02f);
                Assert.AreEqual(expected.y, topFace.y, 0.02f);

                var cover = marker.transform.Find("Visual_3D/Livro_05/Capa_Superior");
                Assert.IsNotNull(cover, "A capa do livro de cima deve existir");
                var renderer = cover.GetComponent<MeshRenderer>();
                Assert.IsNotNull(renderer);

                // A face superior da capa e inclinada; comparar o centro da face, nao o AABB.
                Vector3 coverTopCenter = cover.position +
                    cover.rotation * new Vector3(0f, cover.localScale.y * 0.5f, 0f);
                Assert.AreEqual(topFace.y, coverTopCenter.y, 0.02f,
                    "O tampo visivel do livro de cima deve coincidir com o colisor");
                Assert.AreEqual(topFace.x, coverTopCenter.x, 0.05f);
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

                var contact = marker.transform.Find("Contacto_Fisico");
                Assert.IsNotNull(contact, "Falta o colisor de contato dedicado");
                var collider = contact.Find("Cabeca_Urso").GetComponent<MeshCollider>();
                Assert.IsNotNull(collider);
                Assert.AreEqual(marker.GetTopCenterWorld().y, collider.bounds.max.y, 0.01f,
                    "O topo do colisor deve ser a superficie de pouso");

                var head = marker.transform.Find("Visual_3D/Cabeca_Urso");
                Assert.IsNotNull(head, "O modelo da cabeca do urso nao foi persistido");
                var headRenderer = head.GetComponent<MeshRenderer>();
                Assert.IsNotNull(headRenderer);
                Assert.AreEqual(collider.bounds.max.y, headRenderer.bounds.max.y, 0.03f,
                    "A superficie visivel deve coincidir com o topo do colisor (sem tampo invisivel)");
                Physics.SyncTransforms();
                foreach (float z in new[] { -1f, 1f })
                {
                    bool hit = Physics.Raycast(marker.GetTopCenterWorld() + new Vector3(0f, .2f, z),
                        Vector3.down, out var rearHit, .5f);
                    Assert.IsFalse(hit && rearHit.collider.GetComponentInParent<DarkRoomPlatformMarker>() == marker,
                        "No invisible bear support beyond the visible head depth.");
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
