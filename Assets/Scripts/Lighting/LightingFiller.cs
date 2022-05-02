using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightingFiller : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;
    private LayerScriptableObject topLayer;
    private int topIndex = -1;

    private const int MAXLIGHTINGVALUE = 5;
    private readonly Queue<Tuple<Voxel, VoxelChunk>> voxelLightingQueue = new Queue<Tuple<Voxel, VoxelChunk>>();

    private int voxelResolution;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        for (int i = 0; i < world.layers.Count; i++) {
            if (topLayer == null || world.layers[i].zIndex < topLayer.zIndex) {
                topLayer = world.layers[i];
                topIndex = i;
            }
        }

        voxelResolution = CORE.voxelResolution;
    }

    // Fills all chunks voxels with lighting values
    public void FillChunksLighting() {
        if (!CORE.useLighting) return;

        List<VoxelChunk> chunks = topLayer.existingChunks.Values.ToList();

        ResetChunkLightValues(chunks);

        LightingFloodFill();

        RemainingFill(chunks);
    }

    // Resets all chunks voxels light values
    private void ResetChunkLightValues(IEnumerable<VoxelChunk> chunks) {
        voxelLightingQueue.Clear();

        // reset all chunks voxel lighting
        foreach (VoxelChunk chunk in chunks) {
            Vector3 position = chunk.transform.position;
            Vector2Int chunkPos = new Vector2Int((int)position.x, (int)position.y);

            foreach (Voxel voxel in chunk.voxels) {
                voxel.lighting = -1;

                int currentIndex = topIndex;
                Voxel currentVoxel = voxel;

                while (currentVoxel.state == 0 && currentIndex > 0 && !currentVoxel.isLuminous) {
                    // search the layer below
                    currentIndex--;
                    LayerScriptableObject layer = world.layers[currentIndex];

                    if (layer.existingChunks.ContainsKey(chunkPos)) {
                        VoxelChunk backgroundChunk = layer.existingChunks[chunkPos];
                        Voxel backgroundVoxel = backgroundChunk.voxels[(int)voxel.position.x + ((int)voxel.position.y * voxelResolution)];
                        currentVoxel = backgroundVoxel;
                    }
                }

                if (currentVoxel.isLuminous || (currentVoxel.state == 0 && currentIndex == 0)) {
                    voxel.lighting = 0;
                    voxelLightingQueue.Enqueue(new Tuple<Voxel, VoxelChunk>(voxel, chunk));
                }
            }
        }
    }

    // Flood fills the voxels in the queue, all the way up to MAXLIGHTINGVALUE
    private void LightingFloodFill() {
        // loop over queue while queue is not empty
        while (voxelLightingQueue.Count > 0) {
            // get first voxel in queue
            (Voxel voxel, VoxelChunk chunk) = voxelLightingQueue.Dequeue();

            if (voxel.lighting < MAXLIGHTINGVALUE) {
                // get all voxels around voxel
                IEnumerable<Tuple<Voxel, VoxelChunk>> checkPointsAroundVoxel = GetVoxelsAroundVoxel(voxel, chunk);

                // loop over voxels around voxel
                foreach ((Voxel checkVoxel, VoxelChunk checkChunk) in checkPointsAroundVoxel) {
                    // if voxel lighting has not been set
                    if (checkVoxel.lighting == -1) {
                        checkVoxel.lighting = voxel.lighting + 1;

                        // add voxel to queue
                        voxelLightingQueue.Enqueue(new Tuple<Voxel, VoxelChunk>(checkVoxel, checkChunk));
                    }
                }
            }
        }
    }

    // Fills any remaining voxels with MAXLIGHTINGVALUE
    private static void RemainingFill(IEnumerable<VoxelChunk> chunks) {
        // if any remaining voxels have lighting = -1 set to maxLightingValue
        foreach (VoxelChunk chunk in chunks) {
            foreach (Voxel voxel in chunk.voxels) {
                if (voxel.lighting == -1) {
                    voxel.lighting = MAXLIGHTINGVALUE;
                }
            }

            chunk.ResetReferencePoints();
        }
    }

    // Gets all voxels around a voxel, from any chunks
    private IEnumerable<Tuple<Voxel, VoxelChunk>> GetVoxelsAroundVoxel(Voxel currentVoxel, VoxelChunk currentChunk) {
        List<Tuple<Voxel, VoxelChunk>> checkPointsAroundVoxel = new List<Tuple<Voxel, VoxelChunk>>();

        Vector2Int currentVoxelPos = new Vector2Int((int)currentVoxel.position.x, (int)currentVoxel.position.y);
        Vector2Int currentChunkPos = new Vector2Int((int)currentChunk.transform.position.x, (int)currentChunk.transform.position.y);

        if (currentVoxelPos.x == 0) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x - voxelResolution, currentChunkPos.y);
            int voxelIndex = currentVoxelPos.y * voxelResolution + voxelResolution - 1;
            LookForChunkWithVoxel(voxelIndex, checkChunkPosition, checkPointsAroundVoxel);
        } else {
            Voxel checkVoxel = currentChunk.voxels[currentVoxelPos.y * voxelResolution + currentVoxelPos.x - 1];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        if (currentVoxelPos.y == 0) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x, currentChunkPos.y - voxelResolution);
            int voxelIndex = (voxelResolution - 1) * voxelResolution + currentVoxelPos.x;
            LookForChunkWithVoxel(voxelIndex, checkChunkPosition, checkPointsAroundVoxel);
        } else {
            Voxel checkVoxel = currentChunk.voxels[(currentVoxelPos.y - 1) * voxelResolution + currentVoxelPos.x];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        if (currentVoxelPos.x == voxelResolution - 1) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x + voxelResolution, currentChunkPos.y);
            int voxelIndex = currentVoxelPos.y * voxelResolution + 0;
            LookForChunkWithVoxel(voxelIndex, checkChunkPosition, checkPointsAroundVoxel);
        } else {
            Voxel checkVoxel = currentChunk.voxels[currentVoxelPos.y * voxelResolution + currentVoxelPos.x + 1];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        if (currentVoxelPos.y == voxelResolution - 1) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x, currentChunkPos.y + voxelResolution);
            int voxelIndex = 0 * voxelResolution + currentVoxelPos.x;
            LookForChunkWithVoxel(voxelIndex, checkChunkPosition, checkPointsAroundVoxel);
        } else {
            Voxel checkVoxel = currentChunk.voxels[(currentVoxelPos.y + 1) * voxelResolution + currentVoxelPos.x];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        return checkPointsAroundVoxel;
    }

    private void LookForChunkWithVoxel(int voxelIndex, Vector2Int checkChunkPosition, ICollection<Tuple<Voxel, VoxelChunk>> checkPointsAroundVoxel) {
        if (topLayer.existingChunks.ContainsKey(checkChunkPosition)) {
            VoxelChunk checkChunk = topLayer.existingChunks[checkChunkPosition];
            Voxel checkVoxel = checkChunk.voxels[voxelIndex];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, checkChunk));
        }
    }
}
