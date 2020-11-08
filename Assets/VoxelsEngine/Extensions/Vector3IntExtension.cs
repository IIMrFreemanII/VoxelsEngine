using UnityEngine;

namespace VoxelsEngine.Extensions
{
    public static class Vector3IntExtension
    {
        public static Vector3 ToFloat(this Vector3Int v3Int)
        {
            return new Vector3(v3Int.x, v3Int.y, v3Int.z);
        }
    }
}