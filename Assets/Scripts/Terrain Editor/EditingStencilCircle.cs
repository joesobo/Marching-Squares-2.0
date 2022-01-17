using UnityEngine;

public class EditingStencilCircle : EditingStencil {
    public override bool IsVoxelInStencil(Vector2 selectPoint, Vector2 hitPoint, int radius) {
        float x = selectPoint.x - hitPoint.x;
        float y = selectPoint.y - hitPoint.y;

        return x * x + y * y <= radius * radius;
    }
}
