using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class VoxelChunkGenerator : MonoBehaviour {
        private CoreScriptableObject CORE;

        // Center point for chunk generation
        private Vector2 playerPosition;

        // The element to spawn at each reference position along the chunk
        public GameObject voxelReferencePointsPrefab;
        // The chunk to spawn
        public GameObject voxelChunkPrefab;

        private int chunkResolution;
        private int voxelResolution;

        private void Awake() {
            CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
            chunkResolution = CORE.chunkResolution;
            voxelResolution = CORE.voxelResolution;
            playerPosition = GameObject.FindGameObjectsWithTag("Player")[0].transform.position;
        }

        public VoxelChunk CreateChunk(Vector2 chunkPosition) {
            VoxelChunk chunk = Instantiate(voxelChunkPrefab, chunkPosition, Quaternion.identity).GetComponent<VoxelChunk>();
            chunk.name = "Chunk (" + chunkPosition.x + ", " + chunkPosition.y + ")";
            chunk.SetupChunk(voxelReferencePointsPrefab);

            CORE.existingChunks.Add(GetWholePosition(chunk), chunk);
            return chunk;
        }

        public VoxelChunk CreatePoolChunk(VoxelChunk chunk, Vector2Int chunkPosition) {
            chunk.name = "Chunk (" + chunkPosition.x + ", " + chunkPosition.y + ")";
            chunk.gameObject.SetActive(true);
            chunk.transform.position = new Vector3(chunkPosition.x, chunkPosition.y, 0);
            chunk.ResetReferencePoints();

            if (!CORE.existingChunks.ContainsKey(chunkPosition)) {
                CORE.existingChunks.Add(GetWholePosition(chunk), chunk);
            }
            return chunk;
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
