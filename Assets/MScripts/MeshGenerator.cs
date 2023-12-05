using Assets;
using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices = { };
    int[] triangles = { };
    List<Triangle> triangleList = new List<Triangle>();

    public void Awake()
    {
        mesh = new Mesh();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void GenerateBlobs(List<List<Vector3>> blobList)
    {
        List<Vector3> verticeList = new List<Vector3>();
        triangleList = new List<Triangle>();


        foreach (List<Vector3> blob in blobList)
        {
            foreach (Vector3 point in blob)
            {
                if (!verticeList.Contains(point))
                    verticeList.Add(point);
            }
        };

        foreach (List<Vector3> blob in blobList)
        {
            List<Triangle> localTriangleList = new List<Triangle>();

            if (Triangulator.TryTriangulateConcavePolygon(blob, out localTriangleList))
            {
                if (IsRemoveableContainerBlob(verticeList, localTriangleList))
                    continue;

                triangleList.AddRange(localTriangleList);
            }
        }

        // conversion to index based structure
        this.vertices = verticeList.ToArray();

        triangles = new int[triangleList.Count * 3];
        int index = 0;
        foreach (Triangle triangle in triangleList)
        {
            triangles[index] = vertices.FindVertexIndex(triangle.v1);
            triangles[index + 1] = vertices.FindVertexIndex(triangle.v2);
            triangles[index + 2] = vertices.FindVertexIndex(triangle.v3);

            index += 3;
        }
        UpdateMesh();
    }

    private bool IsRemoveableContainerBlob(List<Vector3> verticeList, List<Triangle> localTriangleList)
    {
        // Container blob filter
        // if any triangle contains vertice which is not it's edge -> the blob is container blob

        foreach (Triangle triangle in localTriangleList)
        {
            foreach (Vector3 point in verticeList)
            {
                // it is ok if one of the edges
                if (point == triangle.v1.position || point == triangle.v2.position || point == triangle.v3.position)
                    continue;

                if (triangle.PointInTriangle(point))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool ContainsPoint(Vector3 point)
    {
        return triangleList.Any(triangle => triangle.PointInTriangle(point));
    }
}
