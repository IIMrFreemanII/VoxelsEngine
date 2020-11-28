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
        public Action onLastElementRemove;
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

                target.itemsSource = voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes;
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
                    Material material = voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes[i].material;
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
                
                VoxelsSubMesh selectedVoxelsSubMesh = voxelsChunkRenderer.sharedVoxelsChunk.Value.GetSelectedVoxelsSubMesh();
                if (selectedVoxelsSubMesh != null)
                {
                    int posInArr = voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes.FindIndex(item => item == selectedVoxelsSubMesh);
                    target.SetSelection(posInArr);
                }

                target.onSelectionChange += onSelectionChange;
            });
        }

        private void HandleAdd()
        {
            voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes.Add(new VoxelsSubMesh());
            listView.SetSelection(voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes.Count - 1);
            listView.Refresh();
        }

        private void HandleRemove()
        {
            int selectedVoxelsSubMeshIndex = listView.selectedIndex;
            voxelsChunkRenderer.sharedVoxelsChunk.Value.RemoveVoxelsSubMesh(selectedVoxelsSubMeshIndex);
            voxelsChunkRenderer.sharedVoxelsChunk.Value.SetSelectedVoxelsSubMeshIndex(selectedVoxelsSubMeshIndex);
            
            listView.Refresh();
            
            int newSelectedItemIndex = voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes.Count - 1;
            if (newSelectedItemIndex >= 0)
                listView.SetSelection(newSelectedItemIndex);

            if (voxelsChunkRenderer.sharedVoxelsChunk.Value.voxelsSubMeshes.Count == 0)
            {
                voxelsChunkRenderer.UpdateSubMeshesChunk();
                onLastElementRemove?.Invoke();
            }
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