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

        // List of chunks
        private List<VoxelChunk> chunks;

        // Generators for building
        private VoxelChunkGenerator chunkGenerator;
        private VoxelMeshGenerator meshGenerator;

        void Awake() {
            chunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
            meshGenerator = FindObjectOfType<VoxelMeshGenerator>();

            FreshGeneration();
        }

        private void FreshGeneration() {
            chunks = new List<VoxelChunk>();

            GenerateTerrain();
        }

        private void GenerateTerrain() {
            chunks = chunkGenerator.SetupChunks(chunkResolution, voxelResolution, showVoxelReferencePoints);
            chunkGenerator.CreateChunks(chunks);

            meshGenerator.Setup(voxelResolution, chunkResolution);
            meshGenerator.GenerateWholeMesh(chunks);
        }
    }
}