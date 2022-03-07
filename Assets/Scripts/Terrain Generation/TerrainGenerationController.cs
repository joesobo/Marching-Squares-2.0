using UnityEngine;
using static NoisePerlin;
using static NoiseRandom;
using static NoiseFill;

public class TerrainGenerationController : MonoBehaviour {
    private int voxelResolution;

    public int GetTerrainNoise(LayerScriptableObject layer, int x, int y, Vector3 chunkPosition) {
        voxelResolution = layer.CORE.voxelResolution;

        int scaledX = Mathf.RoundToInt(x * (voxelResolution - 1) + chunkPosition.x * voxelResolution);
        int scaledY = Mathf.RoundToInt(y * (voxelResolution - 1) + chunkPosition.y * voxelResolution);

        TerrainGenerationTypes currentType = layer.terrainNoiseScriptableObject.TerrainType;

        return currentType switch {
            TerrainGenerationTypes.Perlin when CanSpawnPerlin(scaledX, scaledY, layer) => PerlinNoise(scaledX, scaledY,
                layer),
            TerrainGenerationTypes.Fill => FillNoise(scaledX, scaledY, layer),
            TerrainGenerationTypes.Random => RandomNoise(layer.terrainNoiseScriptableObject),
            _ => 0
        };
    }
}
