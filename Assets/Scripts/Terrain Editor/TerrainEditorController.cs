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

        // Setup collider area
        playerEditingArea.size = new Vector3(chunkResolution * voxelResolution * 2, chunkResolution * voxelResolution * 2);
    }

    // TODO: implement different stencils
    private void Update() {
        transform.position = player.transform.position;

        // Check for player editing in area
        if (Input.GetMouseButton(0) && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo, 100, layerMask)) {
            radius = editingScriptableObject.Radius;

            Edit();
        }
    }

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

                Vector2Int chunkWorldPosition = ChunkHelper.GetChunkWorldPosition(hitPosition, voxelResolution);

                if (CORE.existingChunks.ContainsKey(chunkWorldPosition) && !chunksToUpdate.Contains(CORE.existingChunks[chunkWorldPosition])) {
                    chunksToUpdate.Add(CORE.existingChunks[chunkWorldPosition]);
                }
            }
        }
    }

    private void EditChunks() {
        foreach (VoxelChunk chunk in chunksToUpdate) {
            List<Voxel> selectedVoxels = chunk.GetSelectedVoxels(hitInfo.point, radius, voxelResolution);
            TerrainEditor.EditVoxels(selectedVoxels, editingScriptableObject.EditingType);
        }
    }

    private void UpdateChunks() {
        if (chunksToUpdate.Count > 0) {
            infiniteGenerator.GenerateChunkList(chunksToUpdate);
            infiniteGenerator.GenerateChunkList(infiniteGenerator.FindImportantNeighbors(chunksToUpdate));

            chunksToUpdate.Clear();
        }
    }

    private void OnDrawGizmos() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        TerrainEditorGizmos.DrawChunksGizmo(mousePos, editingScriptableObject.Radius, voxelResolution);
        TerrainEditorGizmos.DrawVoxelEditingGizmo(mousePos, editingScriptableObject.Radius, voxelResolution);
    }
}
