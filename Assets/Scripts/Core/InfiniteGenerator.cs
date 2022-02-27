using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChunkHelper;

public class InfiniteGenerator : MonoBehaviour {
    private List<CoreScriptableObject> COREs;
    private int voxelResolution, chunkResolution;

    private GameObject player;
    private Vector2 playerPosition;

    private VoxelChunkGenerator voxelChunkGenerator;

    private bool startGeneration;
    private readonly List<VoxelChunk> chunksToUpdate = new List<VoxelChunk>();

    private void Awake() {
        COREs = FindObjectOfType<VoxelCore>().GetAllCoreScriptableObjects();
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        voxelChunkGenerator = this.GetComponent<VoxelChunkGenerator>();
        this.voxelResolution = COREs[0].voxelResolution;
        this.chunkResolution = COREs[0].chunkResolution;
    }

    private void Update() {
        if (startGeneration && COREs[0].doInfiniteGeneration) {
            playerPosition = player.transform.position;
            UpdateAroundPlayer();
        }
    }

    public void StartGeneration() {
        startGeneration = true;
        UpdateAroundPlayer();
    }

    private void UpdateAroundPlayer() {
        foreach (CoreScriptableObject CORE in COREs) {
            RemoveOutOfBoundsChunks(CORE);

            GetInBoundsChunks(CORE);

            GenerateNewChunks(CORE);
        }
    }

    private void RemoveOutOfBoundsChunks(CoreScriptableObject CORE) {
        // Find chunks to remove
        List<Vector2Int> removeChunkPositionList = (
            from chunk
            in CORE.existingChunks
            where chunk.Value != null
            where IsOutOfBounds(playerPosition, chunk.Value.transform.position, voxelResolution, chunkResolution)
            select chunk.Key
        ).ToList();

        // Remove chunks
        foreach (Vector2Int position in removeChunkPositionList) {
            CORE.RemoveChunk(CORE.existingChunks[position]);
        }
    }

    private void GetInBoundsChunks(CoreScriptableObject CORE) {
        Vector2 p = playerPosition / voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        for (int x = -chunkResolution; x < chunkResolution; x++) {
            for (int y = -chunkResolution; y < chunkResolution; y++) {
                Vector2Int chunkCoord = new Vector2Int((int)(playerChunkCoord.x + x), (int)(playerChunkCoord.y + y));
                Vector2Int chunkPosition = new Vector2Int(chunkCoord.x * voxelResolution, chunkCoord.y * voxelResolution);

                if (CORE.existingChunks.ContainsKey(chunkPosition)) continue;

                chunksToUpdate.Add(voxelChunkGenerator.GetNewChunk(CORE, chunkPosition));
            }
        }
    }

    private void GenerateNewChunks(CoreScriptableObject CORE) {
        if (chunksToUpdate.Count > 0) {
            GenerateChunkList(CORE, chunksToUpdate);
            GenerateChunkList(CORE, FindImportantNeighbors(CORE, chunksToUpdate));

            chunksToUpdate.Clear();
        }
    }

    public void GenerateChunkList(CoreScriptableObject CORE, IEnumerable<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            chunk.GenerateChunk(CORE);
        }
    }

    public IEnumerable<VoxelChunk> FindImportantNeighbors(CoreScriptableObject CORE, IEnumerable<VoxelChunk> chunks) {
        List<VoxelChunk> neighborChunksToUpdate = new List<VoxelChunk>();

        foreach (Vector2Int setupCoord in chunks.Select(chunk => chunk.GetWholePosition())) {
            // Checks if bottom-left 3 neighbors are existing and adds to list if not already in list
            for (int i = -1; i < 1; i++) {
                for (int j = -1; j < 1; j++) {
                    if (i == 0 && j == 0) continue;

                    Vector2Int neighborCoord = new Vector2Int(setupCoord.x + (voxelResolution * i), setupCoord.y + (voxelResolution * j));
                    if (!CORE.existingChunks.ContainsKey(neighborCoord)) continue;

                    VoxelChunk neighborChunk = CORE.existingChunks[neighborCoord];
                    if (chunksToUpdate.Contains(neighborChunk) || neighborChunksToUpdate.Contains(neighborChunk)) continue;

                    neighborChunksToUpdate.Add(neighborChunk);
                }
            }
        }

        return neighborChunksToUpdate;
    }
}
