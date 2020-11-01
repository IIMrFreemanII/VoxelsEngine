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
        public Vector3Int size = new Vector3Int(3, 3, 3);

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

        private void OnValidate()
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
                        if (temp.active)
                            savedVoxels.Add(curPos, temp);
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

        public bool GetNeighbor(Vector3Int coordinate, Direction dir)
        {
            Vector3Int offsetToCheck = _offsets[(int) dir];
            Vector3Int neighborCoord = coordinate + offsetToCheck;

            if (
                neighborCoord.x < 0 || neighborCoord.x >= Width ||
                neighborCoord.y < 0 || neighborCoord.y >= Height ||
                neighborCoord.z < 0 || neighborCoord.z >= Depth
            )
            {
                return false;
            }

            return GetCell(neighborCoord.x, neighborCoord.y, neighborCoord.z).active;
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