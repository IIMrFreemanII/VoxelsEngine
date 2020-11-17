using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [CreateAssetMenu(fileName = "Voxels Chunk", menuName = "Voxels Engine/Voxels Chunk")]
    public class VoxelsChunk : SerializedScriptableObject
    {
        public List<VoxelsSubMesh> voxelsSubMeshes;
        
        public VoxelsSubMesh selectedVoxelsSubMesh;
        [OdinSerialize]
        public Dictionary<Material, VoxelsSubMesh> matToSubMesh;
        public void MapMaterialToSubMesh()
        {
            matToSubMesh = new Dictionary<Material, VoxelsSubMesh>();
            
            foreach (VoxelsSubMesh voxelsSubMesh in voxelsSubMeshes)
            {
                Material material = voxelsSubMesh.material;
                if (material)
                {
                    matToSubMesh.Add(material, voxelsSubMesh);
                }
            }
        }
        
        [SerializeField, HideInInspector] private Vector3Int _size = new Vector3Int(3, 3, 3);

        public Vector3Int Size
        {
            get => _size;
            set
            {
                _size.x = value.x >= 1 ? value.x : 1;
                _size.y = value.y >= 1 ? value.y : 1;
                _size.z = value.z >= 1 ? value.z : 1;

                Resize();
            }
        }

        public int Width => Size.x;
        public int Height => Size.y;
        public int Depth => Size.z;

        [SerializeField, HideInInspector] private VoxelData[] data;

        private VoxelData[] Data
        {
            get => data ?? (data = new VoxelData[Size.x * Size.y * Size.z]);
            set => data = value;
        }

        private void OnValidate()
        {
            if (Width <= 0 || Height <= 0 || Depth <= 0)
            {
                Debug.Log($"Invalid size in {name}!");
            }
        }

        private int From3DTo1DIndex(int x, int y, int z)
        {
            return (x * Height * Depth) + (y * Depth + z);
        }
        private int From3DTo1DIndex(Vector3Int position)
        {
            return From3DTo1DIndex(position.x, position.y, position.z);
        }

        public VoxelData GetCell(int x, int y, int z)
        {
            return Data[From3DTo1DIndex(x, y, z)];
        }
        public VoxelData GetCell(Vector3Int posInArr)
        {
            return GetCell(posInArr.x, posInArr.y, posInArr.z);
        }
        public void SetSell(VoxelData data, int x, int y, int z)
        {
            Data[From3DTo1DIndex(x, y, z)] = data;
        }
        public void SetSell(VoxelData data, Vector3Int position)
        {
            SetSell(data, position.x, position.y, position.z);
        }

        public void Clear()
        {
            Data = new VoxelData[Size.x * Size.y * Size.z];
        }

        public void Resize()
        {
            Data = new VoxelData[Size.x * Size.y * Size.z];

            // find all set values
            Dictionary<int, VoxelData> savedVoxels = new Dictionary<int, VoxelData>();
            
            for (int i = 0; i < Data.Length; i++)
            {
                VoxelData temp = Data[i];
                if (temp.active)
                    savedVoxels.Add(i, temp);
            }
            
            Data = new VoxelData[Size.x * Size.y * Size.z];
            
            foreach (var savedVoxel in savedVoxels)
            {
                if (savedVoxel.Key < Data.Length)
                {
                    Data[savedVoxel.Key] = savedVoxel.Value;
                }
            }
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