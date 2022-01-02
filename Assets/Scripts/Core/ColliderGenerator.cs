using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColliderGenerator : MonoBehaviour {
    private CoreScriptableObject CORE;

    private void Awake() {
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
    }

    public void GenerateChunkColliders(VoxelChunk chunk) {
        // Reset logic for chunk
        ColliderLogic.Reset(CORE.voxelResolution * CORE.chunkResolution);
        // Calculate outlines
        IEnumerable<List<Vector3>> outlines = ColliderLogic.CalculateOutlines(chunk);
        // Create new colliders
        CreateColliders(chunk, outlines);
    }

    // Uses the outlines to create colliders for the chunk
    private static void CreateColliders(VoxelChunk chunk, IEnumerable<List<Vector3>> outlines) {
        foreach (List<Vector3> outline in outlines) {
            EdgeCollider2D edgeCollider = chunk.gameObject.AddComponent<EdgeCollider2D>();

            edgeCollider.points = outline.Select(point => (Vector2)point).ToArray();
        }
    }
}
