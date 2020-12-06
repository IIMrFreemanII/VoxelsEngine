using System;

namespace VoxelsEngine.Voxels.Scripts
{
    [Serializable]
    public struct VoxelData
    {
        public VoxelMeshData mesh;
        public bool enabled;
        public bool visible;
        public float durability;

        public VoxelData SetEnabled(bool enabled)
        {
            this.enabled = enabled;
            return this;
        }
    }
}