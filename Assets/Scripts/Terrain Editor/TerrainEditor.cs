using System.Collections.Generic;
using UnityEngine;

public static class TerrainEditor {
    public static void EditVoxels(IEnumerable<Voxel> voxels, TerrainEditingScriptableObject.Type editingType) {
        foreach (Voxel voxel in voxels) {
            voxel.state = voxel.state switch {
                1 when editingType == TerrainEditingScriptableObject.Type.Remove => 0,
                0 when editingType == TerrainEditingScriptableObject.Type.Fill => 1,
                _ => voxel.state
            };
        }
    }

    public static List<Voxel> GetSelectedVoxels(this VoxelChunk chunk, Vector2 selectPoint, int radius, int voxelResolution, TerrainEditingScriptableObject.Type editingType) {
        List<Voxel> selectedVoxels = new List<Voxel>();

        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                Vector2 hitPosition = new Vector2(selectPoint.x + i, selectPoint.y + j);

                // check if position exists in this chunk
                if (ChunkHelper.ChunkContainsPosition(chunk, hitPosition, voxelResolution)) {
                    Voxel voxel = chunk.voxels[ChunkHelper.GetVoxelIndex(hitPosition, voxelResolution)];

                    if (!selectedVoxels.Contains(voxel)) {
                        selectedVoxels.Add(voxel);
                    }
                }
            }
        }

        // Filter selected voxels by editing type
        List<Voxel> filteredVoxels = new List<Voxel>();
        foreach (Voxel voxel in selectedVoxels) {
            switch (voxel.state) {
                case 1 when editingType == TerrainEditingScriptableObject.Type.Remove:
                case 0 when editingType == TerrainEditingScriptableObject.Type.Fill:
                    filteredVoxels.Add(voxel);
                    break;
            }
        }

        return filteredVoxels;
    }
}
