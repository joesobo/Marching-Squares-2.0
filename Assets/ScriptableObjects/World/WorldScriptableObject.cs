using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldData", menuName = "ScriptableObjects/WorldScriptableObject", order = 3)]
public class WorldScriptableObject : SerializedScriptableObject {
    public string worldName;
    public int seed;

    public List<LayerScriptableObject> layers;
}
