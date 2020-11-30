using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class ArrayUtils
    {
        public static int From3DTo1DIndex(int x, int y, int z, int height, int depth)
        {
            return (x * height * depth) + (y * depth + z);
        }
        public static int From3DTo1DIndex(Vector3Int position, int height, int depth)
        {
            return From3DTo1DIndex(position.x, position.y, position.z, height, depth);
        }
    }
}