using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

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

    // Dictionary of current chunks
    public readonly Dictionary<Vector2Int, VoxelChunk> existingChunks = new Dictionary<Vector2Int, VoxelChunk>();
    // Dictionary of current background chunks
    public readonly Dictionary<Vector2Int, BackgroundChunk> existingBackgroundChunks = new Dictionary<Vector2Int, BackgroundChunk>();
    // Queue of chunks to be recycled
    public readonly Queue<VoxelChunk> recycleableChunks = new Queue<VoxelChunk>();

    private void OnEnable() {
        existingChunks.Clear();
        existingBackgroundChunks.Clear();
        recycleableChunks.Clear();
    }
}
