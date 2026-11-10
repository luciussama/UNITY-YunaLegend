using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    [ExecuteAlways]
    public class DarkRoomBlockoutBuilder : MonoBehaviour
    {
        [Header("Configuracao de Escala")]
        [Tooltip("Fator de conversao entre as coordenadas de design 2D do roteiro e unidades 3D da Unity")]
        [SerializeField] private float coordinateScale = 0.02f;

        [Header("Materiais Base")]
        [SerializeField] private Material defaultPlatformMaterial;

        [Header("Estrutura Gerada")]
        [SerializeField] private Transform rampAscentRoot;
        [SerializeField] private Transform returnChaoticRoot;
        [SerializeField] private Transform floorRoot;

        public float CoordinateScale => coordinateScale;

        [ContextMenu("Construir Blockout Completo")]
        public void BuildBlockout()
        {
            ClearExisting();
            EnsureRoots();

            BuildFloor();

            // Gera a subida constante em rampa (Fase 1 + 2 unificadas ate o Poster na parede)
            var ascentList = DarkRoomRampPlatformGenerator.GenerateAscentRampPlatforms();
            BuildPlatformList(ascentList, rampAscentRoot, "SubidaConstante");

            // Gera a corrida reversa caotica (Fase 3 apos o poster)
            var returnList = DarkRoomRampPlatformGenerator.GenerateReturnChaoticPlatforms();
            BuildPlatformList(returnList, returnChaoticRoot, "VoltaCaotica");

            Debug.Log($"[DarkRoomBlockoutBuilder] Trilha Runner gerada: {ascentList.Count} plataformas na subida constante e {returnList.Count} na volta.");
        }

        [ContextMenu("Limpar Blockout")]
        public void ClearExisting()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void EnsureRoots()
        {
            floorRoot = CreateGroup("00_Piso_Madeira_Escura");
            rampAscentRoot = CreateGroup("01_Subida_Constante_Rampa");
            returnChaoticRoot = CreateGroup("02_Volta_Caotica_Reversa");
        }

        private Transform CreateGroup(string groupName)
        {
            var go = new GameObject(groupName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void BuildFloor()
        {
            // Chao escuro da sala: qualquer queda aqui e Game Over imediato
            float worldY = -9.6f;
            float worldWidth = 140.0f;
            float worldDepth = 12.0f;
            float thickness = 1.0f;

            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorGo.name = "Chao_Escuro_Sala";
            floorGo.transform.SetParent(floorRoot, false);
            floorGo.transform.position = new Vector3(worldWidth * 0.5f - 10f, worldY, 0f);
            floorGo.transform.localScale = new Vector3(worldWidth, thickness, worldDepth);

            // Colisor de chao
            var col = floorGo.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = false;
        }

        private void BuildPlatformList(List<PlatformSpec> platforms, Transform parent, string prefix)
        {
            foreach (var spec in platforms)
            {
                var platGo = new GameObject($"{prefix}_{spec.index:D2}_{SanitizeName(spec.name)}");
                platGo.transform.SetParent(parent, false);
                platGo.transform.position = new Vector3(spec.x, spec.y, 0f);

                // Malha visual natural 3D
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = "Visual_3D";
                visual.transform.SetParent(platGo.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(spec.width, spec.height, spec.depth);

                var renderer = visual.GetComponent<MeshRenderer>();
                if (renderer != null && defaultPlatformMaterial != null)
                {
                    renderer.sharedMaterial = defaultPlatformMaterial;
                }

                // Colisor de caixa solido no topo
                var collider = visual.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.isTrigger = false;
                }

                platGo.AddComponent<PlatformCollisionDetector>();
                var marker = platGo.AddComponent<DarkRoomPlatformMarker>();
                marker.spec = spec;
            }
        }

        private static string SanitizeName(string raw)
        {
            return raw.Replace(" ", "_").Replace("/", "_").Replace("(", "").Replace(")", "");
        }
    }
}
