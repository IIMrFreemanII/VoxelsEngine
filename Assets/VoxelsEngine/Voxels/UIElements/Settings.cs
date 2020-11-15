using ReactElements;
using ReactElements.Core;
using UnityEngine.UIElements;

namespace VoxelsEngine.Voxels.UIElements
{
    public class Settings : ReactElement
    {
        public override VisualElement Render()
        {
            base.Render();
            
            return Append(
                React.CreateElement<Label>(target =>
                {
                    target.text = "Settings";
                })
            );
        }
    }
}