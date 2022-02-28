using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

public class OutlineDrawGenerator : MonoBehaviour {
    private OutlineShaderController outlineShaderController;

    public Color color;
    public float thickness = 0.1f;

    private Transform cam;
    private Vector3 playerPosition;

    private void Awake() {
        outlineShaderController = GetComponent<OutlineShaderController>();
        cam = Camera.main.transform;
        playerPosition = GameObject.FindGameObjectsWithTag("Player")[0].transform.position;
    }

    public void GenerateChunkOutlines(CoreScriptableObject CORE, VoxelChunk chunk) {
        if (CORE.doOutlines) {
            // Remove outlines from chunk if regenerating its outlines
            chunk.RemoveOutlines();

            // recalculate chunks outline values
            outlineShaderController.ShaderTriangulate(chunk);

            // Reset logic for chunk
            OutlineLogic.Reset(CORE.voxelResolution * CORE.chunkResolution);
            // Calculate outlines
            IEnumerable<List<Vector3>> outlines = OutlineLogic.CalculateOutlines(chunk);
            // Create new outlines
            DrawOutline(CORE, chunk, outlines);
        }
    }

    // Uses the outlines to create lines around the chunks edges
    private void DrawOutline(CoreScriptableObject CORE, VoxelChunk chunk, IEnumerable<List<Vector3>> outlines) {
        foreach (List<Vector3> outline in outlines) {
            Vector3 startPoint = new Vector3(outline[0].x + chunk.transform.position.x - playerPosition.x, outline[0].y + chunk.transform.position.y - playerPosition.y, CORE.zIndex);
            Vector3 endPoint;
            int index = 1;

            while (index < outline.Count()) {
                endPoint = new Vector3(outline[index].x + chunk.transform.position.x - playerPosition.x, outline[index].y + chunk.transform.position.y - playerPosition.y, CORE.zIndex);
                DrawLine(chunk, startPoint, endPoint);
                startPoint = endPoint;
                index++;
            }

            DrawLine(chunk, startPoint, new Vector3(outline[0].x + chunk.transform.position.x - playerPosition.x, outline[0].y + chunk.transform.position.y - playerPosition.y, CORE.zIndex));
        }
    }

    // Draws a line between two points
    private void DrawLine(VoxelChunk chunk, Vector3 start, Vector3 end) {
        Vector3 localStart = start - chunk.transform.position;
        Vector3 localEnd = end - chunk.transform.position;

        if (localStart == new Vector3(-7.5f, 0.5f) ||
            localEnd == new Vector3(-7.5f, 0.5f) ||
            (localStart.x == -7.5f && localEnd.x == -7.5f) ||
            (localStart.y == 0.5f && localEnd.y == 0.5f) ||
            localStart == new Vector3(0.5f, 8.5f) ||
            localEnd == new Vector3(0.5f, 8.5f) ||
            (localStart.x == 0.5f && localEnd.x == 0.5f) ||
            (localStart.y == 8.5f && localEnd.y == 8.5f) ||
             (localStart.x == 8.5f && localEnd.x == 8.5f)) return;

        GameObject myLine = new GameObject();
        myLine.name = "Outline: [(" + localStart.x + ", " + localStart.y + "), (" + localEnd.x + ", " + localEnd.y + ")]";
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
}
