using System.Collections.Generic;
using UnityEngine;

public class TerrainEditorController : MonoBehaviour {
    public TerrainEditingScriptableObject terrainEditingSO;
    public LayerMask layerMask;

    private List<LayerScriptableObject> layers = new List<LayerScriptableObject>();
    private CoreScriptableObject CORE;
    private LayerScriptableObject currentLayer;
    private TerrainEditor terrainEditor;

    private int voxelResolution, chunkResolution, radius, start, end, inc;

    private Camera cam;
    private GameObject player;
    private InfiniteGenerator infiniteGenerator;
    private BoxCollider playerEditingArea;

    private RaycastHit hitInfo;
    private Vector3 lastMousePosition;

    private readonly List<VoxelChunk> chunksToUpdate = new List<VoxelChunk>();

    private void Start() {
        cam = Camera.main;
        terrainEditor = FindObjectOfType<TerrainEditor>();
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        playerEditingArea = GetComponent<BoxCollider>();
        infiniteGenerator = FindObjectOfType<InfiniteGenerator>();

        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();

        voxelResolution = CORE.voxelResolution;
        chunkResolution = CORE.chunkResolution;

        // Setup collider area
        playerEditingArea.size = new Vector3(chunkResolution * voxelResolution * 2, chunkResolution * voxelResolution * 2);
    }

    private void Update() {
        transform.position = player.transform.position;

        // Check for player editing in area
        if (IsPlayerEditing()) {
            currentLayer = layers[terrainEditingSO.LayerIndex];
            radius = terrainEditingSO.Radius;
            start = radius > 0 ? -radius : 0;
            end = radius > 0 ? radius : 0;
            inc = radius > 0 ? radius : 1;
            lastMousePosition = Input.mousePosition + transform.position;

            Edit();
        }
    }

    private void Edit() {
        GetChunks();

        EditChunks();

        UpdateChunks();
    }

    private void GetChunks() {
        for (int i = start; i <= end; i += inc) {
            for (int j = start; j <= end; j += inc) {
                Vector2 hitPosition = new Vector2(hitInfo.point.x + i, hitInfo.point.y + j);

                Vector2Int chunkWorldPosition = ChunkHelper.GetChunkWorldPosition(hitPosition, voxelResolution);

                if (currentLayer.existingChunks.ContainsKey(chunkWorldPosition) &&
                    !chunksToUpdate.Contains(currentLayer.existingChunks[chunkWorldPosition])) {
                    chunksToUpdate.Add(currentLayer.existingChunks[chunkWorldPosition]);
                }
            }
        }
    }

    private void EditChunks() {
        for (int i = 0; i < chunksToUpdate.Count; i++) {
            VoxelChunk chunk = chunksToUpdate[i];
            List<Voxel> selectedVoxels = terrainEditor.GetSelectedVoxels(chunk, hitInfo.point);

            if (selectedVoxels.Count > 0) {
                terrainEditor.EditVoxels(selectedVoxels);
            } else {
                chunksToUpdate.Remove(chunk);
                i--;
            }
        }
    }

    // TODO: add a way to save chunks on edit
    private void UpdateChunks() {
        if (chunksToUpdate.Count > 0) {
            InfiniteGenerator.GenerateChunkList(currentLayer, chunksToUpdate);
            InfiniteGenerator.GenerateChunkList(currentLayer, infiniteGenerator.FindImportantNeighbors(currentLayer, chunksToUpdate));

            chunksToUpdate.Clear();
        }
    }

    private bool IsPlayerEditing() {
        return Input.GetMouseButton(0) && lastMousePosition != Input.mousePosition + transform.position && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo, 100, layerMask);
    }

    private void OnDrawGizmos() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        TerrainEditorGizmos.DrawChunksGizmo(mousePos, terrainEditingSO.Radius, voxelResolution);
        TerrainEditorGizmos.DrawVoxelEditingGizmo(mousePos, terrainEditingSO, voxelResolution);
    }

    public TerrainEditingScriptableObject GetTerrainEditingScriptableObject() {
        return terrainEditingSO;
    }
}
