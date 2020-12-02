using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    public class VoxelsChunkColliderController : SerializedMonoBehaviour
    {
        [HideInInspector]
        public VoxelsChunkRenderer voxelsChunkRenderer;
        [OdinSerialize]
        public Dictionary<Vector3Int, BoxCollider> activeBoxColliders = new Dictionary<Vector3Int, BoxCollider>();
        [OdinSerialize]
        public Queue<BoxCollider> deactivatedBoxColliders = new Queue<BoxCollider>();

        public void RefreshColliders()
        {
            activeBoxColliders.Clear();

            BoxCollider[] boxColliders = GetComponents<BoxCollider>();
            for (int i = 0; i < boxColliders.Length; i++)
            {
                BoxCollider boxCollider = boxColliders[i];
                boxCollider.enabled = false;
                deactivatedBoxColliders.Enqueue(boxCollider);
            }
            
            ClearDeactivatedColliders();
        }
        
        private void ClearDeactivatedColliders()
        {
            if (Application.IsPlaying(this))
            {
                while (deactivatedBoxColliders.Count > 0)
                {
                    Destroy(deactivatedBoxColliders.Dequeue());
                }
            }
            else
            {
                while (deactivatedBoxColliders.Count > 0)
                {
                    DestroyImmediate(deactivatedBoxColliders.Dequeue());
                }
            }
        }

        public void InitBoxColliders(List<Vector3Int> coords)
        {
            RefreshColliders();
            
            for (int i = 0; i < coords.Count; i++)
            {
                AddBoxCollider(coords[i]);
            }
        }

        public void ResizeBoxColliders(List<Vector3Int> coords)
        {
            for (int i = 0; i < coords.Count; i++)
            {
                if (activeBoxColliders.TryGetValue(coords[i], out BoxCollider boxCollider))
                {
                    boxCollider.center = voxelsChunkRenderer.GetCubePosition(coords[i]);
                    boxCollider.size = Vector3.one * voxelsChunkRenderer.scale.Value;
                }
                else
                {
                    Debug.LogWarning("Out of range!");
                }
            }
        }
        
        public void AddBoxCollider(Vector3Int coordinate)
        {
            BoxCollider boxCollider = GetFreeBoxCollider();
            boxCollider.center = voxelsChunkRenderer.GetCubePosition(coordinate);
            boxCollider.size = Vector3.one * voxelsChunkRenderer.scale.Value;
            boxCollider.enabled = true;

            activeBoxColliders.Add(coordinate, boxCollider);
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
            if (activeBoxColliders.TryGetValue(coordinate, out BoxCollider boxCollider))
            {
                boxCollider.enabled = false;
                
                bool success = activeBoxColliders.Remove(coordinate);
                deactivatedBoxColliders.Enqueue(boxCollider);
                if (!success)
                {
                    Debug.LogWarning("Can't remove box collider from dictionary.");
                }
            }
            else
            {
                Debug.LogWarning($"No such key: {coordinate}");
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