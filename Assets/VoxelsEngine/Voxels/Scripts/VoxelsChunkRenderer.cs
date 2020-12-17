using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using VoxelsEngine.Extensions;
using VoxelsEngine.Utils;
using VoxelsEngine.Voxels.Jobs;
using VoxelsEngine.Voxels.Scripts.CustomValues;

namespace VoxelsEngine.Voxels.Scripts
{
    public enum EditVoxelType
    {
        Add,
        Remove,
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshCollider))]
    public class VoxelsChunkRenderer : SerializedMonoBehaviour
    {
        private static Material _defaultVoxelMaterial;

        public static Material DefaultVoxelMaterial
        {
            get
            {
                if (_defaultVoxelMaterial)
                {
                    return _defaultVoxelMaterial;
                }

#if UNITY_EDITOR
                InitDefaultMaterial();  
#endif
                
                return _defaultVoxelMaterial;
            }
        }

        [HorizontalGroup("VoxelsChunk")]
        public HandleVoxelsChunkChange sharedVoxelsChunk = new HandleVoxelsChunkChange {Value = null};

        [HideInInspector] public VoxelsChunk copiedVoxelsChunk;

        public VoxelsChunk GetVoxelsChunk()
        {
            return Application.IsPlaying(this) ? copiedVoxelsChunk : sharedVoxelsChunk.Value;
        }
#if UNITY_EDITOR
        [Button]
        [HorizontalGroup("VoxelsChunk")]
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
                temp.voxelsSubMeshes = new List<VoxelsSubMesh> {new VoxelsSubMesh {material = DefaultVoxelMaterial}};
                temp.SetSelectedVoxelsSubMeshIndex(0);

                sharedVoxelsChunk.Value = temp;
                //---------------------------------------------

                AssetDatabase.CreateAsset(sharedVoxelsChunk.Value, path);
                AssetDatabase.SaveAssets();
            }
        }
#endif

        public void HandleSharedVoxelsChunkChange(VoxelsChunk value)
        {
            if (value)
            {
                size.Value = value.Size;
                scale.Value = value.scale;
            }

            UpdateChunkBounds(size.Value);
            UpdateSubMeshesChunk();
            InitBoxColliders();
#if UNITY_EDITOR
            VoxelsChunkEditorWindow.HandleDrawChunkEditor();
#endif
        }

        [HideInInspector] public VoxelsChunkColliderController colliderController;

        [HideInInspector] public GameObject chunkBoundsGO;
        [HideInInspector] public MeshCollider chunkBoundsMeshCollider;

        private void UpdateChunkBounds(Vector3Int size)
        {
            chunkBoundsGO.transform.localScale = size.ToFloat() * scale.Value;
        }

        [DelayedProperty] public HandleVoxelsChunkSizeChange size = new HandleVoxelsChunkSizeChange
            {Value = new Vector3Int(3, 3, 3)};

        private void HandleSizeChange(Vector3Int value)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            if (voxelsChunk)
            {
                voxelsChunk.Size = value;
                UpdateChunkBounds(value);
                ClearChunk();
                UpdateSubMeshesChunk();
            }
        }

        [Delayed] public HandleScaleChange scale = new HandleScaleChange {Value = 1f};
        [SerializeField, HideInInspector] private float adjustedScale;

        public void HandleScaleChange(float value)
        {
            adjustedScale = value * 0.5f;
            sharedVoxelsChunk.Value.scale = value;
            UpdateChunkBounds(size.Value);
            UpdateSubMeshesChunk();
            // ResizeActiveColliders();
        }
#if UNITY_EDITOR
        [Button]
        private void Edit()
        {
            VoxelsChunkEditorWindow.Open();
        }
#endif

        [SerializeField, HideInInspector] private MeshRenderer _meshRenderer;

        public MeshRenderer MeshRenderer =>
            _meshRenderer ? _meshRenderer : _meshRenderer = GetComponent<MeshRenderer>();

        [SerializeField, HideInInspector] private MeshFilter _meshFilter;
        public MeshFilter MeshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();
        [SerializeField, HideInInspector] private MeshCollider _meshCollider;

        public MeshCollider MeshCollider =>
            _meshCollider ? _meshCollider : _meshCollider = GetComponent<MeshCollider>();

        [SerializeField, HideInInspector] private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();

        [SerializeField, HideInInspector] private Mesh oldMesh;
        [SerializeField, HideInInspector] private Mesh meshCopy;

        public Mesh Mesh => meshCopy;

#if UNITY_EDITOR
        private static void InitDefaultMaterial()
        {
            if (_defaultVoxelMaterial == null)
            {
                _defaultVoxelMaterial =
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/VoxelsEngine/Voxels/Materials/DefaultVoxel.mat");
            }
        }
