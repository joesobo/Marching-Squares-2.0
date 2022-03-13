using System.Collections.Generic;
using UnityEngine;

public class VoxelCore : MonoBehaviour {
    public CoreScriptableObject CORE;
    public WorldScriptableObject worldScriptableObject;

    // Generators for building
    private InfiniteGenerator infiniteGenerator;

    private void Awake() {
        infiniteGenerator = GetComponent<InfiniteGenerator>();

        foreach (LayerScriptableObject layer in worldScriptableObject.layers) {
            GameObject layerObject = new GameObject(layer.layerName);
            layerObject.transform.parent = transform;
            layer.parentReference = layerObject.transform;
        }
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
        return worldScriptableObject.layers[index];
    }

    public List<LayerScriptableObject> GetAllLayerScriptableObjects() {
        return worldScriptableObject.layers;
    }
}
