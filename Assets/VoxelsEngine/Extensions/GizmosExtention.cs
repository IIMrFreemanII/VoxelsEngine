using UnityEngine;

namespace VoxelsEngine.Extensions
{
    public static class GizmosExtension
    {
        public static void DrawWireRect(Vector3 origin, Vector3 size)
        {
            // front
            Vector3 p1 = origin;
            Vector3 p2 = origin + new Vector3(0, 1f * size.y, 0);
            Vector3 p3 = origin + new Vector3(1f * size.x, 1f * size.y, 0);
            Vector3 p4 = origin + new Vector3(1f * size.x, 0, 0);
            // back
            Vector3 p5 = origin + new Vector3(0, 0, size.z);
            Vector3 p6 = origin + new Vector3(0, 1f * size.y, size.z);
            Vector3 p7 = origin + new Vector3(1f * size.x, 1f * size.y, size.z);
            Vector3 p8 = origin + new Vector3(1f * size.x, 0, size.z);

            // front face
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);

            // back face
            Gizmos.DrawLine(p5, p6);
            Gizmos.DrawLine(p6, p7);
            Gizmos.DrawLine(p7, p8);
            Gizmos.DrawLine(p8, p5);

            // top lines
            Gizmos.DrawLine(p1, p5);
            Gizmos.DrawLine(p2, p6);

            // bottom lines
            Gizmos.DrawLine(p4, p8);
            Gizmos.DrawLine(p3, p7);
        }
    }
}