using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VoxelsEngine.Extensions;
using VoxelsEngine.Utils;
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
        public static Material defaultVoxelMaterial;

        [HorizontalGroup("VoxelsChunk")]
        public HandleVoxelsChunkChange sharedVoxelsChunk = new HandleVoxelsChunkChange {Value = null};

        [HideInInspector] public VoxelsChunk voxelsChunk;

        public VoxelsChunk GetVoxelsChunk()
        {
            return Application.IsPlaying(this) ? voxelsChunk : sharedVoxelsChunk.Value;
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
                temp.voxelsSubMeshes = new List<VoxelsSubMesh> {new VoxelsSubMesh {material = defaultVoxelMaterial}};
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
            }

            UpdateChunkBounds(size.Value);
            UpdateSubMeshesChunk();

#if UNITY_EDITOR
            VoxelsChunkEditorWindow.HandleDrawChunkEditor();
#endif
        }

        [HideInInspector] public GameObject collidersGO;

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
                UpdateSubMeshesChunk();
            }
        }

        [Delayed] public HandleScaleChange scale = new HandleScaleChange {Value = 1f};
        [SerializeField, HideInInspector] private float adjustedScale;

        public void HandleScaleChange(float value)
        {
            adjustedScale = value * 0.5f;
            UpdateChunkBounds(size.Value);
            UpdateSubMeshesChunk();
        }
#if UNITY_EDITOR
        [Button]
        private void Edit()
        {
            VoxelsChunkEditorWindow.Open();
        }
#endif

        private MeshRenderer _meshRenderer;

        public MeshRenderer MeshRenderer =>
            _meshRenderer ? _meshRenderer : _meshRenderer = GetComponent<MeshRenderer>();

        private MeshFilter _meshFilter;
        public MeshFilter MeshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();
        private MeshCollider _meshCollider;

        public MeshCollider MeshCollider =>
            _meshCollider ? _meshCollider : _meshCollider = GetComponent<MeshCollider>();

        private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();

        [SerializeField, HideInInspector] private Mesh oldMesh;
        [SerializeField, HideInInspector] private Mesh meshCopy;

        public Mesh Mesh => meshCopy;

        private List<Vector3> _vertices;

#if UNITY_EDITOR
        private void InitDefaultMaterial()
        {
            if (defaultVoxelMaterial == null)
            {
                defaultVoxelMaterial =
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
                if (voxelsChunk)
                {
                    Destroy(voxelsChunk);
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
#if UNITY_EDITOR
                InitDefaultMaterial();
#endif
                InitVariables();
                InitMesh();
                InitChunkBounds();
                InitVoxelsChunkEditorWindow();
                InitBoxColliders();
            }
            else
            {
                InitVoxelsChunk();
            }
            
            InitComponents();
        }

        private void InitVoxelsChunk()
        {
            voxelsChunk = Instantiate(sharedVoxelsChunk.Value);
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
            meshCopy.vertices = oldMesh.vertices;
            meshCopy.triangles = oldMesh.triangles;
            meshCopy.normals = oldMesh.normals;
            meshCopy.uv = oldMesh.uv;
                    
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

        private void InitBoxColliders()
        {
            if (!collidersGO)
            {
                collidersGO = new GameObject();
                collidersGO.name = "Colliders";
                collidersGO.transform.SetParent(transform);
                collidersGO.transform.localPosition = Vector3.zero;
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

        [ContextMenu("Fill voxels")]
        public void FillVoxelsData()
        {
            GetVoxelsChunk().FillVoxelsData();
            UpdateSubMeshesChunk();
        }

        public Vector3Int GetPosInArr(Vector3 worldPos)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();

            float scale = this.scale.Value;
            int width = voxelsChunk.Width;
            int height = voxelsChunk.Height;
            int depth = voxelsChunk.Depth;
            Vector3 normalizedPointInLocalSpace = transform.InverseTransformPoint(worldPos) / scale;

            // we do " + _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
            // because before in VoxelChunkRenderer.GenerateVoxelMesh()
            // we subtracted " - _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
            // in order to revert value array index format
            // Vector3 rawIndexPos = pointInLocalSpace + _voxelsChunkRenderer.size.ToFloat() * 0.5f;
            Vector3 rawIndexPos = normalizedPointInLocalSpace + size.Value.ToFloat() * 0.5f;

            int x = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.x), 0, width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.y), 0, height - 1);
            int z = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.z), 0, depth - 1);

            Vector3 posInArray = new Vector3(x, y, z);

            return posInArray.ToInt();
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

            GenerateVoxelsSubMeshes(voxelsChunk);
            UpdateSubMeshes(voxelsChunk);
            UpdateMaterials(voxelsChunk);

            if (!Application.IsPlaying(this))
            {
                EditorUtility.SetDirty(voxelsChunk);
            }

            // Debug.Log($"{name} update sub meshes");
        }

        public void AddVoxel(Vector3Int posInArr, VoxelData voxelData)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();

            voxelsChunk.SetSell(voxelData, posInArr);
            UpdateSubMeshesChunk();
        }

        public void RemoveVoxel(Vector3Int posInArr)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            voxelsChunk.SetSell(new VoxelData(), posInArr);
            UpdateSubMeshesChunk();
        }

        private void GenerateVoxelsSubMeshes(VoxelsChunk data)
        {
            _vertices = new List<Vector3>();

            BoxCollider[] boxColliders = collidersGO.GetComponents<BoxCollider>();

            if (Application.IsPlaying(this))
            {
                foreach (BoxCollider boxCollider in boxColliders)
                {
                    Destroy(boxCollider);
                }
            }
            else
            {
                foreach (BoxCollider boxCollider in boxColliders)
                {
                    DestroyImmediate(boxCollider);
                }
            }


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

        private void MakeCube(float scale, Vector3 cubePos, Vector3Int coordinate, VoxelsChunk data,
            VoxelsSubMesh voxelsSubMesh)
        {
            bool isOuterCube = false;

            for (int i = 0; i < 6; i++)
            {
                if (data.GetNeighbor(coordinate, (Direction) i) == false)
                {
                    MakeFace((Direction) i, scale, cubePos, voxelsSubMesh);
                    isOuterCube = true;
                }
            }

            if (isOuterCube)
                CreateBoxCollider(cubePos, Vector3.one * this.scale.Value);
        }

        private void CreateBoxCollider(Vector3 center, Vector3 size)
        {
            BoxCollider boxCollider = collidersGO.AddComponent<BoxCollider>();
            boxCollider.center = center;
            boxCollider.size = size;
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

            if (!Application.IsPlaying(this))
            {
                MeshCollider.enabled = false;
                MeshCollider.enabled = true;
            }
        }

        private void UpdateMaterials(VoxelsChunk voxelsChunk)
        {
            Material[] materials = voxelsChunk.voxelsSubMeshes.Select(item => item.material).ToArray();
            MeshRenderer meshRenderer = MeshRenderer;
            meshRenderer.materials = materials;
        }
    }
}