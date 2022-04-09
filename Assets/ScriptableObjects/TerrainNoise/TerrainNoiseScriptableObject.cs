using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainNoise", menuName = "ScriptableObjects/TerrainNoiseScriptableObject", order = 5)]
public class TerrainNoiseScriptableObject : SerializedScriptableObject {
    [Range(0.1f, 1)]
    // Frequency of terrain noise
    public float frequency = 1f;
    [Range(0, 2)]
    // Size of terrain noise
    public float amplitude = 1f;
    [Range(0.1f, 1)]
    // Range of terrin noise
    public float range = 1f;

    // Type of terrain noise to use
    public TerrainGenerationTypes TerrainType = TerrainGenerationTypes.Fill;
    // Available blocks to use in terrain generation
    public List<BlockType> AvailableBlocks = new List<BlockType>(new BlockType[] { BlockType.Empty });
}
