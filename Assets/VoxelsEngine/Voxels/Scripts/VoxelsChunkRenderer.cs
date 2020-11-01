using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class VoxelsChunkRenderer : MonoBehaviour
    {
        public VoxelsChunk voxelsChunk;
        [OnValueChanged("ValidateAdjustedScale")]
        public float scale = 1f;

        private float _adjustedScale;

        private MeshFilter _meshFilter;
        private MeshFilter MeshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();
        private Mesh _mesh;
        private Mesh Mesh => _mesh ? _mesh : _mesh = MeshFilter.sharedMesh;
        
        private List<Vector3> _vertices;
        private List<int> _triangles;
        
        [Header("Gizmos")] 
        public bool drawBorder;
        public Color borderColor = Color.white;
        [Space]
        public bool drawVolume;
        public Color volumeColor = Color.white;
        [OnValueChanged("ValidatePointsSize")]
        public float pointsSize = 0.02f;

        private void OnEnable()
        {
            Debug.Log("enable test");
            voxelsChunk.OnDataChange += UpdateChunk;
        }
        
        private void OnDisable()
        {
            Debug.Log("disable test");
            voxelsChunk.OnDataChange -= UpdateChunk;
        }

        private void Start()
        {
            UpdateChunk();
        }

        private void OnValidate()
        {
            UpdateChunk();
        }

        private void ValidateAdjustedScale()
        {
            _adjustedScale = scale * 0.5f;
            UpdateChunk();
        }

        private void ValidatePointsSize()
        {
            pointsSize = pointsSize > 0 ? pointsSize : 0;
        }

        public void UpdateChunk()
        {
            if (!voxelsChunk)
            {
                Debug.LogWarning("There is not voxels chunk to render!");
                return;
            }

            GenerateVoxelsMesh(voxelsChunk);
            UpdateMesh();
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
                            _adjustedScale,
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
                if (data.GetNeighbor(coordinate, (Direction)i) == false)
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

            Mesh.vertices = _vertices.ToArray();
            Mesh.triangles = _triangles.ToArray();

            Mesh.RecalculateNormals();
        }
    }
}