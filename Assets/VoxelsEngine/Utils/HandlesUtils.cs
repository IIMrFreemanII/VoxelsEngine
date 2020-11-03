using UnityEditor;
using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class HandlesUtils
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
            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p2, p3);
            Handles.DrawLine(p3, p4);
            Handles.DrawLine(p4, p1);

            // back face
            Handles.DrawLine(p5, p6);
            Handles.DrawLine(p6, p7);
            Handles.DrawLine(p7, p8);
            Handles.DrawLine(p8, p5);

            // top lines
            Handles.DrawLine(p1, p5);
            Handles.DrawLine(p2, p6);

            // bottom lines
            Handles.DrawLine(p4, p8);
            Handles.DrawLine(p3, p7);
        }
    }
}