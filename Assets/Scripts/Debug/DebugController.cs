using UnityEngine;
using TMPro;

public class DebugController : MonoBehaviour {
    private CoreScriptableObject CORE;
    private WorldScriptableObject world;

    public GameObject debugPanel;
    public TextMeshProUGUI voxelPositionText;
    public TextMeshProUGUI chunkPositionText;
    public TextMeshProUGUI worldNameText;
    public TextMeshProUGUI seedText;
    public TextMeshProUGUI layerCountText;

    public bool isActive;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        world = FindObjectOfType<VoxelCore>().GetWorldScriptableObject();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            isActive = !isActive;
        }

        if (isActive) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 voxelPosition = ChunkHelper.GetVoxelPosition(mousePosition, CORE.voxelResolution);
            Vector2Int chunkPosition = ChunkHelper.GetChunkPosition(mousePosition, CORE.voxelResolution);

            voxelPositionText.text = "Voxel Position: (" + voxelPosition.x + ", " + voxelPosition.y + ")";
            chunkPositionText.text = "Chunk Position: (" + chunkPosition.x + ", " + chunkPosition.y + ")";
            worldNameText.text = "World Name: " + world.worldName;
            seedText.text = "Seed: " + world.seed;
            layerCountText.text = "Layer Count: " + world.layers.Count;
            debugPanel.SetActive(true);
        } else {
            debugPanel.SetActive(false);
        }
    }
}
