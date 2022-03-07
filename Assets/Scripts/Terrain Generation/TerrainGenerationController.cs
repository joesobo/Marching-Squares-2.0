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

        if (currentType == TerrainGenerationTypes.Perlin && CanSpawnPerlin(scaledX, scaledY, layer)) {
            return PerlinNoise(scaledX, scaledY, layer);
        } else if (currentType == TerrainGenerationTypes.Fill) {
            return FillNoise(scaledX, scaledY, layer);
        } else if (currentType == TerrainGenerationTypes.Random) {
            return RandomNoise(layer.terrainNoiseScriptableObject);
        } else {
            return 0;
        }
    }
}
