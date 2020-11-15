using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.UIElements
{
    public class MaterialsContainer : VisualElement
    {
        private VoxelsChunkRenderer _voxelsChunkRenderer;
        private MaterialListView _materialsListView;
        public VisualElement materialFieldContainer;
        
        public MaterialsContainer()
        {
            _voxelsChunkRenderer = VoxelsChunkEditorWindow.voxelsChunkRenderer;
            materialFieldContainer = new VisualElement();
            _materialsListView = new MaterialListView(this);
            _materialsListView.listView.onSelectionChange += OnSelectionChange;

            Add(_materialsListView);
            Add(materialFieldContainer);
        }

        private void OnSelectionChange(IEnumerable<object> objects)
        {
            foreach (object obj in objects)
            {
                VoxelsSubMesh voxelsSubMesh = obj as VoxelsSubMesh;
                _voxelsChunkRenderer.voxelsChunk.selectedVoxelsSubMesh = voxelsSubMesh;

                HandleSelectedMaterial(voxelsSubMesh);
            }
        }
        
        public void HandleSelectedMaterial(VoxelsSubMesh voxelsSubMesh)
        {
            if (voxelsSubMesh != null)
            {
                ObjectField materialField = new ObjectField("Selected material");
                materialField.objectType = typeof(Material);
                materialField.allowSceneObjects = false;
                materialField.value = voxelsSubMesh.material;
            
                materialField.RegisterCallback<ChangeEvent<Object>>(evt =>
                { 
                    Material material = evt.newValue as Material;
                    voxelsSubMesh.material = material;
                    _voxelsChunkRenderer.voxelsChunk.MapMaterialToSubMesh();
                    _materialsListView.listView.Refresh();
                });

                materialFieldContainer.Clear();
                materialFieldContainer.Add(materialField);
            }
            else
            {
                materialFieldContainer.Clear();
            }
        }
    }
}