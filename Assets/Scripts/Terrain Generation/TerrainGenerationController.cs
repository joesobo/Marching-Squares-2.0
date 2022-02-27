using UnityEngine;

public class TerrainGenerationController : MonoBehaviour {
    public int GetTerrainNoise(CoreScriptableObject CORE) {
        return (int)CORE.EditingType;
    }
}