#endif
        
        private void OnEnable()
        {
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

        private void OnDestroy()
        {
            if (Application.IsPlaying(this))
            {
                if (copiedVoxelsChunk)
                {
                    Destroy(copiedVoxelsChunk);
                }
            }
        }

        public Vector3 GetVoxelWorldPos(Vector3 worldPos, Vector3 normal, EditVoxelType type)
        {
            switch (type)
            {
                case EditVoxelType.Add:
                {
                    return worldPos + normal * Constants.LittleOffset;
                }
                default:
                {
                    return worldPos - normal * Constants.LittleOffset;
                }
            }
        }

        private void Awake()
        {
            if (!Application.IsPlaying(this))
            {
                InitVariables();
                InitMesh();
                InitChunkBounds();
#if UNITY_EDITOR
                InitVoxelsChunkEditorWindow();
#endif
                InitColliderController();
            }
            else
            {
                InitVoxelsChunk();
            }

            InitComponents();
        }

        private void InitVoxelsChunk()
        {
            copiedVoxelsChunk = Instantiate(sharedVoxelsChunk.Value);
        }

        private void InitComponents()
        {
            if (Application.IsPlaying(this))
            {
                MeshCollider.enabled = false;
                chunkBoundsGO.SetActive(false);
            }
            else
            {
                Rigidbody.isKinematic = true;
            }
        }

        private void InitMesh()
        {
            oldMesh = MeshFilter.sharedMesh ? MeshFilter.sharedMesh : new Mesh();

            meshCopy = new Mesh();
            meshCopy.name = $"{name} clone";
            meshCopy.indexFormat = IndexFormat.UInt32;
            meshCopy.vertices = oldMesh.vertices;
            meshCopy.triangles = oldMesh.triangles;
            meshCopy.normals = oldMesh.normals;
            meshCopy.uv = oldMesh.uv;

            MeshCollider meshCollider = MeshCollider;
            meshCollider.sharedMesh = meshCopy;

            MeshFilter.mesh = meshCopy;
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

        private void InitColliderController()
        {
            if (!colliderController)
            {
                GameObject colliderGO = new GameObject();
                colliderGO.name = "Collider Controller";
                colliderController = colliderGO.AddComponent<VoxelsChunkColliderController>();
                colliderController.voxelsChunkRenderer = this;
                colliderController.transform.SetParent(transform);
                colliderController.transform.localPosition = Vector3.zero;
            }
        }

        private void InitVariables()
        {
            adjustedScale = scale.Value * 0.5f;
        }


#if UNITY_EDITOR
        private void InitVoxelsChunkEditorWindow()
        {
            if (EditorWindow.HasOpenInstances<VoxelsChunkEditorWindow>())
                VoxelsChunkEditorWindow.HandleVoxelsChunkRenderer();
        }
#endif

        [ContextMenu("Clear chunk")]
        public void ClearChunk()
        {
            if (sharedVoxelsChunk.Value)
            {
                sharedVoxelsChunk.Value.Clear();
                colliderController.RefreshColliders();
                UpdateSubMeshesChunk();
            }
        }

        [ContextMenu("Resize chunk")]
        public void ResizeChunk()
        {
            sharedVoxelsChunk.Value.Resize();
        }

        public bool useJobs;
        [ContextMenu("Fill voxels")]
        public void FillVoxelsData()
        {
            ClearChunk();
            
            VoxelsChunk voxelsChunk = GetVoxelsChunk();

            TimeUtil.GetExecutionTime("fill voxels data", () =>
            {
                if (useJobs)
                {
                    NativeArray<VoxelData> voxelsData = new NativeArray<VoxelData>(voxelsChunk.data, Allocator.TempJob);
                    FillVoxelsDataJob fillVoxelDataJob = new FillVoxelsDataJob
                    {
                        voxelsData = voxelsData,
                        subMeshIndex = 0,
                    };
                    JobHandle fillVoxelDataJobHandle = fillVoxelDataJob.Schedule(voxelsData.Length, 128);
                    
                    NativeList<int> activeVoxelsIndices = new NativeList<int>(Allocator.TempJob);
                    FilterVisibleVoxelsJob filterVisibleVoxelsJob = new FilterVisibleVoxelsJob
                    {
                        voxelsData = voxelsData,
                        chunkSize = voxelsChunk.Size.ToInt3()
                    };
                    JobHandle filterVisibleVoxelsJobHandle =
                        filterVisibleVoxelsJob.ScheduleAppend(activeVoxelsIndices, voxelsData.Length, 128, fillVoxelDataJobHandle);
                    
                    filterVisibleVoxelsJobHandle.Complete();
                    
                    voxelsChunk.data = voxelsData.ToArray();
                    
                    voxelsChunk.activeVoxelsIndices.Clear();
                    voxelsChunk.activeVoxelsIndices.AddRange(activeVoxelsIndices.ToArray());

                    voxelsChunk.GenerateMeshDataJob(activeVoxelsIndices);

                    voxelsData.Dispose();
                    activeVoxelsIndices.Dispose();
                }
                else
                {
                    voxelsChunk.IterateMatrix3d((x, y, z) =>
                    {
                        AppendVoxel(new Vector3Int(x, y, z), false);
                    });
                }
            });

            UpdateSubMeshesChunk();
            // InitBoxColliders();
        }

        public Vector3Int GetPosInArr(Vector3 worldPos)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            return voxelsChunk.GetPosInArr(worldPos, transform);
        }

        [ContextMenu("Update sub-meshes")]
        public void UpdateSubMeshesChunk()
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();

            if (!voxelsChunk)
            {
                Debug.LogWarning("There is no voxels chunk to render!");
                Mesh.Clear();
                return;
            }

            // TimeUtil.GetExecutionTime(voxelsChunk.GenerateMeshDataFast);
            UpdateSubMeshes(voxelsChunk);

            if (!Application.IsPlaying(this))
            {
                UpdateMaterials(voxelsChunk);
#if UNITY_EDITOR
                EditorUtility.SetDirty(voxelsChunk);
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public void AppendVoxel(Vector3Int coordinate, bool updateMesh = true)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            VoxelData voxelData = voxelsChunk.GetCell(coordinate);

            voxelsChunk.SetCell(new VoxelData
                {
                    mesh = new VoxelMeshData
                    {
                        subMeshIndex = voxelsChunk.SelectedVoxelsSubMeshIndex,
                    },
                    enabled = true,
                    visible = true,
                    durability = 0.1f,
                }
                , coordinate
            );

            if (!voxelData.enabled)
            {
                // colliderController.AddBoxCollider(coordinate);
                voxelsChunk.AddActiveVoxelCoordinate(coordinate.ToInt3());
                voxelsChunk.RecalculateNeighbors(coordinate, colliderController);
                // voxelsChunk.MakeCube(coordinate, voxelsChunk.GetSelectedVoxelsSubMesh());
                
                if (updateMesh) UpdateSubMeshesChunk();
            }
        }

        public void UpdateVoxelData(Vector3Int posInArr, VoxelData voxelData)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            voxelsChunk.SetCell(voxelData, posInArr);
        }

        public void RemoveVoxel(Vector3Int coordinate, bool updateMesh = true)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            VoxelData voxelData = voxelsChunk.GetCell(coordinate);

            if (voxelData.enabled)
            {
                // colliderController.RemoveBoxCollider(coordinate);
                voxelsChunk.RemoveActiveVoxelCoordinate(coordinate.ToInt3());
                voxelsChunk.SetCell(new VoxelData(), coordinate);
                voxelsChunk.RecalculateNeighbors(coordinate, colliderController);
                
                if (updateMesh) UpdateSubMeshesChunk();
            }
        }

        public Vector3 GetCubePosition(Vector3Int coordinate)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            Vector3 cubePos = (coordinate.ToFloat() - voxelsChunk.Size.ToFloat() * 0.5f) * scale.Value;
            Vector3 offset = Vector3.one * scale.Value * 0.5f;
            return cubePos + offset;
        }

        private void RemoveCube(Vector3Int coordinate, VoxelData voxelData, VoxelsChunk voxelsChunk, bool updateMesh)
        {
            // VoxelsSubMesh voxelsSubMesh = voxelsChunk.GetVoxelsSubMesh(voxelData.mesh.subMeshIndex);
            // voxelsSubMesh.RemoveVoxelTriangles(coordinate);
            
            if (updateMesh) UpdateSubMeshesChunk();
        }

        private void UpdateSubMeshes(VoxelsChunk voxelsChunk)
        {
            int subMeshCount = voxelsChunk.voxelsSubMeshes.Count;

            Mesh mesh = Mesh;
            mesh.Clear();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.subMeshCount = subMeshCount;
            mesh.vertices = voxelsChunk.vertices.ToArray();

            for (int i = 0; i < subMeshCount; i++)
            {
                mesh.SetTriangles(voxelsChunk.voxelsSubMeshes[i].GetTrianglesTest(), i, false);
            }
            
            mesh.RecalculateNormals();

            if (!Application.IsPlaying(this))
            {
                ReloadMeshCollider();
            }
        }

        private void ReloadMeshCollider()
        {
            MeshCollider.enabled = false;
            MeshCollider.enabled = true;
        }

        private void UpdateMaterials(VoxelsChunk voxelsChunk)
        {
            Material[] materials = voxelsChunk.voxelsSubMeshes.Select(item => item.material).ToArray();
            MeshRenderer meshRenderer = MeshRenderer;
            meshRenderer.materials = materials;
        }

        public void InitBoxColliders()
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            if (voxelsChunk)
            {
                // colliderController.InitBoxColliders(voxelsChunk.activeVoxelsCoordinates);
            }
            else
            {
                colliderController.RefreshColliders();
            }
        }

        public void ResizeActiveColliders()
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            // colliderController.ResizeBoxColliders(voxelsChunk.activeVoxelsCoordinates);
        }
    }
}