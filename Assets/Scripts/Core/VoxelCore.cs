using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
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

        // Dictionary of current chunks
        private Dictionary<Vector2Int, VoxelChunk> existingChunks;

        // Generators for building
        private VoxelChunkGenerator chunkGenerator;
        private VoxelMeshGenerator meshGenerator;

        private void Awake() {
            chunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
            meshGenerator = FindObjectOfType<VoxelMeshGenerator>();

            FreshGeneration();
        }

        private void FreshGeneration() {
            GenerateTerrain();
        }

        private void GenerateTerrain() {
            existingChunks = chunkGenerator.SetupChunks(chunkResolution, voxelResolution, showVoxelReferencePoints);
            VoxelChunkGenerator.CreateChunks(existingChunks);

            meshGenerator.Setup(voxelResolution, chunkResolution);
            meshGenerator.GenerateWholeMesh(existingChunks);
        }
    }
}