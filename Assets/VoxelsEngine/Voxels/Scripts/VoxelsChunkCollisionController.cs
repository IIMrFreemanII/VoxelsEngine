using UnityEngine;
using UnityEngine.EventSystems;

namespace VoxelsEngine.Voxels.Scripts
{
    [RequireComponent(typeof(VoxelsChunkRenderer))]
    public class VoxelsChunkCollisionController : MonoBehaviour, IPointerDownHandler
    {
        private VoxelsChunkRenderer _voxelsChunkRenderer;
        private Camera _camera;
        private ContactPoint[] _contactPoints;
        public VoxelsChunk _voxelsChunk;
        private Rigidbody _rigidbody;
        public bool isKinematic;
        // private Vector3 position;
        // private Vector3 normal;

        private void Awake()
        {
            _voxelsChunkRenderer = GetComponent<VoxelsChunkRenderer>();
            _camera = Camera.main;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _voxelsChunk = _voxelsChunkRenderer.GetVoxelsChunk();
            _rigidbody.isKinematic = isKinematic;
        }

        private void HandleClickDestroyVoxel()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                bool hit = Physics.Raycast(ray, out RaycastHit hitInfo);
                if (hit)
                {
                    if (hitInfo.rigidbody)
                    {
                        // position = hitInfo.point;
                        // normal = hitInfo.normal;
                        Debug.Log(hitInfo.transform.name);
                        
                        Vector3 voxelWorldPos =
                            _voxelsChunkRenderer.GetVoxelWorldPos(hitInfo.point, hitInfo.normal, EditVoxelType.Remove);
                        RemoveVoxel(voxelWorldPos);
                    }
                }
            }
        }

        private void RemoveVoxel(Vector3 voxelWorldPos)
        {
            Vector3Int posInArr = _voxelsChunkRenderer.GetPosInArr(voxelWorldPos);
            _voxelsChunkRenderer.RemoveVoxel(posInArr);
        }

        private void HandleVoxelsCollision(float impulse, ContactPoint[] contactPoints)
        {
            bool hasDestroyedVoxels = false;
            float damage = impulse;
            
            for (int i = 0; i < contactPoints.Length; i++)
            {
                ContactPoint contactPoint = contactPoints[i];

                Vector3 voxelWorldPos =
                    _voxelsChunkRenderer.GetVoxelWorldPos(
                        contactPoint.point,
                        contactPoint.normal,
                        EditVoxelType.Remove
                    );
                Vector3Int posInArr = _voxelsChunkRenderer.GetPosInArr(voxelWorldPos);
                VoxelData voxelData = _voxelsChunk.GetCell(posInArr);
                float newDurability = voxelData.durability - damage;

                if (newDurability > 0)
                {
                    voxelData.durability = newDurability;
                    _voxelsChunkRenderer.UpdateVoxelData(posInArr, voxelData);
                }
                else
                {
                    _voxelsChunkRenderer.RemoveVoxelWithoutUpdate(posInArr);
                    hasDestroyedVoxels = true;
                }
            }

            if (hasDestroyedVoxels)
            {
                // Debug.Log($"Update: {name}, impulse: {impulse}");
                _voxelsChunkRenderer.UpdateSubMeshesChunk();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impulse = collision.impulse.magnitude;
            if (impulse >= 0.5f && _voxelsChunk)
            {
                _contactPoints = new ContactPoint[collision.contactCount];
                collision.GetContacts(_contactPoints);

                HandleVoxelsCollision(impulse, _contactPoints);
            }
        }

        // private void OnDrawGizmos()
        // {
        //     // Gizmos.color = Color.red;
        //     // Gizmos.DrawSphere(position, 0.1f);
        //     // Gizmos.color = Color.blue;
        //     // Gizmos.DrawRay(position, normal);
        //
        //     if (_contactPoints != null)
        //     {
        //         for (int i = 0; i < _contactPoints.Length; i++)
        //         {
        //             ContactPoint contactPoint = _contactPoints[i];
        //             Gizmos.color = Color.red;
        //             Gizmos.DrawSphere(contactPoint.point, 0.05f);
        //             Gizmos.color = Color.blue;
        //             Gizmos.DrawRay(contactPoint.point, contactPoint.normal);
        //         }
        //     }
        // }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            // if (eventData.button == PointerEventData.InputButton.Left)
            // {
            //     RaycastResult raycastResult = eventData.pointerPressRaycast;
            //     position = raycastResult.worldPosition;
            //     normal = raycastResult.worldNormal;
            //
            //     Vector3 voxelWorldPos =
            //         _voxelsChunkRenderer.GetVoxelWorldPos(position, normal, EditVoxelType.Remove);
            //     RemoveVoxel(voxelWorldPos);
            // }
        }
    }
}