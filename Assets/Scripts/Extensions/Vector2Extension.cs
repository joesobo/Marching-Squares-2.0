using UnityEngine;

public static class Vector2Extension {
    public static bool Equal(this Vector2 a, Vector2 b) {
        return a.x == b.x && a.y == b.y;
    }
}
