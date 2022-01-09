using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEditorController : MonoBehaviour {
    public TerrainEditingScriptableObject terrainEditingScriptableObject;
    private CoreScriptableObject CORE;

    private int voxelResolution, chunkResolution;

    private GameObject player;
    private InfiniteGenerator infiniteGenerator;
    private BoxCollider playerEditingArea;

    private RaycastHit hitInfo;
    public LayerMask layerMask;

    private readonly List<VoxelChunk> chunksToUpdate = new List<VoxelChunk>();

    private void Start() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        infiniteGenerator = FindObjectOfType<InfiniteGenerator>();
        playerEditingArea = GetComponent<BoxCollider>();

        voxelResolution = CORE.voxelResolution;
        chunkResolution = CORE.chunkResolution;

        SetupColliderArea();
    }

    // TODO: connect to debug controller
    private void Update() {
        // Follow player
        transform.position = player.transform.position;

        // Check for player editing in area
        if (Input.GetMouseButton(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100, layerMask)) {
            EditChunks();
        }

        // update mesh / collider for chunk and important neighbors
        UpdateChunks();
    }

    private void EditChunks() {
        Vector2Int chunkWorldPosition = GetChunkWorldPosition(hitInfo.point);

        if (CORE.existingChunks.ContainsKey(chunkWorldPosition)) {
            VoxelChunk chunk = CORE.existingChunks[chunkWorldPosition];
            Voxel voxel = chunk.voxels[GetVoxelIndex(hitInfo.point)];

            if (voxel.state == 1 && terrainEditingScriptableObject.EditingType == TerrainEditingScriptableObject.Type.Remove) {
                voxel.state = 0;
            } else if (voxel.state == 0 && terrainEditingScriptableObject.EditingType == TerrainEditingScriptableObject.Type.Fill) {
                voxel.state = 1;
            }

            chunksToUpdate.Add(chunk);
        }
    }

    private void UpdateChunks() {
        if (chunksToUpdate.Count > 0) {
            foreach (VoxelChunk chunk in chunksToUpdate) {
                infiniteGenerator.GenerateChunkList(chunksToUpdate);
                infiniteGenerator.GenerateChunkList(infiniteGenerator.FindImportantNeighbors(chunksToUpdate));
            }

            chunksToUpdate.Clear();
        }
    }

    private void SetupColliderArea() {
        playerEditingArea.size = new Vector3(chunkResolution * voxelResolution * 2, chunkResolution * voxelResolution * 2);
    }

    private Vector2Int GetChunkPosition(Vector3 point) {
        return new Vector2Int((int)Mathf.Floor(Mathf.Floor(hitInfo.point.x) / voxelResolution), (int)Mathf.Floor(Mathf.Floor(hitInfo.point.y) / voxelResolution));
    }

    private Vector2Int GetChunkWorldPosition(Vector3 point) {
        return GetChunkPosition(point) * voxelResolution;
    }

    private Vector2 GetVoxelPosition(Vector3 point) {
        Vector2Int chunkWorldOffset = GetChunkWorldPosition(point);
        return new Vector2((Mathf.Floor(hitInfo.point.x) - chunkWorldOffset.x) + 0.5f, (Mathf.Floor(hitInfo.point.y) - chunkWorldOffset.y) + 0.5f);
    }

    private int GetVoxelIndex(Vector3 point) {
        Vector2 voxelPos = GetVoxelPosition(point);
        float halfSize = voxelResolution * 0.5f;
        return (int)((voxelPos.x + voxelPos.y * voxelResolution) - halfSize);
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int chunkPos = GetChunkWorldPosition(mousePos);
        float halfSize = voxelResolution * 0.5f;

        Gizmos.DrawCube(new Vector3(chunkPos.x + halfSize, chunkPos.y + halfSize, 0), Vector3.one * voxelResolution);

        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Vector2 voxelLocalPosition = GetVoxelPosition(mousePos);
        Vector2 voxelPosition = voxelLocalPosition + chunkPos;

        Gizmos.DrawCube(new Vector3(voxelPosition.x, voxelPosition.y, 0), Vector3.one);
    }
}
