using UnityEngine;
using UnityEngine.UIElements;
using VoxelsEngine.Voxels.Scripts;

namespace VoxelsEngine.Voxels.UIElements
{
    public class MaterialScrollItem : Label
    {
        public VoxelsSubMesh voxelsSubMesh;
        private Color normal = new Color(0.32f, 0.32f, 0.32f);
        private Color hover = new Color(0.38f, 0.38f, 0.38f);
        public bool active;
        
        public MaterialScrollItem(VoxelsSubMesh voxelsSubMesh, bool isLast = false)
        {
            this.voxelsSubMesh = voxelsSubMesh;
            text = voxelsSubMesh.material.name;
            
            style.backgroundColor = normal;
            
            style.paddingTop = 0;
            style.paddingBottom = 0;
            style.paddingLeft = 8;
            style.paddingRight = 8;
                
            style.marginTop = 0;
            style.marginBottom = isLast ? 0 : 5;
            style.marginLeft = 0;
            style.marginRight = 0;
                
            style.borderTopLeftRadius = 5;
            style.borderTopRightRadius = 5;
            style.borderBottomLeftRadius = 5;
            style.borderBottomRightRadius = 5;
                
            style.height = 20;
            style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
            style.fontSize = 12;
            
            RegisterCallback<MouseOverEvent>(evt =>
            {
                style.backgroundColor = hover;
            });
            RegisterCallback<MouseOutEvent>(evt =>
            {
                if (!active)
                {
                    style.backgroundColor = normal;
                }
            });
        }

        public void SetActive(bool active)
        {
            this.active = active;
            
            style.backgroundColor = active ? hover : normal;
        }
    }
}