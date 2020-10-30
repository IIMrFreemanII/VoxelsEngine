using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [CreateAssetMenu(fileName = "Voxels Chunk", menuName = "Voxels Engine/Voxels Chunk")]
    public class VoxelsChunk : ScriptableObject
    {
        public Vector3Int size = new Vector3Int(3, 3, 3);

        private VoxelData[,,] _data;
        // {
        //     {
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //     },
        //     {
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //     },
        //     {
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //         {
        //             new VoxelData(Color.black), new VoxelData(Color.black), new VoxelData(Color.black),
        //         },
        //     }
        // };

        public int Width => _data.GetLength(0);
        public int Height => _data.GetLength(1);
        public int Depth => _data.GetLength(2);

        private void OnValidate()
        {
            size.x = size.x >= 1 ? size.x : 1;
            size.y = size.y >= 1 ? size.y : 1;
            size.z = size.z >= 1 ? size.z : 1;

            _data = new VoxelData[size.x, size.y, size.z];
        }

        public VoxelData GetCell(int x, int y, int z)
        {
            return _data[x, y, z];
        }

        public void SetSell(VoxelData data, Vector3Int position)
        {
            _data[position.x, position.y, position.z] = data;
        }

        public VoxelData GetNeighbor(Vector3Int coordinate, Direction dir)
        {
            Vector3Int offsetToCheck = _offsets[(int) dir];
            Vector3Int neighborCoord = coordinate + offsetToCheck;

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

        private readonly Vector3Int[] _offsets =
        {
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 0, -1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
        };
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