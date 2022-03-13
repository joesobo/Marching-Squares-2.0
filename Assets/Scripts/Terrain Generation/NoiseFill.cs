using static NoisePerlin;

public static class NoiseFill {
    public static int FillNoise(int x, int y, LayerScriptableObject layer, CoreScriptableObject CORE) {
        return layer.terrainNoiseScriptableObject.AvailableBlocks.Count > 0 ? PerlinNoise(x, y, layer, CORE) : 0;
    }
}
