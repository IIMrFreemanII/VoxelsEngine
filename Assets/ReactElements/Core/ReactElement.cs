using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactElements.Core
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
        public IEnumerable<VisualElement> children { get; set; }
        
        public virtual VisualElement Render()
        {
            Clear();
            return this;
        }

        protected VisualElement Append(params VisualElement[] elements)
        {
            foreach (VisualElement element in elements)
            {
                Add(element);
            }

            return this;
        }
    }
}