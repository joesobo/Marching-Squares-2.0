using UnityEngine;

public class VoxelCore : MonoBehaviour {
    public CoreScriptableObject CoreData;

    // Generators for building
    private InfiniteGenerator infiniteGenerator;

    private void Awake() {
        Random.InitState(CoreData.seed);

        infiniteGenerator = FindObjectOfType<InfiniteGenerator>();
    }

    private void Start() {
        GenerateTerrain();
    }

    private void GenerateTerrain() {
        // Update
        infiniteGenerator.StartGeneration();
    }

    public CoreScriptableObject GetCoreScriptableObject() {
        return CoreData;
    }
}
