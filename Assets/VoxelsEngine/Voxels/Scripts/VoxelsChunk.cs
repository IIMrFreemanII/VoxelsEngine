using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [CreateAssetMenu(fileName = "Voxels Chunk", menuName = "Voxels Engine/Voxels Chunk")]
    public class VoxelsChunk : ScriptableObject
    {
        VoxelData[,,] _data =
        {
            // x
            {
                // y
                {
                    //z
                    null, new VoxelData(Color.blue), new VoxelData(Color.blue)
                },
                {
                    new VoxelData(Color.blue), new VoxelData(Color.blue), new VoxelData(Color.blue)
                },
                {
                    new VoxelData(Color.blue), new VoxelData(Color.blue), null
                }
            },
            {
                {
                    null, new VoxelData(Color.blue), new VoxelData(Color.blue)
                },
                {
                    new VoxelData(Color.blue), new VoxelData(Color.blue), new VoxelData(Color.blue)
                },
                {
                    new VoxelData(Color.blue), new VoxelData(Color.blue), null
                }
            },
            {
                {
                    null, new VoxelData(Color.blue), new VoxelData(Color.blue)
                },
                {
                    new VoxelData(Color.blue), new VoxelData(Color.blue), new VoxelData(Color.blue)
                },
                {
                    new VoxelData(Color.blue), new VoxelData(Color.blue), null
                }
            },
        };

        public int Width => _data.GetLength(0);
        public int Height => _data.GetLength(1);
        public int Depth => _data.GetLength(2);

        public VoxelData GetCell(int x, int y, int z)
        {
            return _data[x, y, z];
        }

        public VoxelData GetNeighbor(int x, int y, int z, Direction dir)
        {
            DataCoordinate offsetToCheck = _offsets[(int) dir];
            DataCoordinate neighborCoord =
                new DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

            if (
                neighborCoord.x < 0 || neighborCoord.x >= Width ||
                neighborCoord.y < 0 || neighborCoord.y >= Height ||
                neighborCoord.z < 0 || neighborCoord.z >= Depth
            )
            {
                return null;
            }

            return GetCell(neighborCoord.x, neighborCoord.y, neighborCoord.z);
        }

        private readonly DataCoordinate[] _offsets =
        {
            new DataCoordinate(0, 0, 1),
            new DataCoordinate(1, 0, 0),
            new DataCoordinate(0, 0, -1),
            new DataCoordinate(-1, 0, 0),
            new DataCoordinate(0, 1, 0),
            new DataCoordinate(0, -1, 0),
        };

        struct DataCoordinate
        {
            public int x;
            public int y;
            public int z;

            public DataCoordinate(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
    }

    public enum Direction
    {
        North,
        East,
        South,
        West,
        Up,
        Down
    }
}