using UnityEngine;
using TMPro;

public class DebugController : MonoBehaviour {
    private CoreScriptableObject CORE;

    public GameObject debugPanel;
    public TextMeshProUGUI voxelPositionText;
    public TextMeshProUGUI chunkPositionText;

    public bool isActive;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
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
            debugPanel.SetActive(true);
        } else {
            debugPanel.SetActive(false);
        }
    }
}
