using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;

public class ColliderGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;

    // Stores the checked vertices in a chunk
    private readonly HashSet<Vector3> checkedVertices = new HashSet<Vector3>();
    // Stores a list of outlines made up of a list of vertices
    private readonly List<List<Vector3>> outlines = new List<List<Vector3>>();
    // Resolution to scale triangles to chunkSpace
    private int scaleResolution;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();

        scaleResolution = CORE.voxelResolution * CORE.chunkResolution;
    }

    public void GenerateChunkColliders(VoxelChunk chunk) {
        chunk.RemoveChunkColliders();
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

    private void CalculateChunkOutlines(VoxelChunk chunk) {
        foreach (Vector3 vertex in chunk.vertices) {
            if (checkedVertices.Contains(vertex)) continue;

            Vector3? newOutlineVertex = GetNextVertex(chunk, vertex);
            if (newOutlineVertex == null) continue;

            checkedVertices.Add(vertex);

            List<Vector3> newOutline = new List<Vector3> { vertex };
            outlines.Add(newOutline);
            FollowOutline(chunk, (Vector3)newOutlineVertex);
            outlines[outlines.Count - 1].Add(vertex);
        }
    }

    private void CreateColliders(VoxelChunk chunk) {
        foreach (List<Vector3> outline in outlines) {
            EdgeCollider2D edgeCollider = chunk.gameObject.AddComponent<EdgeCollider2D>();

            edgeCollider.points = outline.Select(dir => (Vector2)dir).ToArray();
        }
    }

    private Vector3? GetNextVertex(VoxelChunk chunk, Vector3 vertex) {
        foreach (Triangle triangle in chunk.triangleDictionary[vertex]) {
            // Loop through all vertices of the triangle
            for (int i = 0; i < 3; i++) {
                if (VectorsEqual(triangle[i] * scaleResolution, vertex)) {
                    Vector3 searchForVertex = triangle[(i + 1) % 3];
                    Vector3 scaledVertex = searchForVertex * scaleResolution;

                    if (!checkedVertices.Contains(scaledVertex) && IsOutlineEdge(vertex, searchForVertex, chunk)) {
                        return scaledVertex;
                    }
                }
            }
        }

        return null;
    }

    private void FollowOutline(VoxelChunk chunk, Vector3 newVertex) {
        outlines[outlines.Count - 1].Add(newVertex);
        checkedVertices.Add(newVertex);

        Vector3? nextVertex = GetNextVertex(chunk, newVertex);
        if (nextVertex != null) {
            FollowOutline(chunk, (Vector3)nextVertex);
        }
    }

    private static bool IsOutlineEdge(Vector3 startVertice, Vector3 endVertice, VoxelChunk chunk) {
        int sharedTriangleCount = 0;

        List<Triangle> startIndexTriangles = chunk.triangleDictionary[startVertice];

        foreach (Triangle unused in startIndexTriangles.Where(triangle =>
            (VectorsEqual(triangle[0], endVertice)) ||
            (VectorsEqual(triangle[1], endVertice)) ||
            (VectorsEqual(triangle[2], endVertice)))) {
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
