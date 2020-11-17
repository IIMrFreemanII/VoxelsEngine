using System;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts.CustomValues
{
    [Serializable]
    public class Health
    {
        public int health;
        public bool test;
    }

    [Serializable]
    public class HandleHealthChange : HandleValueChange<Health>
    {
        public override bool IsEqual(Health prev, Health current)
        {
            return prev?.health == current?.health && prev?.test == current?.test;
        }

        public override void HandleChange(Health value)
        {
            if (!IsEqual(prevValue, value))
            {
                base.HandleChange(value);
            }
        }
    }
}