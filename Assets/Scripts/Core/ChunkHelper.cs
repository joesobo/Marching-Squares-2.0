using UnityEngine;

public static class ChunkHelper {
    public static Vector2Int GetWholePosition(VoxelChunk chunk) {
        Vector2 position = chunk.transform.position;
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public static bool IsOutOfBounds(Vector2 playerPosition, Vector2 chunkPosition, int voxelResolution, int chunkResolution) {
        Vector2 p = playerPosition / voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        return Vector2.Distance(chunkPosition / voxelResolution, playerChunkCoord) > chunkResolution + 1;
    }
}