using System.Collections.Generic;
using UnityEngine;

public static class TerrainEditorGizmos {
    public static void DrawChunksGizmo(Vector3 mousePos, int radius, int voxelResolution) {
        List<Vector2Int> chunkPositions = new List<Vector2Int>();
        Gizmos.color = Color.red;

        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                Vector2 hitPosition = new Vector2(mousePos.x + i, mousePos.y + j);
                Vector2Int chunkPos = ChunkHelper.GetChunkWorldPosition(hitPosition, voxelResolution);
                if (!chunkPositions.Contains(chunkPos)) {
                    chunkPositions.Add(chunkPos);
                }
            }
        }
    }

    public static void DrawVoxelEditingGizmo(Vector3 mousePos, TerrainEditingScriptableObject editingScriptableObject, int voxelResolution) {
        Gizmos.color = Color.blue;
        Vector2Int chunkPos = ChunkHelper.GetChunkWorldPosition(mousePos, voxelResolution);
        Vector2 voxelLocalPosition = ChunkHelper.GetVoxelWorldPosition(mousePos, voxelResolution);
        Vector2 voxelPosition = voxelLocalPosition + chunkPos;
        int radius = editingScriptableObject.Radius;
        Stencil stencil = editingScriptableObject.StencilType;

        if (stencil == Stencil.Circle) {
            Gizmos.DrawWireSphere(new Vector3(voxelPosition.x, voxelPosition.y, 0), radius + 0.5f);
        } else {
            Gizmos.DrawWireCube(new Vector3(voxelPosition.x, voxelPosition.y, 0), Vector3.one * (radius * 2 + 1));
        }
    }
}
