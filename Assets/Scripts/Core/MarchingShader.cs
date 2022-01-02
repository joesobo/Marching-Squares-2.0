using System.Collections.Generic;
using UnityEngine;
using Core;

public class MarchingShader : MonoBehaviour {
    private CoreScriptableObject CORE;

    public ComputeShader marchingSquareShader;

    private const int SHADER_THREADS = 8;

    private int voxelResolution, chunkResolution;
    // Compute shader buffer storage
    private ComputeBuffer verticeBuffer, triangleBuffer, triCountBuffer, stateBuffer;

    // voxel state values for calculating type of voxel
    private int[] stateValues;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();

        this.voxelResolution = CORE.voxelResolution;
        this.chunkResolution = CORE.chunkResolution;

        stateValues = new int[(voxelResolution + 1) * (voxelResolution + 1)];

        CreateBuffers();
    }

    public void ShaderTriangulate(VoxelChunk chunk, out int[] triangles, out Color32[] colors) {
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

        chunk.triangleDictionary.Clear();
        chunk.vertices = new Vector3[numTris * 3];
        triangles = new int[numTris * 3];
        colors = new Color32[numTris * 3];

        GetShaderData(numTris, tris, triangles, colors, chunk);
    }

    private void GetShaderData(int numTris, IList<Triangle> tris, IList<int> triangles, IList<Color32> colors, VoxelChunk chunk) {
        for (int i = 0; i < numTris; i++) {
            for (int j = 0; j < 3; j++) {
                int index = i * 3 + j;
                colors[index] = new Color32((byte)(tris[i].red * 255), (byte)(tris[i].green * 255),
                    (byte)(tris[i].blue * 255), 255);

                triangles[index] = index;

                Vector2 vertex = tris[i][j];
                vertex.x *= chunkResolution * voxelResolution;
                vertex.y *= chunkResolution * voxelResolution;

                chunk.vertices[index] = vertex;

                AddTrianglesToDictionary(chunk.vertices[index], tris[i], chunk);
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
            if (chunk.xNeighbor && chunk.xNeighbor.voxels != null) {
                stateValues[y * voxelResolution + voxelResolution + y] = chunk.xNeighbor.voxels[y * voxelResolution].state;
            } else {
                stateValues[y * voxelResolution + voxelResolution + y] = -1;
            }
        }

        for (int x = 0; x < voxelResolution; x++) {
            if (chunk.yNeighbor && chunk.yNeighbor.voxels != null) {
                stateValues[(voxelResolution + 1) * voxelResolution + x] = chunk.yNeighbor.voxels[x].state;
            } else {
                stateValues[(voxelResolution + 1) * voxelResolution + x] = -1;
            }
        }

        if (chunk.xyNeighbor && chunk.xyNeighbor.voxels != null) {
            stateValues[(voxelResolution + 1) * (voxelResolution + 1) - 1] = chunk.xyNeighbor.voxels[0].state;
        } else {
            stateValues[(voxelResolution + 1) * (voxelResolution + 1) - 1] = -1;
        }
    }

    private static void AddTrianglesToDictionary(Vector3 vertice, Triangle triangle, VoxelChunk chunk) {
        if (chunk.triangleDictionary.ContainsKey(vertice)) {
            chunk.triangleDictionary[vertice].Add(triangle);
        } else {
            List<Triangle> triangleList = new List<Triangle> { triangle };
            chunk.triangleDictionary.Add(vertice, triangleList);
        }
    }


    private void CreateBuffers() {
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
