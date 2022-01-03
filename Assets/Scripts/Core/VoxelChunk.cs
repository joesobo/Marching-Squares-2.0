using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Core;

[SelectionBase]
public class VoxelChunk : MonoBehaviour {
    private CoreScriptableObject CORE;

    private VoxelChunkGenerator voxelChunkGenerator;
    private VoxelMeshGenerator voxelMeshGenerator;
    private ColliderGenerator colliderGenerator;

    // Reference to neighbor chunks for edge voxel information
    public VoxelChunk xNeighbor, yNeighbor, xyNeighbor;
    // Storage of chunks voxels
    public Voxel[] voxels;
    // Storage of chunks vertices
    public Vector3[] vertices = null;
    // Storage of relationship between triangles and vertices
    public readonly Dictionary<Vector2, List<Triangle>> triangleDictionary = new Dictionary<Vector2, List<Triangle>>();
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
        vertices = null;
        triangleDictionary.Clear();
        xNeighbor = null;
        yNeighbor = null;
        xyNeighbor = null;
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Mesh", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    private void RefreshMesh() {
        voxelMeshGenerator.GenerateChunkMesh(this);
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button("Refresh Collider", ButtonSizes.Large), GUIColor(0.4f, 1, 0.8f)]
    private void RefreshCollider() {
        colliderGenerator.GenerateChunkColliders(this);
    }

    [Button("Refresh Whole Chunk", ButtonSizes.Large), GUIColor(0.6f, 0.4f, 0.8f)]
    private void RefreshChunk() {
        voxelChunkGenerator.SetupChunkNeighbors(this);
        voxelMeshGenerator.GenerateChunkMesh(this);
        colliderGenerator.GenerateChunkColliders(this);
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
        Gizmos.color = Color.red;
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(new Vector3(position.x + halfSize, position.y + halfSize), Vector3.one * voxelResolution);
    }

    public void RemoveChunkColliders() {
        foreach (EdgeCollider2D chunkCollider in gameObject.GetComponents<EdgeCollider2D>()) {
            Destroy(chunkCollider);
        }
    }
}
