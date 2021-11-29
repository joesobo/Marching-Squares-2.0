using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelCore : MonoBehaviour {
        public CoreScriptableObject CoreData;

        // Generators for building
        private VoxelChunkGenerator chunkGenerator;
        private VoxelMeshGenerator meshGenerator;
        private MarchingShader marchingShader;
        private VoxelMesh voxelMesh;

        private void Awake() {
            chunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
            meshGenerator = FindObjectOfType<VoxelMeshGenerator>();
            marchingShader = FindObjectOfType<MarchingShader>();
            voxelMesh = FindObjectOfType<VoxelMesh>();
        }

        void Start() {
            FreshGeneration();
        }

        private void FreshGeneration() {
            GenerateTerrain();
        }

        private void GenerateTerrain() {
            chunkGenerator.SetupChunks();
            marchingShader.Setup();
            voxelMesh.Setup();

            meshGenerator.GenerateWholeMesh();
        }

        public CoreScriptableObject GetCoreScriptableObject() {
            return CoreData;
        }
    }
}