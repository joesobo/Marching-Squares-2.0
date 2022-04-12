using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Shapes;
using TMPro;

[SelectionBase]
public class VoxelChunk : MonoBehaviour {
    [HideInInspector] public LayerScriptableObject currentLayer;
    private CoreScriptableObject CORE;

    private VoxelChunkGenerator voxelChunkGenerator;
    private VoxelMeshGenerator voxelMeshGenerator;
    private ColliderGenerator colliderGenerator;
    private OutlineDrawGenerator outlineDrawGenerator;
    private LightingGenerator lightingGenerator;
    private TerrainGenerationController terrainGenerationController;

    [HideInInspector] public MeshFilter meshFilter;
    [HideInInspector] public MeshRenderer meshRenderer;

    // Reference to neighbor chunks for edge voxel information
    public VoxelChunk xNeighbor, yNeighbor, xyNeighbor;
    // Storage of chunks voxels
    public Voxel[] voxels;
    // Storage of chunks mesh vertices
    public Vector3[] meshVertices = null;
    // Storage of chunks outline vertices
    public Vector3[] outlineVertices = null;
    // Storage of relationship between triangles and outline vertices
    public readonly Dictionary<Vector2, List<OutlineTriangle>> triangleDictionary = new Dictionary<Vector2, List<OutlineTriangle>>();
    // Storage of chunks vertice reference points
    private List<GameObject> voxelReferencePoints;
    // Storage of chunks vertice reference points
    private List<GameObject> lightingValueTexts;

    // The element to spawn at each reference position along the chunk
    private GameObject voxelRefPointsPrefab;
    // The element to spawn at each voxel position for displaying lighting values
    private GameObject lightingValuesTextPrefab;
    private Transform lightingTransform;
    // The amount of voxels in each direction of the chunk
    private int voxelResolution;

    // Half the resolution for finding center of chunks
    private float halfSize;

    [ReadOnly] public bool hasEditsToSave = false;

    private void Awake() {
        voxelChunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
        voxelMeshGenerator = FindObjectOfType<VoxelMeshGenerator>();
        colliderGenerator = FindObjectOfType<ColliderGenerator>();
        outlineDrawGenerator = FindObjectOfType<OutlineDrawGenerator>();
        lightingGenerator = FindObjectOfType<LightingGenerator>();
        meshFilter = FindObjectOfType<MeshFilter>();
        meshRenderer = FindObjectOfType<MeshRenderer>();
        terrainGenerationController = FindObjectOfType<TerrainGenerationController>();
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
    }

    public void SetupChunk(LayerScriptableObject layer, GameObject voxelReferencePointsPrefab, GameObject lightingValuesTextPrefab, Transform lightingTransform, Vector2 chunkPosition) {
        voxelRefPointsPrefab = voxelReferencePointsPrefab;
        this.lightingValuesTextPrefab = lightingValuesTextPrefab;
        this.lightingTransform = lightingTransform;
        voxelResolution = CORE.voxelResolution;
        voxels = new Voxel[voxelResolution * voxelResolution];
        voxelReferencePoints = new List<GameObject>();
        lightingValueTexts = new List<GameObject>();
        halfSize = 0.5f * voxelResolution;
        hasEditsToSave = false;

        name = "VoxelChunk (" + chunkPosition.x / voxelResolution + ", " + chunkPosition.y / voxelResolution + ")";
        transform.position = new Vector3(chunkPosition.x, chunkPosition.y, layer.zIndex);
        currentLayer = layer;
        FillChunk();
        gameObject.SetActive(true);
    }

