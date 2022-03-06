using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/TerrainEditingScriptableObject", order = 3)]
public class TerrainEditingScriptableObject : SerializedScriptableObject {
    [Range(0, 5)]
    public int Radius = 1;

    [EnumToggleButtons]
    public Stencil StencilType = Stencil.Square;

    [EnumToggleButtons]
    public BlockType EditingType = BlockType.Remove;

    [Range(0, 2)]
    public int LayerIndex = 0;
}
