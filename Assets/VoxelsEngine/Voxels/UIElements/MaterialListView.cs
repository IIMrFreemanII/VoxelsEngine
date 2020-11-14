using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.UIElements
{
    public class MaterialListView : VisualElement
    {
        private MaterialMenuButtonContainer _materialMenuButtonContainer;
        public VoxelsChunkRenderer voxelsChunkRenderer;
        public ListView listView;

        public MaterialListView(MaterialMenuButtonContainer materialMenuButtonContainer)
        {
            _materialMenuButtonContainer = materialMenuButtonContainer;
            voxelsChunkRenderer = VoxelsChunkEditorWindow.voxelsChunkRenderer;

            style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            //=================================================================

            listView = new ListView();

            listView.style.borderTopWidth = 1;
            listView.style.borderBottomWidth = 1;
            listView.style.borderLeftWidth = 1;
            listView.style.borderRightWidth = 1;

            listView.style.borderTopColor = Color.black;
            listView.style.borderBottomColor = Color.black;
            listView.style.borderLeftColor = Color.black;
            listView.style.borderRightColor = Color.black;

            listView.style.borderTopLeftRadius = 5;
            listView.style.borderTopRightRadius = 5;
            listView.style.borderBottomLeftRadius = 5;
            listView.style.borderBottomRightRadius = 5;

            listView.style.width = 150;
            listView.style.height = 150;

            listView.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);

            List<VoxelsSubMesh> voxelsSubMeshes = voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes;

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
                Material material = voxelsSubMeshes[i].material;
                if (material)
                {
                    label.text = material.name;
                }
                else
                {
                    label.text = "Empty";
                }
            };

            listView.itemsSource = voxelsSubMeshes;
            listView.itemHeight = 22;
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;

            listView.selectionType = SelectionType.Single;

            VoxelsSubMesh selectedVoxelsSubMesh = voxelsChunkRenderer.voxelsChunk.selectedVoxelsSubMesh;
            if (selectedVoxelsSubMesh != null)
            {
                int posInArr =
                    voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes.FindIndex(subMesh =>
                        subMesh.material == selectedVoxelsSubMesh.material);
                listView.SetSelection(posInArr);
                _materialMenuButtonContainer.HandleSelectedMaterial(selectedVoxelsSubMesh);
            }

            //========================================================

            VisualElement actionButtons = new VisualElement();

            Button add = new Button();
            add.clickable.clicked += HandleAdd;
            add.text = "Add";
            Button remove = new Button();
            remove.clickable.clicked += HandleRemove;
            remove.text = "Remove";

            actionButtons.Add(add);
            actionButtons.Add(remove);

            //=========================================================

            Add(listView);
            Add(actionButtons);
        }

        private void HandleAdd()
        {
            MaterialMenuButtonContainer materialMenuButtonContainer =
                VoxelsChunkEditorWindow.root.Q<MaterialMenuButtonContainer>();

            voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes.Add(new VoxelsSubMesh());
            listView.Refresh();
            listView.SetSelection(voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes.Count - 1);

            VoxelsSubMesh voxelsSubMesh = listView.selectedItem as VoxelsSubMesh;
            materialMenuButtonContainer.HandleSelectedMaterial(voxelsSubMesh);
        }

        private void HandleRemove()
        {
            if (voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes.Count > 1)
            {
                MaterialMenuButtonContainer materialMenuButtonContainer =
                    VoxelsChunkEditorWindow.root.Q<MaterialMenuButtonContainer>();

                VoxelsSubMesh selectedVoxelsSubMesh = listView.selectedItem as VoxelsSubMesh;
                voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes.Remove(selectedVoxelsSubMesh);

                listView.Refresh();
                int selectedItemIndex = voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes.Count - 1;
                listView.SetSelection(selectedItemIndex);

                materialMenuButtonContainer.HandleSelectedMaterial(
                    voxelsChunkRenderer.voxelsChunk.voxelsSubMeshes[selectedItemIndex]);
                
                voxelsChunkRenderer.voxelsChunk.MapMaterialToSubMesh();
            }
        }
    }
}