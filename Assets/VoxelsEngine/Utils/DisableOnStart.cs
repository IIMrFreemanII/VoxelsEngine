using UnityEngine;

namespace VoxelsEngine.Utils
{
    public class DisableOnStart : MonoBehaviour
    {
        void Start()
        {
            gameObject.SetActive(false);
        }
    }
}
