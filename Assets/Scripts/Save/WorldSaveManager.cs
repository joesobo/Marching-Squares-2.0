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

    private GameObject player;

    private List<LayerScriptableObject> layers;
    private WorldScriptableObject world;

    private BinaryFormatter formatter;

    private WorldSaveData currentWorldData;

    private void Awake() {
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        formatter = GetBinaryFormatter();

        player = GameObject.FindGameObjectsWithTag("Player")[0];

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
        currentWorldData = new WorldSaveData(worldName, seed, DateTime.Now.ToString(), Vector2.zero);
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

        player.transform.position = currentWorldData.player_pos;

        stream.Dispose();
    }

    private void SavePlayerPosition() {
        FileStream stream = new FileStream(worldDataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        currentWorldData = (WorldSaveData)formatter.Deserialize(stream);
        currentWorldData.player_pos = player.transform.position;

        stream.SetLength(0);
        formatter.Serialize(stream, currentWorldData);

        stream.Dispose();
    }

    private void OnApplicationQuit() {
        SavePlayerPosition();
    }
}
