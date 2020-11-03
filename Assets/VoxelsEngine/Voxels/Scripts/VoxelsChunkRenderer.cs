using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent
        (
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(VoxelsChunkDebugger)
        )
    ]
    public class VoxelsChunkRenderer : MonoBehaviour
    {
        [InlineButton("CreateAsset")]
        public VoxelsChunk voxelsChunk;
        [SerializeField, HideInInspector]
        private VoxelsChunk prevVoxelsChunk;
        private void ValidateVoxelsChunk()
        {
            if (voxelsChunk && prevVoxelsChunk != voxelsChunk)
            {
                prevVoxelsChunk = voxelsChunk;
                size = voxelsChunk.Size;
                UpdateChunk();
            }

            if (!voxelsChunk && prevVoxelsChunk)
            {
                prevVoxelsChunk = null;
                UpdateChunk();
            }
        }
        private void CreateAsset()
        {
            Debug.Log("Create asset!");
        }

        [DelayedProperty] public Vector3Int size = new Vector3Int(3, 3, 3);
        [SerializeField, HideInInspector] private Vector3Int _prevSize;

        private void ValidateSize()
        {
            if (size != _prevSize)
            {
                _prevSize = size;

                if (voxelsChunk)
                {
                    voxelsChunk.Size = size;
                    UpdateChunk();
                }
            }
        }

        [Delayed] public float scale = 1f;
        [SerializeField, HideInInspector] private float prevScale;
        [SerializeField, HideInInspector] private float adjustedScale;

        private void ValidateScale()
        {
            if (scale != prevScale)
            {
                prevScale = scale;
                adjustedScale = scale * 0.5f;

                UpdateChunk();
            }
        }

        private MeshFilter _meshFilter;
        private MeshFilter MeshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();
        [SerializeField, HideInInspector]
        private Mesh oldMesh;
        [SerializeField, HideInInspector]
        private Mesh meshCopy;

        private Mesh Mesh => meshCopy;

        private List<Vector3> _vertices;
        private List<int> _triangles;

        private void Awake()
        {
            if (!meshCopy || !oldMesh)
            {
                Debug.Log("Init mesh");
                oldMesh = MeshFilter.sharedMesh ? MeshFilter.sharedMesh : new Mesh();
            
                meshCopy = new Mesh();
                meshCopy.name = $"{name} clone";
                meshCopy.vertices = oldMesh.vertices;
                meshCopy.triangles = oldMesh.triangles;
                meshCopy.normals = oldMesh.normals;
                meshCopy.uv = oldMesh.uv;
                    
                MeshFilter.mesh = meshCopy;
            }
        }

        [ContextMenu("Clear chunk")]
        public void ClearChunk()
        {
            voxelsChunk.Clear();
            UpdateChunk();
        }

        [ContextMenu("Resize chunk")]
        public void ResizeChunk()
        {
            voxelsChunk.Resize();
        }

        public void SetSell(VoxelData voxelData, Vector3Int posInArr)
        {
            voxelsChunk.SetSell(voxelData, posInArr);
            UpdateChunk();
        }

        public VoxelData GetSell(Vector3Int posInArr)
        {
            return voxelsChunk.GetCell(posInArr);
        }

        [ContextMenu("Update chunk")]
        public void UpdateChunk()
        {
            if (!voxelsChunk)
            {
                Debug.LogWarning("There is not voxels chunk to render!");
                Mesh.Clear();
                return;
            }

            GenerateVoxelsMesh(voxelsChunk);
            UpdateMesh();
            EditorUtility.SetDirty(voxelsChunk);
            Debug.Log($"Update Chunk! {name} saved!");
        }

        private void OnValidate()
        {
            ValidateScale();
            ValidateSize();
            ValidateVoxelsChunk();
        }

        private void GenerateVoxelsMesh(VoxelsChunk data)
        {
            _vertices = new List<Vector3>();
            _triangles = new List<int>();

            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    for (int z = 0; z < data.Depth; z++)
                    {
                        VoxelData voxelData = data.GetCell(x, y, z);
                        if (voxelData.active == false) continue;

                        Vector3 cubePos = new Vector3(x, y, z) * scale;
                        Vector3 offset = Vector3.one * scale * 0.5f;

                        MakeCube(
                            adjustedScale,
                            cubePos + offset,
                            new Vector3Int(x, y, z),
                            data
                        );
                    }
                }
            }
        }

        private void MakeCube(float scale, Vector3 cubePos, Vector3Int coordinate, VoxelsChunk data)
        {
            for (int i = 0; i < 6; i++)
            {
                if (data.GetNeighbor(coordinate, (Direction) i) == false)
                    MakeFace((Direction) i, scale, cubePos);
            }
        }

        private void MakeFace(Direction dir, float scale, Vector3 facePos)
        {
            _vertices.AddRange(VoxelMeshData.FaceVertices(dir, scale, facePos));

            int vertCount = _vertices.Count;

            _triangles.Add(vertCount - 4);
            _triangles.Add(vertCount - 4 + 1);
            _triangles.Add(vertCount - 4 + 2);
            _triangles.Add(vertCount - 4);
            _triangles.Add(vertCount - 4 + 2);
            _triangles.Add(vertCount - 4 + 3);
        }

        private void UpdateMesh()
        {
            Mesh.Clear();
            Mesh.name = name;
            Mesh.vertices = _vertices.ToArray();
            Mesh.triangles = _triangles.ToArray();

            Mesh.RecalculateNormals();
        }
    }
}