using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [Serializable]
    public struct VoxelMeshData
    {
        public int subMeshIndex;

        public static readonly Direction[] Directions =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West,
            Direction.Up,
            Direction.Down
        };

        public const int DirectionsAmount = 6;

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

        public const int VerticesPerVoxel = 24;
        public const int TrianglesPerVoxel = 36;
        public const int TrianglesPerFace = 6;
        public const int VerticesPerFace = 4;

        public static readonly int[][] FaceTriangles =
        {
            new[] {0, 1, 2, 3},
            new[] {5, 0, 3, 6},
            new[] {4, 5, 6, 7},
            new[] {1, 4, 7, 2},
            new[] {5, 4, 1, 0},
            new[] {3, 2, 7, 6},
        };

        public static readonly int[] FaceTriangles1D =
        {
            0, 1, 2, 3,
            5, 0, 3, 6,
            4, 5, 6, 7,
            1, 4, 7, 2,
            5, 4, 1, 0,
            3, 2, 7, 6,
        };

        public static Vector3[] FaceVertices(int dir, float adjustedScale, Vector3 pos)
        {
            Vector3[] faceVert = new Vector3[VerticesPerFace];
        
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
