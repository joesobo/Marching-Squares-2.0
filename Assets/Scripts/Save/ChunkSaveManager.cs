using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static SerializationManager;

public class ChunkSaveManager : MonoBehaviour {
    private WorldSaveManager worldSaveManager;

    private BinaryFormatter formatter;
    private List<LayerScriptableObject> layers;

    private void Awake() {
        worldSaveManager = FindObjectOfType<WorldSaveManager>();
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();

        formatter = GetBinaryFormatter();
    }

    private void Start() {
        foreach (LayerScriptableObject layer in layers) {
            worldSaveManager.currentLayerDatas.Add(ReadLayer(layer));
        }
    }

    public void SaveChunk(VoxelChunk chunk, LayerScriptableObject layer) {
        if (chunk.hasEditsToSave) {
            int index = layers.IndexOf(layer);
            LayerSaveData layerData = worldSaveManager.currentLayerDatas[index];
            FileStream layerStream = worldSaveManager.layerStreams[index];

            // overwrite or add new chunk info
            if (layerData.chunkDataDictionary.ContainsKey(chunk.transform.position)) {
                layerData.chunkDataDictionary[chunk.transform.position] = new ChunkSaveData(chunk);
            } else {
                layerData.chunkDataDictionary.Add(chunk.transform.position, new ChunkSaveData(chunk));
            }

            layerStream.SetLength(0);
            formatter.Serialize(layerStream, layerData);
        }
    }

    private void SaveAllChunks(LayerScriptableObject layer) {
        int index = layers.IndexOf(layer);
        LayerSaveData layerData = worldSaveManager.currentLayerDatas[index];
        FileStream layerStream = worldSaveManager.layerStreams[index];

        foreach (VoxelChunk chunk in layer.existingChunks.Values) {
            if (chunk.hasEditsToSave) {
                if (layerData.chunkDataDictionary.ContainsKey(chunk.transform.position)) {
                    layerData.chunkDataDictionary[chunk.transform.position] = new ChunkSaveData(chunk);
                } else {
                    layerData.chunkDataDictionary.Add(chunk.transform.position, new ChunkSaveData(chunk));
                }
            }
        }

        layerStream.SetLength(0);
        formatter.Serialize(layerStream, layerData);
    }

    public void LoadChunkData(Vector2 chunkPos, LayerScriptableObject layer, VoxelChunk chunk) {
        int index = layers.IndexOf(layer);
        LayerSaveData layerData = worldSaveManager.currentLayerDatas[index];

        if (layerData.chunkDataDictionary.ContainsKey(chunkPos)) {
            ChunkSaveData chunkData = layerData.chunkDataDictionary[chunkPos];

            chunk.transform.position = chunkData.position;
            for (int i = 0; i < chunk.voxels.Length; i++) {
                chunk.voxels[i].position = chunkData.voxelPositions[i];
                chunk.voxels[i].state = chunkData.voxelStates[i];
            }
        }
    }

    private LayerSaveData ReadLayer(LayerScriptableObject layer) {
        int index = layers.IndexOf(layer);

        FileStream layerStream = worldSaveManager.layerStreams[index];
        layerStream.Position = 0;
        LayerSaveData layerData = (LayerSaveData)formatter.Deserialize(layerStream);

        return layerData;
    }

    private void OnApplicationQuit() {
        foreach (LayerScriptableObject layer in layers) {
            Debug.Log("Saving layer: " + layer.name);
            SaveAllChunks(layer);
        }

        worldSaveManager.CloseWorld();
    }
}
