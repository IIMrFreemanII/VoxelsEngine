#if UNITY_EDITOR

using ReactElements.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Extensions;
using VoxelsEngine.Utils;
using VoxelsEngine.Voxels.UIElements;
using Event = VoxelsEngine.Utils.Event;
using Object = UnityEngine.Object;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsChunkEditorWindow : EditorWindow
    {
        public static VoxelsChunkRenderer voxelsChunkRenderer;
        private Vector3? _posInVolume;
        private bool _needRepaint;

        public static VisualElement root;


        [MenuItem("Window/VoxelsEngine/Voxels Chunk Editor")]
        public static void Open()
        {
            VoxelsChunkEditorWindow window = GetWindow<VoxelsChunkEditorWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            root = rootVisualElement;

            HandleVoxelsChunkRenderer();
            SceneView.duringSceneGui += OnSceneGUI;
            DrawChunkEditor();

            Debug.Log("Open");
        }

        private void OnDisable()
        {
            voxelsChunkRenderer = null;
            RepaintScene();
            SceneView.duringSceneGui -= OnSceneGUI;

            Debug.Log("Close");
        }

        public static string lastActiveMenu;

        public static void DrawChunkEditor()
        {
            root.Clear();

            if (voxelsChunkRenderer && voxelsChunkRenderer.sharedVoxelsChunk.Value)
            {
                root.Add(React.CreateElement<VoxelsChunkEditor>());
            }
            else
            {
                Box box = new Box();
                box.style.paddingTop = 10;
                box.style.paddingBottom = 10;
                box.style.paddingLeft = 10;
                box.style.paddingRight = 10;
                box.style.width = 400;

                Label warn =
                    new Label(
                        "Select GameObject with VoxelsChunkRenderer component attached and VoxelsChunk field should be assigned!");
                warn.style.whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal);
                warn.style.color = Color.yellow;

                box.Add(warn);
                root.Add(box);
            }
        }

        public static void HandleDrawChunkEditor()
        {
            if (HasOpenInstances<VoxelsChunkEditorWindow>())
            {
                DrawChunkEditor();
            }
        }

        private void RepaintScene()
        {
            if (SceneView.currentDrawingSceneView)
            {
                SceneView.currentDrawingSceneView.Repaint();
            }
            else if (SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        private void OnSelectionChange()
        {
            HandleVoxelsChunkRenderer();
        }

        public static void HandleVoxelsChunkRenderer()
        {
            GameObject selectedGO = Selection.activeGameObject;
            voxelsChunkRenderer = selectedGO ? selectedGO.GetComponent<VoxelsChunkRenderer>() : null;

            if (voxelsChunkRenderer)
            {
                DrawChunkEditor();
            }
            else
            {
                root.Clear();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!voxelsChunkRenderer || !voxelsChunkRenderer.sharedVoxelsChunk.Value) return;

            DrawChunkBorder();
            HandleVolumeSelection();
            HandleSelectedVoxel();

            if (_needRepaint)
            {
                RepaintScene();
                _needRepaint = false;
            }
        }

        private void HandleAddVoxel(Vector3Int posInArr)
        {
            if (Event.LeftMouseDown)
            {
                AddVoxel(posInArr);
            }
        }

        private void AddVoxel(Vector3Int posInArr)
        {
            VoxelsSubMesh selectedVoxelsSubMesh =
                voxelsChunkRenderer.sharedVoxelsChunk.Value.GetSelectedVoxelsSubMesh();
            if (selectedVoxelsSubMesh.material)
            {
                int subMeshIndex =
                    voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes.FindIndex(item =>
                        item == selectedVoxelsSubMesh);

                if (subMeshIndex >= 0 &&
                    subMeshIndex <= voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes.Count - 1)
                {
                    VoxelData voxelData = voxelsChunkRenderer.sharedVoxelsChunk.Value.GetCell(posInArr);
                    voxelData.active = true;
                    voxelData.subMeshIndex = subMeshIndex;
                    voxelData.durability = 25;
                    
                    voxelsChunkRenderer.AddVoxel(posInArr, voxelData);

                    _needRepaint = true;
                }
                else
                {
                    Debug.LogWarning("Can't add voxel. Index is out of range!");
                }
            }
            else
            {
                Debug.LogWarning("Can't add voxel. No material selected!");
            }
        }

        private void RemoveOneVoxel(Vector3Int posInArr)
        {
            VoxelData voxelData = voxelsChunkRenderer.sharedVoxelsChunk.Value.GetCell(posInArr);
            voxelData.active = false;
            voxelsChunkRenderer.sharedVoxelsChunk.Value.SetSell(voxelData, posInArr);
            voxelsChunkRenderer.UpdateSubMeshesChunk();
        }

        private void HandleSelectedVoxel()
        {
            if (_posInVolume.HasValue && !Event.Alt)
            {
                Transform transform = voxelsChunkRenderer.transform;
                float scale = voxelsChunkRenderer.scale.Value;
                
                Vector3 posInArray = voxelsChunkRenderer.GetPosInArr(_posInVolume.Value);

                Vector3 drawPos =
                    ((posInArray - voxelsChunkRenderer.size.Value.ToFloat() * 0.5f) + (Vector3.one * 0.5f)) *
                    scale;

                Handles.color = Color.red;
                HandlesUtils.DrawWireCube(drawPos, transform, Vector3.one * scale, false, false);

                HandleAddVoxel(posInArray.ToInt());

                if (Event.MouseMove)
                    _needRepaint = true;
            }
        }

        private void HandleVolumeSelection()
        {
            if (voxelsChunkRenderer && Event.MouseMove || Event.IsUsed && Event.IsLeftBtn)
            {
                Ray ray = MousePosToWorldRay();
                MeshCollider meshCollider = voxelsChunkRenderer.MeshCollider;

                if (!meshCollider.sharedMesh)
                {
                    meshCollider.sharedMesh = voxelsChunkRenderer.Mesh;
                }

                if (meshCollider.Raycast(ray, out RaycastHit hit, float.MaxValue))
                {
                    _posInVolume = voxelsChunkRenderer.GetVoxelWorldPos(hit.point, hit.normal, EditVoxelType.Add);

                    if (Event.IsUsed) SetSelection(voxelsChunkRenderer.gameObject);

                    return;
                }

                //===========================================================================

                MeshCollider boundsCollider = voxelsChunkRenderer.chunkBoundsMeshCollider;

                if (boundsCollider.Raycast(ray, out RaycastHit hit1, float.MaxValue))
                {
                    _posInVolume = voxelsChunkRenderer.GetVoxelWorldPos(hit1.point, hit1.normal, EditVoxelType.Add);

                    if (Event.IsUsed) SetSelection(voxelsChunkRenderer.gameObject);

                    return;
                }

                //===========================================================================

                if (_posInVolume.HasValue) _needRepaint = true;
                _posInVolume = null;
            }
        }

        private Ray MousePosToWorldRay() => HandleUtility.GUIPointToWorldRay(Event.MousePosition);

        private void SetSelection(Object activeObject) => Selection.activeObject = activeObject;

        private void DrawChunkBorder()
        {
            // if (!settingsTab.drawBorder) return;

            Handles.color = Color.white;
            Transform transform = voxelsChunkRenderer.transform;

            int x = voxelsChunkRenderer.sharedVoxelsChunk.Value.Width;
            int y = voxelsChunkRenderer.sharedVoxelsChunk.Value.Height;
            int z = voxelsChunkRenderer.sharedVoxelsChunk.Value.Depth;

            float scale = voxelsChunkRenderer.scale.Value;

            HandlesUtils.DrawWireCube(Vector3.zero, transform, new Vector3(x, y, z) * scale, true, true);
        }
    }
}

#endif