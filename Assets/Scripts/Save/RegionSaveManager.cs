using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static SerializationManager;

public class RegionSaveManager : MonoBehaviour {
    private BinaryFormatter formatter;

    private WorldSaveManager worldSaveManager;
    private WorldSaveData currentWorldData;

    public Dictionary<Vector3, RegionSaveData> regionDatas;
    public Dictionary<Vector3, FileStream> regionStreams;

    private string regionPath = "";

    private void Awake() {
        formatter = GetBinaryFormatter();

        worldSaveManager = FindObjectOfType<WorldSaveManager>();

        regionDatas = new Dictionary<Vector3, RegionSaveData>();
        regionStreams = new Dictionary<Vector3, FileStream>();
    }

    public void OpenRegion(Vector3 regionPos, LayerScriptableObject layer) {
        string regionDataPath = worldSaveManager.worldPath + "layers/layer_regions_" + layer.name + "/region_" + ((Vector2)regionPos) + ".save";

        if (regionDatas.ContainsKey(regionPos)) return;
        FileStream regionStream = new FileStream(regionDataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        regionStreams.Add(regionPos, regionStream);

        if (regionStream.Length > 0) {
            RegionSaveData regionData = (RegionSaveData)formatter.Deserialize(regionStream);
            regionDatas.Add(regionPos, regionData);
        } else {
            regionDatas.Add(regionPos, new RegionSaveData(new List<VoxelChunk>()));
        }
    }

    public void CloseRegion(Vector3 regionPos) {
        FileStream regionStream = regionStreams[regionPos];

        regionStream.Dispose();
        regionStreams.Remove(regionPos);
        RegionSaveData regionData = regionDatas[regionPos];
        regionData.chunkDataDictionary.Clear();
        regionDatas.Remove(regionPos);
    }

    private void CloseRegions() {
        foreach (FileStream stream in regionStreams.Values) {
            stream.Dispose();
        }
        regionDatas.Clear();
        regionStreams.Clear();
    }

    private void OnApplicationQuit() {
        CloseRegions();
    }
}
