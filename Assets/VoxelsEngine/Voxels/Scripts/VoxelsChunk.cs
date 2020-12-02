using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [CreateAssetMenu(fileName = "Voxels Chunk", menuName = "Voxels Engine/Voxels Chunk")]
    public class VoxelsChunk : SerializedScriptableObject
    {
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

        [SerializeField] private VoxelData[] data;

        private VoxelData[] Data
        {
            get => data ?? (data = new VoxelData[Size.x * Size.y * Size.z]);
            set => data = value;
        }

        public List<Vector3Int> activeVoxelsCoordinates = new List<Vector3Int>();
        public void AddActiveVoxelCoordinate(Vector3Int coordinate)
        {
            activeVoxelsCoordinates.Add(coordinate);
        }
        public bool RemoveActiveVoxelCoordinate(Vector3Int coordinate)
        {
            return activeVoxelsCoordinates.Remove(coordinate);
        }
        
        public List<Vector3> vertices = new List<Vector3>();

        public List<VoxelsSubMesh> voxelsSubMeshes = new List<VoxelsSubMesh>();
        [SerializeField, HideInInspector] private int selectedVoxelsSubMeshIndex;
        public int SelectedVoxelsSubMeshIndex => selectedVoxelsSubMeshIndex;

        public VoxelsSubMesh GetSelectedVoxelsSubMesh()
        {
            return voxelsSubMeshes[selectedVoxelsSubMeshIndex];
        }

        public void SetSelectedVoxelsSubMeshIndex(int index)
        {
            selectedVoxelsSubMeshIndex = index;
        }

        public int GetVoxelsSubMeshIndex(VoxelsSubMesh voxelsSubMesh)
        {
            return voxelsSubMeshes.FindIndex(item => item == voxelsSubMesh);
        }

        public VoxelsSubMesh GetVoxelsSubMesh(int index)
        {
            return voxelsSubMeshes[index];
        }

        public void RemoveVoxelsSubMesh(int index)
        {
            voxelsSubMeshes.RemoveAt(index);

            for (int i = 0; i < data.Length; i++)
            {
                VoxelData voxelData = data[i];

                if (voxelData.mesh.subMeshIndex == index)
                {
                    voxelData.mesh.subMeshIndex = 0;
                    voxelData.enabled = false;

                    data[i] = voxelData;
                }
                else
                {
                    if (voxelData.mesh.subMeshIndex > index)
                    {
                        voxelData.mesh.subMeshIndex = voxelData.mesh.subMeshIndex - 1;
                        data[i] = voxelData;
                    }
                }
            }
        }
        
        private void OnValidate()
        {
            if (Width <= 0 || Height <= 0 || Depth <= 0)
            {
                Debug.Log($"Invalid size in {name}!");
            }
        }

        public int From3DTo1DIndex(int x, int y, int z)
        {
            return (x * Height * Depth) + (y * Depth + z);
        }

        public int From3DTo1DIndex(Vector3Int position)
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
            activeVoxelsCoordinates.Clear();
            vertices.Clear();

            voxelsSubMeshes.Clear();
            voxelsSubMeshes.Add(new VoxelsSubMesh {material = VoxelsChunkRenderer.DefaultVoxelMaterial});
            SetSelectedVoxelsSubMeshIndex(0);
        }

        public void Resize()
        {
            Data = new VoxelData[Size.x * Size.y * Size.z];

            // find all set values
            Dictionary<int, VoxelData> savedVoxels = new Dictionary<int, VoxelData>();

            for (int i = 0; i < Data.Length; i++)
            {
                VoxelData temp = Data[i];
                if (temp.enabled)
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

        public bool CheckOutOfRange(Vector3Int coordinate)
        {
            if (
                coordinate.x < 0 || coordinate.x >= Width ||
                coordinate.y < 0 || coordinate.y >= Height ||
                coordinate.z < 0 || coordinate.z >= Depth
            )
            {
                return true;
            }

            return false;
        }
        public bool GetNeighbor(Vector3Int coordinate, Direction dir)
        {
            Vector3Int offsetToCheck = PossibleVoxelOffsets[(int) dir];
            Vector3Int neighborCoord = coordinate + offsetToCheck;

            if (CheckOutOfRange(neighborCoord))
            {
                return false;
            }

            return GetCell(neighborCoord).enabled;
        }

        public List<Vector3Int> GetAllActiveNeighbors(Vector3Int coordinate)
        {
            List<Vector3Int> activeNeighborsCoordinates = new List<Vector3Int>();
            
            for (int i = 0; i < VoxelMeshData.directions.Length; i++)
            {
                Direction direction = VoxelMeshData.directions[i];
                Vector3Int offsetToCheck = PossibleVoxelOffsets[(int) direction];
                Vector3Int neighborCoord = coordinate + offsetToCheck;

                if (!CheckOutOfRange(neighborCoord))
                {
                    bool activeNeighbor = GetCell(neighborCoord).enabled;
                    if (activeNeighbor)
                    {
                        activeNeighborsCoordinates.Add(neighborCoord);
                    }
                }
            }

            return activeNeighborsCoordinates;
        }

        public bool IsVoxelVisible(Vector3Int coordinate)
        {
            List<Vector3Int> activeNeighbors = GetAllActiveNeighbors(coordinate);

            return activeNeighbors.Count != VoxelMeshData.directions.Length;
        }

        public void RecalculateNeighbors(Vector3Int coordinate, VoxelsChunkColliderController colliderController)
        {
            List<Vector3Int> activeNeighbors = GetAllActiveNeighbors(coordinate);

            for (int i = 0; i < activeNeighbors.Count; i++)
            {
                Vector3Int neighborCoordinate = activeNeighbors[i];
                bool isNeighborVisible = IsVoxelVisible(neighborCoordinate);
                VoxelData neighborVoxelData = GetCell(neighborCoordinate);
                if (!isNeighborVisible)
                {
                    neighborVoxelData.visible = false;
                    RemoveActiveVoxelCoordinate(neighborCoordinate);
                    colliderController.RemoveBoxCollider(neighborCoordinate);
                    // Debug.Log("Hide invisible voxel");
                }
                else
                {
                    if (!neighborVoxelData.visible)
                    {
                        neighborVoxelData.visible = true;
                        AddActiveVoxelCoordinate(neighborCoordinate);
                        
                        colliderController.AddBoxCollider(neighborCoordinate);
                        // Debug.Log("Show invisible voxel");
                    }
                }

                SetSell(neighborVoxelData, neighborCoordinate);
            }
        }

        public static readonly Vector3Int[] PossibleVoxelOffsets =
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