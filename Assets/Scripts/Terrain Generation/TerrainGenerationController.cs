using UnityEngine;

public class TerrainGenerationController : MonoBehaviour {
    private int voxelResolution;
    private int chunkResolution;

    public int GetTerrainNoise(LayerScriptableObject layer, int x, int y, Vector3 chunkPosition) {
        voxelResolution = layer.CORE.voxelResolution;
        chunkResolution = layer.CORE.chunkResolution;

        if (layer.terrainNoiseScriptableObject.TerrainType == TerrainGenerationTypes.Perlin) {
            int scaledX = Mathf.RoundToInt(x * (voxelResolution - 1) + chunkPosition.x * voxelResolution);
            int scaledY = Mathf.RoundToInt(y * (voxelResolution - 1) + chunkPosition.y * voxelResolution);

            return scaledY > PerlinNoise1D(scaledX, layer.terrainNoiseScriptableObject) ? 0 : PerlinNoise(scaledX, scaledY, layer.terrainNoiseScriptableObject);
        }

        return (int)layer.terrainNoiseScriptableObject.TerrainType;
    }

    private int PerlinNoise(int x, int y, TerrainNoiseScriptableObject terrainNoise) {
        float scaledX = x / 1f / voxelResolution;
        float scaledY = y / 1f / voxelResolution;

        return (int)(Mathf.PerlinNoise(scaledX + terrainNoise.seed, scaledY + terrainNoise.seed) * 3);
    }

    private float PerlinNoise1D(int x, TerrainNoiseScriptableObject terrainNoise) {
        float scaledX = x / 1f / voxelResolution;

        return Mathf.PerlinNoise((scaledX + terrainNoise.seed) * terrainNoise.frequency, 0) * terrainNoise.amplitude / terrainNoise.range * voxelResolution * chunkResolution;
    }
}
