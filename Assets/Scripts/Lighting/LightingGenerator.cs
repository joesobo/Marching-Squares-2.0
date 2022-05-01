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
    private List<Color32> colors;
    private Texture2D texture;
    private Texture2D dynamicTexture;

    private Texture2D scaledTexture;
    public List<Transform> dynamicLights = new List<Transform>();
    private List<Vector3> dynamicLightsPositions = new List<Vector3>();

    public int textureScaling = 1;
    public int radius = 2;
    Color32[] saveColors;

    private int minX, maxX, minY, maxY;
    private int textureWidth, textureHeight;
    private int oldTextureWidth, oldTextureHeight = 0;

    private readonly Color32[] lightingColors = {
        Color.clear, Color.clear,
        new Color(0.0f, 0.0f, 0.0f, 0.4f),
        new Color(0.0f, 0.0f, 0.0f, 0.6f),
        new Color(0.0f, 0.0f, 0.0f, 0.75f),
        new Color(0.0f, 0.0f, 0.0f, 0.9f)
    };

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();
        cam = Camera.main;

        voxelResolution = CORE.voxelResolution;

        lighting = transform.GetChild(0).gameObject;
        lightingRenderer = lighting.GetComponent<MeshRenderer>();

        colors = new List<Color32>();

        for (int i = 0; i < world.layers.Count; i++) {
            if (topLayer == null || world.layers[i].zIndex < topLayer.zIndex) {
                topLayer = world.layers[i];
            }
        }

        foreach (Transform dynamicLight in dynamicLights) {
            dynamicLightsPositions.Add(Vector3.zero);
        }
    }

    // TODO: refactor out dynamic and static lighting
    private void Update() {
        if (DynamicLightHasUpdates()) {
            Graphics.CopyTexture(scaledTexture, dynamicTexture);
            Color32[] currentColors = dynamicTexture.GetPixels32();

            for (int index = 0; index < dynamicLights.Count(); index++) {
                Transform dynamicLight = dynamicLights[index];
                dynamicLightsPositions[index] = dynamicLight.position;

                // repositioned light pos so they are relative to texture coords
                float voxelMinY = minY * voxelResolution;
                float voxelMaxY = maxY * voxelResolution;
                float voxelMinX = minX * voxelResolution;
                float voxelMaxX = maxX * voxelResolution;

                Vector2 distance = (Vector2)dynamicLight.position - new Vector2(voxelMinX, voxelMinY);
                Vector2 size = new Vector2(voxelMaxX - voxelMinX, voxelMaxY - voxelMinY);
                Vector2 scale = new Vector2(oldTextureWidth, oldTextureHeight) * textureScaling / size;
                Vector2 texCoords = distance * scale;

                float radiusSqr = radius * radius;

                for (int i = -radius; i <= radius; i++) {
                    for (int j = -radius; j <= radius; j++) {
                        Vector2 pos = new Vector2(i, j) + texCoords;
                        float fallofDist = Vector2.Distance(pos, texCoords);
                        float distSqr = fallofDist * fallofDist;

                        if (distSqr < radiusSqr) {

                            int textureIndex = ((int)texCoords.x + i) + (((int)texCoords.y + j) * textureWidth * textureScaling);

                            if (textureIndex >= 0 && textureIndex < currentColors.Length) {
                                currentColors[textureIndex] = Color.clear;
                            }
                        }
                    }
                }
            }

            dynamicTexture.SetPixels32(currentColors);
            dynamicTexture.Apply();

            lightingRenderer.material.SetTexture("ShadowTexture", dynamicTexture);
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

    public void GenerateChunkLighting() {
        if (!CORE.useLighting) return;

        List<VoxelChunk> chunks = topLayer.existingChunks.Values.ToList();

        // generate bounding box for all chunks
        FindChunkBounds(chunks);

        int chunkWidth = Mathf.Abs(maxX - minX);
        int chunkHeight = Mathf.Abs(maxY - minY);

        textureWidth = chunkWidth * voxelResolution;
        textureHeight = chunkHeight * voxelResolution;

        // create new texture if sizing has changed
        if (textureWidth != oldTextureWidth || textureHeight != oldTextureHeight) {
            texture = CreateTexture(textureWidth, textureHeight);
            dynamicTexture = CreateTexture(textureWidth * textureScaling, textureHeight * textureScaling);
        }

        // fill texture with nothing
        ResetTexture();

        // color from lighting values
        ColorFromLightValues(chunks);

        // set texture to lighting colors
        SetTexture();

        oldTextureWidth = textureWidth;
        oldTextureHeight = textureHeight;
    }

    // Sets the texture to color array and attaches it to the sprite renderer
    private void SetTexture() {
        texture.SetPixels32(colors.ToArray());
        texture.Apply();

        // attach the texture to the chunk as a child
        Vector3 lightingOffset = new Vector3((minX * voxelResolution) + (textureWidth / 2), (minY * voxelResolution) + (textureHeight / 2), -5);
        lighting.transform.parent = transform;
        lighting.transform.position = lightingOffset;
        lighting.transform.localScale = new Vector3(textureWidth / 2, textureHeight / 2, 1);

        scaledTexture = ScaleTexture(texture, textureWidth * textureScaling, textureHeight * textureScaling);

        lightingRenderer.material.SetTexture("ShadowTexture", scaledTexture);

        saveColors = texture.GetPixels32();
    }

    // TODO: see if we can get rid of this completely
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight) {
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false) {
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        List<Color32> resultColors = new List<Color32>();
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int i = 0; i < result.height; ++i) {
            for (int j = 0; j < result.width; ++j) {
                resultColors.Add(source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height));
            }
        }
        result.SetPixels32(resultColors.ToArray());
        result.Apply();
        return result;
    }

    // Fills color array from lighting values relative to texture coords
    private void ColorFromLightValues(IEnumerable<VoxelChunk> chunks) {
        foreach (VoxelChunk chunk in chunks) {
            // chunk position in terms of chunks
            Vector2Int chunkPos = new Vector2Int((int)chunk.transform.position.x / voxelResolution, (int)chunk.transform.position.y / voxelResolution);

            // repositioned chunks so they are relative to texture coords
            int textureChunkX = (int)chunkPos.x - minX;
            int textureChunkY = (int)chunkPos.y - minY;

            foreach (Voxel voxel in chunk.voxels) {
                int lightingValue = voxel.lighting;
                int voxelX = (int)voxel.position.x;
                int voxelY = (int)voxel.position.y;

                // get texture index from chunk and voxel position relative to texture coords
                int voxelIndexOffset = voxelY * textureWidth + voxelX;
                int chunkPosIndex = textureChunkY * textureWidth * voxelResolution + textureChunkX * voxelResolution;

                int index = chunkPosIndex + voxelIndexOffset;
                ColorLightingAtIndex(lightingValue, index);
            }
        }
    }

    // Sets the correct lighting color at a given index
    private void ColorLightingAtIndex(int lightingValue, int index) {
        colors[index] = lightingColors[lightingValue];
    }

    // Creates a new texture
    private Texture2D CreateTexture(int width, int height) {
        return new Texture2D(width, height, TextureFormat.RGBA32, false) {
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
