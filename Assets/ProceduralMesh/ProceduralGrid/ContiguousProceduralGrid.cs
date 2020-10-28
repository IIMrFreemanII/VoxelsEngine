using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiguousProceduralGrid : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] verticies;
    private int[] triangles;
    
    // grid settings
    public float cellSize = 1;
    public Vector3 gridOffset = Vector3.zero;
    public int gridSize = 2;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    private void Start()
    {
        MakeContiguousProceduralGrid();
        UpdateMesh();
    }

    private void MakeContiguousProceduralGrid()
    {
        verticies = new Vector3[(gridSize + 1) * (gridSize + 1)];
        triangles = new int[gridSize * gridSize * 6];

        int vert = 0;
        int tris = 0;

        float vertexOffset = cellSize * 0.5f;

        // create vertex grid
        for (int x = 0; x <= gridSize; x++)
        {
            for (int y = 0; y <= gridSize; y++)
            {
                verticies[vert] = new Vector3(x * cellSize - vertexOffset, y * cellSize - vertexOffset, 0);
                vert++;
            }
        }

        // reset vertex tracker
        vert = 0;

        // setting each cell's triangle
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + 1;
                triangles[tris + 2] = vert + (gridSize + 1);
                triangles[tris + 3] = vert + (gridSize + 1);
                triangles[tris + 4] = vert + 1;
                triangles[tris + 5] = vert + (gridSize + 1) + 1;
                
                vert++;
                tris += 6;
            }
            vert++;
        }

        // Vector3 cellOffset = new Vector3(x * cellSize, y * cellSize, 0);
        //
        // int v0 = vert;
        // int v1 = vert + 1;
        // int v2 = vert + 2;
        // int v3 = vert + 3;
        //
        // verticies[v0] = new Vector3(-vertexOffset, -vertexOffset, 0) + cellOffset + gridOffset;
        // verticies[v1] = new Vector3(-vertexOffset, vertexOffset, 0) + cellOffset + gridOffset;
        // verticies[v2] = new Vector3(vertexOffset, -vertexOffset, 0) + cellOffset + gridOffset;
        // verticies[v3] = new Vector3(vertexOffset, vertexOffset, 0) + cellOffset + gridOffset;
        //
        // triangles[tris] = v0;
        // triangles[tris + 1] = v1;
        // triangles[tris + 2] = v2;
        // triangles[tris + 3] = v2;
        // triangles[tris + 4] = v1;
        // triangles[tris + 5] = v3;
        //
        // vert += 4;
        // tris += 6;
    }
    
    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
    }
}
