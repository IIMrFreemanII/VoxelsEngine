using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelsEngine.Voxels.Scripts
{
    [Serializable]
    public class VoxelsSubMesh
    {
        // [HideInInspector]
        public List<int> triangles = new List<int>();
        public Material material;
    }
}