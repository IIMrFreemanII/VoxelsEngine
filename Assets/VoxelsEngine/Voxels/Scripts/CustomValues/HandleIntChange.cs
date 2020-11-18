using System;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts.CustomValues
{
    [Serializable]
    public class HandleIntChange : HandleValueChange<int>
    {
        public override bool IsEqual(int prev, int current)
        {
            return prev == current;
        }
    }
}