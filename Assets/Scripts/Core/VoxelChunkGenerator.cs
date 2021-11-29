using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelChunkGenerator : MonoBehaviour {
        private CoreScriptableObject coreScriptableObject;

        // Center point for chunk generation
        public Vector2 playerPosition = Vector2.zero;

        // The element to spawn at each reference position along the chunk
        public GameObject voxelReferencePointsPrefab;
        // The chunk to spawn
        public GameObject voxelChunkPrefab;

        private void Awake() {
            coreScriptableObject = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        }

        // Creates the chunks within a radius around the player
        public void SetupChunks() {
            int chunkResolution = coreScriptableObject.chunkResolution;
            int voxelResolution = coreScriptableObject.voxelResolution;
            coreScriptableObject.existingChunks.Clear();

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

                    coreScriptableObject.existingChunks.Add(GetWholePosition(chunk), chunk);
                }
            }

            SetupAllNeighbors();
        }

        public void CreateChunks() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in coreScriptableObject.existingChunks) {
                chunk.Value.FillChunk();
            }
        }

        public void SetupAllNeighbors() {
            foreach (KeyValuePair<Vector2Int, VoxelChunk> chunk in coreScriptableObject.existingChunks) {
                SetupChunkNeighbors(chunk.Value);
            }
        }

        private void SetupChunkNeighbors(VoxelChunk chunk) {
            int voxelResolution = coreScriptableObject.voxelResolution;
            Vector2Int setupCoord = GetWholePosition(chunk);

            Vector2Int pxCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
            Vector2Int pyCoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
            Vector2Int pxyCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);

            if (!coreScriptableObject.existingChunks.ContainsKey(setupCoord)) return;
            if (coreScriptableObject.existingChunks.ContainsKey(pxCoord)) {
                chunk.xNeighbor = coreScriptableObject.existingChunks[pxCoord];
            }

            if (coreScriptableObject.existingChunks.ContainsKey(pyCoord)) {
                chunk.yNeighbor = coreScriptableObject.existingChunks[pyCoord];
            }

            if (coreScriptableObject.existingChunks.ContainsKey(pxyCoord)) {
                chunk.xyNeighbor = coreScriptableObject.existingChunks[pxyCoord];
            }
        }

        private static Vector2Int GetWholePosition(VoxelChunk chunk) {
            Vector2 position = chunk.transform.position;
            return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        }
    }
}