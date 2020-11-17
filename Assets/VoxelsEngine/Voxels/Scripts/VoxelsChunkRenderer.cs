using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VoxelsEngine.Extensions;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent
        (
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(MeshCollider)
        )
    ]
    public class VoxelsChunkRenderer : SerializedMonoBehaviour
    {
        [HorizontalGroup("VoxelsChunk")]
        public VoxelsChunk voxelsChunk;
        [Button][HorizontalGroup("VoxelsChunk")]
        private void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save VoxelsChunk as asset",
                "voxel",
                "asset",
                "Enter a file name to save the voxels chunk to"
            );

            if (path.Length != 0)
            {
                voxelsChunk = ScriptableObject.CreateInstance<VoxelsChunk>();
                
                // init voxels chunk asset with default material data
                Material defaultVoxelMaterial =
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/VoxelsEngine/Voxels/Materials/DefaultVoxel.mat");
                voxelsChunk.voxelsSubMeshes = new List<VoxelsSubMesh>
                {
                    new VoxelsSubMesh
                    {
                        material = defaultVoxelMaterial
                    }
                };
                voxelsChunk.selectedVoxelsSubMesh = voxelsChunk.voxelsSubMeshes[0];
                voxelsChunk.MapMaterialToSubMesh();
                //---------------------------------------------
                
                AssetDatabase.CreateAsset(voxelsChunk, path);
                AssetDatabase.SaveAssets();

                // order matter
                ValidateVoxelsChunk();
                ValidateSize();
                //---------------------

                Debug.Log(path);
            }
        }
        [SerializeField, HideInInspector]
        private VoxelsChunk prevVoxelsChunk;
        private void ValidateVoxelsChunk()
        {
            if (voxelsChunk && prevVoxelsChunk != voxelsChunk)
            {
                prevVoxelsChunk = voxelsChunk;
                size = voxelsChunk.Size;
                UpdateChunkBounds(size);
                UpdateChunk();
                VoxelsChunkEditorWindow.HandleDrawChunkEditor();
            }

            if (!voxelsChunk && prevVoxelsChunk)
            {
                prevVoxelsChunk = null;
                UpdateChunk();
                VoxelsChunkEditorWindow.HandleDrawChunkEditor();
            }
        }

        [HideInInspector]
        public GameObject chunkBoundsGO;
        [HideInInspector]
        public MeshCollider chunkBoundsMeshCollider;
        private void UpdateChunkBounds(Vector3Int size)
        {
            chunkBoundsGO.transform.localScale = size.ToFloat() * scale;
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
                    UpdateChunkBounds(size);
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
                UpdateChunkBounds(size);
                UpdateChunk();
            }
        }

        [Button]
        private void Edit()
        {
            VoxelsChunkEditorWindow.Open();
        }


        private MeshRenderer _meshRenderer;
        public MeshRenderer MeshRenderer =>
            _meshRenderer ? _meshRenderer : _meshRenderer = GetComponent<MeshRenderer>();
        private MeshFilter _meshFilter;
        public MeshFilter MeshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();
        private MeshCollider _meshCollider;
        public MeshCollider MeshCollider =>
            _meshCollider ? _meshCollider : _meshCollider = GetComponent<MeshCollider>();
        
        [SerializeField, HideInInspector]
        private Mesh oldMesh;
        [SerializeField, HideInInspector]
        private Mesh meshCopy;

        public Mesh Mesh => meshCopy;

        private List<Vector3> _vertices;
        private List<int> _triangles;

        private void Awake()
        {
            InitMesh();
            InitChunkBounds();
            InitVoxelsChunkEditorWindow();
        }

        private void InitMesh()
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

        private void InitChunkBounds()
        {
            if (!chunkBoundsGO)
            {
                chunkBoundsGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chunkBoundsGO.name = "ChunkBounds";
            
                MeshFilter meshFilter = chunkBoundsGO.GetComponent<MeshFilter>();
            
                Mesh chunkBoundsOldMesh = meshFilter.sharedMesh;
                Mesh chunkBoundsCopyMesh = new Mesh();
                chunkBoundsCopyMesh.name = $"{chunkBoundsGO.name} clone";
                chunkBoundsCopyMesh.vertices = chunkBoundsOldMesh.vertices;
                chunkBoundsCopyMesh.triangles = chunkBoundsOldMesh.triangles.Reverse().ToArray();
                chunkBoundsCopyMesh.normals = chunkBoundsOldMesh.normals;
                chunkBoundsCopyMesh.uv = chunkBoundsOldMesh.uv;
                meshFilter.mesh = chunkBoundsCopyMesh;
            
                DestroyImmediate(chunkBoundsGO.GetComponent<MeshRenderer>());
                DestroyImmediate(chunkBoundsGO.GetComponent<BoxCollider>());
                chunkBoundsMeshCollider = chunkBoundsGO.AddComponent<MeshCollider>();
                chunkBoundsGO.transform.SetParent(transform);
                chunkBoundsGO.transform.localPosition = Vector3.zero;
            }
        }

        private void InitVoxelsChunkEditorWindow()
        {
            if (EditorWindow.HasOpenInstances<VoxelsChunkEditorWindow>())
                VoxelsChunkEditorWindow.HandleVoxelsChunkRenderer();
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
            // UpdateChunk();
            UpdateSubMeshesChunk();
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
                Debug.LogWarning("There is no voxels chunk to render!");
                Mesh.Clear();
                return;
            }

            GenerateVoxelsMesh(voxelsChunk);
            UpdateMesh();
            Debug.Log(Mesh.subMeshCount);
            EditorUtility.SetDirty(voxelsChunk);
            // Debug.Log($"Update Chunk! {name} saved!");
        }
        
        [ContextMenu("Update sub-meshes")]
        public void UpdateSubMeshesChunk()
        {
            if (!voxelsChunk)
            {
                Debug.LogWarning("There is no voxels chunk to render!");
                Mesh.Clear();
                return;
            }

            if (voxelsChunk.selectedVoxelsSubMesh == null || voxelsChunk.selectedVoxelsSubMesh.material == null)
            {
                Debug.LogWarning("No material selected! You must choose material!");
                return;
            }

            GenerateVoxelsSubMeshes(voxelsChunk);
            UpdateSubMeshes();
            UpdateMaterials();
            Debug.Log(Mesh.subMeshCount);
            EditorUtility.SetDirty(voxelsChunk);
            // Debug.Log($"Update Chunk! {name} saved!");
        }

        private void OnValidate()
        {
            ValidateScale();
            
            // order matter
            ValidateVoxelsChunk();
            ValidateSize();
            //---------------------
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
                        
                        Vector3 cubePos = (new Vector3(x, y, z) - data.Size.ToFloat() * 0.5f) * scale;
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
        
        private void GenerateVoxelsSubMeshes(VoxelsChunk data)
        {
            _vertices = new List<Vector3>();

            // reset sub mesh triangles
            foreach (VoxelsSubMesh voxelsSubMesh in voxelsChunk.voxelsSubMeshes)
            {
                voxelsSubMesh.triangles.Clear();
            }
            // for (int i = 0; i < voxelsChunk.voxelsSubMeshes.Count; i++)
            // {
            //     voxelsChunk.voxelsSubMeshes[i].triangles.Clear();
            // }

            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    for (int z = 0; z < data.Depth; z++)
                    {
                        VoxelData voxelData = data.GetCell(x, y, z);
                        if (voxelData.active == false) continue;

                        VoxelsSubMesh voxelsSubMesh = voxelsChunk.matToSubMesh[voxelData.material];
                        Vector3 cubePos = (new Vector3(x, y, z) - data.Size.ToFloat() * 0.5f) * scale;
                        Vector3 offset = Vector3.one * scale * 0.5f;

                        MakeCube(
                            adjustedScale,
                            cubePos + offset,
                            new Vector3Int(x, y, z),
                            data,
                            voxelsSubMesh
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
        private void MakeCube(float scale, Vector3 cubePos, Vector3Int coordinate, VoxelsChunk data, VoxelsSubMesh voxelsSubMesh)
        {
            for (int i = 0; i < 6; i++)
            {
                if (data.GetNeighbor(coordinate, (Direction) i) == false)
                    MakeFace((Direction) i, scale, cubePos, voxelsSubMesh);
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
        private void MakeFace(Direction dir, float scale, Vector3 facePos, VoxelsSubMesh voxelsSubMesh)
        {
            _vertices.AddRange(VoxelMeshData.FaceVertices(dir, scale, facePos));

            int vertCount = _vertices.Count;

            voxelsSubMesh.triangles.Add(vertCount - 4);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 1);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 2);
            voxelsSubMesh.triangles.Add(vertCount - 4);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 2);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 3);
        }

        private void UpdateMesh()
        {
            Mesh mesh = Mesh;
            mesh.Clear();
            mesh.name = name;
            mesh.vertices = _vertices.ToArray();
            mesh.triangles = _triangles.ToArray();

            mesh.RecalculateNormals();

            MeshCollider.enabled = false;
            MeshCollider.enabled = true;
        }
        private void UpdateSubMeshes()
        {
            int subMeshCount = voxelsChunk.voxelsSubMeshes.Count;

            Mesh mesh = Mesh;
            mesh.Clear();
            mesh.subMeshCount = subMeshCount;
            mesh.name = name;
            mesh.vertices = _vertices.ToArray();

            for (int i = 0; i < subMeshCount; i++)
            {
                mesh.SetTriangles(voxelsChunk.voxelsSubMeshes[i].triangles.ToArray(), i, false);
            }

            mesh.RecalculateNormals();

            MeshCollider.enabled = false;
            MeshCollider.enabled = true;
        }

        public void UpdateMaterials()
        {
            Material[] materials = voxelsChunk.voxelsSubMeshes.Select(mesh => mesh.material).ToArray();
            MeshRenderer.materials = materials;
        }
    }
}