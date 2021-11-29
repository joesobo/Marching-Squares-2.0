using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelChunkGenerator : MonoBehaviour {
        private CoreScriptableObject CORE;

        // Center point for chunk generation
        public Vector2 playerPosition = Vector2.zero;

        // The element to spawn at each reference position along the chunk
        public GameObject voxelReferencePointsPrefab;
        // The chunk to spawn
        public GameObject voxelChunkPrefab;

        private void Awake() {
            CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        }

        // Creates the chunks within a radius around the player
        public void SetupChunks() {
            int chunkResolution = CORE.chunkResolution;
            int voxelResolution = CORE.voxelResolution;

            for (int x = -chunkResolution; x < chunkResolution; x++) {
                for (int y = -chunkResolution; y < chunkResolution; y++) {
                    int xPoint = x * voxelResolution;
                    int yPoint = y * voxelResolution;

                    Vector2 p = playerPosition / voxelResolution;
                    Vector2 playerChunkCoord = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

                    Vector2 worldPosition = new Vector2(xPoint, yPoint) + playerChunkCoord * voxelResolution;
                    Vector2 chunkPosition = new Vector2(x + playerChunkCoord.x, y + playerChunkCoord.y);

                    VoxelChunk chunk = Instantiate(voxelChunkPrefab, worldPosition, Quaternion.identity).GetComponent<VoxelChunk>();
                    chunk.name = "Chunk (" + chunkPosition.x + ", " + chunkPosition.y + ")";
                    chunk.SetupChunk(voxelReferencePointsPrefab);

                    CORE.existingChunks.Add(GetWholePosition(chunk), chunk);
                }
            }

            SetupAllNeighbors();
        }

        public void CreateChunks() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in CORE.existingChunks) {
                chunk.Value.FillChunk();
            }
        }

        public void SetupAllNeighbors() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in CORE.existingChunks) {
                SetupChunkNeighbors(chunk.Value);
            }
        }

        private void SetupChunkNeighbors(VoxelChunk chunk) {
            int voxelResolution = CORE.voxelResolution;
            Vector2Int setupCoord = GetWholePosition(chunk);

            Vector2Int pxCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
            Vector2Int pyCoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
            Vector2Int pxyCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);

            if (!CORE.existingChunks.ContainsKey(setupCoord)) return;
            if (CORE.existingChunks.ContainsKey(pxCoord)) {
                chunk.xNeighbor = CORE.existingChunks[pxCoord];
            }

            if (CORE.existingChunks.ContainsKey(pyCoord)) {
                chunk.yNeighbor = CORE.existingChunks[pyCoord];
            }

            if (CORE.existingChunks.ContainsKey(pxyCoord)) {
                chunk.xyNeighbor = CORE.existingChunks[pxyCoord];
            }
        }

        private static Vector2Int GetWholePosition(VoxelChunk chunk) {
            Vector2 position = chunk.transform.position;
            return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        }
    }
}