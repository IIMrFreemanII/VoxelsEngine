﻿using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VoxelsEngine.Extensions;
using VoxelsEngine.Voxels.Scripts.CustomValues;

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
        public static Material defaultVoxelMaterial;

        [HorizontalGroup("VoxelsChunk")]
        public HandleVoxelsChunkChange sharedVoxelsChunk = new HandleVoxelsChunkChange {Value = null};
        [HideInInspector]
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
                VoxelsChunk temp = ScriptableObject.CreateInstance<VoxelsChunk>();
                
                // init voxels chunk asset with default material data
                temp.voxelsSubMeshes = new List<VoxelsSubMesh> { new VoxelsSubMesh { material = defaultVoxelMaterial } };
                temp.SetSelectedVoxelsSubMeshIndex(0);
                
                sharedVoxelsChunk.Value = temp;
                //---------------------------------------------
                
                AssetDatabase.CreateAsset(sharedVoxelsChunk.Value, path);
                AssetDatabase.SaveAssets();
            }
        }
        
        public void HandleSharedVoxelsChunkChange(VoxelsChunk value)
        {
            if (value)
            {
                size.Value = value.Size;
            }

            UpdateChunkBounds(size.Value);
            UpdateSubMeshesChunk();
            VoxelsChunkEditorWindow.HandleDrawChunkEditor();
        }

        [HideInInspector]
        public GameObject chunkBoundsGO;
        [HideInInspector]
        public MeshCollider chunkBoundsMeshCollider;
        private void UpdateChunkBounds(Vector3Int size)
        {
            chunkBoundsGO.transform.localScale = size.ToFloat() * scale.Value;
        }

        [DelayedProperty] public HandleVoxelsChunkSizeChange size = new HandleVoxelsChunkSizeChange { Value = new Vector3Int(3, 3, 3) };
        private void HandleSizeChange(Vector3Int value)
        {
            if (sharedVoxelsChunk.Value)
            {
                sharedVoxelsChunk.Value.Size = value;
                UpdateChunkBounds(value);
                UpdateSubMeshesChunk();
            }
        }

        [Delayed] public HandleScaleChange scale = new HandleScaleChange { Value = 1f };
        [SerializeField, HideInInspector] private float adjustedScale;

        public void HandleScaleChange(float value)
        {
            adjustedScale = value * 0.5f;
            UpdateChunkBounds(size.Value);
            UpdateSubMeshesChunk();
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

        private void InitDefaultMaterial()
        {
            if (defaultVoxelMaterial == null)
            {
                defaultVoxelMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/VoxelsEngine/Voxels/Materials/DefaultVoxel.mat");
            }
        }

        private void OnEnable()
        {
            InitDefaultMaterial();
            size.onChange += HandleSizeChange;
            scale.onChange += HandleScaleChange;
            sharedVoxelsChunk.onChange += HandleSharedVoxelsChunkChange;
        }
        private void OnDisable()
        {
            size.onChange -= HandleSizeChange;
            scale.onChange -= HandleScaleChange;
            sharedVoxelsChunk.onChange -= HandleSharedVoxelsChunkChange;
        }

        private void Awake()
        {
            InitVariables();
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

        private void InitVariables()
        {
            adjustedScale = scale.Value * 0.5f;
        }

        private void InitVoxelsChunkEditorWindow()
        {
            if (EditorWindow.HasOpenInstances<VoxelsChunkEditorWindow>())
                VoxelsChunkEditorWindow.HandleVoxelsChunkRenderer();
        }

        [ContextMenu("Clear chunk")]
        public void ClearChunk()
        {
            sharedVoxelsChunk.Value.Clear();
            UpdateSubMeshesChunk();
        }

        [ContextMenu("Resize chunk")]
        public void ResizeChunk()
        {
            sharedVoxelsChunk.Value.Resize();
        }

        [ContextMenu("Update sub-meshes")]
        public void UpdateSubMeshesChunk()
        {
            if (!sharedVoxelsChunk.Value)
            {
                Debug.LogWarning("There is no voxels chunk to render!");
                Mesh.Clear();
                return;
            }

            GenerateVoxelsSubMeshes(sharedVoxelsChunk.Value);
            UpdateSubMeshes(sharedVoxelsChunk.Value);
            UpdateMaterials(sharedVoxelsChunk.Value);
            EditorUtility.SetDirty(sharedVoxelsChunk.Value);
        }

        private void GenerateVoxelsSubMeshes(VoxelsChunk data)
        {
            _vertices = new List<Vector3>();

            for (int i = 0; i < data.voxelsSubMeshes.Count; i++)
            {
                data.voxelsSubMeshes[i].triangles.Clear();
            }

            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    for (int z = 0; z < data.Depth; z++)
                    {
                        VoxelData voxelData = data.GetCell(x, y, z);
                        if (voxelData.active == false) continue;
                        
                        VoxelsSubMesh voxelsSubMesh = data.voxelsSubMeshes[voxelData.subMeshIndex];
                        Vector3 cubePos = (new Vector3(x, y, z) - data.Size.ToFloat() * 0.5f) * scale.Value;
                        Vector3 offset = Vector3.one * scale.Value * 0.5f;

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
        
        private void MakeCube(float scale, Vector3 cubePos, Vector3Int coordinate, VoxelsChunk data, VoxelsSubMesh voxelsSubMesh)
        {
            for (int i = 0; i < 6; i++)
            {
                if (data.GetNeighbor(coordinate, (Direction) i) == false)
                    MakeFace((Direction) i, scale, cubePos, voxelsSubMesh);
            }
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
        
        private void UpdateSubMeshes(VoxelsChunk voxelsChunk)
        {
            int subMeshCount = voxelsChunk.voxelsSubMeshes.Count;

            Undo.RecordObject(Mesh, "Set mesh data of voxels chunk renderer");
            Mesh mesh = Mesh;
            mesh.Clear();
            mesh.subMeshCount = subMeshCount;
            mesh.name = name;
            mesh.vertices = _vertices.ToArray();

            for (int i = 0; i < subMeshCount; i++)
            {
                mesh.SetTriangles(voxelsChunk.voxelsSubMeshes[i].triangles, i, false);
            }

            mesh.RecalculateNormals();

            MeshCollider.enabled = false;
            MeshCollider.enabled = true;
        }

        private void UpdateMaterials(VoxelsChunk voxelsChunk)
        {
            Material[] materials = voxelsChunk.voxelsSubMeshes.Select(item => item.material).ToArray();
            MeshRenderer meshRenderer = MeshRenderer;
            Undo.RecordObject(meshRenderer, "Set mesh renderer materials");
            meshRenderer.materials = materials;
        }
    }
}