using System;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts.CustomValues
{
    [Serializable]
    public class HandleVoxelsChunkChange : HandleUnityObjectChange<VoxelsChunk>
    {
        public override bool IsEqual(VoxelsChunk prev, VoxelsChunk current)
        {
            return prev == current;
        }
    }
}