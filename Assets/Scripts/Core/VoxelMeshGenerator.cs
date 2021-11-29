using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelMeshGenerator : MonoBehaviour {
        private CoreScriptableObject CORE;

        private VoxelMesh voxelMesh;

        private void Awake() {
            CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
            voxelMesh = FindObjectOfType<VoxelMesh>();
        }

        public void GenerateWholeMesh() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in CORE.existingChunks) {
                voxelMesh.TriangulateChunkMesh(chunk.Value);
            }
        }
    }
}