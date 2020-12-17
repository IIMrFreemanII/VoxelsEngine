using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelsEngine.Utils;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.Jobs
{
    [BurstCompile]
    public struct GenerateVoxelsMeshJob : IJobParallelFor
    {
        [ReadOnly] public NativeList<int> activeVoxelsIndices;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> vertices;
        [NativeDisableParallelForRestriction] public NativeArray<int> triangles;
        
        [ReadOnly] public NativeArray<int> nativeFaceTriangles1D;
        [ReadOnly] public NativeArray<Vector3> nativeVertices;

        public int3 chunkSize;
        public float scale;
        public float adjustedScale;

        public void Execute(int index)
        {
            MakeCube(index);
        }

        public void MakeCube(int index)
        {
            int voxelIndex = activeVoxelsIndices[index];
            int3 coordinate = MathUtil.From1DTo3DIndex(voxelIndex, chunkSize);

            Vector3 pos = VoxelsChunkJobUtil.GetCubePosition(coordinate, chunkSize, scale);

            for (int i = 0; i < VoxelMeshData.DirectionsAmount; i++)
            {
                int vertCount = index * VoxelMeshData.VerticesPerVoxel + VoxelMeshData.VerticesPerFace * i;
                int trisOffset = index * VoxelMeshData.TrianglesPerVoxel + i * VoxelMeshData.DirectionsAmount;
                int vertFaceOffset = index * VoxelMeshData.VerticesPerVoxel + i * VoxelMeshData.VerticesPerFace;
                
                // vertices
                // i = dir
                for (int j = 0; j < VoxelMeshData.VerticesPerFace; j++)
                {
                    int vertIndex = nativeFaceTriangles1D[i * VoxelMeshData.VerticesPerFace + j];
                    // int vertIndex = VoxelMeshData.NativeFaceTriangles[i][j];
                    vertices[vertFaceOffset + j] = (nativeVertices[vertIndex] * adjustedScale) + pos;
                }

                // triangles
                triangles[trisOffset] = vertCount;
                triangles[trisOffset + 1] = vertCount + 1;
                triangles[trisOffset + 2] = vertCount + 2;
                triangles[trisOffset + 3] = vertCount;
                triangles[trisOffset + 4] = vertCount + 2;
                triangles[trisOffset + 5] = vertCount + 3;
            }
        }
    }
}