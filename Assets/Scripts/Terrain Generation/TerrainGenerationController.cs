using UnityEngine;

public class TerrainGenerationController : MonoBehaviour {
    public static int GetTerrainNoise(CoreScriptableObject CORE) {
        return (int)CORE.TerrainType;
    }
}
