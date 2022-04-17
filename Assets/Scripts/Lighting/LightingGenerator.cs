using System.Collections.Generic;
using UnityEngine;

public class LightingGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;

    private int voxelResolution;

    private GameObject lighting = null;
    private SpriteRenderer lightingRenderer;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        this.voxelResolution = CORE.voxelResolution;

        lighting = new GameObject("Lighting");
        lightingRenderer = lighting.AddComponent<SpriteRenderer>();
    }

    public void GenerateChunkLighting(IEnumerable<VoxelChunk> chunks) {
        if (!CORE.useLighting) return;

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

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

        // generate bounding box for all chunks
        int chunkWidth = Mathf.Abs(maxX - minX);
        int chunkHeight = Mathf.Abs(maxY - minY);

        int width = chunkWidth * voxelResolution;
        int height = chunkHeight * voxelResolution;

        // create new texture
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false) {
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        List<Color> colors = new List<Color>();

        // fill texture with nothing
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                colors.Add(Color.clear);
            }
        }

        // fill texture with lighting values
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
                int index = ((int)textureChunkY * width * voxelResolution) + ((int)textureChunkX * voxelResolution) + (voxelY * width) + voxelX;

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
        }

        //SetPixels32 is faster than SetPixel
        texture.SetPixels(colors.ToArray());
        texture.Apply();

        // attach the texture to the chunk as a child
        Vector2 lightingOffset = new Vector2((minX * voxelResolution) + (width / 2), (minY * voxelResolution) + (height / 2));
        lighting.transform.parent = transform;
        lighting.transform.position = lightingOffset;

        lightingRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f);
    }
}
