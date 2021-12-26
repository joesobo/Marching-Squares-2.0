using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;

    private VoxelMesh voxelMesh;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        voxelMesh = FindObjectOfType<VoxelMesh>();
    }

    public void GenerateChunkMesh(VoxelChunk chunk) {
        voxelMesh.TriangulateChunkMesh(chunk);
    }
}
