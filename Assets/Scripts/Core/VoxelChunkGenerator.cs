using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunkGenerator : MonoBehaviour {
    // The element to spawn at each reference position along the chunk
    public GameObject voxelReferencePointsPrefab;
    // The chunk to spawn
    public GameObject voxelChunkPrefab;

    public List<VoxelChunk> SetupChunks(int chunkResolution, int voxelResolution, bool showVoxelReferencePoints) {
        List<VoxelChunk> chunks = new List<VoxelChunk>();
        for (int x = -chunkResolution; x < chunkResolution; x++) {
            for (int y = -chunkResolution; y < chunkResolution; y++) {
                int xPoint = x * voxelResolution;
                int yPoint = y * voxelResolution;

                Vector2 chunkPosition = new Vector2(xPoint, yPoint);

                VoxelChunk chunk = Instantiate(voxelChunkPrefab, chunkPosition, Quaternion.identity).GetComponent<VoxelChunk>();
                chunk.name = "Chunk (" + xPoint + ", " + yPoint + ")";
                chunk.SetupChunk(voxelResolution, showVoxelReferencePoints, voxelReferencePointsPrefab);

                chunks.Add(chunk);
            }
        }
        return chunks;
    }

    public void CreateChunks(List<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            chunk.FillChunk();
        }
    }
}
