#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class HandlesUtils
    {
        public static void DrawWireQuad(Vector3 localCenter, Vector2 localSize, Vector3 localDir, Transform transform)
        {
            Quaternion rotation = Quaternion.LookRotation(localDir);
            Vector3 point = localSize * 0.5f;
            
            Vector3[] points =
            {
                localCenter + rotation * new Vector3(-point.x, -point.y, 0),
                localCenter + rotation * new Vector3(-point.x, point.y, 0),
                localCenter + rotation * new Vector3(point.x, point.y, 0),
                localCenter + rotation * new Vector3(point.x, -point.y, 0),
                localCenter + rotation * new Vector3(-point.x, -point.y, 0),
            };

            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 p0 = transform.TransformPoint(points[i]);
                Vector3 p1 = transform.TransformPoint(points[i + 1]);
                
                Handles.DrawLine(p0, p1);
            }
        }

        public static void DrawWireCube(Vector3 localCenter, Transform transform, Vector3 localSize, bool invertedNormals, bool culling)
        {
            Vector3 camDir = (transform.position - SceneView.currentDrawingSceneView.camera.transform.position).normalized;

            float[] mapFloatsToDir =
            {
                localSize.z,
                localSize.x,
                localSize.z,
                localSize.x,
                localSize.y,
                localSize.y
            };
            
            Vector2[] mapSizesToDir =
            {
                new Vector2(localSize.x, localSize.y),
                new Vector2(localSize.z, localSize.y),
                new Vector2(localSize.x, localSize.y),
                new Vector2(localSize.z, localSize.y),
                new Vector2(localSize.x, localSize.z),
                new Vector2(localSize.x, localSize.z)
            };
            
            for (int i = 0; i < 6; i++)
            {
                Vector3 faceDir = Offsets[i];
                Vector3 worldFarDir = transform.TransformDirection(faceDir);

                bool culled = invertedNormals
                    ? Vector3.Dot(camDir, worldFarDir) < -0.28f
                    : Vector3.Dot(camDir, worldFarDir) > 0;

                if (culled && culling) continue;
                
                DrawWireQuad(localCenter + faceDir * mapFloatsToDir[i] * 0.5f, mapSizesToDir[i], faceDir, transform);
            }
        }

        public static void DrawRay(Vector3 origin, Vector3 dir)
        {
            Handles.DrawLine(origin, origin + dir);
        }
        
        private static readonly Vector3[] Offsets =
        {
            Vector3.forward,
            Vector3.right,
            Vector3.back,
            Vector3.left, 
            Vector3.up, 
            Vector3.down, 
        };
    }
}
#endif