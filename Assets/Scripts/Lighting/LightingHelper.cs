using UnityEngine;

public static class LightingHelper {
    // Creates a new texture
    public static Texture2D CreateTexture(int width, int height) {
        return new Texture2D(width, height, TextureFormat.ARGB32, false) {
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp
        };
    }
}
