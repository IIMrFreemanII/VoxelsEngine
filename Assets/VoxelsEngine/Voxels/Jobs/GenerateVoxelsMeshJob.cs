using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.Jobs
{
    public struct GenerateVoxelsMeshJob : IJobParallelFor
    {
        public NativeArray<int> activeVoxelsIndices;
        public int3 chunkSize;
        public float adjustedScale;

        public void Execute(int index)
        {
            
        }
        
        // public void MakeCube(Vector3Int coordinate, VoxelsSubMesh voxelsSubMesh)
        // {
        //     int voxelIndex = From3DTo1DIndex(coordinate);
        //     int[] voxelTriangles = new int[VoxelMeshData.TrianglesPerVoxel];
        //
        //     for (int i = 0; i < 6; i++)
        //     {
        //         int vertCount = voxelIndex * VoxelMeshData.VerticesPerVoxel + VoxelMeshData.VerticesPerFace * i;
        //
        //         MakeFace(vertCount, i * 6, voxelTriangles);
        //     }
        //
        //     // voxelsSubMesh.AddVoxelTriangles(coordinate, voxelTriangles);
        // }
        //
        // private void MakeFace(int vertCount, int posOffset, int[] triangles)
        // {
        //     triangles[posOffset] = vertCount;
        //     triangles[posOffset + 1] = vertCount + 1;
        //     triangles[posOffset + 2] = vertCount + 2;
        //     triangles[posOffset + 3] = vertCount;
        //     triangles[posOffset + 4] = vertCount + 2;
        //     triangles[posOffset + 5] = vertCount + 3;
        // }
    }
}