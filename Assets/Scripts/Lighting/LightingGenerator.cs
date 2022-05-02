using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightingGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;
    private LayerScriptableObject topLayer = null;
    private Camera cam;

    private int voxelResolution;

    private GameObject lighting;
    private MeshRenderer lightingRenderer;

    private StaticLighting staticLighting;
    private DynamicLighting dynamicLighting;

    private Texture2D scaledTexture;
    private Texture2D texture;

    public int textureScaling = 1;
    private Color32[] saveColors;

    private int minX, maxX, minY, maxY;
    private int textureWidth, textureHeight;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();
        staticLighting = FindObjectOfType<StaticLighting>();
        dynamicLighting = FindObjectOfType<DynamicLighting>();
        cam = Camera.main;

        voxelResolution = CORE.voxelResolution;

        dynamicLighting.Setup(textureScaling);

        lighting = transform.GetChild(0).gameObject;
        lightingRenderer = lighting.GetComponent<MeshRenderer>();

        for (int i = 0; i < world.layers.Count; i++) {
            if (topLayer == null || world.layers[i].zIndex < topLayer.zIndex) {
                topLayer = world.layers[i];
            }
        }
    }

    private void Update() {
        if (scaledTexture) {
            texture = dynamicLighting.GenerateDynamicLighting(scaledTexture);

            if (texture) {
                lightingRenderer.material.SetTexture("ShadowTexture", texture);
            }
        }
    }

    public void GenerateChunkLighting() {
        if (!CORE.useLighting) return;

        // generate bounding box for all chunks
        List<VoxelChunk> chunks = topLayer.existingChunks.Values.ToList();
        FindChunkBounds(chunks);

        int chunkWidth = Mathf.Abs(maxX - minX);
        int chunkHeight = Mathf.Abs(maxY - minY);

        textureWidth = chunkWidth * voxelResolution;
        textureHeight = chunkHeight * voxelResolution;

        staticLighting.SetupChunkInfo(minX, maxX, minY, maxY, textureWidth, textureHeight);
        dynamicLighting.SetupChunkInfo(minX, maxX, minY, maxY, textureWidth, textureHeight);

        // attach the texture to the chunk as a child
        Vector3 lightingOffset = new Vector3((minX * voxelResolution) + (textureWidth / 2), (minY * voxelResolution) + (textureHeight / 2), -5);
        lighting.transform.parent = transform;
        lighting.transform.position = lightingOffset;
        lighting.transform.localScale = new Vector3(textureWidth / 2, textureHeight / 2, 1);

        scaledTexture = TextureScaler.scaled(staticLighting.GenerateStaticLighting(chunks), textureWidth * textureScaling, textureHeight * textureScaling);
        scaledTexture.Apply();
        lightingRenderer.material.SetTexture("ShadowTexture", scaledTexture);
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
