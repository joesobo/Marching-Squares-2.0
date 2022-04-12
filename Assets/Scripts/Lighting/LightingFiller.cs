using System;
using System.Collections.Generic;
using UnityEngine;

public class LightingFiller : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;
    private LayerScriptableObject layer;

    private int maxLightingValue = 5;
    private Queue<Tuple<Voxel, VoxelChunk>> VoxelLightingQueue = new Queue<Tuple<Voxel, VoxelChunk>>();

    private int voxelResolution;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();
        layer = world.layers[0];

        this.voxelResolution = CORE.voxelResolution;
    }

    public void FillChunksLighting(IEnumerable<VoxelChunk> chunks) {
        if (!CORE.useLighting) return;

        Reset(chunks);
        SetupQueue(chunks);

        LightingFloodFill(chunks);
        RemainingFill(chunks);
    }

    private void Reset(IEnumerable<VoxelChunk> chunks) {
        VoxelLightingQueue.Clear();

        // reset all chunks voxel lighting
        foreach (VoxelChunk chunk in chunks) {
            foreach (Voxel voxel in chunk.voxels) {
                voxel.lighting = -1;

                if (voxel.state == 0) {
                    voxel.lighting = 0;
                }
            }
        }
    }

    private void SetupQueue(IEnumerable<VoxelChunk> chunks) {
        // fill up queue will all voxels of state = 0
        foreach (VoxelChunk chunk in chunks) {
            foreach (Voxel voxel in chunk.voxels) {
                if (voxel.lighting == 0) {
                    VoxelLightingQueue.Enqueue(new Tuple<Voxel, VoxelChunk>(voxel, chunk));
                }
            }
        }
    }

    private void LightingFloodFill(IEnumerable<VoxelChunk> chunks) {
        // loop over queue while queue is not empty
        while (VoxelLightingQueue.Count > 0) {
            // get first voxel in queue
            var tuple = VoxelLightingQueue.Dequeue();
            Voxel voxel = tuple.Item1;
            VoxelChunk chunk = tuple.Item2;

            if (voxel.lighting < maxLightingValue) {
                // get all voxels around voxel
                List<Tuple<Voxel, VoxelChunk>> checkPointsAroundVoxel = GetVoxelsAroundVoxel(voxel, chunk, chunks);

                // loop over voxels around voxel
                foreach (Tuple<Voxel, VoxelChunk> check in checkPointsAroundVoxel) {
                    Voxel checkVoxel = check.Item1;
                    VoxelChunk checkChunk = check.Item2;

                    // if voxel lighting has not been set
                    if (checkVoxel.lighting == -1) {
                        // set voxel to state = 0
                        checkVoxel.lighting = voxel.lighting + 1;

                        // add voxel to queue
                        VoxelLightingQueue.Enqueue(new Tuple<Voxel, VoxelChunk>(checkVoxel, checkChunk));
                    }
                }
            }
        }
    }

    private void RemainingFill(IEnumerable<VoxelChunk> chunks) {
        // if any remaining voxels have lighting = -1 set to maxLightingValue
        foreach (VoxelChunk chunk in chunks) {
            foreach (Voxel voxel in chunk.voxels) {
                if (voxel.lighting == -1) {
                    voxel.lighting = maxLightingValue;
                }
            }

            chunk.ResetReferencePoints();
        }
    }

    private List<Tuple<Voxel, VoxelChunk>> GetVoxelsAroundVoxel(Voxel currentVoxel, VoxelChunk currentChunk, IEnumerable<VoxelChunk> chunks) {
        List<Tuple<Voxel, VoxelChunk>> checkPointsAroundVoxel = new List<Tuple<Voxel, VoxelChunk>>();
        Vector2 currentVoxelPos = currentVoxel.position;
        Vector2Int currentChunkPos = new Vector2Int((int)currentChunk.transform.position.x, (int)currentChunk.transform.position.y);

        if (currentVoxelPos.x == 0) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x - voxelResolution, currentChunkPos.y);

            if (layer.existingChunks.ContainsKey(checkChunkPosition)) {
                VoxelChunk checkChunk = layer.existingChunks[checkChunkPosition];
                Voxel checkVoxel = checkChunk.voxels[(int)currentVoxelPos.y * voxelResolution + voxelResolution - 1];

                checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, checkChunk));
            }
        } else {
            Voxel checkVoxel = currentChunk.voxels[(int)(currentVoxelPos.y * voxelResolution + currentVoxelPos.x - 1)];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        if (currentVoxelPos.y == 0) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x, currentChunkPos.y - voxelResolution);

            if (layer.existingChunks.ContainsKey(checkChunkPosition)) {
                VoxelChunk checkChunk = layer.existingChunks[checkChunkPosition];
                Voxel checkVoxel = checkChunk.voxels[(int)((voxelResolution - 1) * voxelResolution + currentVoxelPos.x)];

                checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, checkChunk));
            }
        } else {
            Voxel checkVoxel = currentChunk.voxels[(int)((currentVoxelPos.y - 1) * voxelResolution + currentVoxelPos.x)];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        if (currentVoxelPos.x == voxelResolution - 1) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x + voxelResolution, currentChunkPos.y);

            if (layer.existingChunks.ContainsKey(checkChunkPosition)) {
                VoxelChunk checkChunk = layer.existingChunks[checkChunkPosition];
                Voxel checkVoxel = checkChunk.voxels[(int)(currentVoxelPos.y * voxelResolution + 0)];

                checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, checkChunk));
            }
        } else {
            Voxel checkVoxel = currentChunk.voxels[(int)(currentVoxelPos.y * voxelResolution + currentVoxelPos.x + 1)];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        if (currentVoxelPos.y == voxelResolution - 1) {
            Vector2Int checkChunkPosition = new Vector2Int(currentChunkPos.x, currentChunkPos.y + voxelResolution);

            if (layer.existingChunks.ContainsKey(checkChunkPosition)) {
                VoxelChunk checkChunk = layer.existingChunks[checkChunkPosition];
                Voxel checkVoxel = checkChunk.voxels[(int)(0 * voxelResolution + currentVoxelPos.x)];

                checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, checkChunk));
            }
        } else {
            Voxel checkVoxel = currentChunk.voxels[(int)((currentVoxelPos.y + 1) * voxelResolution + currentVoxelPos.x)];

            checkPointsAroundVoxel.Add(new Tuple<Voxel, VoxelChunk>(checkVoxel, currentChunk));
        }

        return checkPointsAroundVoxel;
    }
}
