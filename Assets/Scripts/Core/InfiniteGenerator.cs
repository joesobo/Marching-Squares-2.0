using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class InfiniteGenerator : MonoBehaviour {
        private CoreScriptableObject CORE;

        private GameObject player;
        private Vector2 playerPosition;

        private VoxelChunkGenerator voxelChunkGenerator;
        private VoxelMeshGenerator voxelMeshGenerator;

        private bool startGeneration = false;

        private void Awake() {
            CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
            player = GameObject.FindGameObjectsWithTag("Player")[0];
            voxelChunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
            voxelMeshGenerator = FindObjectOfType<VoxelMeshGenerator>();
        }

        private void Update() {
            if (startGeneration) {
                UpdateAroundPlayer();
            }
        }

        public void StartGeneration() {
            startGeneration = true;
        }

        private void UpdateAroundPlayer() {
            playerPosition = player.transform.position;

            RemoveOutOfBoundsChunks();

            CreateInBoundsChunks();

            TriangulateNewChunks();
        }

        public void RemoveOutOfBoundsChunks() {
            // Find chunks to remove
            List<Vector2Int> removeChunkPositionList = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in CORE.existingChunks) {
                int index = 0;
                if (chunk.Value != null) {
                    if (IsOutOfBounds(chunk.Value.transform.position)) {
                        removeChunkPositionList.Add(chunk.Key);
                    }
                }
                index++;
            }

            // Remove chunks
            foreach (Vector2Int position in removeChunkPositionList) {
                RemoveChunk(CORE.existingChunks[position]);
            }
        }

        private void CreateInBoundsChunks() {
            int chunkResolution = CORE.chunkResolution;
            int voxelResolution = CORE.voxelResolution;
            Vector2 p = playerPosition / voxelResolution;
            Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

            for (int x = -chunkResolution; x < chunkResolution; x++) {
                for (int y = -chunkResolution; y < chunkResolution; y++) {
                    Vector2Int chunkCoord = new Vector2Int((int)(playerChunkCoord.x + x), (int)(playerChunkCoord.y + y));
                    Vector2Int chunkPosition = new Vector2Int(chunkCoord.x * voxelResolution, chunkCoord.y * voxelResolution);

                    if (!CORE.existingChunks.ContainsKey(chunkPosition)) {
                        VoxelChunk currentChunk = GetObjectPoolChunk(chunkPosition);
                        voxelChunkGenerator.CreatePoolChunk(currentChunk, chunkPosition);
                    }
                }
            }
        }

        private void TriangulateNewChunks() {
            voxelChunkGenerator.SetupAllNeighbors();

            voxelMeshGenerator.GenerateWholeMesh();
        }

        private bool IsOutOfBounds(Vector2 chunkPosition) {
            Vector2 p = playerPosition / CORE.voxelResolution;
            Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

            return Vector2.Distance(chunkPosition / CORE.voxelResolution, playerChunkCoord) > CORE.chunkResolution + 1;
        }

        private void RemoveChunk(VoxelChunk chunk) {
            Vector2Int chunkCoord = new Vector2Int(Mathf.RoundToInt(chunk.transform.position.x), Mathf.RoundToInt(chunk.transform.position.y));
            CORE.existingChunks.Remove(chunkCoord);
            CORE.recycleableChunks.Enqueue(chunk);
            chunk.gameObject.SetActive(false);
        }

        private VoxelChunk GetObjectPoolChunk(Vector2 chunkCoord) {
            VoxelChunk currentChunk;
            if (CORE.recycleableChunks.Count > 0) {
                currentChunk = CORE.recycleableChunks.Dequeue();
            } else {
                currentChunk = voxelChunkGenerator.CreateChunk(chunkCoord);
            }
            currentChunk.FillChunk();

            return currentChunk;
        }
    }
}
