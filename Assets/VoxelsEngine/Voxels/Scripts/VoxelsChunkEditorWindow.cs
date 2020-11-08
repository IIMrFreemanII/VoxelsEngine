#if UNITY_EDITOR

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using VoxelsEngine.Extensions;
using VoxelsEngine.Utils;
using Event = VoxelsEngine.Utils.Event;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsChunkEditorWindow : OdinEditorWindow
    {
        private static VoxelsChunkRenderer _voxelsChunkRenderer;
        private Vector3? _posInVolume;
        private bool _needRepaint;

        [ToggleGroup("drawBorder", 0, "Draw Border")] [OnValueChanged("RepaintScene")]
        public bool drawBorder = true;

        [ToggleGroup("drawBorder", 0, "Draw Border")] [OnValueChanged("RepaintScene")]
        public Color borderColor = Color.white;

        [MenuItem("Window/VoxelsEngine/Voxels Chunk Editor")]
        public static void Open()
        {
            VoxelsChunkEditorWindow window = GetWindow<VoxelsChunkEditorWindow>();
            window.minSize = new Vector2(250f, 20f);
            window.Show();
        }

        private new void OnEnable()
        {
            base.OnEnable();

            HandleVoxelsChunkRenderer();
            SceneView.duringSceneGui += OnSceneGUI;

            Debug.Log("Open");
        }

        private void OnDisable()
        {
            _voxelsChunkRenderer = null;
            RepaintScene();
            SceneView.duringSceneGui -= OnSceneGUI;

            Debug.Log("Close");
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
            _voxelsChunkRenderer = selectedGO ? selectedGO.GetComponent<VoxelsChunkRenderer>() : null;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_voxelsChunkRenderer || !_voxelsChunkRenderer.voxelsChunk) return;

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
            VoxelData voxelData = _voxelsChunkRenderer.GetSell(posInArr);
            voxelData.active = true;
            _voxelsChunkRenderer.SetSell(voxelData, posInArr);

            _needRepaint = true;
        }
        private void RemoveVoxel(Vector3Int posInArr)
        {
            VoxelData voxelData = _voxelsChunkRenderer.GetSell(posInArr);
            voxelData.active = false;
            _voxelsChunkRenderer.SetSell(voxelData, posInArr);
        }

        private void HandleSelectedVoxel()
        {
            if (_posInVolume.HasValue && !Event.Alt)
            {
                Transform transform = _voxelsChunkRenderer.transform;
                float scale = _voxelsChunkRenderer.scale;
                int width = _voxelsChunkRenderer.voxelsChunk.Width;
                int height = _voxelsChunkRenderer.voxelsChunk.Height;
                int depth = _voxelsChunkRenderer.voxelsChunk.Depth;
                Vector3 pointInLocalSpace = transform.InverseTransformPoint(_posInVolume.Value);

                // we do " + _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
                // because before in VoxelChunkRenderer.GenerateVoxelMesh()
                // we subtracted " - _voxelsChunkRenderer.size.ToFloat() * 0.5f;"
                // in order to revert value array index format
                Vector3 rawIndexPos = pointInLocalSpace + _voxelsChunkRenderer.size.ToFloat() * 0.5f;

                int x = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.x), 0, width - 1);
                int y = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.y), 0, height - 1);
                int z = Mathf.Clamp(Mathf.FloorToInt(rawIndexPos.z), 0, depth - 1);

                Vector3 posInArray = new Vector3(x, y, z);
                Vector3 drawPos = (posInArray - _voxelsChunkRenderer.size.ToFloat() * 0.5f) + Vector3.one * scale * 0.5f;

                Handles.color = Color.red;
                HandlesUtils.DrawWireCube(drawPos, transform, Vector3.one * scale, false, false);
                
                HandleAddVoxel(posInArray.ToInt());

                if (Event.MouseMove)
                    _needRepaint = true;
            }
        }

        private static Vector3[] Offsets =
        {
            Vector3.forward,
            Vector3.right,
            Vector3.back,
            Vector3.left,
            Vector3.up,
            Vector3.down,
        };

        private void HandleVolumeSelection()
        {
            if (_voxelsChunkRenderer && Event.MouseMove || Event.IsUsed && Event.IsLeftBtn)
            {
                Ray ray = MousePosToWorldRay();
                MeshCollider meshCollider = _voxelsChunkRenderer.MeshCollider;

                if (!meshCollider.sharedMesh)
                {
                    meshCollider.sharedMesh = _voxelsChunkRenderer.Mesh;
                }
                
                if (meshCollider.Raycast(ray, out RaycastHit hit, float.MaxValue))
                {
                    Vector3 hitPoint = hit.point;
                    _posInVolume = hitPoint + hit.normal * 0.1f;
                    return;
                }
                
                //===========================================================================
                
                MeshCollider boundsCollider = _voxelsChunkRenderer.chunkBoundsMeshCollider;
                
                if (boundsCollider.Raycast(ray, out RaycastHit hit1, float.MaxValue))
                {
                    Vector3 hitPoint = hit1.point;
                    _posInVolume = hitPoint + hit1.normal * 0.1f;
                    return;
                }
                
                //===========================================================================

                if (_posInVolume.HasValue)_needRepaint = true;
                _posInVolume = null;
            }
        }

        private Ray MousePosToWorldRay() => HandleUtility.GUIPointToWorldRay(Event.MousePosition);

        private void DrawChunkBorder()
        {
            if (!drawBorder) return;

            Handles.color = borderColor;
            Transform transform = _voxelsChunkRenderer.transform;

            int x = _voxelsChunkRenderer.voxelsChunk.Width;
            int y = _voxelsChunkRenderer.voxelsChunk.Height;
            int z = _voxelsChunkRenderer.voxelsChunk.Depth;

            float scale = _voxelsChunkRenderer.scale;

            HandlesUtils.DrawWireCube(Vector3.zero, transform, new Vector3(x, y, z) * scale, true, true);
        }
    }
}

#endif