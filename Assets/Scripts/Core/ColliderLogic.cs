using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

public static class ColliderLogic {
    // Stores the checked vertices in a chunk
    private static readonly HashSet<Vector3> checkedVertices = new HashSet<Vector3>();
    // Stores a list of outlines made up of a list of vertices
    private static readonly List<List<Vector3>> outlines = new List<List<Vector3>>();
    // Resolution to scale triangles to chunkSpace
    private static int scaleResolution;

    public static void Reset(int resolution) {
        scaleResolution = resolution;
        checkedVertices.Clear();
        outlines.Clear();
    }

    // Loop through all vertices until all are used in outlines
    public static IEnumerable<List<Vector3>> CalculateOutlines(VoxelChunk chunk) {
        foreach (Vector3 vertex in chunk.outlineVertices) {
            if (checkedVertices.Contains(vertex)) continue;

            Vector3? newOutlineVertex = GetNextVertex(chunk, vertex);
            if (newOutlineVertex == null) continue;

            checkedVertices.Add(vertex);

            // Create whole outline
            List<Vector3> newOutline = new List<Vector3> { vertex };
            outlines.Add(newOutline);
            FollowOutline(chunk, (Vector3)newOutlineVertex);
            outlines[outlines.Count - 1].Add(vertex);
        }
        return outlines;
    }

    // Searches for the next viable vertex in the outline
    private static Vector3? GetNextVertex(VoxelChunk chunk, Vector3 vertex) {
        foreach (Triangle triangle in chunk.triangleDictionary[vertex]) {
            // Loop through all vertices of the triangle
            for (int i = 0; i < 3; i++) {
                Vector2 triangleVertice = triangle[i] * scaleResolution;
                if (triangleVertice.Equal(vertex)) {
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

    // Follows the outline until it can't anymore (missing first vertice for full outline loop)
    private static void FollowOutline(VoxelChunk chunk, Vector3 newVertex) {
        outlines[outlines.Count - 1].Add(newVertex);
        checkedVertices.Add(newVertex);

        Vector3? nextVertex = GetNextVertex(chunk, newVertex);
        if (nextVertex != null) {
            FollowOutline(chunk, (Vector3)nextVertex);
        }
    }

    // Outline edges are a pair of vertices whos edges are only contains in one triangle
    private static bool IsOutlineEdge(Vector3 startVertice, Vector3 endVertice, VoxelChunk chunk) {
        int sharedTriangleCount = 0;

        List<Triangle> startVerticeTriangles = chunk.triangleDictionary[startVertice];

        foreach (Triangle unused in startVerticeTriangles.Where(triangle =>
            (triangle[0].Equal(endVertice)) ||
            (triangle[1].Equal(endVertice)) ||
            (triangle[2].Equal(endVertice)))) {
            sharedTriangleCount++;

            // Shared by more than 2 triangles, so it's not an outline edge
            if (sharedTriangleCount > 1) {
                return false;
            }
        }

        return sharedTriangleCount == 1;
    }
}
