using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelMeshGenerator : MonoBehaviour {
        public Material material;

        private MarchingShader marchingShader;
        private VoxelMesh voxelMesh;

        private void Awake() {
            marchingShader = FindObjectOfType<MarchingShader>();
            voxelMesh = FindObjectOfType<VoxelMesh>();
        }

        public void Setup(int voxelResolution, int chunkResolution) {
            marchingShader.Setup(voxelResolution, chunkResolution);
            voxelMesh.Setup(voxelResolution, chunkResolution, material);
        }

        public void GenerateWholeMesh(Dictionary<Vector2Int, VoxelChunk> existingChunks) {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in existingChunks) {
                voxelMesh.TriangulateChunkMesh(chunk.Value);
            }
        }
    }
}