using ReactElements;
using ReactElements.Core;
using UnityEngine.UIElements;

namespace VoxelsEngine.Voxels.UIElements
{
    public class Materials : ReactElement
    {
        public override VisualElement Render()
        {
            base.Render();
            
            return Append(
                React.CreateElement<MaterialsContainer>()
            );
        }
    }
}