using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class VoxelRenderer : MonoBehaviour
{
    public float scale = 1f;

    private float adjustedScale;

    private Mesh mesh;
    private List<Vector3> verticies;
    private List<int> triangles;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        adjustedScale = scale * 0.5f;
    }

    private void Start()
    {
        GenerateVoxelMesh(new VoxelData());
        UpdateMesh();
    }

    private void GenerateVoxelMesh(VoxelData data)
    {
        verticies = new List<Vector3>();
        triangles = new List<int>();
        
        for (int z = 0; z < data.Depth; z++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                if (data.GetCell(x, z) == 0) continue;
                
                MakeCube(adjustedScale, new Vector3(x, 0, z) * scale, x, z, data);
            }
        }
    }

    private void MakeCube(float scale, Vector3 cubePos, int x, int z, VoxelData data)
    {
        for (int i = 0; i < 6; i++)
        {
            if (data.GetNeighbor(x, z, (Direction) i) == 0)
                MakeFace((Direction)i, scale, cubePos);
        }
    }

    private void MakeFace(Direction dir, float scale, Vector3 facePos)
    {
        verticies.AddRange(CubeMeshData.FaceVertices(dir, scale, facePos));

        int vertCount = verticies.Count;

        triangles.Add(vertCount - 4);
        triangles.Add(vertCount - 4 + 1);
        triangles.Add(vertCount - 4 + 2);
        triangles.Add(vertCount - 4);
        triangles.Add(vertCount - 4 + 2);
        triangles.Add(vertCount - 4 + 3);
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = verticies.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
    }
}