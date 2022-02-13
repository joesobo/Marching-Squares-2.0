using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/TerrainEditingScriptableObject", order = 2)]
public class TerrainEditingScriptableObject : SerializedScriptableObject {
    public enum Stencil {
        Square,
        Circle
    }

    public enum Type {
        Remove,
        Dirt,
        Grass,
        Stone,
        White
    }

    [Range(0, 5)]
    public int Radius = 1;

    public Stencil StencilType = Stencil.Square;

    public Type EditingType = Type.Remove;
}
