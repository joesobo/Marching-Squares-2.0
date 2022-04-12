using System.Collections.Generic;
using UnityEngine;

public class LightingGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;

    private int voxelResolution;

    private GameObject lighting = null;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        this.voxelResolution = CORE.voxelResolution;
    }

    public void GenerateChunkLighting(IEnumerable<VoxelChunk> chunks) {
        if (!CORE.useLighting) return;

        if (lighting != null) {
            Destroy(lighting);
        }

        int minX = 0;
        int maxX = 0;
        int minY = 0;
        int maxY = 0;

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
        int chunkWidth = (Mathf.Abs(minX) + Mathf.Abs(maxX));
        int chunkHeight = (Mathf.Abs(minY) + Mathf.Abs(maxY));

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
                colors.Add(Color.black);
            }
        }

        // fill texture with lighting values
        foreach (VoxelChunk chunk in chunks) {
            Vector2 chunkPos = chunk.transform.position / voxelResolution;

            int chunkX = (int)Mathf.Abs(Mathf.Abs(chunkPos.x) + (chunkPos.x > 0 ? (chunkWidth / 2) : -(chunkWidth / 2)));
            int chunkY = (int)Mathf.Abs(Mathf.Abs(chunkPos.y) + (chunkPos.y > 0 ? (chunkHeight / 2) : -(chunkHeight / 2)));
            Vector2 textureChunkPos = new Vector2(chunkX, chunkY);

            foreach (Voxel voxel in chunk.voxels) {
                Vector2 texturePos = (textureChunkPos * voxelResolution) + voxel.position;

                int x = (int)voxel.position.x;
                int y = (int)voxel.position.y;

                int index = ((int)textureChunkPos.y * width * voxelResolution) + ((int)textureChunkPos.x * voxelResolution) + (y * width) + x;
                int lightingValue = voxel.lighting;

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

        texture.SetPixels(colors.ToArray());
        texture.Apply();

        // attach the texture to the chunk as a child
        lighting = new GameObject("Lighting");
        lighting.transform.parent = transform;

        SpriteRenderer lightingRenderer = lighting.AddComponent<SpriteRenderer>();
        lightingRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f);
    }
}
