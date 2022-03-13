using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LayerData", menuName = "ScriptableObjects/LayerScriptableObject", order = 2)]
public class LayerScriptableObject : SerializedScriptableObject {
    // Whether or not to generate new chunks after runtime
    public bool doInfiniteGeneration = true;
    // Whether or not to generate chunks with colliders
    public bool doColliderGeneration = true;
    // Whether or not to generate chunks with outlines
    public bool doOutlines = true;
    // For setting the z offset of layers
    public int zIndex = 0;
    // The material the chunk should use
    public Material material;

    // Dictionary of current chunks
    public readonly Dictionary<Vector2Int, VoxelChunk> existingChunks = new Dictionary<Vector2Int, VoxelChunk>();
    // Queue of chunks to be recycled
    public readonly Queue<VoxelChunk> recycleableChunks = new Queue<VoxelChunk>();
    // Terrain noise data for this layer
    public TerrainNoiseScriptableObject terrainNoiseScriptableObject;
    // Name of chunks
    public string chunkName = "Voxel Chunk";

    private void OnEnable() {
        existingChunks.Clear();
        recycleableChunks.Clear();
    }
}
