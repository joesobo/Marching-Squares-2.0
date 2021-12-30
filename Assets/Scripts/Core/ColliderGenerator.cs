using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;

public class ColliderGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;

    // Stores indices of checked vertices in a chunk
    private readonly HashSet<int> checkedVertices = new HashSet<int>();
    // Stores a list of outlines made up of a list of vertice indices
    private readonly List<List<int>> outlines = new List<List<int>>();
    // Resolution to scale triangles to chunkSpace
    private int scaleResolution;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();

        scaleResolution = CORE.voxelResolution * CORE.chunkResolution;
    }

    public void GenerateChunkColliders(VoxelChunk chunk) {
        RemoveChunkColliders(chunk);
        // Reset checked vertices
        ResetStores();

        // Calculate outlines
        CalculateChunkOutlines(chunk);

        // Create new colliders
        CreateColliders(chunk);
    }

    private void ResetStores() {
        checkedVertices.Clear();
        outlines.Clear();
    }

    private static void RemoveChunkColliders(VoxelChunk chunk) {
        foreach (EdgeCollider2D collider in chunk.gameObject.GetComponents<EdgeCollider2D>()) {
            Destroy(collider);
        }
    }

    private void CalculateChunkOutlines(VoxelChunk chunk) {
        for (int index = 0; index < chunk.vertices.Length; index++) {
            if (checkedVertices.Contains(index)) continue;

            int newOutlineIndex = GetNextVertexIndex(chunk, index);
            if (newOutlineIndex == -1) continue;

            checkedVertices.Add(index);

            List<int> newOutline = new List<int> { index };
            outlines.Add(newOutline);
            FollowOutline(chunk, newOutlineIndex);
            outlines[outlines.Count - 1].Add(index);
        }
    }

    private void CreateColliders(VoxelChunk chunk) {
        foreach (List<int> outline in outlines) {
            EdgeCollider2D edgeCollider = chunk.gameObject.AddComponent<EdgeCollider2D>();

            edgeCollider.points = outline.Select(point => chunk.vertices[point]).Select(dir => (Vector2)dir).ToArray();
        }
    }

    private int GetNextVertexIndex(VoxelChunk chunk, int index) {
        Vector2 currentVertice = chunk.vertices[index];

        foreach (Triangle triangle in chunk.triangleDictionary[currentVertice]) {
            // Loop through all vertices of the triangle
            for (int i = 0; i < 3; i++) {
                if (VectorsEqual(triangle[i] * scaleResolution, currentVertice)) {
                    Vector3 searchForVertex = triangle[(i + 1) % 3] * scaleResolution;
                    int foundIndex = chunk.verticeDictionary[searchForVertex];

                    if (foundIndex == -1) continue;

                    if (!checkedVertices.Contains(foundIndex) && IsOutlineEdge(index, foundIndex, chunk)) {
                        return foundIndex;
                    }
                }
            }
        }

        return -1;
    }

    private void FollowOutline(VoxelChunk chunk, int index) {
        outlines[outlines.Count - 1].Add(index);
        checkedVertices.Add(index);

        int nextIndex = GetNextVertexIndex(chunk, index);

        if (nextIndex != -1) {
            FollowOutline(chunk, nextIndex);
        }
    }

    private bool IsOutlineEdge(int startIndex, int endIndex, VoxelChunk chunk) {
        int sharedTriangleCount = 0;

        Vector2 startVertice = chunk.vertices[startIndex];
        Vector2 endVertice = chunk.vertices[endIndex];

        List<Triangle> startIndexTriangles = chunk.triangleDictionary[startVertice];

        foreach (Triangle unused in startIndexTriangles.Where(triangle =>
            (VectorsEqual(triangle[0] * scaleResolution, endVertice)) ||
            (VectorsEqual(triangle[1] * scaleResolution, endVertice)) ||
            (VectorsEqual(triangle[2] * scaleResolution, endVertice)))) {
            sharedTriangleCount++;

            if (sharedTriangleCount > 1) {
                break;
            }
        }

        return sharedTriangleCount == 1;
    }

    private static bool VectorsEqual(Vector2 a, Vector2 b) {
        return a.x == b.x && a.y == b.y;
    }
}
