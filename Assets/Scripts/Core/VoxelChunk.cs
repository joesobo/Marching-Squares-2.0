using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[SelectionBase]
public class VoxelChunk : MonoBehaviour {
    private CoreScriptableObject CORE;

    private VoxelChunkGenerator voxelChunkGenerator;
    private VoxelMeshGenerator voxelMeshGenerator;
    private ColliderGenerator colliderGenerator;

    [HideInInspector] public MeshFilter meshFilter;
    [HideInInspector] public MeshRenderer meshRenderer;

    public Material material;

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

    // The element to spawn at each reference position along the chunk
    private GameObject voxelRefPointsPrefab;
    // The amount of voxels in each direction of the chunk
    private int voxelResolution;

    // Half the resolution for finding center of chunks
    private float halfSize;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        voxelChunkGenerator = FindObjectOfType<VoxelChunkGenerator>();
        voxelMeshGenerator = FindObjectOfType<VoxelMeshGenerator>();
        colliderGenerator = FindObjectOfType<ColliderGenerator>();
        meshFilter = FindObjectOfType<MeshFilter>();
        meshRenderer = FindObjectOfType<MeshRenderer>();
    }

    public void SetupChunk(GameObject voxelReferencePointsPrefab, Vector2 chunkPosition) {
        voxelRefPointsPrefab = voxelReferencePointsPrefab;
        voxelResolution = CORE.voxelResolution;
        voxels = new Voxel[voxelResolution * voxelResolution];
        voxelReferencePoints = new List<GameObject>();
        halfSize = 0.5f * voxelResolution;

        name = "Chunk (" + chunkPosition.x / CORE.voxelResolution + ", " + chunkPosition.y / CORE.voxelResolution + ")";
        transform.position = new Vector3(chunkPosition.x, chunkPosition.y, 0);
        FillChunk();
        gameObject.SetActive(true);
    }

    public void ResetChunk() {
        voxels = null;
        meshVertices = null;
        outlineVertices = null;
        triangleDictionary.Clear();
        xNeighbor = null;
        yNeighbor = null;
        xyNeighbor = null;
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Mesh", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    private void RefreshMesh() {
        voxelMeshGenerator.GenerateChunkMesh(this, material);
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Collider", ButtonSizes.Large), GUIColor(0.4f, 1, 0.8f)]
    private void RefreshCollider() {
        colliderGenerator.GenerateChunkColliders(this);
    }

    [Button("Refresh Whole Chunk", ButtonSizes.Large), GUIColor(0.6f, 0.4f, 0.8f)]
    public void GenerateChunk() {
        voxelChunkGenerator.SetupChunkNeighbors(this);
        RefreshMesh();
        RefreshCollider();
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
        voxels[i] = new Voxel(x, y, 1f);
        CreateReferencePoint(i, x, y);
    }

    public void ResetReferencePoints() {
        // Remove voxel reference points
        foreach (GameObject voxelRefPoint in voxelReferencePoints) {
            Destroy(voxelRefPoint);
        }

        // Regenerate voxel reference points
        for (int i = 0, y = 0; y < voxelResolution; y++) {
            for (int x = 0; x < voxelResolution; x++, i++) {
                CreateReferencePoint(i, x, y);
            }
        }
    }

    private void CreateReferencePoint(int i, int x, int y) {
        if (CORE.showVoxelReferencePoints) {
            GameObject voxelRef = Instantiate(voxelRefPointsPrefab, transform, true);
            voxelRef.transform.parent = transform;
            voxelRef.transform.position = new Vector3((x + 0.5f), (y + 0.5f)) + transform.position;
            voxelRef.transform.localScale = Vector2.one * 0.1f;
            voxelReferencePoints.Add(voxelRef);

            SpriteRenderer voxelRenderer = voxelRef.GetComponent<SpriteRenderer>();
            voxelRenderer.color = voxels[i].state == 1 ? Color.clear : Color.black;
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
}
