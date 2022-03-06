using System.Collections.Generic;
using UnityEngine;

public class VoxelMesh : MonoBehaviour {
    private int voxelResolution, chunkResolution;

    // Mesh information storage
    private Vector2[] uvs;
    private int[] triangles;
    private Color32[] colors;

    // Scale of textures when applied to voxels
    private int textureTileAmount;

    private MeshShaderController meshShaderController;

    private void Awake() {
        CoreScriptableObject CORE = GetComponent<VoxelCore>().GetCoreScriptableObject(0);
        meshShaderController = GetComponent<MeshShaderController>();

        voxelResolution = CORE.voxelResolution;
        chunkResolution = CORE.chunkResolution;

        textureTileAmount = (voxelResolution * chunkResolution) / 2;
    }

    public void TriangulateChunkMesh(VoxelChunk chunk, Material material) {
        Mesh mesh = new Mesh { name = "VoxelChunk Mesh" };

        meshShaderController.ShaderTriangulate(chunk, out triangles, out colors);

        mesh.vertices = chunk.meshVertices;
        mesh.triangles = triangles;
        mesh.uv = GetUVs(chunk.meshVertices);
        mesh.colors32 = colors;
        mesh.RecalculateNormals();

        chunk.meshFilter.mesh = mesh;
        chunk.meshRenderer.material = material;
    }

    private Vector2[] GetUVs(IList<Vector3> vertices) {
        Vector2[] temp = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++) {
            float percentX = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].x) * textureTileAmount;
            float percentY = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].y) * textureTileAmount;
            temp[i] = new Vector2(percentX, percentY);
        }

        return temp;
    }
}
