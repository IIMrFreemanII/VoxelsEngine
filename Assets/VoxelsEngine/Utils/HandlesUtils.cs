using UnityEditor;
using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class HandlesUtils
    {
        public static void DrawWireQuad(Vector3 center, Vector2 size, Vector3 dir)
        {
            Quaternion rotation = Quaternion.LookRotation(dir);
            Vector3 point = size * 0.5f;
            
            Vector3[] points =
            {
                center + rotation * new Vector3(-point.x, -point.y, 0),
                center + rotation * new Vector3(-point.x, point.y, 0),
                center + rotation * new Vector3(point.x, point.y, 0),
                center + rotation * new Vector3(point.x, -point.y, 0),
                center + rotation * new Vector3(-point.x, -point.y, 0),
            };

            for (int i = 0; i < points.Length - 1; i++)
            {
                Handles.DrawLine(points[i], points[i + 1]);
            }
        }

        public static void DrawWireCube(Vector3 origin, Vector3 size, bool invertedNormals, bool culling)
        {
            Vector3 cameraForward = origin - SceneView.currentDrawingSceneView.camera.transform.position;
            for (int i = 0; i < 6; i++)
            {
                Vector3 faceDir = Offsets[i];

                bool culled = invertedNormals
                    ? Vector3.Dot(cameraForward, faceDir) < -0.9f
                    : Vector3.Dot(cameraForward, faceDir) > 0;

                if (culled && culling) continue;
                
                if (faceDir == Vector3.forward || faceDir == Vector3.back)
                {
                    DrawWireQuad(origin + faceDir * size.z * 0.5f, new Vector2(size.x, size.y), faceDir);
                } else
                if (faceDir == Vector3.left || faceDir == Vector3.right)
                {
                    DrawWireQuad(origin + faceDir * size.x * 0.5f, new Vector2(size.z, size.y), faceDir);
                } else
                if (faceDir == Vector3.up || faceDir == Vector3.down)
                {
                    DrawWireQuad(origin + faceDir * size.y * 0.5f, new Vector2(size.x, size.z), faceDir);
                }
            }
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