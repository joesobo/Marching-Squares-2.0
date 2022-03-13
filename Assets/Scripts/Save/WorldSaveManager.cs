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
    [HideInInspector] public List<LayerSaveData> currentLayerDatas;

    private void Awake() {
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        formatter = GetBinaryFormatter();

        currentLayerDatas = new List<LayerSaveData>();

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

        stream.Close();

        // create layers.save file
        foreach (LayerScriptableObject layer in layers) {
            string layerDataPath = worldPath + "layer_" + layer.name + ".save";

            FileStream layerStream = new FileStream(layerDataPath, FileMode.Create);
            LayerSaveData layerData = new LayerSaveData(layer.name, new List<VoxelChunk>());
            currentLayerDatas.Add(layerData);
            formatter.Serialize(layerStream, layerData);

            layerStream.Close();
        }
    }

    private void OpenWorld() {
        Debug.Log("Opening world: " + worldName);

        // read world.save file
        FileStream stream = new FileStream(worldDataPath, FileMode.OpenOrCreate);
        currentWorldData = (WorldSaveData)formatter.Deserialize(stream);

        stream.Close();

        // read layers.save files
        foreach (LayerScriptableObject layer in layers) {
            string layerDataPath = worldPath + "layer_" + layer.name + ".save";

            FileStream layerStream = new FileStream(layerDataPath, FileMode.OpenOrCreate);
            LayerSaveData layerData = (LayerSaveData)formatter.Deserialize(layerStream);
            currentLayerDatas.Add(layerData);

            layerStream.Close();
        }

        Debug.Log("World loaded: " + worldName);
    }
}
