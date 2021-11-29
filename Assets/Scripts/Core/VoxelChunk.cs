using UnityEngine;

namespace Core {
    [SelectionBase]
    public class VoxelChunk : MonoBehaviour {
        private CoreScriptableObject coreScriptableObject;

        // Reference to neighbor chunks for edge voxel information
        [HideInInspector] public VoxelChunk xNeighbor, yNeighbor, xyNeighbor;
        // Storage of chunks voxels
        [HideInInspector] public Voxel[] voxels;

        // The element to spawn at each reference position along the chunk
        private GameObject voxelRefPointsPrefab;
        // The amount of voxels in each direction of the chunk
        private int voxelResolution;

        // Half the resolution for finding center of chunks
        private float halfSize;

        private void Awake() {
            coreScriptableObject = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        }

        public void SetupChunk(GameObject voxelReferencePointsPrefab) {
            voxelRefPointsPrefab = voxelReferencePointsPrefab;
            voxelResolution = coreScriptableObject.voxelResolution;
            voxels = new Voxel[voxelResolution * voxelResolution];
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

        private void CreateReferencePoint(int i, int x, int y) {
            if (coreScriptableObject.showVoxelReferencePoints) {
                GameObject voxelRef = Instantiate(voxelRefPointsPrefab, transform, true);
                voxelRef.transform.position = new Vector3((x + 0.5f), (y + 0.5f)) + transform.position;
                voxelRef.transform.localScale = Vector2.one * 0.1f;
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
}
