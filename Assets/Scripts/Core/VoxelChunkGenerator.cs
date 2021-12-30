using UnityEngine;
using static ChunkHelper;

public class VoxelChunkGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;

    // The element to spawn at each reference position along the chunk
    public GameObject voxelReferencePointsPrefab;
    // The chunk to spawn
    public GameObject voxelChunkPrefab;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
    }

    public VoxelChunk CreateChunk(Vector2 chunkPosition) {
        GameObject chunkObject = Instantiate(voxelChunkPrefab, chunkPosition, Quaternion.identity);
        VoxelChunk chunk = chunkObject.AddComponent<VoxelChunk>();
        chunk.name = "Chunk (" + chunkPosition.x / CORE.voxelResolution + ", " + chunkPosition.y / CORE.voxelResolution + ")";
        chunk.SetupChunk(voxelReferencePointsPrefab);

        CORE.existingChunks.Add(GetWholePosition(chunk), chunk);
        return chunk;
    }

    public VoxelChunk CreatePoolChunk(VoxelChunk chunk, Vector2Int chunkPosition) {
        chunk.name = "Chunk (" + chunkPosition.x / CORE.voxelResolution + ", " + chunkPosition.y / CORE.voxelResolution + ")";
        chunk.gameObject.SetActive(true);
        chunk.transform.position = new Vector3(chunkPosition.x, chunkPosition.y, 0);
        chunk.ResetReferencePoints();

        if (!CORE.existingChunks.ContainsKey(chunkPosition)) {
            CORE.existingChunks.Add(GetWholePosition(chunk), chunk);
        }
        return chunk;
    }

    public void SetupChunkNeighbors(VoxelChunk chunk) {
        int voxelResolution = CORE.voxelResolution;
        Vector2Int setupCoord = GetWholePosition(chunk);

        if (!CORE.existingChunks.ContainsKey(setupCoord)) return;

        // Setup the chunk's neighbors
        Vector2Int xCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
        Vector2Int yCoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
        Vector2Int xyCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);

        // Setup the neighbors neighbor value with this chunk
        Vector2Int bxCoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y);
        Vector2Int byCoord = new Vector2Int(setupCoord.x, setupCoord.y - voxelResolution);
        Vector2Int bxyCoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y - voxelResolution);

        VoxelChunk tempChunk;

        if (CORE.existingChunks.ContainsKey(xCoord)) {
            chunk.xNeighbor = CORE.existingChunks[xCoord];
        }

        if (CORE.existingChunks.ContainsKey(yCoord)) {
            chunk.yNeighbor = CORE.existingChunks[yCoord];
        }

        if (CORE.existingChunks.ContainsKey(xyCoord)) {
            chunk.xyNeighbor = CORE.existingChunks[xyCoord];
        }

        if (CORE.existingChunks.ContainsKey(bxCoord)) {
            tempChunk = CORE.existingChunks[bxCoord];
            tempChunk.xNeighbor = chunk;
        }

        if (CORE.existingChunks.ContainsKey(byCoord)) {
            tempChunk = CORE.existingChunks[byCoord];
            tempChunk.yNeighbor = chunk;
        }

        if (CORE.existingChunks.ContainsKey(bxyCoord)) {
            tempChunk = CORE.existingChunks[bxyCoord];
            tempChunk.xyNeighbor = chunk;
        }
    }
}
