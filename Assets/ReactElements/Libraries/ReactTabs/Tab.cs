using System;
using ReactElements.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactElements.Libraries.ReactTabs
{
    public class Tab : ReactElement
    {
        public string title;
        public string eventKey;
        public bool active;
        public Action<string> onClick;
        
        private Color normal = Color.clear;
        private Color hover = new Color(0.30f, 0.30f, 0.30f);
        private Color selected = new Color(0.17f, 0.36f, 0.53f);
        
        public override VisualElement Render()
        {
            base.Render();

            return Append(
                React.CreateElement<Label>(target =>
                {
                    target.text = title;
                    
                    target.style.paddingTop = 0;
                    target.style.paddingBottom = 0;
                    target.style.paddingLeft = 0;
                    target.style.paddingRight = 0;

                    target.style.height = 30;
                    target.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    target.style.fontSize = 14;

                    target.style.backgroundColor = active ? selected : normal;
                    
                    target.RegisterCallback<MouseDownEvent>(evt => { onClick?.Invoke(eventKey); });
                    target.RegisterCallback<MouseOverEvent>(evt =>
                    {
                        if (!active)
                        {
                            style.backgroundColor = hover;
                        }
                    });
                    target.RegisterCallback<MouseOutEvent>(evt =>
                    {
                        if (!active)
                        {
                            style.backgroundColor = normal;
                        }
                    });
                })
            );
        }
    }
}