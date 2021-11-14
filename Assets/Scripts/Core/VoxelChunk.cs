using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class VoxelChunk : MonoBehaviour {
    // Reference to neighbor chunks for edge voxel information
    [HideInInspector] public VoxelChunk xNeighbor, yNeighbor, xyNeighbor;

    // The element to spawn at each reference position along the chunk
    private GameObject voxelRefPointsPrefab;
    // The amount of voxels in each direction of the chunk
    private int voxelResolution;

    // Storage of chunks voxels
    private Voxel[] voxels;
    // Whether or not reference points should spawn
    private bool spawnReferencePoints;
    // Half the resolution for finding center of chunks
    private float halfSize;
    
    // private Vector2[] vertices;
    // private int[] triangles;
    // private Color[] colors;

    public void SetupChunk(int resolution, bool useVoxelReferencePoints, GameObject voxelReferencePointsPrefab) {
        voxelRefPointsPrefab = voxelReferencePointsPrefab;
        voxelResolution = resolution;
        voxels = new Voxel[voxelResolution * voxelResolution];
        spawnReferencePoints = useVoxelReferencePoints;
        halfSize = 0.5f * resolution;
    }

    public void FillChunk() {
        for (int i = 0, y = 0; y < voxelResolution; y++) {
            for (int x = 0; x < voxelResolution; x++, i++) {
                CreateVoxelPoint(i, x, y);
            }
        }
    }

    private void CreateVoxelPoint(int i, int x, int y) {
        if (spawnReferencePoints) {
            GameObject voxelRef = Instantiate(voxelRefPointsPrefab, transform, true);
            voxelRef.transform.position = new Vector3((x + 0.5f), (y + 0.5f)) + transform.position;
            voxelRef.transform.localScale = Vector2.one * 0.1f;
        }

        voxels[i] = new Voxel(x, y, 1f);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        var position = transform.position;
        Gizmos.DrawWireCube(new Vector3(position.x + halfSize, position.y + halfSize), Vector3.one * voxelResolution);
    }
}