    public void ResetChunk() {
        transform.parent = null;
        voxels = null;
        meshVertices = null;
        outlineVertices = null;
        triangleDictionary.Clear();
        xNeighbor = null;
        yNeighbor = null;
        xyNeighbor = null;
        hasEditsToSave = false;
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Mesh", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    private void RefreshMesh(LayerScriptableObject layer) {
        voxelMeshGenerator.GenerateChunkMesh(this, layer.material);
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Collider", ButtonSizes.Large), GUIColor(0.4f, 1, 0.8f)]
    private void RefreshCollider(LayerScriptableObject layer) {
        colliderGenerator.GenerateChunkColliders(layer, CORE, this);
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Collider", ButtonSizes.Large), GUIColor(0.8f, 1, 0.4f)]
    private void RefreshOutline(LayerScriptableObject layer) {
        outlineDrawGenerator.GenerateChunkOutlines(layer, this);
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Collider", ButtonSizes.Large), GUIColor(0.8f, 1, 0.4f)]
    private void RefreshLighting(LayerScriptableObject layer) {
        lightingGenerator.GenerateChunkLighting(layer, this);
    }

    [Button("Refresh Whole Chunk", ButtonSizes.Large), GUIColor(0.6f, 0.4f, 0.8f)]
    public void GenerateChunk(LayerScriptableObject layer) {
        VoxelChunkGenerator.SetupChunkNeighbors(layer, this);
        RefreshMesh(layer);
        RefreshCollider(layer);
        RefreshOutline(layer);
        RefreshLighting(layer);
    }

    private void FillChunk() {
        voxels = new Voxel[voxelResolution * voxelResolution];

        for (int i = 0, y = 0; y < voxelResolution; y++) {
            for (int x = 0; x < voxelResolution; x++, i++) {
                CreateVoxelPoint(i, x, y);
            }
        }
    }

    private void CreateVoxelPoint(int i, int x, int y) {
        int noiseVal = terrainGenerationController.GetTerrainNoise(currentLayer, x, y, transform.position);

        voxels[i] = new Voxel(x, y, noiseVal);
        CreateReferencePoint(i, x, y);
        CreateLightingValue(i, x, y);
    }

    public void ResetReferencePoints() {
        // Remove voxel reference points
        foreach (GameObject voxelRefPoint in voxelReferencePoints) {
            Destroy(voxelRefPoint);
        }

        // Remove lighting values
        foreach (GameObject lightValueText in lightingValueTexts) {
            Destroy(lightValueText);
        }

        // Regenerate voxel reference points
        for (int i = 0, y = 0; y < voxelResolution; y++) {
            for (int x = 0; x < voxelResolution; x++, i++) {
                CreateReferencePoint(i, x, y);
                CreateLightingValue(i, x, y);
            }
        }
    }

    private void CreateReferencePoint(int i, int x, int y) {
        if (CORE.showVoxelReferencePoints) {
            GameObject voxelRef = Instantiate(voxelRefPointsPrefab, transform, true);
            voxelRef.transform.position = new Vector3((x + 0.5f), (y + 0.5f)) + transform.position;
            voxelRef.transform.localScale = Vector2.one * 0.1f;
            voxelReferencePoints.Add(voxelRef);

            SpriteRenderer voxelRenderer = voxelRef.GetComponent<SpriteRenderer>();
            voxelRenderer.color = voxels[i].state == 1 ? Color.white : Color.black;
        }
    }

    private void CreateLightingValue(int i, int x, int y) {
        if (CORE.showLightingValues && CORE.useLighting) {
            GameObject lightingRef = Instantiate(lightingValuesTextPrefab, lightingTransform, true);
            lightingRef.transform.position = new Vector3((x + 0.5f), (y + 0.5f)) + transform.position;
            lightingRef.transform.localScale = Vector2.one;
            lightingValueTexts.Add(lightingRef);

            TextMeshProUGUI lightingText = lightingRef.GetComponent<TextMeshProUGUI>();
            lightingText.text = voxels[i].lighting.ToString();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(new Vector3(position.x + halfSize, position.y + halfSize), Vector3.one * voxelResolution);
    }

    public void RemoveChunkColliders() {
        foreach (EdgeCollider2D chunkCollider in gameObject.GetComponents<EdgeCollider2D>()) {
            Destroy(chunkCollider);
        }
    }

    public void RemoveOutlines() {
        foreach (Line chunkOutlineLine in gameObject.GetComponentsInChildren<Line>()) {
            Destroy(chunkOutlineLine.gameObject);
        }
    }

    public int[] GetChunkStates() {
        int[] stateValues = new int[(voxelResolution + 1) * (voxelResolution + 1)];

        for (int i = 0, y = 0; y < voxelResolution; y++) {
            for (int x = 0; x < voxelResolution; x++, i++) {
                stateValues[y * voxelResolution + x + y] = voxels[i].state;
            }
        }

        for (int y = 0; y < voxelResolution; y++) {
            if (xNeighbor && xNeighbor.voxels != null) {
                stateValues[y * voxelResolution + voxelResolution + y] = xNeighbor.voxels[y * voxelResolution].state;
            } else {
                stateValues[y * voxelResolution + voxelResolution + y] = -1;
            }
        }

        for (int x = 0; x < voxelResolution; x++) {
            if (yNeighbor && yNeighbor.voxels != null) {
                stateValues[(voxelResolution + 1) * voxelResolution + x] = yNeighbor.voxels[x].state;
            } else {
                stateValues[(voxelResolution + 1) * voxelResolution + x] = -1;
            }
        }

        if (xyNeighbor && xyNeighbor.voxels != null) {
            stateValues[(voxelResolution + 1) * (voxelResolution + 1) - 1] = xyNeighbor.voxels[0].state;
        } else {
            stateValues[(voxelResolution + 1) * (voxelResolution + 1) - 1] = -1;
        }

        return stateValues;
    }

    public void ClearReferences() {
        // Remove voxel reference points
        foreach (GameObject voxelRefPoint in voxelReferencePoints) {
            Destroy(voxelRefPoint);
        }

        // Remove lighting values
        foreach (GameObject lightValueText in lightingValueTexts) {
            Destroy(lightValueText);
        }
    }
}
