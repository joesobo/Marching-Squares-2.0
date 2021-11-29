using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelMeshGenerator : MonoBehaviour {
        private CoreScriptableObject coreScriptableObject;

        private MarchingShader marchingShader;
        private VoxelMesh voxelMesh;

        private void Awake() {
            coreScriptableObject = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
            marchingShader = FindObjectOfType<MarchingShader>();
            voxelMesh = FindObjectOfType<VoxelMesh>();
        }

        public void Setup() {
            marchingShader.Setup();
            voxelMesh.Setup();
        }

        public void GenerateWholeMesh() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in coreScriptableObject.existingChunks) {
                voxelMesh.TriangulateChunkMesh(chunk.Value);
            }
        }

        public void CreateChunk() {}
    }
}