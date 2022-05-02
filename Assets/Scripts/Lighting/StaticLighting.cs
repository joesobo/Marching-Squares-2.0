using System.Collections.Generic;
using UnityEngine;
using static LightingHelper;

public class StaticLighting : MonoBehaviour {
    private CoreScriptableObject CORE;

    private int voxelResolution;
    private int minX, maxX, minY, maxY;
    private int currentTextureWidth, currentTextureHeight = 0;

    private readonly List<Color32> staticColors = new List<Color32>();

    private Texture2D staticTexture;

    private readonly Color32[] lightingColors = {
        Color.clear, Color.clear,
        new Color(0.0f, 0.0f, 0.0f, 0.4f),
        new Color(0.0f, 0.0f, 0.0f, 0.6f),
        new Color(0.0f, 0.0f, 0.0f, 0.75f),
        new Color(0.0f, 0.0f, 0.0f, 0.9f)
    };

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();

        voxelResolution = CORE.voxelResolution;
    }

    public Texture2D GenerateStaticLighting(IEnumerable<VoxelChunk> chunks) {
        // fill texture with nothing
        ResetTexture();

        // color from lighting values
        ColorFromLightValues(chunks);

        staticTexture.SetPixels32(staticColors.ToArray());
        staticTexture.Apply();

        return staticTexture;
    }

    public void SetupChunkInfo(int minX, int minY, int textureWidth, int textureHeight) {
        this.minX = minX;
        this.minY = minY;

        // create new texture if sizing has changed
        if (currentTextureWidth != textureWidth || currentTextureHeight != textureHeight) {
            staticTexture = CreateTexture(textureWidth, textureHeight);
            currentTextureWidth = textureWidth;
            currentTextureHeight = textureHeight;
        }
    }

    // Sets the texture to all clear
    private void ResetTexture() {
        staticColors.Clear();

        for (int x = 0; x < currentTextureWidth; x++) {
            for (int y = 0; y < currentTextureHeight; y++) {
                staticColors.Add(Color.clear);
            }
        }
    }

    // Fills color array from lighting values relative to texture coords
    private void ColorFromLightValues(IEnumerable<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            // chunk position in terms of chunks
            Vector3 chunkWorldPos = chunk.transform.position;
            Vector2Int chunkPos = new Vector2Int((int)chunkWorldPos.x / voxelResolution, (int)chunkWorldPos.y / voxelResolution);

            // repositioned chunks so they are relative to texture coords
            int textureChunkX = chunkPos.x - minX;
            int textureChunkY = chunkPos.y - minY;

            foreach (Voxel voxel in chunk.voxels) {
                int lightingValue = voxel.lighting;
                int voxelX = (int)voxel.position.x;
                int voxelY = (int)voxel.position.y;

                // get texture index from chunk and voxel position relative to texture coords
                int voxelIndexOffset = voxelY * currentTextureWidth + voxelX;
                int chunkPosIndex = textureChunkY * currentTextureWidth * voxelResolution + textureChunkX * voxelResolution;
                int index = chunkPosIndex + voxelIndexOffset;

                staticColors[index] = lightingColors[lightingValue];
            }
        }
    }
}
