using UnityEngine;

public class EditingStencil {
    public virtual bool IsVoxelInStencil(Vector2 voxelPosition, Vector2 selectPoint, int radius) {
        return true;
    }
}
