using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkSaveData {
    public Vector3 position;
    public List<Vector2> voxelPositions = new List<Vector2>();
    public List<int> voxelStates = new List<int>();

    public ChunkSaveData(VoxelChunk chunk) {
        position = chunk.transform.position;

        foreach (Voxel voxel in chunk.voxels) {
            voxelPositions.Add(voxel.position);
            voxelStates.Add(voxel.state);
        }
    }
}
