using System.Collections.Generic;
using UnityEngine;

public class MeshShaderController : MonoBehaviour {
    public ComputeShader marchingSquareShader;

    private const int SHADER_THREADS = 8;

    private int voxelResolution, chunkResolution;
    // Compute shader buffer storage
    private ComputeBuffer triangleBuffer, triCountBuffer, stateBuffer;

    // voxel state values for calculating type of voxel
    private int[] stateValues;

    private void Awake() {
        CoreScriptableObject CORE = GetComponent<VoxelCore>().GetCoreScriptableObject(0);

        voxelResolution = CORE.voxelResolution;
        chunkResolution = CORE.chunkResolution;

        stateValues = new int[(voxelResolution + 1) * (voxelResolution + 1)];

        CreateBuffers();
    }

    public void ShaderTriangulate(VoxelChunk chunk, out int[] triangles, out Color32[] colors) {
        int numThreadsPerResolution = Mathf.CeilToInt(voxelResolution / SHADER_THREADS);

        stateValues = chunk.GetChunkStates();
        stateBuffer.SetData(stateValues);

        // Set compute shader parameters for mesh shader
        triangleBuffer.SetCounterValue(0);
        marchingSquareShader.SetBuffer(0, "_Triangles", triangleBuffer);
        marchingSquareShader.SetBuffer(0, "_States", stateBuffer);
        marchingSquareShader.SetInt("_VoxelResolution", voxelResolution);
        marchingSquareShader.SetInt("_ChunkResolution", chunkResolution);

        marchingSquareShader.Dispatch(0, numThreadsPerResolution, numThreadsPerResolution, 1);

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

    private void CreateBuffers() {
        int numPoints = (voxelResolution + 1) * (voxelResolution + 1);
        int numVoxelsPerResolution = voxelResolution - 1;
        int numVoxels = numVoxelsPerResolution * numVoxelsPerResolution;
        int maxTriangleCount = numVoxels * 12;

        ReleaseBuffers();
        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        stateBuffer = new ComputeBuffer(numPoints, sizeof(int));
    }

    private void ReleaseBuffers() {
        if (triangleBuffer != null) {
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
