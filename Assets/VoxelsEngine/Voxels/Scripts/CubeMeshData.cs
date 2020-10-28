using UnityEngine;

public static class CubeMeshData
{
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

    public static Vector3[] FaceVertices(int dir, float scale, Vector3 pos)
    {
        Vector3[] faceVert = new Vector3[4];
        
        for (int i = 0; i < faceVert.Length; i++)
        {
            int vertIndex = FaceTriangles[dir][i];
            faceVert[i] = (Vertices[vertIndex] * scale) + pos;
        }

        return faceVert;
    }
    public static Vector3[] FaceVertices(Direction dir, float scale, Vector3 pos)
    {
        return FaceVertices((int)dir, scale, pos);
    }
}
