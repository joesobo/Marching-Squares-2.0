using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelChunkGenerator {
    public static List<VoxelChunk> SetupChunks(int chunkResolution, int voxelResolution, bool showVoxelReferencePoints, GameObject voxelReferencePointsPrefab) {
        List<VoxelChunk> chunks = new List<VoxelChunk>();
        for (int x = -chunkResolution; x < chunkResolution; x++) {
            for (int y = -chunkResolution; y < chunkResolution; y++) {
                int xPoint = x * voxelResolution;
                int yPoint = y * voxelResolution;

                Vector2 chunkPosition = new Vector2(xPoint, yPoint);

                VoxelChunk chunk = new GameObject("Chunk (" + xPoint + ", " + yPoint + ")", typeof(VoxelChunk)).GetComponent<VoxelChunk>();
                chunk.transform.position = chunkPosition;
                chunk.SetupChunk(voxelResolution, showVoxelReferencePoints, voxelReferencePointsPrefab);

                chunks.Add(chunk);
            }
        }
        return chunks;
    }

    public static void CreateChunks(List<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            chunk.FillChunk();
        }
    }
}
