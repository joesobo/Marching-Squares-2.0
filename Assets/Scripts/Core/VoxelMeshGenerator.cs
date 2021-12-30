using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour {
    private VoxelMesh voxelMesh;

    private void Awake() {
        voxelMesh = FindObjectOfType<VoxelMesh>();
    }

    public void GenerateChunkMesh(VoxelChunk chunk) {
        voxelMesh.TriangulateChunkMesh(chunk);
    }
}
