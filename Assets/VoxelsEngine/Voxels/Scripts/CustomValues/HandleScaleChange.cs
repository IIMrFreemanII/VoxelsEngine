using System;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts.CustomValues
{
    [Serializable]
    public class HandleScaleChange : HandleValueChange<float>
    {
        public override bool IsEqual(float prev, float current)
        {
            return prev == current;
        }

        public override bool IsValid(float value)
        {
            return value > 0;
        }
    }
}