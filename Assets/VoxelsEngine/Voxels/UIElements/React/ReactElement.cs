using UnityEngine.UIElements;

namespace VoxelsEngine.Voxels.UIElements.React
{
    public class ReactElement<TState, TProps> : ReactElement<TState>
    {
        public TProps props;

        public ReactElement(TProps props)
        {
            this.props = props;
        }
    }

    public class ReactElement<TState> : ReactElement
    {
        public TState state;

        public void SetState(TState state)
        {
            this.state = state;
            Render();
        }
    }

    public class ReactElement : VisualElement
    {
        public virtual VisualElement Render()
        {
            Clear();
            return this;
        }
    }
}