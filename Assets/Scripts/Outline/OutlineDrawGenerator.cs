using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

public class OutlineDrawGenerator : MonoBehaviour {
    private OutlineShaderController outlineShaderController;
    private CoreScriptableObject CORE;

    public Color color;
    public float thickness = 0.1f;

    private Transform cam;
    private Vector3 playerPosition;

    private void Awake() {
        outlineShaderController = GetComponent<OutlineShaderController>();
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
        cam = Camera.main.transform;
        playerPosition = GameObject.FindGameObjectsWithTag("Player")[0].transform.position;
    }

    public void GenerateChunkOutlines(LayerScriptableObject layer, VoxelChunk chunk) {
        if (layer.doOutlines) {
            // Remove outlines from chunk if regenerating its outlines
            chunk.RemoveOutlines();

            // recalculate chunks outline values
            outlineShaderController.ShaderTriangulate(chunk);

            // Reset logic for chunk
            OutlineLogic.Reset(CORE.voxelResolution * CORE.chunkResolution);
            // Calculate outlines
            IEnumerable<List<Vector3>> outlines = OutlineLogic.CalculateOutlines(chunk);
            // Create new outlines
            DrawOutline(chunk, outlines, layer.zIndex);
        }
    }

    // Uses the outlines to create lines around the chunks edges
    private void DrawOutline(VoxelChunk chunk, IEnumerable<List<Vector3>> outlines, int zIndex) {
        foreach (List<Vector3> outline in outlines) {
            Vector3 chunkPos = chunk.transform.position;
            Vector3 startPoint = new Vector3(outline[0].x + chunkPos.x - playerPosition.x, outline[0].y + chunkPos.y - playerPosition.y, zIndex);
            int index = 1;

            while (index < outline.Count()) {
                Vector3 endPoint = new Vector3(outline[index].x + chunkPos.x - playerPosition.x, outline[index].y + chunkPos.y - playerPosition.y, zIndex);
                DrawLine(chunk, startPoint, endPoint);
                startPoint = endPoint;
                index++;
            }

            DrawLine(chunk, startPoint, new Vector3(outline[0].x + chunkPos.x - playerPosition.x, outline[0].y + chunkPos.y - playerPosition.y, zIndex));
        }
    }

    // Draws a line between two points
    // TODO: Add pooling
    private void DrawLine(VoxelChunk chunk, Vector3 start, Vector3 end) {
        Vector3 localStart = start - chunk.transform.position;
        Vector3 localEnd = end - chunk.transform.position;

        if (IsOutlineChunkEdge(localStart, localEnd)) return;

        GameObject myLine = new GameObject {
            name = "Outline: [(" + localStart.x + ", " + localStart.y + "), (" + localEnd.x + ", " + localEnd.y + ")]"
        };
        myLine.transform.parent = chunk.transform;
        Line line = myLine.AddComponent<Line>();
        line.Color = color;
        line.Start = start;
        line.End = end;
        line.Thickness = thickness;
        line.Geometry = LineGeometry.Billboard;
        line.Dashed = false;
        line.BlendMode = ShapesBlendMode.Opaque;
    }

    private static bool IsOutlineChunkEdge(Vector3 start, Vector3 end) {
        return (start == new Vector3(-7.5f, 0.5f) ||
            end == new Vector3(-7.5f, 0.5f) ||
            (start.x == -7.5f && end.x == -7.5f) ||
            (start.y == 0.5f && end.y == 0.5f) ||
            start == new Vector3(0.5f, 8.5f) ||
            end == new Vector3(0.5f, 8.5f) ||
            (start.x == 0.5f && end.x == 0.5f) ||
            (start.y == 8.5f && end.y == 8.5f) ||
             (start.x == 8.5f && end.x == 8.5f));
    }
}
