using System.Collections.Generic;
using UnityEngine;
using Core;

public class MarchingShader : MonoBehaviour {
    private CoreScriptableObject CORE;

    public ComputeShader marchingSquareShader;
    public ComputeShader outlineShader;

    private const int SHADER_THREADS = 8;

    private int voxelResolution, chunkResolution;
    // Compute shader buffer storage
    private ComputeBuffer triangleBuffer, triCountBuffer, stateBuffer, outlineTriBuffer, outlineTriCountBuffer;

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

        SetupStates(chunk);
        stateBuffer.SetData(stateValues);

        // Set compute shader parameters for mesh shader
        triangleBuffer.SetCounterValue(0);
        marchingSquareShader.SetBuffer(0, "_Triangles", triangleBuffer);
        marchingSquareShader.SetBuffer(0, "_States", stateBuffer);
        marchingSquareShader.SetInt("_VoxelResolution", voxelResolution);
        marchingSquareShader.SetInt("_ChunkResolution", chunkResolution);

        // Set compute shader parameters for outlines shader
        outlineTriBuffer.SetCounterValue(0);
        outlineShader.SetBuffer(0, "_Triangles", outlineTriBuffer);
        outlineShader.SetBuffer(0, "_States", stateBuffer);
        outlineShader.SetInt("_VoxelResolution", voxelResolution);
        outlineShader.SetInt("_ChunkResolution", chunkResolution);

        marchingSquareShader.Dispatch(0, numThreadsPerResolution, numThreadsPerResolution, 1);
        outlineShader.Dispatch(0, numThreadsPerResolution, numThreadsPerResolution, 1);

        // Get data from mesh buffers
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

        // Get data from outlines buffers
        ComputeBuffer.CopyCount(outlineTriBuffer, outlineTriCountBuffer, 0);
        int[] outlineTriCountArray = { 0 };
        outlineTriCountBuffer.GetData(outlineTriCountArray);
        int numOutlineTris = outlineTriCountArray[0];

        OutlineTriangle[] verticeTris = new OutlineTriangle[numOutlineTris];
        outlineTriBuffer.GetData(verticeTris, 0, 0, numOutlineTris);

        chunk.triangleDictionary.Clear();
        chunk.outlineVertices = new Vector3[numOutlineTris * 3];

        GetOutlineShaderData(numOutlineTris, verticeTris, chunk);
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

    private void GetOutlineShaderData(int numTris, IList<OutlineTriangle> tris, VoxelChunk chunk) {
        for (int i = 0; i < numTris; i++) {
            for (int j = 0; j < 3; j++) {
                int index = i * 3 + j;

                Vector2 vertex = tris[i][j];
                vertex.x *= chunkResolution * voxelResolution;
                vertex.y *= chunkResolution * voxelResolution;

                chunk.outlineVertices[index] = vertex;

                AddOutlineTrianglesToDictionary(chunk.outlineVertices[index], tris[i], chunk);
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

    private static void AddOutlineTrianglesToDictionary(Vector3 vertice, OutlineTriangle triangle, VoxelChunk chunk) {
        if (chunk.triangleDictionary.ContainsKey(vertice)) {
            chunk.triangleDictionary[vertice].Add(triangle);
        } else {
            List<OutlineTriangle> triangleList = new List<OutlineTriangle> { triangle };
            chunk.triangleDictionary.Add(vertice, triangleList);
        }
    }


    private void CreateBuffers() {
        int numPoints = (voxelResolution + 1) * (voxelResolution + 1);
        int numVoxelsPerResolution = voxelResolution - 1;
        int numVoxels = numVoxelsPerResolution * numVoxelsPerResolution;
        int maxTriangleCount = numVoxels * 12;

        ReleaseBuffers();
        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        outlineTriBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 2, ComputeBufferType.Append);
        outlineTriCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        stateBuffer = new ComputeBuffer(numPoints, sizeof(int));
    }

    private void ReleaseBuffers() {
        if (triangleBuffer != null) {
            triangleBuffer.Release();
            triCountBuffer.Release();
            outlineTriBuffer.Release();
            outlineTriCountBuffer.Release();
            stateBuffer.Release();
        }
    }

    private void OnDestroy() {
        if (Application.isPlaying) {
            ReleaseBuffers();
        }
    }
}
