using System;
using System.Collections.Generic;
using ReactElements.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.UIElements
{
    public class MaterialListView : ReactElement
    {
        public VoxelsChunkRenderer voxelsChunkRenderer;
        public Action<IEnumerable<object>> onSelectionChange;
        public ListView listView;

        public MaterialListView()
        {
            voxelsChunkRenderer = VoxelsChunkEditorWindow.voxelsChunkRenderer;

            style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        }

        private VisualElement GetListView()
        {
            return React.CreateElement<ListView>(target =>
            {
                listView = target;
                
                target.style.borderTopWidth = 1;
                target.style.borderBottomWidth = 1;
                target.style.borderLeftWidth = 1;
                target.style.borderRightWidth = 1;

                target.style.borderTopColor = Color.black;
                target.style.borderBottomColor = Color.black;
                target.style.borderLeftColor = Color.black;
                target.style.borderRightColor = Color.black;

                target.style.borderTopLeftRadius = 5;
                target.style.borderTopRightRadius = 5;
                target.style.borderBottomLeftRadius = 5;
                target.style.borderBottomRightRadius = 5;

                target.style.width = 150;
                target.style.height = 150;

                target.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);

                target.itemsSource = voxelsChunkRenderer.voxelsChunk.Value.materials;
                target.itemHeight = 22;
                
                Func<VisualElement> makeItem = () =>
                {
                    Label label = new Label();

                    label.style.paddingTop = 0;
                    label.style.paddingBottom = 0;
                    label.style.paddingLeft = 8;
                    label.style.paddingRight = 8;

                    label.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);

                    return label;
                };
                Action<VisualElement, int> bindItem = (item, i) =>
                {
                    Label label = item as Label;
                    Material material = voxelsChunkRenderer.voxelsChunk.Value.materials[i];
                    if (material)
                    {
                        label.text = material.name;
                    }
                    else
                    {
                        label.text = "Empty";
                    }
                };
                
                target.makeItem = makeItem;
                target.bindItem = bindItem;
                target.selectionType = SelectionType.Single;
                
                Material selectedMaterial = voxelsChunkRenderer.voxelsChunk.Value.selectedMaterial;
                if (selectedMaterial != null)
                {
                    int posInArr = voxelsChunkRenderer.voxelsChunk.Value.materials.FindIndex(material => material == selectedMaterial);
                    target.SetSelection(posInArr);
                }

                target.onSelectionChange += onSelectionChange;
            });
        }
        
        
        private void HandleAdd()
        {
            voxelsChunkRenderer.voxelsChunk.Value.materials.Add(null);
            listView.SetSelection(voxelsChunkRenderer.voxelsChunk.Value.materials.Count - 1);
            listView.Refresh();
        }

        private void HandleRemove()
        {
            int selectedMatIndex = listView.selectedIndex;
            
            // Material materialToRemove = voxelsChunkRenderer.voxelsChunk.Value.materials[selectedMatIndex];
            // voxelsChunkRenderer.voxelsChunk.Value.RemoveVoxelsWithMaterial(materialToRemove);
            
            voxelsChunkRenderer.voxelsChunk.Value.materials.RemoveAt(selectedMatIndex);
            voxelsChunkRenderer.voxelsChunk.Value.selectedMaterial = null;
            listView.Refresh();
            
            listView.SetSelection(voxelsChunkRenderer.voxelsChunk.Value.materials.Count - 1);
            
            voxelsChunkRenderer.UpdateSubMeshesChunk();
        }

        private VisualElement GetActionButtons()
        {
            return React.CreateElement<VisualElement>(new []
            {
                React.CreateElement<Button>(target =>
                {
                    target.clickable.clicked += HandleAdd;
                    target.text = "Add";
                }),
                React.CreateElement<Button>(target =>
                {
                    target.clickable.clicked += HandleRemove;
                    target.text = "Remove";
                })
            });
        }

        public override VisualElement Render()
        {
            base.Render();
            
            return Append(
                GetListView(),
                GetActionButtons()
            );
        }
    }
}