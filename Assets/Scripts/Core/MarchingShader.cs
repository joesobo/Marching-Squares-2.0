using System.Collections.Generic;
using UnityEngine;
using Core;

public class MarchingShader : MonoBehaviour {
    private CoreScriptableObject CORE;

    public ComputeShader marchingSquareShader;
    public ComputeShader verticeShader;

    private const int SHADER_THREADS = 8;

    private int voxelResolution, chunkResolution;
    // Compute shader buffer storage
    private ComputeBuffer triangleBuffer, triCountBuffer, stateBuffer, verticeTriBuffer, verticeTriCountBuffer;

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
        int numThreadsPerResolution = 1;

        triangleBuffer.SetCounterValue(0);
        marchingSquareShader.SetBuffer(0, "_Triangles", triangleBuffer);
        marchingSquareShader.SetBuffer(0, "_States", stateBuffer);
        marchingSquareShader.SetInt("_VoxelResolution", voxelResolution);
        marchingSquareShader.SetInt("_ChunkResolution", chunkResolution);

        verticeTriBuffer.SetCounterValue(0);
        verticeShader.SetBuffer(0, "_Triangles", verticeTriBuffer);
        verticeShader.SetBuffer(0, "_States", stateBuffer);
        verticeShader.SetInt("_VoxelResolution", voxelResolution);
        verticeShader.SetInt("_ChunkResolution", chunkResolution);

        SetupStates(chunk);
        stateBuffer.SetData(stateValues);

        marchingSquareShader.Dispatch(0, numThreadsPerResolution, numThreadsPerResolution, 1);

        verticeShader.Dispatch(0, numThreadsPerResolution, numThreadsPerResolution, 1);



        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        triangles = new int[numTris * 3];
        colors = new Color32[numTris * 3];

        chunk.meshVertices = new Vector3[numTris * 3];

        GetMeshShaderData(numTris, tris, triangles, colors, chunk);



        ComputeBuffer.CopyCount(verticeTriBuffer, verticeTriCountBuffer, 0);
        int[] triVerticeCountArray = { 0 };
        verticeTriCountBuffer.GetData(triVerticeCountArray);
        int numVerticeTris = triVerticeCountArray[0];

        Triangle[] verticeTris = new Triangle[numVerticeTris];
        verticeTriBuffer.GetData(verticeTris, 0, 0, numVerticeTris);

        chunk.triangleDictionary.Clear();
        chunk.outlineVertices = new Vector3[numVerticeTris * 3];

        GetOutlineShaderData(numVerticeTris, verticeTris, chunk);
    }

    private void GetMeshShaderData(int numTris, IList<Triangle> tris, IList<int> triangles, IList<Color32> colors, VoxelChunk chunk) {
        for (int i = 0; i < numTris; i++) {
            for (int j = 0; j < 3; j++) {
                int index = i * 3 + j;
                colors[index] = new Color32(
                    (byte)(tris[i].red * 255),
                    (byte)(tris[i].green * 255),
                    (byte)(tris[i].blue * 255),
                    255);

                triangles[index] = index;

                Vector2 vertex = tris[i][j];
                vertex.x *= chunkResolution * voxelResolution;
                vertex.y *= chunkResolution * voxelResolution;

                chunk.meshVertices[index] = vertex;
            }
        }
    }

    private void GetOutlineShaderData(int numTris, IList<Triangle> tris, VoxelChunk chunk) {
        for (int i = 0; i < numTris; i++) {
            for (int j = 0; j < 3; j++) {
                int index = i * 3 + j;

                Vector2 vertex = tris[i][j];
                vertex.x *= chunkResolution * voxelResolution;
                vertex.y *= chunkResolution * voxelResolution;

                chunk.outlineVertices[index] = vertex;

                AddTrianglesToDictionary(chunk.outlineVertices[index], tris[i], chunk);
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
        int maxTriangleCount = numVoxels * 13;

        ReleaseBuffers();
        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        verticeTriBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        verticeTriCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        stateBuffer = new ComputeBuffer(numPoints, sizeof(int));
    }

    private void ReleaseBuffers() {
        if (triangleBuffer != null) {
            triangleBuffer.Release();
            verticeTriBuffer.Release();
            triCountBuffer.Release();
            verticeTriCountBuffer.Release();
            stateBuffer.Release();
        }
    }

    private void OnDestroy() {
        if (Application.isPlaying) {
            ReleaseBuffers();
        }
    }
}
