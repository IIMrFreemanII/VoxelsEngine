using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsChunkColliderController : SerializedMonoBehaviour
    {
        public VoxelsChunkRenderer voxelsChunkRenderer;
        [OdinSerialize]
        public Dictionary<Vector3Int, BoxCollider> activeBoxColliders = new Dictionary<Vector3Int, BoxCollider>();
        [OdinSerialize]
        public Queue<BoxCollider> deactivatedBoxColliders = new Queue<BoxCollider>();

        public void RefreshColliders()
        {
            activeBoxColliders.Clear();
            deactivatedBoxColliders.Clear();

            BoxCollider[] boxColliders = GetComponents<BoxCollider>();
            for (int i = 0; i < boxColliders.Length; i++)
            {
                BoxCollider boxCollider = boxColliders[i];
                boxCollider.enabled = false;
                deactivatedBoxColliders.Enqueue(boxCollider);
            }
        }
        
        public BoxCollider AddBoxCollider(Vector3Int coordinate)
        {
            BoxCollider boxCollider = GetFreeBoxCollider();
            boxCollider.center = voxelsChunkRenderer.GetCubePosition(coordinate);
            boxCollider.size = Vector3.one * voxelsChunkRenderer.scale.Value;
            boxCollider.enabled = true;

            activeBoxColliders.Add(coordinate, boxCollider);

            return boxCollider;
        }

        public BoxCollider GetBoxCollider(Vector3Int coordinate)
        {
            if (activeBoxColliders.TryGetValue(coordinate, out BoxCollider boxCollider))
            {
                return boxCollider;
            }

            Debug.LogWarning($"Can't get box collider from key: {coordinate}");

            return null;
        }
        
        
        public void RemoveBoxCollider(Vector3Int coordinate)
        {
            BoxCollider boxCollider = activeBoxColliders[coordinate];
            boxCollider.enabled = false;
            bool success = activeBoxColliders.Remove(coordinate);
            deactivatedBoxColliders.Enqueue(boxCollider);
            if (!success)
            {
                Debug.LogWarning("Can't remove box collider from dictionary.");
            }
        }

        private void PreInitializeBoxColliders(int count)
        {
            for (int i = 0; i < count; i++)
            {
                PreInitializeBoxCollider();
            }
        }

        private void PreInitializeBoxCollider()
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.enabled = false;
            
            deactivatedBoxColliders.Enqueue(boxCollider);
        }

        private BoxCollider GetFreeBoxCollider()
        {
            if (deactivatedBoxColliders.Count == 0)
            {
                PreInitializeBoxCollider();
                return deactivatedBoxColliders.Dequeue();
            }
            
            return deactivatedBoxColliders.Dequeue();
        }
    }
}