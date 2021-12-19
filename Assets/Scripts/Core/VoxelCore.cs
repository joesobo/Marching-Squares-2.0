using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelCore : MonoBehaviour {
        public CoreScriptableObject CoreData;

        // Generators for building
        private MarchingShader marchingShader;
        private VoxelMesh voxelMesh;
        private InfiniteGenerator infiniteGenerator;

        private void Awake() {
            marchingShader = FindObjectOfType<MarchingShader>();
            voxelMesh = FindObjectOfType<VoxelMesh>();
            infiniteGenerator = FindObjectOfType<InfiniteGenerator>();
        }

        void Start() {
            FreshGeneration();
        }

        private void FreshGeneration() {
            GenerateTerrain();
        }

        private void GenerateTerrain() {
            // Setup
            marchingShader.Setup();
            voxelMesh.Setup();

            // Update
            infiniteGenerator.StartGeneration();
        }

        public CoreScriptableObject GetCoreScriptableObject() {
            return CoreData;
        }
    }
}
