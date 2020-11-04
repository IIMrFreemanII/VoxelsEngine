#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using VoxelsEngine.Utils;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsChunkEditorWindow : OdinEditorWindow
    {
        private static VoxelsChunkRenderer _voxelsChunkRenderer;
        
        [Delayed]
        [Title("Visual debug")]
        [OnValueChanged("ValidatePointsSize")]
        [OnValueChanged("RepaintScene")]
        public float pointsSize = 0.02f;
        private void ValidatePointsSize()
        {
            pointsSize = pointsSize > 0 ? pointsSize : 0;
        }
        
        [ToggleGroup("drawBorder", 0, "Draw Border")]
        [OnValueChanged("RepaintScene")]
        public bool drawBorder = true;

        [ToggleGroup("drawBorder", 0, "Draw Border")]
        [OnValueChanged("RepaintScene")]
        public Color borderColor = Color.white;

        [ToggleGroup("drawVolume", 0, "Draw Volume")]
        [OnValueChanged("RepaintScene")]
        public bool drawVolume = true;
        
        [ToggleGroup("drawVolume", 0, "Draw Volume")]
        [OnValueChanged("RepaintScene")]
        public Color volumeColor = Color.white;

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
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.Repaint();
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
            EditVoxels();
            HandleClicks();
        }

        private void HandleClicks()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Debug.Log("Left click");
            }

            if (e.type == EventType.MouseMove)
            {
                Debug.Log("Mouse move");
            }
        }

        private void EditVoxels()
        {
            if (!_voxelsChunkRenderer || !_voxelsChunkRenderer.voxelsChunk) return;

            Handles.matrix = _voxelsChunkRenderer.transform.localToWorldMatrix;

            DrawChunkBorder();
            DrawChunkVolume();
        }

        private void DrawChunkBorder()
        {
            if (!drawBorder) return;

            Handles.color = borderColor;

            HandlesUtils.DrawWireRect(
                Vector3.zero,
                new Vector3(_voxelsChunkRenderer.voxelsChunk.Width, _voxelsChunkRenderer.voxelsChunk.Height,
                    _voxelsChunkRenderer.voxelsChunk.Depth) * _voxelsChunkRenderer.scale
            );
        }

        private void DrawChunkVolume()
        {
            if (!drawVolume) return;

            Handles.color = volumeColor;

            for (int x = 0; x < _voxelsChunkRenderer.voxelsChunk.Width; x++)
            {
                for (int y = 0; y < _voxelsChunkRenderer.voxelsChunk.Height; y++)
                {
                    for (int z = 0; z < _voxelsChunkRenderer.voxelsChunk.Depth; z++)
                    {
                        Vector3 cubePos = new Vector3(x, y, z) * _voxelsChunkRenderer.scale;
                        Vector3 offset = Vector3.one * _voxelsChunkRenderer.scale * 0.5f;

                        // Vector3Int posInArr1 = new Vector3Int(x, y, z);
                        // VoxelData voxelData1 = _voxelsChunkRenderer.voxelsChunk.GetCell(posInArr1);
                        //
                        // if (voxelData1.active)
                        // {
                        //     Handles.DrawWireCube(cubePos + offset, Vector3.one);
                        // }

                        // Handles.FreeMoveHandle(
                        //     cubePos + offset,
                        //     Quaternion.identity,
                        //     _voxelsChunkRenderer.pointsSize,
                        //     Vector3.zero,
                        //     Handles.DotHandleCap
                        // );

                        if (
                            Handles.Button(cubePos + offset,
                                Quaternion.identity,
                                pointsSize,
                                0.02f,
                                Handles.DotHandleCap
                            )
                        )
                        {
                            Vector3Int posInArr = new Vector3Int(x, y, z);
                            VoxelData voxelData = _voxelsChunkRenderer.GetSell(posInArr);

                            voxelData.active = voxelData.active == false;

                            _voxelsChunkRenderer.SetSell(voxelData, posInArr);
                        }
                    }
                }
            }
        }
    }
}

#endif