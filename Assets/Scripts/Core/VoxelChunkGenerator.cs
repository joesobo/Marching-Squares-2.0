using UnityEngine;

public class VoxelChunkGenerator : MonoBehaviour {
    // The element to spawn at each reference position along the chunk
    public GameObject voxelReferencePointsPrefab;
    // The chunk to spawn
    public GameObject voxelChunkPrefab;

    private VoxelChunk CreateChunk(LayerScriptableObject layer, Vector2 chunkPosition) {
        GameObject chunkObject = Instantiate(voxelChunkPrefab, chunkPosition, Quaternion.identity, transform);
        VoxelChunk chunk = chunkObject.GetComponent<VoxelChunk>();
        chunk.SetupChunk(layer, voxelReferencePointsPrefab, chunkPosition);

        layer.existingChunks.Add(chunk.GetWholePosition(), chunk);
        return chunk;
    }

    private VoxelChunk CreatePoolChunk(LayerScriptableObject layer, VoxelChunk chunk, Vector2 chunkPosition) {
        chunk.SetupChunk(layer, voxelReferencePointsPrefab, chunkPosition);
        chunk.ResetReferencePoints();

        layer.existingChunks.Add(chunk.GetWholePosition(), chunk);
        return chunk;
    }

    public VoxelChunk GetNewChunk(LayerScriptableObject layer, Vector2 chunkPosition) {
        return layer.recycleableChunks.Count > 0 ? CreatePoolChunk(layer, layer.recycleableChunks.Dequeue(), chunkPosition) : CreateChunk(layer, chunkPosition);
    }

    public static void SetupChunkNeighbors(LayerScriptableObject layer, VoxelChunk chunk) {
        int voxelResolution = layer.CORE.voxelResolution;
        Vector2Int setupCoord = chunk.GetWholePosition();

        if (!layer.existingChunks.ContainsKey(setupCoord)) return;

        // Setup the chunk's neighbors
        Vector2Int xCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
        Vector2Int yCoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
        Vector2Int xyCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);

        // Setup the neighbors neighbor value with this chunk
        Vector2Int bxCoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y);
        Vector2Int byCoord = new Vector2Int(setupCoord.x, setupCoord.y - voxelResolution);
        Vector2Int bxyCoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y - voxelResolution);

        VoxelChunk tempChunk;

        if (layer.existingChunks.ContainsKey(xCoord)) {
            chunk.xNeighbor = layer.existingChunks[xCoord];
        }

        if (layer.existingChunks.ContainsKey(yCoord)) {
            chunk.yNeighbor = layer.existingChunks[yCoord];
        }

        if (layer.existingChunks.ContainsKey(xyCoord)) {
            chunk.xyNeighbor = layer.existingChunks[xyCoord];
        }

        if (layer.existingChunks.ContainsKey(bxCoord)) {
            tempChunk = layer.existingChunks[bxCoord];
            tempChunk.xNeighbor = chunk;
        }

        if (layer.existingChunks.ContainsKey(byCoord)) {
            tempChunk = layer.existingChunks[byCoord];
            tempChunk.yNeighbor = chunk;
        }

        if (layer.existingChunks.ContainsKey(bxyCoord)) {
            tempChunk = layer.existingChunks[bxyCoord];
            tempChunk.xyNeighbor = chunk;
        }
    }
}
