using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Voxel {
    // The index of the element type to spawn in this voxel
    public int state = 0;

    // The position of the voxel in the chunk
    public Vector2 position;

    public Voxel(int x, int y, float size, int? state = null) {
        position.x = (x + 0.5f) * size;
        position.y = (y + 0.5f) * size;

        if (state != null) {
            this.state = (int)state;
        }
    }
}
