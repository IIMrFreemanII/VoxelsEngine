using System;
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

        // [HideInInspector]
        public List<Vector3> _vertices;

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
            if (!colliderController)
            {
                GameObject colliderGO = new GameObject();
                colliderGO.name = "Colliders";
                colliderController = colliderGO.AddComponent<VoxelsChunkColliderController>();
                colliderController.transform.SetParent(transform);
                colliderController.transform.localPosition = Vector3.zero;
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
            AddVoxelWithoutUpdate(posInArr, voxelData);
            UpdateSubMeshesChunk();
        }

        public void AddVoxelWithoutUpdate(Vector3Int posInArr, VoxelData voxelData)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            voxelsChunk.SetSell(voxelData, posInArr);
        }

        public void RemoveVoxel(Vector3Int posInArr)
        {
            RemoveVoxelWithoutUpdate(posInArr);
            UpdateSubMeshesChunk();
        }

        public void RemoveVoxelWithoutUpdate(Vector3Int posInArr)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            voxelsChunk.SetSell(new VoxelData(), posInArr);
        }

        private void GenerateVoxelsSubMeshes(VoxelsChunk data)
        {
            _vertices = new List<Vector3>();
            colliderController.RefreshColliders();

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
                        Vector3Int coordinate = new Vector3Int(x, y, z);
                        VoxelData voxelData = data.GetCell(coordinate);
                        if (voxelData.active == false) continue;

                        VoxelsSubMesh voxelsSubMesh = data.voxelsSubMeshes[voxelData.subMeshIndex];
                        Vector3 cubePos = GetCubePosition(coordinate);

                        MakeCube(
                            adjustedScale,
                            cubePos,
                            coordinate,
                            data,
                            voxelsSubMesh
                        );
                    }
                }
            }
        }

        public Vector3 GetCubePosition(Vector3Int coordinate)
        {
            VoxelsChunk voxelsChunk = GetVoxelsChunk();
            Vector3 cubePos = (coordinate.ToFloat() - voxelsChunk.Size.ToFloat() * 0.5f) * scale.Value;
            Vector3 offset = Vector3.one * scale.Value * 0.5f;
            return cubePos + offset;
        }

        private void MakeCube(
            float adjustedScale,
            Vector3 cubePos,
            Vector3Int coordinate,
            VoxelsChunk data,
            VoxelsSubMesh voxelsSubMesh
        )
        {
            // bool isOuterCube = false;

            for (int i = 0; i < 6; i++)
            {
                if (data.GetNeighbor(coordinate, (Direction) i) == false)
                {
                    MakeFace((Direction) i, adjustedScale, cubePos, voxelsSubMesh);
                    // isOuterCube = true;
                }
            }

            //
            // if (isOuterCube)
            //     CreateBoxCollider(cubePos, Vector3.one * this.scale.Value, coordinate);
        }

        public void CreateBoxCollider(Vector3 center, Vector3 size, Vector3Int coordinate)
        {
            colliderController.AddBoxCollider(center, size, coordinate);
        }

        public void RemoveBoxCollider()
        {
        }

        private void MakeFace(Direction dir, float adjustedScale, Vector3 facePos, VoxelsSubMesh voxelsSubMesh)
        {
            _vertices.AddRange(VoxelMeshData.FaceVertices(dir, adjustedScale, facePos));

            int vertCount = _vertices.Count;

            voxelsSubMesh.triangles.Add(vertCount - 4);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 1);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 2);
            voxelsSubMesh.triangles.Add(vertCount - 4);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 2);
            voxelsSubMesh.triangles.Add(vertCount - 4 + 3);
        }

        public void AppendVoxel(Vector3Int coordinate)
        {
            var (vertexData, trianglesData) = 
                GetCubeMeshData(adjustedScale, GetCubePosition(coordinate), coordinate, GetVoxelsChunk());

            cubeVerticesData = vertexData;
            cubeTrianglesData = trianglesData;

            bool isOk = true;

            for (int i = 0; i < vertexData.Length; i++)
            {
                VertexData vertData = vertexData[i];
                
                bool temp = _vertices[vertData.index] == vertData.value;

                if (!temp) isOk = false;
            }

            if (isOk)
            {
                Debug.Log("Ok");
            }
            else
            {
                Debug.Log("Bad");
            }
        }

        public VertexData[] cubeVerticesData;
        public TriangleData[] cubeTrianglesData;
        
        [Serializable]
        public struct VertexData
        {
            public int index;
            public Vector3 value;
        }
        [Serializable]
        public struct TriangleData
        {
            public int index;
            public int value;
        }

        private (VertexData[] cubeVerticesData, TriangleData[] cubeTrianglesData) GetCubeMeshData
        (
            float scale,
            Vector3 cubePos,
            Vector3Int coordinate,
            VoxelsChunk data
        )
        {
            List<Vector3> cubeVertices = new List<Vector3>();
            List<int> cubeTriangles = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                if (data.GetNeighbor(coordinate, (Direction) i) == false)
                {
                    var (vertices, triangles) = GetFaceMeshData((Direction) i, scale, cubePos, cubeVertices);
                    cubeVertices.AddRange(vertices);
                    cubeTriangles.AddRange(triangles);
                }
            }

            int cubeIndex = data.From3DTo1DIndex(coordinate) + 1;
            VertexData[] cubeVerticesData = cubeVertices
                .Select((vertex, i) => new VertexData {index = i * cubeIndex, value = vertex}).ToArray();
            TriangleData[] cubeTrianglesData = cubeTriangles
                .Select((triangle, i) => new TriangleData {index = i * cubeIndex, value = triangle}).ToArray();

            return (cubeVerticesData, cubeTrianglesData);
        }
        private (Vector3[] vertices, List<int> triangles) GetFaceMeshData
        (
            Direction dir,
            float scale,
            Vector3 facePos,
            List<Vector3> cubeVertices
        )
        {
            Vector3[] vertices = VoxelMeshData.FaceVertices(dir, scale, facePos);
            List<int> triangles = new List<int>();

            int vertCount = cubeVertices.Count;

            triangles.Add(vertCount - 4);
            triangles.Add(vertCount - 4 + 1);
            triangles.Add(vertCount - 4 + 2);
            triangles.Add(vertCount - 4);
            triangles.Add(vertCount - 4 + 2);
            triangles.Add(vertCount - 4 + 3);

            return (vertices, triangles);
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