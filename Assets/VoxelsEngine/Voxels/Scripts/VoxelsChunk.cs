using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [CreateAssetMenu(fileName = "Voxels Chunk", menuName = "Voxels Engine/Voxels Chunk")]
    public class VoxelsChunk : SerializedScriptableObject
    {
        [OnValueChanged("ValidateSize")] public Vector3Int size = new Vector3Int(3, 3, 3);

        public ChunkData3D chunkData3D;
        private VoxelData[,,] _data;

        private VoxelData[,,] Data
        {
            get => _data ?? (_data = new VoxelData[size.x, size.y, size.z]);
            set => _data = value;
        }

        public int Width => Data.GetLength(0);
        public int Height => Data.GetLength(1);
        public int Depth => Data.GetLength(2);

        public event Action OnDataChange;

        private void ValidateSize()
        {
            size.x = size.x >= 1 ? size.x : 1;
            size.y = size.y >= 1 ? size.y : 1;
            size.z = size.z >= 1 ? size.z : 1;

            Resize();
        }

        public VoxelData GetCell(int x, int y, int z)
        {
            return Data[x, y, z];
        }

        public VoxelData GetCell(Vector3Int posInArr)
        {
            return GetCell(posInArr.x, posInArr.y, posInArr.z);
        }

        public void SetSell(VoxelData data, Vector3Int position)
        {
            Data[position.x, position.y, position.z] = data;
        }

        [Button(ButtonSizes.Small)]
        public void Clear()
        {
            Data = new VoxelData[size.x, size.y, size.z];
            OnDataChange?.Invoke();
        }

        public void Resize()
        {
            // find all set values
            Dictionary<Vector3Int, VoxelData> savedVoxels = new Dictionary<Vector3Int, VoxelData>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        Vector3Int curPos = new Vector3Int(x, y, z);
                        VoxelData temp = GetCell(curPos);
                        if (temp != null)
                        {
                            // Debug.Log(temp);
                            savedVoxels.Add(curPos, temp);
                        }
                    }
                }
            }

            Data = new VoxelData[size.x, size.y, size.z];

            foreach (KeyValuePair<Vector3Int, VoxelData> savedVoxel in savedVoxels)
            {
                int x = savedVoxel.Key.x;
                int y = savedVoxel.Key.y;
                int z = savedVoxel.Key.z;

                if (x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth)
                {
                    SetSell(savedVoxel.Value, savedVoxel.Key);
                }
            }

            OnDataChange?.Invoke();
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

    [Serializable]
    public class ChunkData3D
    {
        public ChunkDataX[] chunkDataX;

        public ChunkDataX this[int index]
        {
            get => chunkDataX[index];
            set => chunkDataX[index] = value;
        }

        [Serializable]
        public class ChunkDataX
        {
            public ChunkDataY[] chunkDataY;

            public ChunkDataY this[int index]
            {
                get => chunkDataY[index];
                set => chunkDataY[index] = value;
            }

            [Serializable]
            public class ChunkDataY
            {
                public ChunkDataZ[] chunkDataZ;

                public ChunkDataZ this[int index]
                {
                    get => chunkDataZ[index];
                    set => chunkDataZ[index] = value;
                }

                [Serializable]
                public class ChunkDataZ
                {
                    public VoxelData[] voxelsData;

                    public VoxelData this[int index]
                    {
                        get => voxelsData[index];
                        set => voxelsData[index] = value;
                    }
                }
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