using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldData", menuName = "ScriptableObjects/WorldScriptableObject", order = 3)]
public class WorldScriptableObject : SerializedScriptableObject {
    // Name of world
    public string worldName;
    // Random seed for world generation
    public int seed;

    // List of all layers in the world
    public List<LayerScriptableObject> layers;
}
