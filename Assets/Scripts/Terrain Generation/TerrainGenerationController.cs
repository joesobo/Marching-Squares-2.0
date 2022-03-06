using UnityEngine;

public partial class TerrainGenerationController : MonoBehaviour {

    [Header("Height Noise")]
    [Range(0.1f, 1)]
    public float frequency = 1f;
    // [Range(1, 8)]
    // public int octaves = 1;
    // [Range(1f, 4f)]
    // public float lacunarity = 2f;
    // [Range(0f, 1f)]
    // public float persistence = 0.5f;
    [Range(0, 2)]
    public float amplitude = 1f;
    [Range(0.1f, 1)]
    public float range = 1f;

    private int voxelResolution;
    private int chunkResolution;
    private int seed;

    public int GetTerrainNoise(LayerScriptableObject layer, int x, int y, Vector3 chunkPosition) {
        voxelResolution = layer.CORE.voxelResolution;
        chunkResolution = layer.CORE.chunkResolution;
        seed = layer.terrainNoiseScriptableObject.seed;

        if (layer.terrainNoiseScriptableObject.TerrainType == TerrainGenerationType.Perlin) {
            int scaledX = Mathf.RoundToInt(x * (voxelResolution - 1) + chunkPosition.x * voxelResolution);
            int scaledY = Mathf.RoundToInt(y * (voxelResolution - 1) + chunkPosition.y * voxelResolution);

            return scaledY > PerlinNoise1D(scaledX) ? 0 : PerlinNoise(scaledX, scaledY);
        } else {
            return (int)layer.terrainNoiseScriptableObject.TerrainType;
        }
    }

    private int PerlinNoise(int x, int y) {
        float scaledXHeight = x / 1f / voxelResolution;
        float scaledYHeight = y / 1f / voxelResolution;

        return (int)(Mathf.PerlinNoise(scaledXHeight + seed, scaledYHeight + seed) * 3);
    }

    private float PerlinNoise1D(int x) {
        float scaledXHeight = x / 1f / voxelResolution;

        return ((Mathf.PerlinNoise((scaledXHeight + seed) * frequency, 0) * amplitude) / range) * voxelResolution * chunkResolution;
    }
}
