using System;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunkGenerator : MonoBehaviour {
    // The element to spawn at each reference position along the chunk
    public GameObject voxelReferencePointsPrefab;
    // The chunk to spawn
    public GameObject voxelChunkPrefab;

    // The size of the chunk
    private int voxelResolution;

    public void CreateChunks(Dictionary<Vector2Int, VoxelChunk> existingChunks) {
        foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in existingChunks) {
            chunk.Value.FillChunk();
        }
    }

    public Dictionary<Vector2Int, VoxelChunk> SetupChunks(int chunkResolution, int voxelResolution, bool showVoxelReferencePoints) {
        this.voxelResolution = voxelResolution;
        Dictionary<Vector2Int, VoxelChunk> existingChunks = new Dictionary<Vector2Int, VoxelChunk>();

        for (int x = -chunkResolution; x < chunkResolution; x++) {
            for (int y = -chunkResolution; y < chunkResolution; y++) {
                int xPoint = x * voxelResolution;
                int yPoint = y * voxelResolution;

                Vector2 chunkPosition = new Vector2(xPoint, yPoint);

                VoxelChunk chunk = Instantiate(voxelChunkPrefab, chunkPosition, Quaternion.identity).GetComponent<VoxelChunk>();
                chunk.name = "Chunk (" + xPoint + ", " + yPoint + ")";
                chunk.SetupChunk(voxelResolution, showVoxelReferencePoints, voxelReferencePointsPrefab);

                existingChunks.Add(GetWholePosition(chunk), chunk);
            }
        }

        SetupAllNeighbors(existingChunks);
        return existingChunks;
    }

    private void SetupAllNeighbors(Dictionary<Vector2Int, VoxelChunk> existingChunks) {
        foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in existingChunks) {
            SetupChunkNeighbors(chunk.Value, existingChunks);
        }
    }

    private void SetupChunkNeighbors(VoxelChunk chunk, Dictionary<Vector2Int, VoxelChunk> existingChunks) {
        Vector2Int setupCoord = GetWholePosition(chunk);

        Vector2Int pxCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
        Vector2Int pyCoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
        Vector2Int pxyCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);

        if (!existingChunks.ContainsKey(setupCoord)) return;
        if (existingChunks.ContainsKey(pxCoord)) {
            chunk.xNeighbor = existingChunks[pxCoord];
        }

        if (existingChunks.ContainsKey(pyCoord)) {
            chunk.yNeighbor = existingChunks[pyCoord];
        }

        if (existingChunks.ContainsKey(pxyCoord)) {
            chunk.xyNeighbor = existingChunks[pxyCoord];
        }
    }

    private Vector2Int GetWholePosition(VoxelChunk chunk) {
        Vector2 position = chunk.transform.position;
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
}
