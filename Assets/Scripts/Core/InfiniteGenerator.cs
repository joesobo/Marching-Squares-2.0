using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChunkHelper;

public class InfiniteGenerator : MonoBehaviour {
    private List<LayerScriptableObject> layers;
    private CoreScriptableObject CORE;
    private ChunkSaveManager chunkSaveManager;
    private int voxelResolution, chunkResolution;

    private GameObject player;
    private Vector2 playerPosition;

    private VoxelChunkGenerator voxelChunkGenerator;

    private bool startGeneration;
    private readonly List<VoxelChunk> chunksToUpdate = new List<VoxelChunk>();

    private void Awake() {
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        chunkSaveManager = FindObjectOfType<ChunkSaveManager>();
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        voxelChunkGenerator = GetComponent<VoxelChunkGenerator>();
        voxelResolution = CORE.voxelResolution;
        chunkResolution = CORE.chunkResolution;
    }

    private void Update() {
        if (startGeneration) {
            playerPosition = player.transform.position;
            UpdateAroundPlayer();
        }
    }

    public void StartGeneration() {
        startGeneration = true;
        UpdateAroundPlayer();
    }

    private void UpdateAroundPlayer() {
        foreach (LayerScriptableObject layer in layers.Where(layer => layer.doInfiniteGeneration)) {
            RemoveOutOfBoundsChunks(layer);

            GetInBoundsChunks(layer);

            GenerateNewChunks(layer);
        }
    }

    private void RemoveOutOfBoundsChunks(LayerScriptableObject layer) {
        // Find chunks to remove
        List<Vector2Int> removeChunkPositionList = (
            from chunk
            in layer.existingChunks
            where chunk.Value != null
            where IsOutOfBounds(playerPosition, chunk.Value.transform.position, voxelResolution, chunkResolution)
            select chunk.Key
        ).ToList();

        // Remove chunks
        foreach (Vector2Int position in removeChunkPositionList) {
            VoxelChunk currentChunk = layer.existingChunks[position];

            chunkSaveManager.SaveChunk(currentChunk, layer);
            layer.RemoveChunk(currentChunk);
        }

        chunkSaveManager.CheckForEmptyRegions();
    }

    private void GetInBoundsChunks(LayerScriptableObject layer) {
        Vector2 p = playerPosition / voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        for (int x = -chunkResolution; x < chunkResolution; x++) {
            for (int y = -chunkResolution; y < chunkResolution; y++) {
                Vector2Int chunkCoord = new Vector2Int((int)(playerChunkCoord.x + x), (int)(playerChunkCoord.y + y));
                Vector2Int chunkPosition = new Vector2Int(chunkCoord.x * voxelResolution, chunkCoord.y * voxelResolution);

                if (layer.existingChunks.ContainsKey(chunkPosition)) continue;

                VoxelChunk newChunk = voxelChunkGenerator.GetNewChunk(layer, chunkPosition);
                chunkSaveManager.LoadChunkData(chunkPosition, layer, newChunk);
                chunksToUpdate.Add(newChunk);
            }
        }
    }

    private void GenerateNewChunks(LayerScriptableObject layer) {
        if (chunksToUpdate.Count > 0) {
            GenerateChunkList(layer, chunksToUpdate);
            GenerateChunkList(layer, FindImportantNeighbors(layer, chunksToUpdate));

            chunksToUpdate.Clear();
        }
    }

    public static void GenerateChunkList(LayerScriptableObject layer, IEnumerable<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            chunk.GenerateChunk(layer);
        }
    }

    public IEnumerable<VoxelChunk> FindImportantNeighbors(LayerScriptableObject layer, IEnumerable<VoxelChunk> chunks) {
        List<VoxelChunk> neighborChunksToUpdate = new List<VoxelChunk>();

        foreach (Vector2Int setupCoord in chunks.Select(chunk => chunk.GetWholePosition())) {
            // Checks if bottom-left 3 neighbors are existing and adds to list if not already in list
            for (int i = -1; i < 1; i++) {
                for (int j = -1; j < 1; j++) {
                    if (i == 0 && j == 0) continue;

                    Vector2Int neighborCoord = new Vector2Int(setupCoord.x + (voxelResolution * i), setupCoord.y + (voxelResolution * j));
                    if (!layer.existingChunks.ContainsKey(neighborCoord)) continue;

                    VoxelChunk neighborChunk = layer.existingChunks[neighborCoord];
                    if (chunksToUpdate.Contains(neighborChunk) || neighborChunksToUpdate.Contains(neighborChunk)) continue;

                    neighborChunksToUpdate.Add(neighborChunk);
                }
            }
        }

        return neighborChunksToUpdate;
    }
}
