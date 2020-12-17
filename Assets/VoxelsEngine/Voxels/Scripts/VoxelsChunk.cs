using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelsEngine.Extensions;
using VoxelsEngine.Utils;
using VoxelsEngine.Voxels.Jobs;

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

        [HideInInspector] public float scale = 1;
        public float AdjustedScale => scale * 0.5f;

        public int TotalSize => Size.x * Size.y * Size.z;

        public int Width => Size.x;
        public int Height => Size.y;
        public int Depth => Size.z;

        // [HideInInspector] 
        public VoxelData[] data;

        private void Awake()
        {
            if (data == null)
            {
                ResizeData();
            }
        }

        public List<int> activeVoxelsIndices = new List<int>();

        public void AddActiveVoxelCoordinate(int3 coordinate)
        {
            activeVoxelsIndices.Add(From3DTo1DIndex(coordinate.ToVector3Int()));
        }

        public bool RemoveActiveVoxelCoordinate(int3 coordinate)
        {
            return activeVoxelsIndices.Remove(From3DTo1DIndex(coordinate.ToVector3Int()));
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

        public Vector3Int From1DTo3DIndex(int index)
        {
            int x = Mathf.FloorToInt(index / (Height * Depth));
            int y;

            int tempY = Mathf.FloorToInt(index / Depth);
            y = tempY % Height;

            int z = index % Depth;

            return new Vector3Int(x, y, z);
        }

        public int From3DTo1DIndex(Vector3Int position)
        {
            return From3DTo1DIndex(position.x, position.y, position.z);
        }

        public VoxelData GetCell(int x, int y, int z)
        {
            return data[From3DTo1DIndex(x, y, z)];
        }

        public VoxelData GetCell(Vector3Int posInArr)
        {
            return GetCell(posInArr.x, posInArr.y, posInArr.z);
        }

        public void SetCell(VoxelData data, int x, int y, int z)
        {
            this.data[From3DTo1DIndex(x, y, z)] = data;
        }

        public void SetCell(VoxelData data, Vector3Int position)
        {
            SetCell(data, position.x, position.y, position.z);
        }

        [ContextMenu("Clear Data")]
        public void Clear()
        {
            ResizeData();
            activeVoxelsIndices.Clear();

            vertices.Clear();
            // normals.Clear();
            // triangles.Clear();
            // InitMeshData();

            voxelsSubMeshes.Clear();
            voxelsSubMeshes.Add(new VoxelsSubMesh {material = VoxelsChunkRenderer.DefaultVoxelMaterial});
            SetSelectedVoxelsSubMeshIndex(0);
        }

        private void ResizeData()
        {
            data = new VoxelData[TotalSize];
        }

        public void Resize()
        {
            ResizeData();

            // find all set values
            Dictionary<int, VoxelData> savedVoxels = new Dictionary<int, VoxelData>();

            for (int i = 0; i < data.Length; i++)
            {
                VoxelData temp = data[i];
                if (temp.enabled)
                    savedVoxels.Add(i, temp);
            }

            foreach (var savedVoxel in savedVoxels)
            {
                if (savedVoxel.Key < data.Length)
                {
                    data[savedVoxel.Key] = savedVoxel.Value;
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

            for (int i = 0; i < VoxelMeshData.Directions.Length; i++)
            {
                Direction direction = VoxelMeshData.Directions[i];
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

            return activeNeighbors.Count != VoxelMeshData.Directions.Length;
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
                    RemoveActiveVoxelCoordinate(neighborCoordinate.ToInt3());

                    // GetVoxelsSubMesh(GetCell(neighborCoordinate).mesh.subMeshIndex);
                    // .RemoveVoxelTriangles(neighborCoordinate);

                    // colliderController.RemoveBoxCollider(neighborCoordinate);
                    // Debug.Log("Hide invisible voxel");
                }
                else
                {
                    if (!neighborVoxelData.visible)
                    {
                        neighborVoxelData.visible = true;
                        AddActiveVoxelCoordinate(neighborCoordinate.ToInt3());

                        // MakeCube(neighborCoordinate, GetVoxelsSubMesh(GetCell(neighborCoordinate).mesh.subMeshIndex));

                        // colliderController.AddBoxCollider(neighborCoordinate);
                        // Debug.Log("Show invisible voxel");
                    }
                }

                SetCell(neighborVoxelData, neighborCoordinate);
            }
        }

        public Vector3 GetCubePosition(Vector3Int coordinate)
        {
            Vector3 cubePos = (coordinate.ToFloat() - Size.ToFloat() * 0.5f) * scale;
            Vector3 offset = Vector3.one * scale * 0.5f;
            return cubePos + offset;
        }

        public void GenerateMeshDataSlow()
        {
            vertices.Clear();
            for (int i = 0; i < voxelsSubMeshes.Count; i++)
            {
                voxelsSubMeshes[i].Clear();
            }

            for (int i = 0; i < TotalSize; i++)
            {
                MakeCubeTest(i);
            }
        }

        public void GenerateMeshDataFast()
        {
            vertices.Clear();
            for (int i = 0; i < voxelsSubMeshes.Count; i++)
            {
                voxelsSubMeshes[i].Clear();
            }

            for (int i = 0; i < activeVoxelsIndices.Count; i++)
            {
                MakeCubeTest(activeVoxelsIndices[i]);
            }
        }

        public void GenerateMeshDataJob(NativeList<int> activeVoxelsIndices)
        {
            vertices.Clear();
            for (int i = 0; i < voxelsSubMeshes.Count; i++)
            {
                voxelsSubMeshes[i].Clear();
            }


            NativeArray<Vector3> vertArr =
                new NativeArray<Vector3>(activeVoxelsIndices.Length * VoxelMeshData.VerticesPerVoxel,
                    Allocator.TempJob);
            NativeArray<int> trisArr =
                new NativeArray<int>(activeVoxelsIndices.Length * VoxelMeshData.TrianglesPerVoxel,
                    Allocator.TempJob);
            NativeArray<Vector3> nativeVertices = new NativeArray<Vector3>(VoxelMeshData.Vertices, Allocator.TempJob);
            NativeArray<int> nativeFaceTriangles1D = new NativeArray<int>(VoxelMeshData.FaceTriangles1D, Allocator.TempJob);

            TimeUtil.GetExecutionTime("generate mesh data", () =>
            {
                GenerateVoxelsMeshJob generateVoxelsMeshJob = new GenerateVoxelsMeshJob
                {
                    vertices = vertArr,
                    triangles = trisArr,
                    nativeVertices = nativeVertices,
                    nativeFaceTriangles1D = nativeFaceTriangles1D,
                    activeVoxelsIndices = activeVoxelsIndices,
                    adjustedScale = AdjustedScale,
                    chunkSize = Size.ToInt3(),
                    scale = scale,
                };

                JobHandle jobHandle = generateVoxelsMeshJob.Schedule(activeVoxelsIndices.Length, 128);
                jobHandle.Complete();
            });

            Debug.Log(vertArr.Length);

            // vertices = vertArr.ToList();
            // voxelsSubMeshes[0].triangles = trisArr.ToList();

            nativeFaceTriangles1D.Dispose();
            nativeVertices.Dispose();
            vertArr.Dispose();
            trisArr.Dispose();
        }

        // public void InitMeshData()
        // {
        //     vertices = new Vector3[TotalSize * VoxelMeshData.VerticesPerVoxel];
        //     normals = new Vector3[TotalSize * VoxelMeshData.VerticesPerVoxel];
        //     triangles = new int[TotalSize * VoxelMeshData.TrianglesPerVoxel];
        //
        //     IterateMatrix3d((x, y, z) =>
        //     {
        //         Vector3 cubePos = GetCubePosition(new Vector3Int(x, y, z));
        //         int cubeIndex = From3DTo1DIndex(x, y, z);
        //         int cubeOffset = cubeIndex * VoxelMeshData.VerticesPerVoxel;
        //         int faceOffset = 4;
        //
        //         for (int i = 0; i < VoxelMeshData.directions.Length; i++)
        //         {
        //             Vector3[] temp = VoxelMeshData.FaceVertices(i, AdjustedScale, cubePos);
        //             
        //             vertices[cubeOffset + i * faceOffset] = temp[0];
        //             vertices[cubeOffset + i * faceOffset + 1] = temp[1];
        //             vertices[cubeOffset + i * faceOffset + 2] = temp[2];
        //             vertices[cubeOffset + i * faceOffset + 3] = temp[3];
        //             
        //             normals[cubeOffset + i * faceOffset] = PossibleVoxelOffsets[i];
        //             normals[cubeOffset + i * faceOffset + 1] = PossibleVoxelOffsets[i];
        //             normals[cubeOffset + i * faceOffset + 2] = PossibleVoxelOffsets[i];
        //             normals[cubeOffset + i * faceOffset + 3] = PossibleVoxelOffsets[i];
        //         }
        //     });
        // }

        public void IterateMatrix3d(Action<int, int, int> callback)
        {
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    for (int z = 0; z < Size.z; z++)
                    {
                        callback.Invoke(x, y, z);
                    }
                }
            }
        }

        public void MakeCubeTest(int index)
        {
            Vector3Int coordinate = From1DTo3DIndex(index);
            VoxelData voxelData = GetCell(coordinate);

            if (!voxelData.enabled) return;

            Vector3 cubePos = GetCubePosition(coordinate);

            for (int i = 0; i < VoxelMeshData.Directions.Length; i++)
            {
                if (!GetNeighbor(coordinate, (Direction) i))
                {
                    MakeFaceTest(i, cubePos, GetVoxelsSubMesh(voxelData.mesh.subMeshIndex));
                }
            }
        }

        public void MakeCube(Vector3Int coordinate, VoxelsSubMesh voxelsSubMesh)
        {
            int voxelIndex = From3DTo1DIndex(coordinate);
            int[] voxelTriangles = new int[VoxelMeshData.TrianglesPerVoxel];

            for (int i = 0; i < 6; i++)
            {
                int vertCount = voxelIndex * VoxelMeshData.VerticesPerVoxel + VoxelMeshData.VerticesPerFace * i;

                MakeFace(vertCount, i * 6, voxelTriangles);
            }

            // voxelsSubMesh.AddVoxelTriangles(coordinate, voxelTriangles);
        }

        private void MakeFace(int vertCount, int posOffset, int[] triangles)
        {
            triangles[posOffset] = vertCount;
            triangles[posOffset + 1] = vertCount + 1;
            triangles[posOffset + 2] = vertCount + 2;
            triangles[posOffset + 3] = vertCount;
            triangles[posOffset + 4] = vertCount + 2;
            triangles[posOffset + 5] = vertCount + 3;
        }

        private void MakeFaceTest(int dir, Vector3 position, VoxelsSubMesh voxelsSubMesh)
        {
            int vertCount = vertices.Count;

            vertices.AddRange(VoxelMeshData.FaceVertices(dir, AdjustedScale, position));

            voxelsSubMesh.triangles.Add(vertCount);
            voxelsSubMesh.triangles.Add(vertCount + 1);
            voxelsSubMesh.triangles.Add(vertCount + 2);
            voxelsSubMesh.triangles.Add(vertCount);
            voxelsSubMesh.triangles.Add(vertCount + 2);
            voxelsSubMesh.triangles.Add(vertCount + 3);
        }

        public Vector3Int GetPosInArr(Vector3 worldPos, Transform transform)
        {
            Vector3 normalizedPointInLocalSpace = transform.InverseTransformPoint(worldPos) / scale;

            // we do " + _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
            // because before in VoxelChunkRenderer.GenerateVoxelMesh()
            // we subtracted " - _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
            // in order to revert value array index format
            // Vector3 rawIndexPos = pointInLocalSpace + _voxelsChunkRenderer.size.ToFloat() * 0.5f;
            Vector3 rawIndexPos = normalizedPointInLocalSpace + Size.ToFloat() * 0.5f;

            int x = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.x), 0, Width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.y), 0, Height - 1);
            int z = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.z), 0, Depth - 1);

            Vector3 posInArray = new Vector3(x, y, z);

            return posInArray.ToInt();
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

        public static readonly int3[] PossibleVoxelOffsetsInt3 =
        {
            new int3(0, 0, 1),
            new int3(1, 0, 0),
            new int3(0, 0, -1),
            new int3(-1, 0, 0),
            new int3(0, 1, 0),
            new int3(0, -1, 0),
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