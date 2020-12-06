using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.Jobs
{
    [BurstCompile]
    public struct FillVoxelsDataJob : IJobParallelFor
    {
        public NativeArray<VoxelData> voxelsData;
        public int subMeshIndex;

        public void Execute(int index)
        {
            AppendVoxel(index);
        }

        private void AppendVoxel(int index)
        {
            SetCell(index, new VoxelData
            {
                durability = 1f,
                enabled = true,
                visible = true,
                mesh = new VoxelMeshData {subMeshIndex = subMeshIndex},
            });
        }

        private void SetCell(int index, VoxelData voxelData)
        {
            voxelsData[index] = voxelData;
        }
    }
}