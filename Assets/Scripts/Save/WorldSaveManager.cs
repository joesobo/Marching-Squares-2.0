using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static SerializationManager;

public class WorldSaveManager : MonoBehaviour {
    [HideInInspector] public string worldPath;
    private string worldDataPath;
    private string worldName;
    private int seed;

    private RegionSaveManager regionSaveManager;

    private List<LayerScriptableObject> layers;
    private WorldScriptableObject world;

    private BinaryFormatter formatter;

    private WorldSaveData currentWorldData;

    private void Awake() {
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        regionSaveManager = FindObjectOfType<RegionSaveManager>();

        formatter = GetBinaryFormatter();

        worldName = world.worldName;
        worldPath = Application.persistentDataPath + "/saves/" + worldName + "/";
        worldDataPath = worldPath + "world.save";
        seed = world.seed;

        regionSaveManager.SetRegionPath(worldPath + "layers/");

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

        foreach (string layerPath in layers.Select(layer => layersDir + "layer_regions_" + layer.name + "/")) {
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

    private void CloseWorld() {
        // close all open regions
        foreach (FileStream stream in regionSaveManager.regionStreams.Values) {
            stream.Dispose();
        }
        regionSaveManager.regionDatas.Clear();
        regionSaveManager.regionStreams.Clear();
    }

    private void OnApplicationQuit() {
        CloseWorld();
    }
}
