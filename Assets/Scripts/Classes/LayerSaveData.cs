using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LayerSaveData {
    public string name;
    public Dictionary<Vector2, ChunkSaveData> chunkDataDictionary = new Dictionary<Vector2, ChunkSaveData>();

    public LayerSaveData(string name, IEnumerable<VoxelChunk> chunks) {
        this.name = name;

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
}
