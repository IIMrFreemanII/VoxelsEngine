using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Voxels.Scripts;
using VoxelsEngine.Voxels.UIElements.React;

namespace VoxelsEngine.Voxels.UIElements
{
    public struct MenuItem
    {
        public string name;
        public bool active;
        public MenuButtonContainer menuButtonContainer;
    }

    public class Menu : ReactElement<List<MenuItem>>
    {
        public Menu()
        {
            MenuButtonContainer settingContainer = new MenuButtonContainer();
            settingContainer.Add(new Label("Settings"));
            MaterialMenuButtonContainer materialMenuButtonContainer = new MaterialMenuButtonContainer();

            state = new List<MenuItem>
            {
                new MenuItem
                {
                    name = "Settings",
                    active = false,
                    menuButtonContainer = settingContainer,
                },
                new MenuItem
                {
                    name = "Materials",
                    active = false,
                    menuButtonContainer = materialMenuButtonContainer,
                }
            }.Select(item => new MenuItem
                {
                    name = item.name,
                    active = item.name == VoxelsChunkEditorWindow.lastActiveMenu,
                    menuButtonContainer = item.menuButtonContainer,
                }
            ).ToList();

            style.flexGrow = 1;
            style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        }

        private void HandleClick(string itemName)
        {
            VoxelsChunkEditorWindow.lastActiveMenu = itemName;

            List<MenuItem> newState = state.Select(item => new MenuItem
            {
                name = item.name,
                active = itemName == item.name,
                menuButtonContainer = item.menuButtonContainer,
            }).ToList();

            SetState(newState);
        }

        public override VisualElement Render()
        {
            base.Render();

            VisualElement leftSide = new VisualElement();
            leftSide.style.width = 120;
            leftSide.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            Add(leftSide);

            VisualElement rightSide = new VisualElement();
            rightSide.style.flexGrow = 1;
            foreach (MenuItem menuItem in state)
            {
                string name = menuItem.name;
                bool active = menuItem.active;

                leftSide.Add(new MenuButton(name, active, HandleClick));

                if (active)
                {
                    rightSide.Clear();
                    rightSide.Add(menuItem.menuButtonContainer);
                }
            }
            Add(rightSide);

            return this;
        }
    }
}