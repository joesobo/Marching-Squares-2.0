using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/CoreScriptableObject", order = 1)]
public class CoreScriptableObject : SerializedScriptableObject {
    // Number of voxels per a chunk
    public int voxelResolution = 8;
    // Radius of chunks around the player
    public int chunkResolution = 2;
    // Whether or not to display the points where voxels are generated
    public bool showVoxelReferencePoints = true;
}
