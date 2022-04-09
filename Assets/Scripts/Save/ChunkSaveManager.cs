using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static SerializationManager;

public class ChunkSaveManager : MonoBehaviour {
    private RegionSaveManager regionSaveManager;

    private BinaryFormatter formatter;
    private List<LayerScriptableObject> layers;
    private CoreScriptableObject CORE;

    private void Awake() {
        regionSaveManager = FindObjectOfType<RegionSaveManager>();
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();

        formatter = GetBinaryFormatter();
    }

    public void SaveChunk(VoxelChunk chunk, LayerScriptableObject layer) {
        if (chunk.hasEditsToSave) {
            Vector3 regionPos = RegionPosFromChunkPos(chunk.transform.position, layer);

            if (regionSaveManager.regionDatas.ContainsKey(regionPos)) {
                RegionSaveData regionData = regionSaveManager.regionDatas[regionPos];
                FileStream regionStream = regionSaveManager.regionStreams[regionPos];

                // overwrite or add new chunk info
                if (regionData.chunkDataDictionary.ContainsKey(chunk.transform.position)) {
                    regionData.chunkDataDictionary[chunk.transform.position] = new ChunkSaveData(chunk);
                } else {
                    regionData.chunkDataDictionary.Add(chunk.transform.position, new ChunkSaveData(chunk));
                }

                regionStream.SetLength(0);
                formatter.Serialize(regionStream, regionData);
            }
        }
    }

    private void SaveAllChunks(LayerScriptableObject layer) {
        foreach (VoxelChunk chunk in layer.existingChunks.Values) {
            SaveChunk(chunk, layer);
        }
    }

    public void LoadChunkData(Vector2 chunkPos, LayerScriptableObject layer, VoxelChunk chunk) {
        Vector3 regionPos = RegionPosFromChunkPos(chunkPos, layer);
        regionSaveManager.OpenRegion(regionPos, layer);
        RegionSaveData regionData = regionSaveManager.regionDatas[regionPos];

        // if chunk data exists, load it
        if (regionData.chunkDataDictionary.ContainsKey(chunkPos)) {
            ChunkSaveData chunkData = regionData.chunkDataDictionary[chunkPos];

            chunk.transform.position = chunkData.position;
            for (int i = 0; i < chunk.voxels.Length; i++) {
                chunk.voxels[i].position = chunkData.voxelPositions[i];
                chunk.voxels[i].state = chunkData.voxelStates[i];
            }
        }
    }

    public GameObject GetChunkRegion(VoxelChunk chunk, LayerScriptableObject layer) {
        Vector3 regionPos = RegionPosFromChunkPos(chunk.transform.position, layer);

        if (layer.regionDictionary.ContainsKey(regionPos)) {
            return layer.regionDictionary[regionPos];
        } else {
            CheckForEmptyRegions();

            GameObject region = new GameObject(regionPos.ToString());
            region.name = "Region " + regionPos.ToString();
            region.transform.parent = layer.parentReference;
            region.transform.position = regionPos;
            layer.regionDictionary.Add(regionPos, region);
            regionSaveManager.OpenRegion(regionPos, layer);

            return region;
        }
    }

    private Vector3 RegionPosFromChunkPos(Vector3 chunkPos, LayerScriptableObject layer) {
        return new Vector3((int)Mathf.Floor(chunkPos.x / (CORE.voxelResolution * CORE.regionResolution)), (int)Mathf.Floor(chunkPos.y / (CORE.voxelResolution * CORE.regionResolution)), layer.zIndex);
    }

    public void CheckForEmptyRegions() {
        foreach (LayerScriptableObject layer in layers) {
            for (int i = 0; i < layer.regionDictionary.Values.Count; i++) {
                GameObject region = layer.regionDictionary.ElementAt(i).Value;
                if (region.transform.childCount == 0) {
                    regionSaveManager.CloseRegion(region.transform.position);
                    layer.regionDictionary.Remove(region.transform.position);
                    Destroy(region.gameObject);
                    i--;
                }
            }
        }
    }

    private void OnApplicationQuit() {
        foreach (LayerScriptableObject layer in layers) {
            SaveAllChunks(layer);
        }
    }
}
