using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;

public class InfiniteGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;

    private GameObject player;
    private Vector2 playerPosition;

    private VoxelChunkGenerator voxelChunkGenerator;
    private VoxelMeshGenerator voxelMeshGenerator;

    private bool startGeneration = false;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        voxelChunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
        voxelMeshGenerator = FindObjectOfType<VoxelMeshGenerator>();
    }

    private void Update() {
        if (startGeneration && CORE.doInfiniteGeneration) {
            UpdateAroundPlayer();
        }
    }

    public void StartGeneration() {
        startGeneration = true;
        UpdateAroundPlayer();
    }

    private void UpdateAroundPlayer() {
        playerPosition = player.transform.position;

        RemoveOutOfBoundsChunks();

        CreateInBoundsChunks();

        TriangulateNewChunks();
    }

    private void RemoveOutOfBoundsChunks() {
        // Find chunks to remove
        List<Vector2Int> removeChunkPositionList = (
            from chunk
            in CORE.existingChunks
            where chunk.Value != null
            where IsOutOfBounds(chunk.Value.transform.position)
            select chunk.Key
        ).ToList();

        // Remove chunks
        foreach (Vector2Int position in removeChunkPositionList) {
            RemoveChunk(CORE.existingChunks[position]);
        }
    }

    private void CreateInBoundsChunks() {
        int chunkResolution = CORE.chunkResolution;
        int voxelResolution = CORE.voxelResolution;
        Vector2 p = playerPosition / voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        for (int x = -chunkResolution; x < chunkResolution; x++) {
            for (int y = -chunkResolution; y < chunkResolution; y++) {
                Vector2Int chunkCoord = new Vector2Int((int)(playerChunkCoord.x + x), (int)(playerChunkCoord.y + y));
                Vector2Int chunkPosition = new Vector2Int(chunkCoord.x * voxelResolution, chunkCoord.y * voxelResolution);

                if (CORE.existingChunks.ContainsKey(chunkPosition)) continue;
                VoxelChunk currentChunk = GetObjectPoolChunk(chunkPosition);
                voxelChunkGenerator.CreatePoolChunk(currentChunk, chunkPosition);
            }
        }
    }

    private void TriangulateNewChunks() {
        // TODO: Setup new neighbors only
        voxelChunkGenerator.SetupAllNeighbors();

        // TODO: Generate only for new chunks or relative neighbor chunks
        voxelMeshGenerator.GenerateWholeMesh();
    }

    private bool IsOutOfBounds(Vector2 chunkPosition) {
        Vector2 p = playerPosition / CORE.voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        return Vector2.Distance(chunkPosition / CORE.voxelResolution, playerChunkCoord) > CORE.chunkResolution + 1;
    }

    private void RemoveChunk(VoxelChunk chunk) {
        Vector3 position = chunk.transform.position;
        Vector2Int chunkCoord = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        CORE.existingChunks.Remove(chunkCoord);
        CORE.recycleableChunks.Enqueue(chunk);
        chunk.gameObject.SetActive(false);
    }

    private VoxelChunk GetObjectPoolChunk(Vector2 chunkCoord) {
        VoxelChunk currentChunk = CORE.recycleableChunks.Count > 0 ? CORE.recycleableChunks.Dequeue() : voxelChunkGenerator.CreateChunk(chunkCoord);
        currentChunk.FillChunk();

        return currentChunk;
    }
}
