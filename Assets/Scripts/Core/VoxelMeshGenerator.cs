using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelMeshGenerator : MonoBehaviour {
        private CoreScriptableObject CORE;

        private MarchingShader marchingShader;
        private VoxelMesh voxelMesh;

        private void Awake() {
            CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
            marchingShader = FindObjectOfType<MarchingShader>();
            voxelMesh = FindObjectOfType<VoxelMesh>();
        }

        public void Setup() {
            marchingShader.Setup();
            voxelMesh.Setup();
        }

        public void GenerateWholeMesh() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in CORE.existingChunks) {
                voxelMesh.TriangulateChunkMesh(chunk.Value);
            }
        }

        public void CreateChunk() {}
    }
}