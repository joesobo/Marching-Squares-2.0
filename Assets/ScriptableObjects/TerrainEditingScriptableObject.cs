using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/TerrainEditingScriptableObject", order = 2)]
public class TerrainEditingScriptableObject : SerializedScriptableObject {
    public enum Stencil {
        Circle,
        Square
    }

    public enum Type {
        Remove,
        Fill,
    }

    [Range(0, 5)]
    public int Radius = 1;

    public Stencil StencilType = Stencil.Square;

    public Type EditingType = Type.Remove;
}
