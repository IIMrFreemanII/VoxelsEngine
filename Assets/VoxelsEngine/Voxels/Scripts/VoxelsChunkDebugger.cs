using Sirenix.OdinInspector;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsChunkDebugger : MonoBehaviour
    {
        private VoxelsChunkRenderer _voxelsChunkRenderer;
        public VoxelsChunkRenderer VoxelsChunkRenderer => _voxelsChunkRenderer
            ? _voxelsChunkRenderer
            : _voxelsChunkRenderer = GetComponent<VoxelsChunkRenderer>();
        
        [Header("Debug")] public bool drawBorder;
        public Color borderColor = Color.white;
        [Space] public bool drawVolume;
        public Color volumeColor = Color.white;

        [OnValueChanged("ValidatePointsSize"), Delayed]
        public float pointsSize = 0.02f;
        private void ValidatePointsSize()
        {
            pointsSize = pointsSize > 0 ? pointsSize : 0;
        }
    }
}