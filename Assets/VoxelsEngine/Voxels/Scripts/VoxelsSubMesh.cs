using System.Collections.Generic;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsSubMesh
    {
        public Material material;
        public Dictionary<Vector3Int, int[]> triangles = new Dictionary<Vector3Int, int[]>();

        public void AddVoxelTriangles(Vector3Int coordinate, int[] triangles)
        {
            this.triangles.Add(coordinate, triangles);
        }

        public void RemoveVoxelTriangles(Vector3Int coordinate)
        {
            if (triangles.ContainsKey(coordinate))
            {
                triangles.Remove(coordinate);
            }
        }

        public int[] GetTriangles()
        {
            int[] tris = new int[triangles.Count * VoxelMeshData.TrianglesPerVoxel];

            int i = 0;
            foreach (KeyValuePair<Vector3Int,int[]> keyValuePair in triangles)
            {
                keyValuePair.Value.CopyTo(tris, i);
                i += VoxelMeshData.TrianglesPerVoxel;
            }

            return tris;
        }
    }
}