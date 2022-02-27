using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour {
    private VoxelMesh voxelMesh;

    private void Awake() {
        voxelMesh = GetComponent<VoxelMesh>();
    }

    public void GenerateChunkMesh(VoxelChunk chunk, Material material) {
        voxelMesh.TriangulateChunkMesh(chunk, material);
    }
}
