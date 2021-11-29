using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class InfiniteGenerator : MonoBehaviour {
        private CoreScriptableObject CORE;

        private Vector2 playerPosition;

        private VoxelChunkGenerator voxelChunkGenerator;
        private VoxelMeshGenerator voxelMeshGenerator;

        private void Awake() {
            CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        }

        private void UpdateAroundPlayer() {
            RemoveOutOfBoundsChunks();

            CreateInBoundsChunks();

            TriangulateNewChunks();
        }

        public void RemoveOutOfBoundsChunks() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in CORE.existingChunks) {
                if (chunk.Value != null) {
                    if (IsOutOfBounds(chunk.Value.transform.position)) {
                        RemoveChunk(chunk.Value);
                    }
                }
            }
        }

        private void CreateInBoundsChunks() {
            int chunkResolution = CORE.chunkResolution;
            int voxelResolution = CORE.voxelResolution;
            Vector2 p = playerPosition / voxelResolution;
            Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

            for (int x = -chunkResolution; x <= chunkResolution; x++) {
                for (int y = -chunkResolution; y <= chunkResolution; y++) {
                    Vector2Int chunkCoord = new Vector2Int((int)(playerChunkCoord.x + x), (int)(playerChunkCoord.y + y));

                    if (!CORE.existingChunks.ContainsKey(chunkCoord)) {
                        VoxelChunk currentChunk = GetObjectPoolChunk();

                        CORE.existingChunks.Add(chunkCoord, currentChunk);
                    }
                }
            }
        }

        // TODO: test this after getting references
        private void TriangulateNewChunks() {
            voxelChunkGenerator.SetupAllNeighbors();

            voxelMeshGenerator.GenerateWholeMesh();
        }

        private bool IsOutOfBounds(Vector2 chunkPosition) {
            Vector2 p = playerPosition / CORE.voxelResolution;
            Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

            return Vector2.Distance(chunkPosition, playerChunkCoord) > CORE.chunkResolution;
        }

        private void RemoveChunk(VoxelChunk chunk) {
            Vector2Int chunkCoord = new Vector2Int(Mathf.RoundToInt(chunk.transform.position.x), Mathf.RoundToInt(chunk.transform.position.y));
            CORE.existingChunks.Remove(chunkCoord);
            CORE.recycleableChunks.Enqueue(chunk);
        }

        // TODO: setup create / fill in of values for chunks
        // TODO: create CreateChunk function
        private VoxelChunk GetObjectPoolChunk() {
            VoxelChunk currentChunk;
            // if (CORE.recycleableChunks.Count > 0) {
                currentChunk = CORE.recycleableChunks.Dequeue();
            // } else {
            //     currentChunk = voxelChunkGenerator.CreateChunk();
            // }
            currentChunk.FillChunk();

            return currentChunk;
        }
    }
}