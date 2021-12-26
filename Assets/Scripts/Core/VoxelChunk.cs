using System.Collections.Generic;
using UnityEngine;
using Core;

[SelectionBase]
public class VoxelChunk : MonoBehaviour {
    private CoreScriptableObject CORE;

    // Reference to neighbor chunks for edge voxel information
    [HideInInspector] public VoxelChunk xNeighbor, yNeighbor, xyNeighbor;
    // Storage of chunks voxels
    [HideInInspector] public Voxel[] voxels;
    private List<GameObject> voxelReferencePoints;

    // The element to spawn at each reference position along the chunk
    private GameObject voxelRefPointsPrefab;
    // The amount of voxels in each direction of the chunk
    private int voxelResolution;

    // Half the resolution for finding center of chunks
    private float halfSize;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
    }

    public void SetupChunk(GameObject voxelReferencePointsPrefab) {
        voxelRefPointsPrefab = voxelReferencePointsPrefab;
        voxelResolution = CORE.voxelResolution;
        voxels = new Voxel[voxelResolution * voxelResolution];
        voxelReferencePoints = new List<GameObject>();
        halfSize = 0.5f * voxelResolution;
    }

    public void FillChunk() {
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
}
