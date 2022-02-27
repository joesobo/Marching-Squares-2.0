using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static TerrainEditingScriptableObject;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/CoreScriptableObject", order = 1)]
public class CoreScriptableObject : SerializedScriptableObject {
    public int seed = 1;
    // Number of voxels per a chunk
    public int voxelResolution = 8;
    // Radius of chunks around the player
    public int chunkResolution = 2;
    // Whether or not to display the points where voxels are generated
    public bool showVoxelReferencePoints = true;
    // Whether or not to generate new chunks after runtime
    public bool doInfiniteGeneration = true;
    // Whether or not to genrate chunks with colliders
    public bool doColliderGeneration = true;
    // For setting the z offset of layers
    public int zIndex = 0;
    // The material the chunk should use
    public Material material;

    // Dictionary of current chunks
    public readonly Dictionary<Vector2Int, VoxelChunk> existingChunks = new Dictionary<Vector2Int, VoxelChunk>();
    // Queue of chunks to be recycled
    public readonly Queue<VoxelChunk> recycleableChunks = new Queue<VoxelChunk>();

    public Type EditingType = Type.Remove;

    public string chunkName = "Voxel Chunk";

    private void OnEnable() {
        existingChunks.Clear();
        recycleableChunks.Clear();
    }
}
