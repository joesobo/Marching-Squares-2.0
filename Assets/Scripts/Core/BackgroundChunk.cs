using UnityEngine;
using Sirenix.OdinInspector;
using System;

[SelectionBase]
public class BackgroundChunk : VoxelChunk {
    private CoreScriptableObject CORE;

    private VoxelChunkGenerator voxelChunkGenerator;
    private VoxelMeshGenerator voxelMeshGenerator;

    // The amount of voxels in each direction of the chunk
    private int voxelResolution;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        voxelChunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
        voxelMeshGenerator = FindObjectOfType<VoxelMeshGenerator>();
        meshFilter = FindObjectOfType<MeshFilter>();
        meshRenderer = FindObjectOfType<MeshRenderer>();
    }

    public void SetupChunk(Vector2 chunkPosition) {
        voxelResolution = CORE.voxelResolution;
        voxels = new Voxel[voxelResolution * voxelResolution];

        name = "Background Chunk (" + chunkPosition.x / CORE.voxelResolution + ", " + chunkPosition.y / CORE.voxelResolution + ")";
        transform.position = new Vector3(chunkPosition.x, chunkPosition.y, 1);

        FillChunk();

        gameObject.SetActive(true);
    }

    private void FillChunk() {
        for (int i = 0, y = 0; y < voxelResolution; y++) {
            for (int x = 0; x < voxelResolution; x++, i++) {
                voxels[i] = new Voxel(x, y, 1f);
            }
        }
    }

    public void GenerateBackgroundChunk() {
        SetupChunkNeighbors(this);
        RefreshMesh();
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Mesh", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    private void RefreshMesh() {
        voxelMeshGenerator.GenerateChunkMesh(this, material);
    }

    public void SetupChunkNeighbors(BackgroundChunk chunk) {
        int voxelResolution = CORE.voxelResolution;
        Vector2Int setupCoord = chunk.GetWholePosition();

        if (!CORE.existingBackgroundChunks.ContainsKey(setupCoord)) return;

        // Setup the chunk's neighbors
        Vector2Int xCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y);
        Vector2Int yCoord = new Vector2Int(setupCoord.x, setupCoord.y + voxelResolution);
        Vector2Int xyCoord = new Vector2Int(setupCoord.x + voxelResolution, setupCoord.y + voxelResolution);

        // Setup the neighbors neighbor value with this chunk
        Vector2Int bxCoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y);
        Vector2Int byCoord = new Vector2Int(setupCoord.x, setupCoord.y - voxelResolution);
        Vector2Int bxyCoord = new Vector2Int(setupCoord.x - voxelResolution, setupCoord.y - voxelResolution);

        VoxelChunk tempChunk;

        if (CORE.existingBackgroundChunks.ContainsKey(xCoord)) {
            chunk.xNeighbor = CORE.existingBackgroundChunks[xCoord];
        }

        if (CORE.existingBackgroundChunks.ContainsKey(yCoord)) {
            chunk.yNeighbor = CORE.existingBackgroundChunks[yCoord];
        }

        if (CORE.existingBackgroundChunks.ContainsKey(xyCoord)) {
            chunk.xyNeighbor = CORE.existingBackgroundChunks[xyCoord];
        }

        if (CORE.existingBackgroundChunks.ContainsKey(bxCoord)) {
            tempChunk = CORE.existingBackgroundChunks[bxCoord];
            tempChunk.xNeighbor = chunk;
        }

        if (CORE.existingBackgroundChunks.ContainsKey(byCoord)) {
            tempChunk = CORE.existingBackgroundChunks[byCoord];
            tempChunk.yNeighbor = chunk;
        }

        if (CORE.existingBackgroundChunks.ContainsKey(bxyCoord)) {
            tempChunk = CORE.existingBackgroundChunks[bxyCoord];
            tempChunk.xyNeighbor = chunk;
        }
    }
}
