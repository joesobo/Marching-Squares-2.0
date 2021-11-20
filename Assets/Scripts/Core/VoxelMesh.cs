using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelMesh : MonoBehaviour {
        private int voxelResolution, chunkResolution;
        private Material material;

        // Mesh information storage
        private Vector2[] uvs;
        private Vector3[] vertices;
        private int[] triangles;
        private Color32[] colors;

        // Scale of textures when applied to voxels
        private int textureTileAmount;

        private MarchingShader marchingShader;

        void Awake() {
            marchingShader = FindObjectOfType<MarchingShader>();
        }

        public void Setup(int voxelResolution, int chunkResolution, Material material) {
            this.voxelResolution = voxelResolution;
            this.chunkResolution = chunkResolution;
            this.material = material;

            textureTileAmount = (voxelResolution * chunkResolution) / 2;
        }

        public void TriangulateChunkMesh(VoxelChunk chunk) {
            Mesh mesh = new Mesh();
            mesh.name = "VoxelChunk Mesh";

            marchingShader.ShaderTriangulate(chunk, out vertices, out triangles, out colors);

            GetUVs(chunk);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors32 = colors;
            mesh.RecalculateNormals();

            chunk.GetComponent<MeshFilter>().mesh = mesh;
            chunk.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void GetUVs(VoxelChunk chunk) {
            uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                float percentX = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].x) * textureTileAmount;
                float percentY = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].y) * textureTileAmount;
                uvs[i] = new Vector2(percentX, percentY);
            }
        }
    }
}