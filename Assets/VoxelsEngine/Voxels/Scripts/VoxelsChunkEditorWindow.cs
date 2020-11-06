#if UNITY_EDITOR

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using VoxelsEngine.Utils;
using Event = VoxelsEngine.Utils.Event;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsChunkEditorWindow : OdinEditorWindow
    {
        private static VoxelsChunkRenderer _voxelsChunkRenderer;
        private Vector3? _posInVolume;

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
        }

        private void AddVoxel(Vector3Int posInArr)
        {
            bool leftClick = Event.Mouse.Click.Left;

            if (leftClick)
            {
                VoxelData voxelData = _voxelsChunkRenderer.GetSell(posInArr);
                voxelData.active = true;
                _voxelsChunkRenderer.SetSell(voxelData, posInArr);
            }
        }

        private void DrawSelectedVoxel()
        {
            bool mouseMove = Event.Mouse.Move;

            if (_posInVolume.HasValue)
            {
                Transform transform = _voxelsChunkRenderer.transform;
                float scale = _voxelsChunkRenderer.scale;

                int width = _voxelsChunkRenderer.voxelsChunk.Width;
                int height = _voxelsChunkRenderer.voxelsChunk.Height;
                int depth = _voxelsChunkRenderer.voxelsChunk.Depth;

                int x = Mathf.Clamp(Mathf.FloorToInt(_posInVolume.Value.x), 0, width - 1);
                int y = Mathf.Clamp(Mathf.FloorToInt(_posInVolume.Value.y), 0, height - 1);
                int z = Mathf.Clamp(Mathf.FloorToInt(_posInVolume.Value.z), 0, depth - 1);

                Vector3 posInArray = new Vector3(x, y, z);
                Vector3 drawPos = posInArray + Vector3.one * scale * 0.5f;

                AddVoxel(new Vector3Int(x, y, z));

                Vector3 localCubePos = transform.InverseTransformPoint(drawPos);
                Handles.color = Color.red;
                Handles.DrawWireCube(localCubePos, Vector3.one * scale);
                
                if (mouseMove) HandleUtility.Repaint();
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
            bool mouseMove = Event.Mouse.Move;

            if (_voxelsChunkRenderer && mouseMove)
            {
                Plane[] planes = new Plane[Offsets.Length];
                Transform cubeTransform = _voxelsChunkRenderer.transform;

                int x = _voxelsChunkRenderer.voxelsChunk.Width;
                int y = _voxelsChunkRenderer.voxelsChunk.Height;
                int z = _voxelsChunkRenderer.voxelsChunk.Depth;

                float scale = _voxelsChunkRenderer.scale;

                Vector3 cubeWorldCenter =
                    cubeTransform.TransformPoint(cubeTransform.position + new Vector3(x, y, z) * scale * 0.5f);

                for (int i = 0; i < Offsets.Length; i++)
                {
                    Vector3 faceDir = Offsets[i];

                    if (faceDir == Vector3.forward || faceDir == Vector3.back)
                    {
                        Vector3 planeCenter = cubeWorldCenter + faceDir * z * scale * 0.5f;
                        planes[i] = new Plane(-faceDir, planeCenter);
                    }
                    else if (faceDir == Vector3.left || faceDir == Vector3.right)
                    {
                        Vector3 planeCenter = cubeWorldCenter + faceDir * x * scale * 0.5f;
                        planes[i] = new Plane(-faceDir, planeCenter);
                    }
                    else if (faceDir == Vector3.up || faceDir == Vector3.down)
                    {
                        Vector3 planeCenter = cubeWorldCenter + faceDir * y * scale * 0.5f;
                        planes[i] = new Plane(-faceDir, planeCenter);
                    }
                }

                Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;

                Ray ray = MousePosToWorldRay();
                float dist;

                MeshCollider meshCollider = _voxelsChunkRenderer.MeshCollider;

                if (meshCollider.Raycast(ray, out RaycastHit hit, float.MaxValue))
                {
                    Vector3 hitPoint = hit.point;
                    _posInVolume = hitPoint + hit.normal * 0.1f;
                }
                else
                {
                    for (int i = 0; i < planes.Length; i++)
                    {
                        Plane plane = planes[i];
                        if (plane.GetSide(camPos))
                        {
                            if (plane.Raycast(ray, out dist))
                            {
                                Vector3 hitPoint = ray.GetPoint(dist);

                                if (InPlaneBounds(cubeWorldCenter, new Vector3(x, y, z), hitPoint))
                                {
                                    _posInVolume = hitPoint;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Ray MousePosToWorldRay() => HandleUtility.GUIPointToWorldRay(Event.Mouse.Position);

        private bool InPlaneBounds(Vector3 center, Vector3 size, Vector3 point)
        {
            Vector3 bound = center + size * 0.5f;

            if (
                bound.x >= point.x && (bound.x - bound.x) <= point.x &&
                bound.y >= point.y && (bound.y - bound.y) <= point.y &&
                bound.z >= point.z && (bound.z - bound.z) <= point.z
            )
            {
                return true;
            }

            return false;
        }

        private void EditVoxels()
        {
            if (!_voxelsChunkRenderer || !_voxelsChunkRenderer.voxelsChunk) return;

            Handles.matrix = _voxelsChunkRenderer.transform.localToWorldMatrix;

            DrawChunkBorder();
            HandleVolumeSelection();
            DrawSelectedVoxel();
        }

        private void DrawChunkBorder()
        {
            if (!drawBorder) return;

            Handles.color = borderColor;

            int x = _voxelsChunkRenderer.voxelsChunk.Width;
            int y = _voxelsChunkRenderer.voxelsChunk.Height;
            int z = _voxelsChunkRenderer.voxelsChunk.Depth;

            float scale = _voxelsChunkRenderer.scale;

            HandlesUtils.DrawWireCube(new Vector3(x, y, z) * scale * 0.5f, new Vector3(x, y, z) * scale, true, true);
        }
    }
}

#endif