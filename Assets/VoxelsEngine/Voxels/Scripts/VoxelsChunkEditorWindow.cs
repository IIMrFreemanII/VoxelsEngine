#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Extensions;
using VoxelsEngine.Utils;
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
            window.minSize = new Vector2(250f, 20f);
            window.Show();
        }

        private void OnEnable()
        {
            root = rootVisualElement;
            
            HandleVoxelsChunkRenderer();
            SceneView.duringSceneGui += OnSceneGUI;
            DrawMenu();

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
        private static void DrawMenu()
        {
            root.Clear();
            
            VisualElement menu = new UIElements.Menu().Render();
            root.Add(menu);
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
                DrawMenu();
            }
            else
            {
                root.Clear();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!voxelsChunkRenderer || !voxelsChunkRenderer.voxelsChunk) return;

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
            VoxelData voxelData = voxelsChunkRenderer.GetSell(posInArr);
            voxelData.active = true;
            voxelData.material = voxelsChunkRenderer.voxelsChunk.selectedVoxelsSubMesh.material;
            voxelsChunkRenderer.SetSell(voxelData, posInArr);
            
            _needRepaint = true;
        }
        private void RemoveVoxel(Vector3Int posInArr)
        {
            VoxelData voxelData = voxelsChunkRenderer.GetSell(posInArr);
            voxelData.active = false;
            voxelsChunkRenderer.SetSell(voxelData, posInArr);
        }

        private void HandleSelectedVoxel()
        {
            if (_posInVolume.HasValue && !Event.Alt)
            {
                Transform transform = voxelsChunkRenderer.transform;
                float scale = voxelsChunkRenderer.scale;
                int width = voxelsChunkRenderer.voxelsChunk.Width;
                int height = voxelsChunkRenderer.voxelsChunk.Height;
                int depth = voxelsChunkRenderer.voxelsChunk.Depth;
                Vector3 normalizedPointInLocalSpace = transform.InverseTransformPoint(_posInVolume.Value) / scale;

                // we do " + _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
                // because before in VoxelChunkRenderer.GenerateVoxelMesh()
                // we subtracted " - _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
                // in order to revert value array index format
                // Vector3 rawIndexPos = pointInLocalSpace + _voxelsChunkRenderer.size.ToFloat() * 0.5f;
                Vector3 rawIndexPos = normalizedPointInLocalSpace + voxelsChunkRenderer.size.ToFloat() * 0.5f;

                int x = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.x), 0, width - 1);
                int y = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.y), 0, height - 1);
                int z = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.z), 0, depth - 1);

                Vector3 posInArray = new Vector3(x, y, z);
                
                Vector3 drawPos = ((posInArray - voxelsChunkRenderer.size.ToFloat() * 0.5f) + (Vector3.one * 0.5f)) * scale;

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
                float littleNumber = 0.001f;

                if (!meshCollider.sharedMesh)
                {
                    meshCollider.sharedMesh = voxelsChunkRenderer.Mesh;
                }
                
                if (meshCollider.Raycast(ray, out RaycastHit hit, float.MaxValue))
                {
                    Vector3 hitPoint = hit.point;
                    _posInVolume = hitPoint + hit.normal * littleNumber;

                    if (Event.IsUsed) SetSelection(voxelsChunkRenderer.gameObject);
                    
                    return;
                }
                
                //===========================================================================
                
                MeshCollider boundsCollider = voxelsChunkRenderer.chunkBoundsMeshCollider;
                
                if (boundsCollider.Raycast(ray, out RaycastHit hit1, float.MaxValue))
                {
                    Vector3 hitPoint = hit1.point;
                    _posInVolume = hitPoint + hit1.normal * littleNumber;
                    
                    if (Event.IsUsed) SetSelection(voxelsChunkRenderer.gameObject);
                    
                    return;
                }
                
                //===========================================================================

                if (_posInVolume.HasValue)_needRepaint = true;
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

            int x = voxelsChunkRenderer.voxelsChunk.Width;
            int y = voxelsChunkRenderer.voxelsChunk.Height;
            int z = voxelsChunkRenderer.voxelsChunk.Depth;

            float scale = voxelsChunkRenderer.scale;

            HandlesUtils.DrawWireCube(Vector3.zero, transform, new Vector3(x, y, z) * scale, true, true);
        }
    }
}

#endif