using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core {
    public class DebugController : MonoBehaviour {
        private CoreScriptableObject CORE;

        public GameObject debugPanel;
        public TextMeshProUGUI mousePositionText;
        public TextMeshProUGUI chunkPositionText;

        private bool isActive = false;

        private void Awake() {
            CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                isActive = !isActive;
            }

            if (isActive) {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int normalizedMouse = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);
                int voxelResolution = CORE.voxelResolution;
                Vector2Int chunkPosition = new Vector2Int(normalizedMouse.x / voxelResolution, normalizedMouse.y / voxelResolution);

                mousePositionText.text = "Mouse Position: (" + normalizedMouse.x + ", " + normalizedMouse.y + ")";
                chunkPositionText.text = "Chunk Position: (" + chunkPosition.x + ", " + chunkPosition.y + ")";
                debugPanel.SetActive(true);
            } else {
                debugPanel.SetActive(false);
            }
        }
    }
}
