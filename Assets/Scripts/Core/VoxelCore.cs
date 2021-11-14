using System;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCore : MonoBehaviour {
    // Number of voxels per a chunk 
    public int voxelResolution = 8;
    // Radius of chunks around the player
    public int chunkResolution = 2;
    // Whether or not to display the points where voxels are generated
    public bool showVoxelReferencePoints = true;
    // The element to spawn at each reference position along the chunk
    public GameObject voxelReferencePointsPrefab;

    // Center point for chunk generation
    private Vector2 playerPosition = Vector2.zero;

    // List of chunks
    private List<VoxelChunk> chunks;

    void Awake() {
        FreshGeneration();
    }

    private void FreshGeneration() {
        chunks = new List<VoxelChunk>();

        GenerateTerrain();
    }

    private void GenerateTerrain() {
        chunks = VoxelChunkGenerator.SetupChunks(chunkResolution, voxelResolution, showVoxelReferencePoints, voxelReferencePointsPrefab);

        VoxelChunkGenerator.CreateChunks(chunks);
    }
}
