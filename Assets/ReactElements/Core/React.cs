using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace ReactElements.Core
{
    public static class React
    {
        public static TElem CreateElement<TElem>(Action<TElem> handleElem, IEnumerable<VisualElement> children = null)
            where TElem : VisualElement
        {
            TElem elem = Activator.CreateInstance<TElem>();

            if (IsSameOrSubclass(typeof(ReactElement), typeof(TElem)))
            {
                ReactElement reactElement = elem as ReactElement;
                reactElement.children = children;
                handleElem?.Invoke(elem);
                elem = (TElem) reactElement.Render();
            }
            else
            {
                handleElem?.Invoke(elem);
                
                if (children != null)
                {
                    foreach (VisualElement child in children)
                    {
                        elem?.Add(child);
                    }
                }
            }

            return elem;
        }

        public static TElem CreateElement<TElem>(IEnumerable<VisualElement> children = null) where TElem : VisualElement
        {
            TElem elem = Activator.CreateInstance<TElem>();

            if (IsSameOrSubclass(typeof(ReactElement), typeof(TElem)))
            {
                ReactElement reactElement = elem as ReactElement;
                reactElement.children = children;
                elem = (TElem) reactElement.Render();
            }
            else
            {
                if (children != null)
                {
                    foreach (VisualElement child in children)
                    {
                        elem?.Add(child);
                    }
                }
            }

            return elem;
        }

        private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }
    }
}