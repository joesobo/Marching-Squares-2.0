using static NoisePerlin;

public static class NoiseFill {
    public static int FillNoise(int x, int y, LayerScriptableObject layer) {
        if (layer.terrainNoiseScriptableObject.AvailableBlocks.Count > 0) {
            return PerlinNoise(x, y, layer);
        } else {
            return 0;
        }
    }
}
