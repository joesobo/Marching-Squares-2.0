using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerationController : MonoBehaviour {
    public TerrainGenerationScriptableObject terrainGenScriptableObject;

    public int GetTerrainNoise() {
        return (int)terrainGenScriptableObject.EditingType;
    }
}
