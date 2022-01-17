using System.Collections.Generic;
using UnityEngine;

public class TerrainEditorController : MonoBehaviour {
    public TerrainEditingScriptableObject editingScriptableObject;
    private CoreScriptableObject CORE;

    private int voxelResolution, chunkResolution, radius;

    private Camera cam;
    private GameObject player;
    private InfiniteGenerator infiniteGenerator;
    private BoxCollider playerEditingArea;

    private RaycastHit hitInfo;
    public LayerMask layerMask;

    private readonly List<VoxelChunk> chunksToUpdate = new List<VoxelChunk>();

    private void Start() {
        cam = Camera.main;
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        infiniteGenerator = FindObjectOfType<InfiniteGenerator>();
        playerEditingArea = GetComponent<BoxCollider>();

        voxelResolution = CORE.voxelResolution;
        chunkResolution = CORE.chunkResolution;

        SetupColliderArea();
    }

    // TODO: connect to debug controller
    // TODO: implement different stencils
    private void Update() {
        // Follow player
        transform.position = player.transform.position;

        // Check for player editing in area
        if (Input.GetMouseButton(0) && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo, 100, layerMask)) {
            radius = editingScriptableObject.Radius;

            Edit();
        }
    }

    // TODO: Refactor out edit chunks
    // TODO: refactor out voxel finding?
    private void Edit() {
        GetChunks();

        EditChunks();

        UpdateChunks();
    }

    // TODO: make this a lot more efficient (dont check every point, just the edges)
    private void GetChunks() {
        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                Vector2 hitPosition = new Vector2(hitInfo.point.x + i, hitInfo.point.y + j);

                Vector2Int chunkWorldPosition = GetChunkWorldPosition(hitPosition);

                if (CORE.existingChunks.ContainsKey(chunkWorldPosition) && !chunksToUpdate.Contains(CORE.existingChunks[chunkWorldPosition])) {
                    chunksToUpdate.Add(CORE.existingChunks[chunkWorldPosition]);
                }
            }
        }
    }

    private void EditChunks() {
        foreach (VoxelChunk chunk in chunksToUpdate) {
            EditVoxels(GetVoxels(chunk));
        }
    }

    private List<Voxel> GetVoxels(VoxelChunk chunk) {
        List<Voxel> editVoxels = new List<Voxel>();

        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                Vector2 hitPosition = new Vector2(hitInfo.point.x + i, hitInfo.point.y + j);

                // check if position exists in this chunk
                if (ChunkContainsPosition(chunk, hitPosition)) {
                    Voxel voxel = chunk.voxels[GetVoxelIndex(hitPosition)];

                    if (!editVoxels.Contains(voxel)) {
                        editVoxels.Add(voxel);
                    }
                }
            }
        }

        return editVoxels;
    }

    private void EditVoxels(IEnumerable<Voxel> voxels) {
        foreach (Voxel voxel in voxels) {
            voxel.state = voxel.state switch {
                1 when editingScriptableObject.EditingType == TerrainEditingScriptableObject.Type.Remove => 0,
                0 when editingScriptableObject.EditingType == TerrainEditingScriptableObject.Type.Fill => 1,
                _ => voxel.state
            };
        }
    }

    private void UpdateChunks() {
        if (chunksToUpdate.Count > 0) {
            infiniteGenerator.GenerateChunkList(chunksToUpdate);
            infiniteGenerator.GenerateChunkList(infiniteGenerator.FindImportantNeighbors(chunksToUpdate));

            chunksToUpdate.Clear();
        }
    }

    private void SetupColliderArea() {
        playerEditingArea.size = new Vector3(chunkResolution * voxelResolution * 2, chunkResolution * voxelResolution * 2);
    }

    // TODO: Refactor out general chunk functions
    private Vector2Int GetChunkPosition(Vector3 point) {
        return new Vector2Int((int)Mathf.Floor(Mathf.Floor(point.x) / voxelResolution), (int)Mathf.Floor(Mathf.Floor(point.y) / voxelResolution));
    }

    private Vector2Int GetChunkWorldPosition(Vector3 point) {
        return GetChunkPosition(point) * voxelResolution;
    }

    private Vector2 GetVoxelPosition(Vector3 point) {
        Vector2Int chunkWorldOffset = GetChunkWorldPosition(point);
        return new Vector2((Mathf.Floor(point.x) - chunkWorldOffset.x) + 0.5f, (Mathf.Floor(point.y) - chunkWorldOffset.y) + 0.5f);
    }

    private int GetVoxelIndex(Vector3 point) {
        Vector2 voxelPos = GetVoxelPosition(point);
        float halfSize = voxelResolution * 0.5f;
        return (int)((voxelPos.x + voxelPos.y * voxelResolution) - halfSize);
    }

    private bool ChunkContainsPosition(VoxelChunk chunk, Vector2 position) {
        Vector2 startPos = chunk.GetWholePosition();
        Vector2 endPos = startPos + Vector2.one * voxelResolution;

        return position.x >= startPos.x && position.x <= endPos.x && position.y >= startPos.y && position.y <= endPos.y;
    }

    private void OnDrawGizmos() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        DrawChunksGizmo(mousePos);
        DrawVoxelEditingGizmo(mousePos);
    }

    private void DrawChunksGizmo(Vector3 mousePos) {
        List<Vector2Int> chunkPositions = new List<Vector2Int>();
        Gizmos.color = Color.red;
        int radius = editingScriptableObject.Radius;
        float halfSize = voxelResolution * 0.5f;

        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                Vector2 hitPosition = new Vector2(mousePos.x + i, mousePos.y + j);
                Vector2Int chunkPos = GetChunkWorldPosition(hitPosition);
                if (!chunkPositions.Contains(chunkPos)) {
                    chunkPositions.Add(chunkPos);
                }
            }
        }

        foreach (Vector2Int chunkPos in chunkPositions) {
            Gizmos.DrawWireCube(new Vector3(chunkPos.x + halfSize, chunkPos.y + halfSize, 0), Vector3.one * voxelResolution);
        }
    }

    private void DrawVoxelEditingGizmo(Vector3 mousePos) {
        Gizmos.color = Color.blue;
        Vector2Int chunkPos = GetChunkWorldPosition(mousePos);
        Vector2 voxelLocalPosition = GetVoxelPosition(mousePos);
        Vector2 voxelPosition = voxelLocalPosition + chunkPos;

        Gizmos.DrawWireCube(new Vector3(voxelPosition.x, voxelPosition.y, 0), Vector3.one * (editingScriptableObject.Radius * 2 + 1));
    }
}
