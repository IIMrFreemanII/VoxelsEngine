﻿using System;
using UnityEditor;
using UnityEngine;
using VoxelsEngine.Utils;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.Editor
{
    [CustomEditor(typeof(VoxelsChunkRenderer))]
    public class VoxelsChunkRendererEditor : UnityEditor.Editor
    {
        private VoxelsChunkRenderer _voxelsChunkRenderer;

        private void OnEnable()
        {
            _voxelsChunkRenderer = (VoxelsChunkRenderer) target;
            // SceneView.duringSceneGui += VoxelsChunkUpdate;
        }

        private void OnDisable()
        {
            // SceneView.duringSceneGui -= VoxelsChunkUpdate;
        }

        private void OnSceneGUI()
        {
            EditVoxels();
        }

        private void EditVoxels()
        {
            if (!_voxelsChunkRenderer.voxelsChunk) return;

            Handles.matrix = _voxelsChunkRenderer.transform.localToWorldMatrix;

            DrawChunkBorder();
            DrawChunkVolume();
        }

        private void DrawChunkBorder()
        {
            if (!_voxelsChunkRenderer.drawBorder) return;

            Handles.color = _voxelsChunkRenderer.borderColor;

            HandlesUtils.DrawWireRect(
                Vector3.zero,
                new Vector3(_voxelsChunkRenderer.voxelsChunk.Width, _voxelsChunkRenderer.voxelsChunk.Height,
                    _voxelsChunkRenderer.voxelsChunk.Depth) * _voxelsChunkRenderer.scale
            );
        }

        private void DrawChunkVolume()
        {
            if (!_voxelsChunkRenderer.drawVolume) return;

            Handles.color = _voxelsChunkRenderer.volumeColor;

            for (int x = 0; x < _voxelsChunkRenderer.voxelsChunk.Width; x++)
            {
                for (int y = 0; y < _voxelsChunkRenderer.voxelsChunk.Height; y++)
                {
                    for (int z = 0; z < _voxelsChunkRenderer.voxelsChunk.Depth; z++)
                    {
                        Vector3 cubePos = new Vector3(x, y, z) * _voxelsChunkRenderer.scale;
                        Vector3 offset = Vector3.one * _voxelsChunkRenderer.scale * 0.5f;

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
                                _voxelsChunkRenderer.pointsSize,
                                0.02f,
                                Handles.DotHandleCap
                            )
                        )
                        {
                            Vector3Int posInArr = new Vector3Int(x, y, z);
                            VoxelData voxelData = _voxelsChunkRenderer.voxelsChunk.GetCell(posInArr);
                            
                            voxelData.active = voxelData.active == false;

                            Debug.Log($"{posInArr}: {voxelData.active}");
                            
                            _voxelsChunkRenderer.voxelsChunk.SetSell(voxelData, posInArr);

                            _voxelsChunkRenderer.UpdateChunk();
                        }
                    }
                }
            }
        }
    }
}