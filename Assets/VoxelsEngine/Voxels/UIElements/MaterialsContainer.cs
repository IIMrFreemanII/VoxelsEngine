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
                    target.value = _voxelsChunkRenderer.voxelsChunk.Value.selectedMaterial;
                    target.RegisterCallback<ChangeEvent<Object>>(evt =>
                    {
                        Material material = evt.newValue as Material;
                        int selectedMatIndex = _materialListView.listView.selectedIndex;
                        
                        if (material)
                        {
                            bool hasSameMaterial =
                                _voxelsChunkRenderer.voxelsChunk.Value.materials.Contains(material);
                            
                            Material selectedMaterial = _voxelsChunkRenderer.voxelsChunk.Value.materials[selectedMatIndex];
                            
                            if (!hasSameMaterial)
                            {
                                _voxelsChunkRenderer.voxelsChunk.Value.materials[selectedMatIndex] = material;
                                _voxelsChunkRenderer.voxelsChunk.Value.selectedMaterial = material;
                                _materialListView.listView.Refresh();
                            }
                            else
                            {
                                target.value = selectedMaterial;
                            }
                        }
                        else
                        {
                            _voxelsChunkRenderer.voxelsChunk.Value.materials[selectedMatIndex] = material;
                            _voxelsChunkRenderer.voxelsChunk.Value.selectedMaterial = material;
                            _materialListView.listView.Refresh();
                            
                            target.value = material;
                        }
                        
                        _voxelsChunkRenderer.voxelsChunk.Value.GenMatToSubMesh();
                    });
                })
            });
        }

        private void OnSelectionChange(IEnumerable<object> objects)
        {
            foreach (object obj in objects)
            {
                Material material = obj as Material;
                
                _voxelsChunkRenderer.voxelsChunk.Value.selectedMaterial = material;
                _materialField.value = material;
            }
        }
    }
}