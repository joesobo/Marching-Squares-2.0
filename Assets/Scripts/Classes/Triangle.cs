using UnityEngine;

public struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
    private Vector2 a;
    private Vector2 b;
    private Vector2 c;
    public readonly float red;
    public readonly float green;
    public readonly float blue;

    public Vector2 this[int i] {
        get {
            switch (i) {
                case 0:
                    return a;
                case 1:
                    return b;
                default:
                    return c;
            }
        }
    }
}
