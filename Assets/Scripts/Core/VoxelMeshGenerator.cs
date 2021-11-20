using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelMeshGenerator : MonoBehaviour {
        public ComputeShader marchingSquareShader;
        public Material material;

        private const int SHADER_THREADS = 8;

        // Compute shader buffer storage
        private ComputeBuffer verticeBuffer, triangleBuffer, triCountBuffer, stateBuffer;

        // Number of voxels per a chunk 
        private int voxelResolution;
        // Radius of chunks around the player
        private int chunkResolution;
        // Scale of textures when applied to voxels
        private int textureTileAmount;

        // voxel state values for calculating type of voxel
        private int[] stateValues;

        // Mesh information storage
        private Vector2[] uvs;
        private Vector3[] vertices;
        private int[] triangles;
        private Color32[] colors;

        // Mesh dictionary information
        public Dictionary<Vector3, int> verticeDictionary;
        public Dictionary<Vector2, List<Triangle>> triangleDictionary;

        // TODO: Refactor the shit out of this file

        public void Setup(int voxelResolution, int chunkResolution) {
            this.voxelResolution = voxelResolution;
            this.chunkResolution = chunkResolution;

            textureTileAmount = (voxelResolution * chunkResolution) / 2;
            stateValues = new int[(voxelResolution + 1) * (voxelResolution + 1)];

            verticeDictionary = new Dictionary<Vector3, int>();
            triangleDictionary = new Dictionary<Vector2, List<Triangle>>();

            CreateBuffers();
        }

        public void GenerateWholeMesh(Dictionary<Vector2Int, VoxelChunk> existingChunks) {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in existingChunks) {
                TriangulateChunkMesh(chunk.Value);
            }
        }

        public void TriangulateChunkMesh(VoxelChunk chunk) {
            Mesh mesh = new Mesh();
            mesh.name = "VoxelChunk Mesh";

            ShaderTriangulate(chunk, out vertices, out triangles, out colors);

            GetUVs(chunk);

            AddVerticeToDictionary(chunk);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors32 = colors;
            mesh.RecalculateNormals();

            chunk.GetComponent<MeshFilter>().mesh = mesh;
            chunk.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void ShaderTriangulate(VoxelChunk chunk, out Vector3[] vertices, out int[] triangles, out Color32[] colors) {
            int numThreadsPerResolution = Mathf.CeilToInt(voxelResolution / SHADER_THREADS);

            triangleBuffer.SetCounterValue(0);
            marchingSquareShader.SetBuffer(0, "_Vertices", verticeBuffer);
            marchingSquareShader.SetBuffer(0, "_Triangles", triangleBuffer);
            marchingSquareShader.SetBuffer(0, "_States", stateBuffer);
            marchingSquareShader.SetInt("_VoxelResolution", voxelResolution);
            marchingSquareShader.SetInt("_ChunkResolution", chunkResolution);

            SetupStates(chunk);
            stateBuffer.SetData(stateValues);

            marchingSquareShader.Dispatch(0, numThreadsPerResolution, numThreadsPerResolution, 1);

            ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
            int[] triCountArray = { 0 };
            triCountBuffer.GetData(triCountArray);
            int numTris = triCountArray[0];

            Triangle[] tris = new Triangle[numTris];
            triangleBuffer.GetData(tris, 0, 0, numTris);

            vertices = new Vector3[numTris * 3];
            triangles = new int[numTris * 3];
            colors = new Color32[numTris * 3];

            GetShaderData(numTris, tris, vertices, triangles, colors, chunk);
        }

        private void GetShaderData(int numTris, IList<Triangle> tris, IList<Vector3> vertices, IList<int> triangles, IList<Color32> colors, VoxelChunk chunk) {
            for (int i = 0; i < numTris; i++) {
                for (int j = 0; j < 3; j++) {
                    colors[i * 3 + j] = new Color32((byte)(tris[i].red * 255), (byte)(tris[i].green * 255),
                        (byte)(tris[i].blue * 255), 255);

                    triangles[i * 3 + j] = i * 3 + j;

                    Vector2 vertex = tris[i][j];
                    vertex.x *= chunkResolution * voxelResolution;
                    vertex.y *= chunkResolution * voxelResolution;

                    vertices[i * 3 + j] = vertex;

                    AddTriangleToDictionary(i * 3 + j, tris[i], chunk);
                }
            }
        }

        private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle, VoxelChunk chunk) {
            if (triangleDictionary.ContainsKey(vertices[vertexIndexKey])) {
                triangleDictionary[vertices[vertexIndexKey]].Add(triangle);
            } else {
                List<Triangle> triangleList = new List<Triangle> { triangle };
                triangleDictionary.Add(vertices[vertexIndexKey], triangleList);
            }
        }

        private void GetUVs(VoxelChunk chunk) {
            uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                float percentX = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].x) *
                                 textureTileAmount;
                float percentY = Mathf.InverseLerp(0, chunkResolution * voxelResolution, vertices[i].y) *
                                 textureTileAmount;
                uvs[i] = new Vector2(percentX, percentY);
            }
        }

        private void AddVerticeToDictionary(VoxelChunk chunk) {
            for (int i = 0; i < vertices.Length; i++) {
                if (!verticeDictionary.ContainsKey(vertices[i])) {
                    verticeDictionary.Add(vertices[i], i);
                }
            }
        }

        private void SetupStates(VoxelChunk chunk) {
            for (int i = 0, y = 0; y < voxelResolution; y++) {
                for (int x = 0; x < voxelResolution; x++, i++) {
                    stateValues[y * voxelResolution + x + y] = chunk.voxels[i].state;
                }
            }

            for (int y = 0; y < voxelResolution; y++) {
                if (chunk.xNeighbor) {
                    stateValues[y * voxelResolution + voxelResolution + y] =
                        chunk.xNeighbor.voxels[y * voxelResolution].state;
                } else {
                    stateValues[y * voxelResolution + voxelResolution + y] = -1;
                }
            }

            for (int x = 0; x < voxelResolution; x++) {
                if (chunk.yNeighbor) {
                    stateValues[(voxelResolution + 1) * voxelResolution + x] = chunk.yNeighbor.voxels[x].state;
                } else {
                    stateValues[(voxelResolution + 1) * voxelResolution + x] = -1;
                }
            }

            if (chunk.xyNeighbor) {
                stateValues[(voxelResolution + 1) * (voxelResolution + 1) - 1] = chunk.xyNeighbor.voxels[0].state;
            } else {
                stateValues[(voxelResolution + 1) * (voxelResolution + 1) - 1] = -1;
            }
        }

        public void CreateBuffers() {
            int numPoints = (voxelResolution + 1) * (voxelResolution + 1);
            int numVoxelsPerResolution = voxelResolution - 1;
            int numVoxels = numVoxelsPerResolution * numVoxelsPerResolution;
            int maxTriangleCount = numVoxels * 12;

            ReleaseBuffers();
            verticeBuffer = new ComputeBuffer(numPoints, sizeof(float) * 3);
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            stateBuffer = new ComputeBuffer(numPoints, sizeof(int));
        }

        private void ReleaseBuffers() {
            if (triangleBuffer != null) {
                verticeBuffer.Release();
                triangleBuffer.Release();
                triCountBuffer.Release();
                stateBuffer.Release();
            }
        }

        private void OnDestroy() {
            if (Application.isPlaying) {
                ReleaseBuffers();
            }
        }
    }
}