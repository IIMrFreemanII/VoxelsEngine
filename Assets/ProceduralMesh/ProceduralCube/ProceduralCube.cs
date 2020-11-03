using System.Collections.Generic;
using UnityEngine;
using VoxelsEngine.Voxels.Scripts;

namespace ProceduralMesh.ProceduralCube
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class ProceduralCube : MonoBehaviour
    {
        public float scale = 1f;
        public int posX;
        public int posY;
        public int posZ;
        
        private float adjustedScale;
        
        private Mesh mesh;
        private List<Vector3> verticies;
        private List<int> triangles;
        
        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            adjustedScale = scale * 0.5f;
        }

        private void Start()
        {
            MakeCube(adjustedScale, new Vector3(posX, posY, posZ) * scale);
            UpdateMesh();
        }

        private void MakeCube(float scale, Vector3 pos)
        {
            verticies = new List<Vector3>();
            triangles = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                MakeFace(i, scale, pos);
            }
        }

        private void MakeFace(int dir, float scale, Vector3 facePos)
        {
            verticies.AddRange(VoxelMeshData.FaceVertices(dir, scale, facePos));

            int vertCount = verticies.Count;
            
            triangles.Add(vertCount - 4);
            triangles.Add(vertCount - 4 + 1);
            triangles.Add(vertCount - 4 + 2);
            triangles.Add(vertCount - 4);
            triangles.Add(vertCount - 4 + 2);
            triangles.Add(vertCount - 4 + 3);
        }

        private void UpdateMesh()
        {
            mesh.Clear();

            mesh.vertices = verticies.ToArray();
            mesh.triangles = triangles.ToArray();
        
            mesh.RecalculateNormals();
        }
    }
}