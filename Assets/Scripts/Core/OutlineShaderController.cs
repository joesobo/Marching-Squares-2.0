using System.Collections.Generic;
using UnityEngine;

public class OutlineShaderController : MonoBehaviour {
    public ComputeShader outlineShader;

    private const int SHADER_THREADS = 8;

    private int voxelResolution, chunkResolution;
    // Compute shader buffer storage
    private ComputeBuffer stateBuffer, outlineTriBuffer, outlineTriCountBuffer;

    // voxel state values for calculating type of voxel
    private int[] stateValues;

    private void Awake() {
        CoreScriptableObject CORE = GetComponent<VoxelCore>().GetCoreScriptableObject();

        voxelResolution = CORE.voxelResolution;
        chunkResolution = CORE.chunkResolution;

        stateValues = new int[(voxelResolution + 1) * (voxelResolution + 1)];

        CreateBuffers();
    }

    public void ShaderTriangulate(VoxelChunk chunk) {
        int numThreadsPerResolution = Mathf.CeilToInt(voxelResolution / SHADER_THREADS);

        stateValues = chunk.GetChunkStates();
        stateBuffer.SetData(stateValues);

        // Set compute shader parameters for outlines shader
        outlineTriBuffer.SetCounterValue(0);
        outlineShader.SetBuffer(0, "_Triangles", outlineTriBuffer);
        outlineShader.SetBuffer(0, "_States", stateBuffer);
        outlineShader.SetInt("_VoxelResolution", voxelResolution);
        outlineShader.SetInt("_ChunkResolution", chunkResolution);

        outlineShader.Dispatch(0, numThreadsPerResolution, numThreadsPerResolution, 1);

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
        outlineTriBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 2, ComputeBufferType.Append);
        outlineTriCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        stateBuffer = new ComputeBuffer(numPoints, sizeof(int));
    }

    private void ReleaseBuffers() {
        if (outlineTriBuffer != null) {
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
