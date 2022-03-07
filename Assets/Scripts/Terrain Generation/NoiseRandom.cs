using UnityEngine;

public static class NoiseRandom {
    public static int RandomNoise(TerrainNoiseScriptableObject terrainNoise) {
        float randValue = Random.Range(0f, 1f);

        if (terrainNoise.AvailableBlocks.Count > 0) {
            float increment = 1f / terrainNoise.AvailableBlocks.Count;
            int blockIndex = 0;

            if (randValue > increment) {
                blockIndex = (int)(randValue / increment);
            }

            if (blockIndex >= terrainNoise.AvailableBlocks.Count) {
                blockIndex = terrainNoise.AvailableBlocks.Count - 1;
            }

            return (int)terrainNoise.AvailableBlocks[blockIndex];
        } else {
            return (int)(randValue * System.Enum.GetValues(typeof(BlockType)).Length) - 1;
        }
    }
}
