using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VoxelsEngine.Utils;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.Jobs
{
    [BurstCompile]
    public struct FilterVisibleVoxelsJob : IJobParallelForFilter
    {
        public NativeArray<VoxelData> voxelsData;
        public int3 chunkSize;
        
        public bool Execute(int index)
        {
            int3 coordinate = MathUtil.From1DTo3DIndex(index, chunkSize);
            return VoxelsChunkJobUtil.IsVoxelVisible(coordinate, chunkSize, voxelsData);
        }
    }
}