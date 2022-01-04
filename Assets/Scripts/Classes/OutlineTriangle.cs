using UnityEngine;

namespace Core {
    public struct OutlineTriangle {
#pragma warning disable 649 // disable unassigned variable warning
        private Vector2 a;
        private Vector2 b;
        private Vector2 c;

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
}
