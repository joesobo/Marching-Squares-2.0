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

        Vector2Int pxCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
        Vector2Int pyCoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
        Vector2Int pxyCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);

        if (!CORE.existingChunks.ContainsKey(setupCoord)) return;
        if (CORE.existingChunks.ContainsKey(pxCoord)) {
            chunk.xNeighbor = CORE.existingChunks[pxCoord];
        }

        if (CORE.existingChunks.ContainsKey(pyCoord)) {
            chunk.yNeighbor = CORE.existingChunks[pyCoord];
        }

        if (CORE.existingChunks.ContainsKey(pxyCoord)) {
            chunk.xyNeighbor = CORE.existingChunks[pxyCoord];
        }
    }
}
