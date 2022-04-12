using System.Collections.Generic;
using UnityEngine;

public class LightingGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;

    private int voxelResolution;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();

        this.voxelResolution = CORE.voxelResolution;
    }

    public void GenerateChunkLighting(LayerScriptableObject layer, VoxelChunk chunk) {
        // reset chunk voxel lighting
        foreach (Transform child in chunk.transform) {
            GameObject.Destroy(child.gameObject);
        }

        if (!CORE.useLighting) return;

        // create new 8x8 texture
        Texture2D texture = new Texture2D(8, 8, TextureFormat.ARGB32, false) {
            // filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        // loop over chunk voxels grabbing lighting values out
        List<Color> colors = new List<Color>();
        foreach (Voxel voxel in chunk.voxels) {
            int lightingValue = voxel.lighting;

            if (lightingValue == 0) {
                colors.Add(Color.clear);
            } else if (lightingValue == 1) {
                colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.0f));
            } else if (lightingValue == 2) {
                colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.4f));
            } else if (lightingValue == 3) {
                colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.5f));
            } else if (lightingValue == 4) {
                colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.6f));
            } else {
                colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.8f));
            }
        }
        texture.SetPixels(colors.ToArray());
        texture.Apply();

        // attach the texture to the chunk as a child
        GameObject lighting = new GameObject("Lighting");
        lighting.transform.parent = chunk.transform;
        lighting.transform.localPosition = Vector2.one * 4f;

        SpriteRenderer lightingRenderer = lighting.AddComponent<SpriteRenderer>();
        lightingRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 1f);
    }
}
