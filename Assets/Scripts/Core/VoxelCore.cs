using System.Collections.Generic;
using UnityEngine;

public class VoxelCore : MonoBehaviour {
    public List<LayerScriptableObject> layers;
    public CoreScriptableObject CORE;
    public WorldScriptableObject worldScriptableObject;

    // Generators for building
    private InfiniteGenerator infiniteGenerator;

    private void Awake() {
        infiniteGenerator = GetComponent<InfiniteGenerator>();
    }

    private void Start() {
        GenerateTerrain();
    }

    private void GenerateTerrain() {
        infiniteGenerator.StartGeneration();
    }

    public CoreScriptableObject GetCoreScriptableObject() {
        return CORE;
    }

    public WorldScriptableObject GetWorldScriptableObject() {
        return worldScriptableObject;
    }

    public LayerScriptableObject GetLayerScriptableObject(int index) {
        return layers[index];
    }

    public List<LayerScriptableObject> GetAllLayerScriptableObjects() {
        return layers;
    }
}
