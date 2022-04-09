using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static SerializationManager;

public class WorldSaveManager : MonoBehaviour {
    [HideInInspector] public string worldPath;
    private string worldDataPath;
    private string worldName;
    private int seed;

    private List<LayerScriptableObject> layers;
    private WorldScriptableObject world;

    private BinaryFormatter formatter;

    private WorldSaveData currentWorldData;

    [HideInInspector] public Dictionary<Vector3, RegionSaveData> regionDatas;
    [HideInInspector] public Dictionary<Vector3, FileStream> regionStreams;

    private void Awake() {
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        formatter = GetBinaryFormatter();

        regionDatas = new Dictionary<Vector3, RegionSaveData>();
        regionStreams = new Dictionary<Vector3, FileStream>();

        worldName = world.worldName;
        worldPath = Application.persistentDataPath + "/saves/" + worldName + "/";
        worldDataPath = worldPath + "world.save";
        seed = world.seed;

        LoadWorld();
    }

    private void LoadWorld() {
        if (Directory.Exists(worldPath)) {
            OpenWorld();
        } else {
            CreateWorld();
        }
    }

    private void CreateWorld() {
        Debug.Log("Creating world: " + worldName);

        Directory.CreateDirectory(worldPath);

        // create world.save file
        FileStream stream = new FileStream(worldDataPath, FileMode.Create);
        currentWorldData = new WorldSaveData(worldName, seed, DateTime.Now.ToString());
        formatter.Serialize(stream, currentWorldData);

        stream.Dispose();

        // create layers folder
        string layersDir = worldPath + "layers/";
        Directory.CreateDirectory(layersDir);

        foreach (LayerScriptableObject layer in layers) {
            string layerPath = layersDir + "layer_regions_" + layer.name + "/";
            Directory.CreateDirectory(layerPath);
        }
    }

    private void OpenWorld() {
        Debug.Log("Opening world: " + worldName);

        // read world.save file
        FileStream stream = new FileStream(worldDataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        currentWorldData = (WorldSaveData)formatter.Deserialize(stream);

        stream.Dispose();
    }

    public void CloseWorld() {
        foreach (FileStream stream in regionStreams.Values) {
            stream.Dispose();
        }
        regionDatas.Clear();
        regionStreams.Clear();
    }

    public void OpenRegion(Vector3 regionPos, LayerScriptableObject layer) {
        string regionDataPath = worldPath + "layers/layer_regions_" + layer.name + "/region_" + ((Vector2)regionPos).ToString() + ".save";


        if (!regionDatas.ContainsKey(regionPos)) {
            FileStream regionStream = new FileStream(regionDataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            regionStreams.Add(regionPos, regionStream);

            if (regionStream.Length > 0) {
                RegionSaveData regionData = (RegionSaveData)formatter.Deserialize(regionStream);
                regionDatas.Add(regionPos, regionData);
            } else {
                regionDatas.Add(regionPos, new RegionSaveData(new List<VoxelChunk>()));
            }
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
}
