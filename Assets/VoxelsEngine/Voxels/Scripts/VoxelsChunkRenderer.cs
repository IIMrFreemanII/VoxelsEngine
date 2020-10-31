using System.Collections.Generic;
using UnityEngine;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class VoxelsChunkRenderer : MonoBehaviour
    {
        public VoxelsChunk voxelsChunk;
        public float scale = 1f;

        private float _adjustedScale;

        private Mesh _mesh;
        private List<Vector3> _vertices;
        private List<int> _triangles;

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _adjustedScale = scale * 0.5f;
        }

        private void OnValidate()
        {
            _adjustedScale = scale * 0.5f;
        }

        private void Start()
        {
            if (!voxelsChunk)
            {
                Debug.LogWarning("There is not voxels chunk to render!");
                return;
            }

            GenerateVoxelsMesh(voxelsChunk);
            UpdateMesh();
        }

        public bool TestGizmo;
        private void OnDrawGizmosSelected()
        {
            if (!voxelsChunk) return;

            Gizmos.color = Color.white;

            if (TestGizmo)
            {
                GizmosUtils.DrawWireRect(
                    transform.position,
                    new Vector3(voxelsChunk.Width, voxelsChunk.Height, voxelsChunk.Depth) * scale
                );
            }
            else
            {
                for (int x = 0; x < voxelsChunk.Width; x++)
                {
                    for (int y = 0; y < voxelsChunk.Height; y++)
                    {
                        for (int z = 0; z < voxelsChunk.Depth; z++)
                        {
                            Vector3 cubePos = new Vector3(x, y, z) * scale;
                            Vector3 offset = Vector3.one * scale * 0.5f;

                            Gizmos.DrawWireCube(cubePos + offset, Vector3.one * scale);
                        }
                    }
                }
            }
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
                        if (data.GetCell(x, y, z) == null) continue;

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
                if (data.GetNeighbor(coordinate, (Direction) i) == null)
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
            _mesh.Clear();

            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = _triangles.ToArray();

            _mesh.RecalculateNormals();
        }
    }
}