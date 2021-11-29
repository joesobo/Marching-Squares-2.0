using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelCore : MonoBehaviour {
        public CoreScriptableObject coreScriptableObject;

        // Generators for building
        private VoxelChunkGenerator chunkGenerator;
        private VoxelMeshGenerator meshGenerator;

        private void Awake() {
            chunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
            meshGenerator = FindObjectOfType<VoxelMeshGenerator>();
        }

        void Start() {
            FreshGeneration();
        }

        private void FreshGeneration() {
            GenerateTerrain();
        }

        private void GenerateTerrain() {
            chunkGenerator.SetupChunks();
            chunkGenerator.CreateChunks();

            meshGenerator.Setup();
            meshGenerator.GenerateWholeMesh();
        }

        public CoreScriptableObject GetCoreScriptableObject() {
            return coreScriptableObject;
        }
    }
}