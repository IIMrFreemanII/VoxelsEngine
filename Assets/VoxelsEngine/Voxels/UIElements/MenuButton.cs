using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VoxelsEngine.Voxels.UIElements
{
    public class MenuButton : Label
    {
        public string itemName;
        public bool active;
        private Color normal = Color.clear;
        private Color hover = new Color(0.30f, 0.30f, 0.30f);
        private Color selected = new Color(0.17f, 0.36f, 0.53f);
        
        public MenuButton(string itemName, bool active, Action<string> onClick)
        {
            this.itemName = itemName;
            this.active = active;
            text = itemName;

            style.paddingTop = 0;
            style.paddingBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;

            style.height = 30;
            style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            style.fontSize = 14;
            
            style.backgroundColor = this.active ? selected : normal;
            
            RegisterCallback<MouseDownEvent>(evt =>
            {
                onClick?.Invoke(this.itemName);
            });
            RegisterCallback<MouseOverEvent>(evt =>
            {
                if (!active)
                {
                    style.backgroundColor = hover;
                }
            });
            RegisterCallback<MouseOutEvent>(evt =>
            {
                if (!active)
                {
                    style.backgroundColor = normal;
                }
            });
        }
    }
}