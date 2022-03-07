using UnityEngine;

public static class NoisePerlin {
    public static bool CanSpawnPerlin(int x, int y, LayerScriptableObject layer) {
        return y < PerlinNoise1D(x, layer);
    }

    public static int PerlinNoise(int x, int y, LayerScriptableObject layer) {
        int voxelResolution = layer.CORE.voxelResolution;
        TerrainNoiseScriptableObject terrainNoise = layer.terrainNoiseScriptableObject;
        float scaledX = x / 1f / voxelResolution;
        float scaledY = y / 1f / voxelResolution;

        float perlinValue = Mathf.PerlinNoise(scaledX + terrainNoise.seed, scaledY + terrainNoise.seed);

        if (terrainNoise.AvailableBlocks.Count > 0) {
            float perlinIncrement = 1f / terrainNoise.AvailableBlocks.Count;
            int blockIndex = 0;

            if (perlinValue > perlinIncrement) {
                blockIndex = (int)(perlinValue / perlinIncrement);
            }

            if (blockIndex >= terrainNoise.AvailableBlocks.Count) {
                blockIndex = terrainNoise.AvailableBlocks.Count - 1;
            }

            return (int)terrainNoise.AvailableBlocks[blockIndex];
        } else {
            return (int)(perlinValue * System.Enum.GetValues(typeof(BlockType)).Length) - 1;
        }
    }

    public static float PerlinNoise1D(int x, LayerScriptableObject layer) {
        int voxelResolution = layer.CORE.voxelResolution;
        int chunkResolution = layer.CORE.chunkResolution;
        TerrainNoiseScriptableObject terrainNoise = layer.terrainNoiseScriptableObject;

        float scaledX = x / 1f / voxelResolution;

        return Mathf.PerlinNoise((scaledX + terrainNoise.seed) * terrainNoise.frequency, 0) * terrainNoise.amplitude / terrainNoise.range * voxelResolution * chunkResolution;
    }
}
