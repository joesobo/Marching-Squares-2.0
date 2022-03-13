using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static SerializationManager;

public class ChunkSaveManager : MonoBehaviour {
    private WorldSaveManager worldSaveManager;

    private BinaryFormatter formatter;
    private List<LayerScriptableObject> layers;

    private void Start() {
        worldSaveManager = FindObjectOfType<WorldSaveManager>();
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();

        formatter = GetBinaryFormatter();

        foreach (LayerScriptableObject layer in layers) {
            worldSaveManager.currentLayerDatas.Add(ReadLayer(layer));
        }
    }

    // TODO: add in some sort of check to see if we should save this chunk by comparing to original noise values
    public void SaveChunk(VoxelChunk chunk, LayerSaveData layerData) {
        layerData.chunkDataDictionary.Add(chunk.transform.position, new ChunkSaveData(chunk));
    }

    private void SaveAllChunks(LayerScriptableObject layer) {
        int index = layers.IndexOf(layer);
        LayerSaveData layerData = worldSaveManager.currentLayerDatas[index];

        layerData.chunkDataDictionary.Clear();

        foreach (VoxelChunk chunk in layer.existingChunks.Values) {
            SaveChunk(chunk, layerData);
            Debug.Log("Saved chunk: " + chunk.transform.position);
        }

        string layerDataPath = worldSaveManager.worldPath + "layer_" + layer.name + ".save";
        FileStream layerStream = new FileStream(layerDataPath, FileMode.OpenOrCreate);

        layerStream.SetLength(0);
        formatter.Serialize(layerStream, layerData);

        layerStream.Close();
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
        string layerDataPath = worldSaveManager.worldPath + "layer_" + layer.name + ".save";

        FileStream layerStream = new FileStream(layerDataPath, FileMode.OpenOrCreate);
        LayerSaveData layerData = (LayerSaveData)formatter.Deserialize(layerStream);
        layerStream.Close();

        return layerData;
    }

    private void OnApplicationQuit() {
        foreach (LayerScriptableObject layer in layers) {
            Debug.Log("Saving layer: " + layer.name);
            SaveAllChunks(layer);
        }
    }
}
