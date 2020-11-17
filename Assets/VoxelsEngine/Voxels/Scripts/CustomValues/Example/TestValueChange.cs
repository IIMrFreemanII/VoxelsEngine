using UnityEngine;
using VoxelsEngine.Utils;
using VoxelsEngine.Voxels.Scripts.CustomValues;

namespace VoxelsEngine.Voxels.Scripts
{
    [ExecuteInEditMode]
    public class TestValueChange : MonoBehaviour
    {
        // value type
        public HandleIntChange customInt;
        // reference type
        public HandleHealthChange customHealth;

        public void HandleInt(int value)
        {
            Debug.Log($"int changed {value}");
        }

        public void OnHealthChange(Health health)
        {
            Debug.Log($"Health: {health}");
        }

        private void OnEnable()
        {
            customInt.onChange += HandleInt;
            customHealth.onChange += OnHealthChange;
        }
        private void OnDisable()
        {
            customInt.onChange -= HandleInt;
            customHealth.onChange -= OnHealthChange;
        }

        [ContextMenu("Change Value")]
        public void ChangeValue()
        {
            customHealth.Value = new Health()
            {
                health = 10,
                test = true,
            };
        }
        
        [ContextMenu("Set same value")]
        public void SetSameValue()
        {
            customInt.Value = 10;
        }

        private void OnValidate()
        {
            customHealth.HandleChange(customHealth.Value);
        }
    }
}