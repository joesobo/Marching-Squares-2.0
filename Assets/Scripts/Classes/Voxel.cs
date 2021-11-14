using System;
using UnityEngine;

[Serializable]
public class Voxel {
    // The index of the element type to spawn in this voxel
    public int state = 1;

    // The position of the voxel in the chunk
    public Vector2 position;

    public Voxel(int x, int y, float size) {
        position.x = (x + 0.5f) * size;
        position.y = (y + 0.5f) * size;
    }
}
