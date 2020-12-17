#if  UNITY_EDITOR
using ReactElements.Core;
using ReactElements.Libraries.ReactTabs;
using Sirenix.Utilities;
using UnityEngine.UIElements;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.UIElements
{
    public class VoxelsChunkEditor : ReactElement<string>
    {
        public VoxelsChunkEditor()
        {
            state = VoxelsChunkEditorWindow.lastActiveMenu.IsNullOrWhitespace()
                ? "settings"
                : VoxelsChunkEditorWindow.lastActiveMenu;
            style.flexGrow = 1;
        }

        private void OnSelect(string key)
        {
            VoxelsChunkEditorWindow.lastActiveMenu = key;
            SetState(key);
        }

        public override VisualElement Render()
        {
            base.Render();

            return Append(
                React.CreateElement<Tabs>(
                    target =>
                    {
                        target.activeKey = state;
                        target.onSelect = OnSelect;

                        target.style.flexGrow = 1;
                        target.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        target.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
                    },
                    new[]
                    {
                        React.CreateElement<Tab>(target =>
                            {
                                target.title = "Settings";
                                target.eventKey = "settings";
                            },
                            new[] {React.CreateElement<Settings>()}
                        ),
                        React.CreateElement<Tab>(target =>
                            {
                                target.title = "Materials";
                                target.eventKey = "materials";
                            },
                            new[] {React.CreateElement<Materials>()}
                        )
                    }
                )
            );
        }
    }
}
#endif