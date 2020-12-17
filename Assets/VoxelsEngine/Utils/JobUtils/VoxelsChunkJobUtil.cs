using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelsEngine.Extensions;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Utils
{
    public static class VoxelsChunkJobUtil
    {
        public static VoxelData GetCell(int3 coordinate, int3 chunkSize, NativeArray<VoxelData> data)
        {
            int index = MathUtil.From3DTo1DIndex(coordinate, chunkSize);
            return data[index];
        }

        public static void SetCell(VoxelData voxelData, NativeArray<VoxelData> data, int3 coordinate, int3 chunkSize)
        {
            int index = MathUtil.From3DTo1DIndex(coordinate, chunkSize);
            data[index] = voxelData;
        }

        public static bool CheckOutOfRange(int3 coordinate, int3 chunkSize)
        {
            if (
                coordinate.x < 0 || coordinate.x >= chunkSize.x ||
                coordinate.y < 0 || coordinate.y >= chunkSize.y ||
                coordinate.z < 0 || coordinate.z >= chunkSize.z
            )
            {
                return true;
            }

            return false;
        }

        public static bool GetNeighbor(
            int3 coordinate,
            int3 chunkSize,
            Direction dir,
            NativeArray<VoxelData> data
        )
        {
            int3 offsetToCheck = VoxelsChunk.PossibleVoxelOffsetsInt3[(int) dir];
            int3 neighborCoord = coordinate + offsetToCheck;

            if (CheckOutOfRange(neighborCoord, chunkSize))
            {
                return false;
            }

            return GetCell(neighborCoord, chunkSize, data).enabled;
        }
        
        public static NativeList<int3> GetAllActiveNeighbors(int3 coordinate, int3 chunkSize, NativeArray<VoxelData> data)
        {
            NativeList<int3> activeNeighborsCoordinates = new NativeList<int3>(Allocator.Temp);
            
            for (int i = 0; i < VoxelMeshData.Directions.Length; i++)
            {
                Direction direction = VoxelMeshData.Directions[i];
                int3 offsetToCheck = VoxelsChunk.PossibleVoxelOffsetsInt3[(int) direction];
                int3 neighborCoord = coordinate + offsetToCheck;

                if (!CheckOutOfRange(neighborCoord, chunkSize))
                {
                    bool activeNeighbor = GetCell(neighborCoord, chunkSize, data).enabled;
                    if (activeNeighbor)
                    {
                        activeNeighborsCoordinates.Add(neighborCoord);
                    }
                }
            }

            return activeNeighborsCoordinates;
        }
        
        public static bool IsVoxelVisible(int3 coordinate, int3 chunkSize, NativeArray<VoxelData> data)
        {
            NativeList<int3> activeNeighbors = GetAllActiveNeighbors(coordinate, chunkSize, data);
            bool result = activeNeighbors.Length != VoxelMeshData.Directions.Length;
            activeNeighbors.Dispose();
            return result;
        }
        
        public static Vector3 GetCubePosition(int3 coordinate, int3 chunkSize, float scale)
        {
            Vector3 cubePos = (coordinate.ToVector3() - chunkSize.ToVector3() * 0.5f) * scale;
            Vector3 offset = Vector3.one * scale * 0.5f;
            return cubePos + offset;
        }
    }
}