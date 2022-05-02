using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LightingHelper;

public class DynamicLighting : MonoBehaviour {
    private CoreScriptableObject CORE;
    private int voxelResolution;
    private int minX, maxX, minY, maxY;

    private int textureScaling;
    private int currentTextureWidth, currentTextureHeight = 0;

    public int falloffRate = 100;
    public int radius = 2;
    private float radiusSqr;

    private Color32[] currentColors;

    public List<Transform> dynamicLights = new List<Transform>();
    private readonly List<Vector3> dynamicLightsPositions = new List<Vector3>();

    private Texture2D dynamicTexture;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        voxelResolution = CORE.voxelResolution;
        radiusSqr = radius * radius;

        for (int i = 0; i < dynamicLights.Count; i++) {
            dynamicLightsPositions.Add(Vector3.zero);
        }
    }

    public void Setup(int textureScaling) {
        this.textureScaling = textureScaling;
    }

    public void SetupChunkInfo(int minX, int maxX, int minY, int maxY, int textureWidth, int textureHeight) {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;

        // create new texture if sizing has changed
        if (currentTextureWidth != textureWidth || currentTextureHeight != textureHeight) {
            dynamicTexture = CreateTexture(textureWidth * textureScaling, textureHeight * textureScaling);
            currentTextureWidth = textureWidth;
            currentTextureHeight = textureHeight;
        }
    }

    public Texture2D GenerateDynamicLighting(Texture2D scaledTexture) {
        if (!DynamicLightHasUpdates()) {
            return null;
        }

        currentColors = scaledTexture.GetPixels32();

        for (int index = 0; index < dynamicLights.Count; index++) {
            Transform dynamicLight = dynamicLights[index];
            dynamicLightsPositions[index] = dynamicLight.position;

            // repositioned light pos so they are relative to texture coords
            float voxelMinY = minY * voxelResolution;
            float voxelMaxY = maxY * voxelResolution;
            float voxelMinX = minX * voxelResolution;
            float voxelMaxX = maxX * voxelResolution;

            // break out early if the light entity is outside the bounds of the world
            if (dynamicLight.position.y < voxelMinY || dynamicLight.position.y > voxelMaxY ||
                dynamicLight.position.x < voxelMinX || dynamicLight.position.x > voxelMaxX) {
                continue;
            }

            // get the position of the light relative to the texture
            Vector2 distance = (Vector2)dynamicLight.position - new Vector2(voxelMinX, voxelMinY);
            Vector2 size = new Vector2(voxelMaxX - voxelMinX, voxelMaxY - voxelMinY);
            Vector2 scale = new Vector2(currentTextureWidth, currentTextureHeight) * textureScaling / size;
            Vector2 texCoords = distance * scale;

            // modify points in radius around the light entity
            ModifyInRadius(texCoords);
        }

        dynamicTexture.SetPixels32(currentColors);
        dynamicTexture.Apply();

        return dynamicTexture;
    }

    private void ModifyInRadius(Vector2 editPoint) {
        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                Vector2 pos = new Vector2(i, j) + editPoint;
                float falloffDist = Vector2.Distance(pos, editPoint);
                float distSqr = falloffDist * falloffDist;

                if (distSqr < radiusSqr) {
                    int textureIndex = ((int)editPoint.x + i) + (((int)editPoint.y + j) * currentTextureWidth * textureScaling);

                    if (textureIndex >= 0 && textureIndex < currentColors.Length) {
                        float alpha = 1f - (1f / (distSqr / falloffRate));

                        if (alpha > 0.9f) {
                            alpha = 0.9f;
                        }

                        if (currentColors[textureIndex].a > alpha * 255) {
                            currentColors[textureIndex] = new Color(0, 0, 0, alpha);
                        }
                    }
                }
            }
        }
    }

    private bool DynamicLightHasUpdates() {
        for (int index = 0; index < dynamicLights.Count(); index++) {
            Transform dynamicLight = dynamicLights[index];
            if (dynamicLight.position != dynamicLightsPositions[index]) {
                return true;
            }
        }
        return false;
    }
}
