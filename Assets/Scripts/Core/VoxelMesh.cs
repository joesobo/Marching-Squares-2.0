using UnityEngine;

namespace Core {
    public class VoxelMesh : MonoBehaviour {
        private CoreScriptableObject coreScriptableObject;
        private int voxelResolution, chunkResolution;

        // Mesh information storage
        private Vector2[] uvs;
        private Vector3[] vertices;
        private int[] triangles;
        private Color32[] colors;

        // Scale of textures when applied to voxels
        private int textureTileAmount;

        private MarchingShader marchingShader;

        public Material material;

        private void Awake() {
            coreScriptableObject = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
            marchingShader = FindObjectOfType<MarchingShader>();
        }

        public void Setup() {
            this.voxelResolution = coreScriptableObject.voxelResolution;
            this.chunkResolution = coreScriptableObject.chunkResolution;

            textureTileAmount = (voxelResolution * chunkResolution) / 2;
        }

        public void TriangulateChunkMesh(VoxelChunk chunk) {
            Mesh mesh = new Mesh {name = "VoxelChunk Mesh"};

            marchingShader.ShaderTriangulate(chunk, out vertices, out triangles, out colors);
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = GetUVs();
            mesh.colors32 = colors;
            mesh.RecalculateNormals();

            chunk.GetComponent<MeshFilter>().mesh = mesh;
            chunk.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private Vector2[] GetUVs() {
            Vector2[] temp = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                float percentX = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].x) * textureTileAmount;
                float percentY = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].y) * textureTileAmount;
                temp[i] = new Vector2(percentX, percentY);
            }

            return temp;
        }
    }
}