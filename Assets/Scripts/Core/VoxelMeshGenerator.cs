using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;

    private VoxelMesh voxelMesh;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        voxelMesh = FindObjectOfType<VoxelMesh>();
    }

    private void Start() {
        voxelMesh.Setup();
    }

    public void GenerateWholeMesh() {
        foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in CORE.existingChunks) {
            voxelMesh.TriangulateChunkMesh(chunk.Value);
        }
    }
}
