using System.Collections.Generic;
using UnityEngine;

public class VoxelCore : MonoBehaviour {
    public List<LayerScriptableObject> layers;

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

    public CoreScriptableObject GetCoreScriptableObject(int index) {
        return layers[index].CORE;
    }

    public List<CoreScriptableObject> GetAllCoreScriptableObjects() {
        List<CoreScriptableObject> cores = new List<CoreScriptableObject>();
        foreach (LayerScriptableObject layer in layers) {
            cores.Add(layer.CORE);
        }
        return cores;
    }

    public LayerScriptableObject GetLayerScriptableObject(int index) {
        return layers[index];
    }

    public List<LayerScriptableObject> GetAllLayerScriptableObjects() {
        return layers;
    }
}
