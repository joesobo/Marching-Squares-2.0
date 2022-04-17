using System.Collections.Generic;
using UnityEngine;

public class LightingGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;

    private int voxelResolution;

    private GameObject lighting = null;
    private SpriteRenderer lightingRenderer;
    private List<Color> colors;
    private Texture2D texture;

    private int minX, maxX, minY, maxY;
    private int textureWidth, textureHeight;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        this.voxelResolution = CORE.voxelResolution;

        lighting = new GameObject("Lighting");
        lightingRenderer = lighting.AddComponent<SpriteRenderer>();

        colors = new List<Color>();
    }

    public void GenerateChunkLighting(IEnumerable<VoxelChunk> chunks) {
        if (!CORE.useLighting) return;

        // generate bounding box for all chunks
        FindChunkBounds(chunks);

        int chunkWidth = Mathf.Abs(maxX - minX);
        int chunkHeight = Mathf.Abs(maxY - minY);

        // create new texture
        textureWidth = chunkWidth * voxelResolution;
        textureHeight = chunkHeight * voxelResolution;
        CreateTexture();

        // fill texture with nothing
        ResetTexture();

        // color from lighting values
        ColorFromLightValues(chunks);

        // set texture to lighting colors
        SetTexture();
    }

    // Sets the texture to color array and attaches it to the sprite renderer
    private void SetTexture() {
        //SetPixels32 is faster than SetPixel
        texture.SetPixels(colors.ToArray());
        texture.Apply();

        // attach the texture to the chunk as a child
        Vector2 lightingOffset = new Vector2((minX * voxelResolution) + (textureWidth / 2), (minY * voxelResolution) + (textureHeight / 2));
        lighting.transform.parent = transform;
        lighting.transform.position = lightingOffset;

        lightingRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), 1f);
    }

    // Filles color array from lighting values relative to texture coords
    private void ColorFromLightValues(IEnumerable<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            // chunk position in terms of chunks
            Vector2 chunkPos = chunk.transform.position / voxelResolution;

            // repositioned chunks so they are relative to texture coords
            int textureChunkX = (int)chunkPos.x - minX;
            int textureChunkY = (int)chunkPos.y - minY;

            foreach (Voxel voxel in chunk.voxels) {
                int lightingValue = voxel.lighting;
                int voxelX = (int)voxel.position.x;
                int voxelY = (int)voxel.position.y;

                // get texture index from chunk and voxel position relative to texture coords
                int voxelIndexOffset = (voxelY * textureWidth) + voxelX;
                int chunkPosIndex = (int)textureChunkY * textureWidth * voxelResolution + (int)textureChunkX * voxelResolution;

                int index = chunkPosIndex + voxelIndexOffset;
                ColorLightingAtIndex(lightingValue, index);
            }
        }
    }

    // Sets the correct lighting color at a given index
    private void ColorLightingAtIndex(int lightingValue, int index) {
        if (lightingValue == 0) {
            colors[index] = Color.clear;
        } else if (lightingValue == 1) {
            colors[index] = Color.clear;
        } else if (lightingValue == 2) {
            colors[index] = new Color(0.0f, 0.0f, 0.0f, 0.4f);
        } else if (lightingValue == 3) {
            colors[index] = new Color(0.0f, 0.0f, 0.0f, 0.6f);
        } else if (lightingValue == 4) {
            colors[index] = new Color(0.0f, 0.0f, 0.0f, 0.75f);
        } else {
            colors[index] = new Color(0.0f, 0.0f, 0.0f, 0.9f);
        }
    }

    // Creates a new texture
    private void CreateTexture() {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false) {
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp
        };
    }

    // Sets the texture to all clear
    private void ResetTexture() {
        colors.Clear();

        for (int x = 0; x < textureWidth; x++) {
            for (int y = 0; y < textureHeight; y++) {
                colors.Add(Color.clear);
            }
        }
    }

    // Finds min and max of all chunks in the world
    private void FindChunkBounds(IEnumerable<VoxelChunk> chunks) {
        minX = int.MaxValue;
        maxX = int.MinValue;
        minY = int.MaxValue;
        maxY = int.MinValue;

        // get chunks bounds
        foreach (VoxelChunk chunk in chunks) {
            Vector2 pos = chunk.transform.position / voxelResolution;
            if (pos.x < minX) {
                minX = (int)pos.x;
            }
            if (pos.x > maxX) {
                maxX = (int)pos.x;
            }
            if (pos.y < minY) {
                minY = (int)pos.y;
            }
            if (pos.y > maxY) {
                maxY = (int)pos.y;
            }
        }
        maxX += 1;
        maxY += 1;
    }
}
