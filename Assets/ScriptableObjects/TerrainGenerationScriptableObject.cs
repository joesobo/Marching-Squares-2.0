using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static TerrainEditingScriptableObject;

[CreateAssetMenu(fileName = "CoreData", menuName = "ScriptableObjects/TerrainGenerationScriptableObject", order = 3)]
public class TerrainGenerationScriptableObject : SerializedScriptableObject {
    public Type EditingType = Type.Remove;
}
