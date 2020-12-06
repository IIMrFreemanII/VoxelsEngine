using Unity.Mathematics;
using UnityEngine;

namespace VoxelsEngine.Extensions
{
    public static class Int3Extension
    {
        public static Vector3Int ToVector3Int(this int3 value)
        {
            return new Vector3Int(value.x, value.y, value.z);
        }
    }
}