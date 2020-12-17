using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    // public struct VoxelsJobSubMesh
    // {
    //     public int subMeshIndex;
    //     
    // }
    
    [Serializable]
    public class VoxelsSubMesh
    {
        public Material material;
        // public Dictionary<Vector3Int, int[]> triangles = new Dictionary<Vector3Int, int[]>();
        public List<int> triangles = new List<int>();

        public void Clear()
        {
            triangles.Clear();
        }

        public void SetMaterial(Material material)
        {
            this.material = material;
        }

        // public void AddVoxelTriangles(Vector3Int coordinate, int[] triangles)
        // {
        //     this.triangles.Add(coordinate, triangles);
        // }
        //
        // public void RemoveVoxelTriangles(Vector3Int coordinate)
        // {
        //     if (triangles.ContainsKey(coordinate))
        //     {
        //         triangles.Remove(coordinate);
        //     }
        // }

        // public int[] GetTriangles()
        // {
        //     int[] tris = new int[triangles.Count * VoxelMeshData.TrianglesPerVoxel];
        //
        //     int i = 0;
        //     foreach (KeyValuePair<Vector3Int,int[]> keyValuePair in triangles)
        //     {
        //         keyValuePair.Value.CopyTo(tris, i);
        //         i += VoxelMeshData.TrianglesPerVoxel;
        //     }
        //
        //     return tris;
        // }
        
        public List<int> GetTrianglesTest()
        {
            return triangles;
        }
    }
}