using UnityEngine;

public class VoxelChunkGenerator : MonoBehaviour {
    // The element to spawn at each reference position along the chunk
    public GameObject voxelReferencePointsPrefab;
    // The chunk to spawn
    public GameObject voxelChunkPrefab;

    private VoxelChunk CreateChunk(CoreScriptableObject CORE, Vector2 chunkPosition) {
        GameObject chunkObject = Instantiate(voxelChunkPrefab, chunkPosition, Quaternion.identity, transform);
        VoxelChunk chunk = chunkObject.GetComponent<VoxelChunk>();
        chunk.SetupChunk(CORE, voxelReferencePointsPrefab, chunkPosition);

        CORE.existingChunks.Add(chunk.GetWholePosition(), chunk);
        return chunk;
    }

    private VoxelChunk CreatePoolChunk(CoreScriptableObject CORE, VoxelChunk chunk, Vector2 chunkPosition) {
        chunk.SetupChunk(CORE, voxelReferencePointsPrefab, chunkPosition);
        chunk.ResetReferencePoints();

        CORE.existingChunks.Add(chunk.GetWholePosition(), chunk);
        return chunk;
    }

    public VoxelChunk GetNewChunk(CoreScriptableObject CORE, Vector2 chunkPosition) {
        return CORE.recycleableChunks.Count > 0 ? CreatePoolChunk(CORE, CORE.recycleableChunks.Dequeue(), chunkPosition) : CreateChunk(CORE, chunkPosition);
    }

    public static void SetupChunkNeighbors(CoreScriptableObject CORE, VoxelChunk chunk) {
        int voxelResolution = CORE.voxelResolution;
        Vector2Int setupCoord = chunk.GetWholePosition();

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
