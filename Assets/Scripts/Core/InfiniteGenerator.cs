using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteGenerator : MonoBehaviour {
    // TODO: setup these variables with info (Setup)
    private Vector2 playerPosition;
    private Dictionary<Vector2Int, VoxelChunk> existingChunks;
    private Queue<VoxelChunk> recycleableChunks;
    private int chunkResolution = 2;
    private int voxelResolution;

    public void Setup() {

    }

    private void UpdateAroundPlayer() {
        RemoveOutOfBoundsChunks();

        CreateInBoundsChunks();

        TriangulateNewChunks();
    }

    public void RemoveOutOfBoundsChunks() {
        foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in existingChunks) {
            if (chunk.Value != null) {
                if (IsOutOfBounds(chunk.Value.transform.position)) {
                    RemoveChunk(chunk.Value);
                }
            }
        }
    }

    private void CreateInBoundsChunks() {
        Vector2 p = playerPosition / voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        for (int x = -chunkResolution; x <= chunkResolution; x++) {
            for (int y = -chunkResolution; y <= chunkResolution; y++) {
                Vector2Int chunkCoord = new Vector2Int((int)(playerChunkCoord.x + x), (int)(playerChunkCoord.y + y));

                if (!existingChunks.ContainsKey(chunkCoord)) {
                    VoxelChunk currentChunk = GetObjectPoolChunk();

                    existingChunks.Add(chunkCoord, currentChunk);
                }
            }
        }
    }

    // TODO: test this after getting references
    private void TriangulateNewChunks() {
        voxelChunkGenerator.SetupAllNeighbors(existingChunks);

        voxelMeshGenerator.GenerateWholeMesh(existingChunks);
    }

    private bool IsOutOfBounds(Vector2 chunkPosition) {
        Vector2 p = playerPosition / voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        return Vector2.Distance(chunkPosition, playerChunkCoord) > chunkResolution;
    }

    private void RemoveChunk(VoxelChunk chunk) {
        Vector2Int chunkCoord = new Vector2Int(Mathf.RoundToInt(chunk.transform.position.x), Mathf.RoundToInt(chunk.transform.position.y));
        existingChunks.Remove(chunkCoord);
        recycleableChunks.Enqueue(chunk);
    }

    // TODO: setup create / fill in of values for chunks
    // TODO: create CreateChunk function
    private VoxelChunk GetObjectPoolChunk() {
        VoxelChunk currentChunk;
        if (recycleableChunks.Count > 0) {
            currentChunk = recycleableChunks.Dequeue();
        } else {
            currentChunk = voxelMeshGenerator.CreateChunk();
        }
        currentChunk.FillChunk();

        return currentChunk;
    }
}
