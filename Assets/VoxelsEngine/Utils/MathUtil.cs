using Unity.Mathematics;

namespace VoxelsEngine.Utils
{
    public static class MathUtil
    {
        public static int From3DTo1DIndex(int3 coordinate, int3 chunkSize)
        {
            return (coordinate.x * chunkSize.y * chunkSize.z) + (coordinate.y * chunkSize.z + coordinate.z);
        }

        public static int3 From1DTo3DIndex(int index, int3 chunkSize)
        {
            int x = (int) math.floor(index / ((float) chunkSize.y * chunkSize.z));
            int y = (int) math.floor(index / (float) chunkSize.z) % chunkSize.y;
            int z = index % chunkSize.z;

            return new int3(x, y, z);
        }
    }
}