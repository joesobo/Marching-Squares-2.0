using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/TerrainEditingScriptableObject", order = 2)]
public class TerrainEditingScriptableObject : SerializedScriptableObject {
    [EnumToggleButtons]
    public enum Stencil {
        Square,
        Circle
    }

    [EnumToggleButtons]
    public enum Type {
        Remove,
        Dirt,
        Grass,
        Stone,
        White,
        Black,
        Blue,
        Brown,
        Cyan,
        Green,
        Light_Blue,
        Lime,
        Light_Gray,
        Magenta,
        Orange,
        Pink,
        Purple,
        Red,
        Yellow,
        Gray,
    }

    [Range(0, 5)]
    public int Radius = 1;

    public Stencil StencilType = Stencil.Square;

    public Type EditingType = Type.Remove;
}
