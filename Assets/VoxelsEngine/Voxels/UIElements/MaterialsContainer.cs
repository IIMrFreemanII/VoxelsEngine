#if UNITY_EDITOR

using System.Collections.Generic;
using ReactElements.Core;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.UIElements
{
    public class MaterialsContainer : ReactElement
    {
        private VoxelsChunkRenderer _voxelsChunkRenderer;
        private MaterialListView _materialListView;
        private ObjectField _materialField;

        public MaterialsContainer()
        {
            _voxelsChunkRenderer = VoxelsChunkEditorWindow.voxelsChunkRenderer;
        }

        public override VisualElement Render()
        {
            base.Render();

            return Append(
                GetMaterialListView(),
                GetMaterialField()
            );
        }

        private VisualElement GetMaterialListView()
        {
            return React.CreateElement<MaterialListView>(target =>
            {
                _materialListView = target;
                target.onSelectionChange = OnSelectionChange;
                target.onLastElementRemove = ResetMaterialField;
            });
        }

        private VisualElement GetMaterialField()
        {
            return React.CreateElement<VisualElement>(new[]
            {
                React.CreateElement<ObjectField>(target =>
                {
                    _materialField = target;

                    target.label = "Selected Material";
                    target.objectType = typeof(Material);
                    target.allowSceneObjects = false;
                    target.value = _voxelsChunkRenderer.sharedVoxelsChunk.Value.GetSelectedVoxelsSubMesh().material;
                    target.RegisterCallback<ChangeEvent<Object>>(evt =>
                    {
                        Material material = evt.newValue as Material;
                        int selectedVoxelsSubMeshIndex = _materialListView.listView.selectedIndex;

                        if (selectedVoxelsSubMeshIndex >= 0)
                        {
                            VoxelsSubMesh selectedVoxelsSubMesh =
                                _voxelsChunkRenderer.sharedVoxelsChunk.Value
                                    .voxelsSubMeshes[selectedVoxelsSubMeshIndex];
                            selectedVoxelsSubMesh.material = material;
                            _materialListView.listView.Refresh();

                            _voxelsChunkRenderer.UpdateSubMeshesChunk();
                        }
                    });
                })
            });
        }

        private void OnSelectionChange(IEnumerable<object> objects)
        {
            foreach (object obj in objects)
            {
                VoxelsSubMesh voxelsSubMesh = obj as VoxelsSubMesh;

                _voxelsChunkRenderer.sharedVoxelsChunk.Value.SetSelectedVoxelsSubMeshIndex(
                    _voxelsChunkRenderer.sharedVoxelsChunk.Value.GetVoxelsSubMeshIndex(voxelsSubMesh)
                );

                _materialField.value = voxelsSubMesh.material;
            }
        }

        private void ResetMaterialField()
        {
            if (_materialField != null)
            {
                _materialField.value = null;
            }
        }
    }
}

#endif