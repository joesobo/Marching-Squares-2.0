using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunkGenerator : MonoBehaviour {
    // The element to spawn at each reference position along the chunk
    public GameObject voxelReferencePointsPrefab;
    // The chunk to spawn
    public GameObject voxelChunkPrefab;

    // Place to store current chunks
    private Dictionary<Vector2Int, VoxelChunk> existingChunks = new Dictionary<Vector2Int, VoxelChunk>();
    private int voxelResolution;

    public List<VoxelChunk> SetupChunks(int chunkResolution, int voxelResolution, bool showVoxelReferencePoints) {
        this.voxelResolution = voxelResolution;
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
                AddChunkToExisting(chunk);
            }
        }
        
        SetupAllNeighbors(chunks);
        return chunks;
    }

    public void CreateChunks(List<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            chunk.FillChunk();
        }
    }

    private void AddChunkToExisting(VoxelChunk chunk) {
        existingChunks.Add(GetWholePosition(chunk), chunk);
    }

    private void SetupAllNeighbors(List<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            SetupChunkNeighbors(chunk);
        }
    }


    private void SetupChunkNeighbors(VoxelChunk chunk) {
        Vector2Int setupCoord = GetWholePosition(chunk);

        Vector2Int nxcoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y);
        Vector2Int nycoord = new Vector2Int(setupCoord.x, setupCoord.y - voxelResolution);
        Vector2Int nxycoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y - voxelResolution);
        Vector2Int pxcoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
        Vector2Int pycoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
        Vector2Int pxycoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);
        VoxelChunk tempChunk;

        if (!existingChunks.ContainsKey(setupCoord)) return;
        if (existingChunks.ContainsKey(nxcoord)) {
            tempChunk = existingChunks[nxcoord];
            tempChunk.xNeighbor = chunk;
        }

        if (existingChunks.ContainsKey(nycoord)) {
            tempChunk = existingChunks[nycoord];
            tempChunk.yNeighbor = chunk;
        }

        if (existingChunks.ContainsKey(nxycoord)) {
            tempChunk = existingChunks[nxycoord];
            tempChunk.xyNeighbor = chunk;
        }

        if (existingChunks.ContainsKey(pxcoord)) {
            tempChunk = existingChunks[pxcoord];
            chunk.xNeighbor = tempChunk;
        }

        if (existingChunks.ContainsKey(pycoord)) {
            tempChunk = existingChunks[pycoord];
            chunk.yNeighbor = tempChunk;
        }

        if (existingChunks.ContainsKey(pxycoord)) {
            tempChunk = existingChunks[pxycoord];
            chunk.xyNeighbor = tempChunk;
        }
    }

    private Vector2Int GetWholePosition(VoxelChunk chunk) {
        Vector2 position = chunk.transform.position;
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
}
