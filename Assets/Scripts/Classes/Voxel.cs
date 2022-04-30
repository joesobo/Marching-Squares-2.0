using System;
using UnityEngine;

[Serializable]
public class Voxel {
    // The index of the element type to spawn in this voxel
    public int state = 0;
    // Lighting value between 0 and 5
    public int lighting = 0;
    // If the voxel emits light
    public bool isLuminous = false;

    // The position of the voxel in the chunk
    public Vector2 position;

    public Voxel(float x, float y, int? state = null) {
        position.x = x;
        position.y = y;

        if (state != null) {
            this.state = (int)state;

            if (this.state == (int)BlockType.Glow) {
                this.isLuminous = true;
            }
        }
    }
}
