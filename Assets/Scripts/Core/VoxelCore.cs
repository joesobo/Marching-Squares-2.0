using UnityEngine;

public class VoxelCore : MonoBehaviour {
    public CoreScriptableObject CoreData;

    // Generators for building
    private MarchingShader marchingShader;
    private InfiniteGenerator infiniteGenerator;

    private void Awake() {
        marchingShader = FindObjectOfType<MarchingShader>();
        infiniteGenerator = FindObjectOfType<InfiniteGenerator>();
    }

    private void Start() {
        FreshGeneration();
    }

    private void FreshGeneration() {
        GenerateTerrain();
    }

    private void GenerateTerrain() {
        // Setup
        marchingShader.Setup();

        // Update
        infiniteGenerator.StartGeneration();
    }

    public CoreScriptableObject GetCoreScriptableObject() {
        return CoreData;
    }
}
