using System;
using System.Linq;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [Serializable]
    public struct VoxelMeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public int subMeshIndex;
        public Direction[] faces;

        public static Direction[] directions =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West,
            Direction.Up,
            Direction.Down
        };

        public void UpdateTriangles(int voxelOrderNumber)
        {
            int facesAmount = 6;
            int totalVertAmount = voxelOrderNumber * 4 * facesAmount + facesAmount * 4;
            triangles = new int[facesAmount * 6];
            // Debug.Log($"voxelOrderNumber: {voxelOrderNumber}");
            for (int i = 0; i < facesAmount; i++)
            {
                if (!faces.Contains((Direction) i)) continue;
                int faceOffset = i * 6;
                
                int vertCount = (totalVertAmount / facesAmount) * i;
                
                triangles[faceOffset] = vertCount;
                triangles[faceOffset + 1] = vertCount + 1;
                triangles[faceOffset + 2] = vertCount + 2;
                triangles[faceOffset + 3] = vertCount;
                triangles[faceOffset + 4] = vertCount + 2;
                triangles[faceOffset + 5] = vertCount + 3;
            }
            // Debug.Log("---------------------------------------");
        }

        public static readonly Vector3[] Vertices =
        {
            new Vector3(1, 1, 1),
            new Vector3(-1, 1, 1),
            new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(1, 1, -1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, -1),
        };

        public static readonly int[][] FaceTriangles =
        {
            new[] {0, 1, 2, 3},
            new[] {5, 0, 3, 6},
            new[] {4, 5, 6, 7},
            new[] {1, 4, 7, 2},
            new[] {5, 4, 1, 0},
            new[] {3, 2, 7, 6},
        };

        public static Vector3[] FaceVertices(int dir, float adjustedScale, Vector3 pos)
        {
            Vector3[] faceVert = new Vector3[4];
        
            for (int i = 0; i < faceVert.Length; i++)
            {
                int vertIndex = FaceTriangles[dir][i];
                faceVert[i] = (Vertices[vertIndex] * adjustedScale) + pos;
            }

            return faceVert;
        }
        public static Vector3[] FaceVertices(Direction dir, float adjustedScale, Vector3 pos)
        {
            return FaceVertices((int)dir, adjustedScale, pos);
        }
    }
}
