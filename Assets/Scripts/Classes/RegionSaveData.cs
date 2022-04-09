using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegionSaveData {
    public string name;
    public Dictionary<Vector2, ChunkSaveData> chunkDataDictionary = new Dictionary<Vector2, ChunkSaveData>();

    public RegionSaveData(IEnumerable<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            chunkDataDictionary.Add(chunk.transform.position, new ChunkSaveData(chunk));
        }
    }

    public List<ChunkSaveData> GetAllChunks() {
        List<ChunkSaveData> chunkDataList = new List<ChunkSaveData>();

        foreach (ChunkSaveData chunkData in chunkDataDictionary.Values) {
            chunkDataList.Add(chunkData);
        }

        return chunkDataList;
    }

    private void OnEnable() {
        chunkDataDictionary.Clear();
    }
}
