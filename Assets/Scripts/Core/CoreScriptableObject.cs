using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core {
    [CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/CoreScriptableObject", order = 1)]
    public class CoreScriptableObject : SerializedScriptableObject {
        // Number of voxels per a chunk 
        public int voxelResolution = 8;
        // Radius of chunks around the player
        public int chunkResolution = 2;
        // Whether or not to display the points where voxels are generated
        public bool showVoxelReferencePoints = true;

        // Dictionary of current chunks
        public Dictionary<Vector2Int, VoxelChunk> existingChunks = new Dictionary<Vector2Int, VoxelChunk>();
    }
}