using UnityEngine;

public static class ChunkHelper {
    public static Vector2Int GetWholePosition(this VoxelChunk chunk) {
        Vector2 position = chunk.transform.position;
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public static bool IsOutOfBounds(Vector2 playerPosition, Vector2 chunkPosition, int voxelResolution, int chunkResolution) {
        Vector2 p = playerPosition / voxelResolution;
        Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        return Vector2.Distance(chunkPosition / voxelResolution, playerChunkCoord) > chunkResolution + 1;
    }

    public static void RemoveChunk(this CoreScriptableObject CORE, VoxelChunk chunk) {
        Vector3 position = chunk.transform.position;
        Vector2Int chunkCoord = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        CORE.existingChunks.Remove(chunkCoord);
        chunk.ResetChunk();
        chunk.RemoveChunkColliders();
        CORE.recycleableChunks.Enqueue(chunk);
        chunk.gameObject.SetActive(false);
    }

    public static Vector2Int GetChunkPosition(Vector3 point, int voxelResolution) {
        return new Vector2Int((int)Mathf.Floor(Mathf.Floor(point.x) / voxelResolution), (int)Mathf.Floor(Mathf.Floor(point.y) / voxelResolution));
    }

    public static Vector2Int GetChunkWorldPosition(Vector3 point, int voxelResolution) {
        return GetChunkPosition(point, voxelResolution) * voxelResolution;
    }

    public static Vector2 GetVoxelPosition(Vector3 point, int voxelResolution) {
        Vector2 voxelPos = GetVoxelWorldPosition(point, voxelResolution);
        Vector2Int chunkPos = GetChunkWorldPosition(point, voxelResolution);
        return new Vector2(voxelPos.x - 0.5f, voxelPos.y - 0.5f);
    }

    public static Vector2 GetVoxelWorldPosition(Vector3 point, int voxelResolution) {
        Vector2Int chunkWorldOffset = GetChunkWorldPosition(point, voxelResolution);
        return new Vector2((Mathf.Floor(point.x) - chunkWorldOffset.x) + 0.5f, (Mathf.Floor(point.y) - chunkWorldOffset.y) + 0.5f);
    }

    public static int GetVoxelIndex(Vector3 point, int voxelResolution) {
        Vector2 voxelPos = GetVoxelPosition(point, voxelResolution);
        return (int)(voxelPos.x + voxelPos.y * voxelResolution);
    }

    public static bool ChunkContainsPosition(VoxelChunk chunk, Vector2 position, int voxelResolution) {
        Vector2 startPos = chunk.GetWholePosition();
        Vector2 endPos = startPos + Vector2.one * voxelResolution;

        return position.x >= startPos.x && position.x <= endPos.x && position.y >= startPos.y && position.y <= endPos.y;
    }
}
