using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/TerrainEditingScriptableObject", order = 4)]
public class TerrainEditingScriptableObject : SerializedScriptableObject {
    [Range(0, 5)]
    // Radius of voxels around mouse
    public int Radius = 1;

    [EnumToggleButtons]
    // Type of shape to edit
    public Stencil StencilType = Stencil.Square;

    [EnumToggleButtons]
    // Type of voxel to use in editing
    public BlockType EditingType = BlockType.Empty;

    [Range(0, 2)]
    // Layer to interact with
    public int LayerIndex = 0;
}
