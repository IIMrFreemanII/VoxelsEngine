using System;
using UnityEngine;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts.CustomValues
{
    [Serializable]
    public class HandleVoxelsChunkSizeChange : HandleValueChange<Vector3Int>
    {
        public override bool IsEqual(Vector3Int prev, Vector3Int current)
        {
            return prev == current;
        }

        public override bool IsValid(Vector3Int value)
        {
            return value.x > 0 || value.y > 0 || value.z > 0;
        }
    }
}