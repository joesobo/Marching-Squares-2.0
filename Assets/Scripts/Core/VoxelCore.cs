using System.Collections.Generic;
using UnityEngine;

public class VoxelCore : MonoBehaviour {
    public List<CoreScriptableObject> CoreDatas;

    // Generators for building
    private InfiniteGenerator infiniteGenerator;

    private void Awake() {
        Random.InitState(CoreDatas[0].seed);

        infiniteGenerator = this.GetComponent<InfiniteGenerator>();
    }

    private void Start() {
        GenerateTerrain();
    }

    private void GenerateTerrain() {
        // Update
        infiniteGenerator.StartGeneration();
    }

    public CoreScriptableObject GetCoreScriptableObject(int index) {
        return CoreDatas[index];
    }

    public List<CoreScriptableObject> GetAllCoreScriptableObjects() {
        return CoreDatas;
    }
}
