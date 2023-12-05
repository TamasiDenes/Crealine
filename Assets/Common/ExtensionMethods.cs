using Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtensionMethods
{
    internal static class ExtensionMethods
    {
        public static int FindVertexIndex(this Vector3[] vertices, Assets.Vertex vertex)
        {
            return vertices.ToList().FindIndex(vertice => vertex.position == vertice);
        }
    }
}
