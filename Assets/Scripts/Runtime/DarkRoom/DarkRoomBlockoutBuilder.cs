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

        [Header("Estrutura Gerada (3 Etapas Canônicas)")]
        [SerializeField] private Transform stage1Root;
        [SerializeField] private Transform stage2Root;
        [SerializeField] private Transform stage3Root;
        [SerializeField] private Transform floorRoot;

        private DarkRoomPlatformMarker[] temporalPlatforms;

        public float CoordinateScale => coordinateScale;

        public void ShowTemporalSection(DarkRoomPhase phase, int platformIndex = -1, bool falling = false)
        {
            if (temporalPlatforms == null)
                temporalPlatforms = GetComponentsInChildren<DarkRoomPlatformMarker>(true);

            // Keep the castle only while it is still the takeoff surface for the escape.
            foreach (var marker in temporalPlatforms)
            {
                if (marker == null) continue;
                int index = marker.spec.index;
                bool visible = !falling && (phase == DarkRoomPhase.Phase1Ascent
                    ? index <= 9
                    : phase == DarkRoomPhase.Phase2Escape
                        ? (index >= 10 && index <= 21) || (index == 9 && platformIndex < 10)
                        : index >= 22);
                marker.gameObject.SetActive(visible);
            }
        }

        [ContextMenu("Construir Blockout Completo")]
        public void BuildBlockout()
        {
            ClearExisting();
            EnsureRoots();

            BuildFloor();

            // Etapa 1: Subida calma ate o Castelinho de Blocos (0 a 9)
            var stage1 = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms();
            BuildPlatformList(stage1, stage1Root, "Etapa1_Castelo");

            // Etapa 2: A Fuga Acelerada com pulos crescentes ate o Poster na parede (10 a 21)
            var lastStage1 = stage1[stage1.Count - 1];
            float stage2FirstPlatWidth = 2.4f;
            float gapStage1To2 = 1.6f;
            float stage2StartX = lastStage1.x + (lastStage1.width * 0.5f) + gapStage1To2 + (stage2FirstPlatWidth * 0.5f);
            var stage2 = DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms(stage2StartX, lastStage1.y + 0.35f);
            BuildPlatformList(stage2, stage2Root, "Etapa2_Fuga");

            // Etapa 3: A Subida Caotica Reversa (22 a 37) da direita para a esquerda apos a queda cinematica
            var lastStage2 = stage2[stage2.Count - 1];
            float posterWallX = lastStage2.x;
            var stage3 = DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms(posterWallX, DarkRoomReturnLayout.FloorY);
            BuildPlatformList(stage3, stage3Root, "Etapa3_Caotica");

            temporalPlatforms = null;
            ShowTemporalSection(DarkRoomPhase.Phase1Ascent);

            Debug.Log($"[DarkRoomBlockoutBuilder] Circuito Canônico em 3 Etapas gerado com sucesso! " +
                      $"Etapa 1: {stage1.Count} | Etapa 2: {stage2.Count} | Etapa 3: {stage3.Count} plataformas.");
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
            stage1Root = CreateGroup("01_Etapa1_SubidaAoCastelo");
            stage2Root = CreateGroup("02_Etapa2_AFugaAcelerada");
            stage3Root = CreateGroup("03_Etapa3_SubidaCaoticaReversa");
        }

        private Transform CreateGroup(string groupName)
        {
            var go = new GameObject(groupName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void BuildFloor()
        {
            // Chao escuro da sala: qualquer queda aqui e Game Over imediato (exceto na queda cinematica)
            float worldY = -9.6f;
            float worldWidth = 140.0f;
            float worldDepth = 12.0f;
            float thickness = 1.0f;

            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorGo.name = "Chao_Escuro_Sala";
            floorGo.transform.SetParent(floorRoot, false);
            floorGo.transform.position = new Vector3(55.0f, worldY, 0f);
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
