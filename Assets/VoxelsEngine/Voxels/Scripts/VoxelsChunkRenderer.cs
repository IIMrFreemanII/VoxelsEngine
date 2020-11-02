using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class VoxelsChunkRenderer : MonoBehaviour
    {
        [InlineButton("CreateAsset")]
        public VoxelsChunk voxelsChunk;

        private void CreateAsset()
        {
            Debug.Log("Create asset!");
        }

        [DelayedProperty] public Vector3Int size = new Vector3Int(3, 3, 3);
        [SerializeField, HideInInspector] private Vector3Int _size;
        private void ValidateSize()
        {
            if (_size != size)
            {
                _size = size;
                voxelsChunk.Size = size;
                UpdateChunk();
                Debug.Log("Resize");
            }
        }

        [Delayed]
        public float scale = 1f;
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
        private Mesh _mesh;
        private Mesh Mesh => _mesh ? _mesh : _mesh = MeshFilter.sharedMesh;

        private List<Vector3> _vertices;
        private List<int> _triangles;

        [Header("Debug")] public bool drawBorder;
        public Color borderColor = Color.white;
        [Space] public bool drawVolume;
        public Color volumeColor = Color.white;
        [OnValueChanged("ValidatePointsSize"), Delayed] 
        public float pointsSize = 0.02f;

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
        }
        private void ValidatePointsSize()
        {
            pointsSize = pointsSize > 0 ? pointsSize : 0;
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