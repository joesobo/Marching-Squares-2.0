using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainNoise", menuName = "ScriptableObjects/TerrainNoiseScriptableObject", order = 5)]
public class TerrainNoiseScriptableObject : SerializedScriptableObject {
    public int seed = 1;

    [Range(0.1f, 1)]
    public float frequency = 1f;
    [Range(0, 2)]
    public float amplitude = 1f;
    [Range(0.1f, 1)]
    public float range = 1f;

    public TerrainGenerationTypes TerrainType = TerrainGenerationTypes.Fill;

    public List<BlockType> AvailableBlocks = new List<BlockType>(new BlockType[] { BlockType.Empty });
}
