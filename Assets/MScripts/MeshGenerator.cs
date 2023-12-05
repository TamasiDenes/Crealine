using Assets;
using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices = { };
    int[] triangles = { };

    // segédváltozó annak megállapítására, hogy egy pont benne van-e valamelyik háromszögbe
    List<Triangle> triangleList = new List<Triangle>();

    public void Awake()
    {
        mesh = new Mesh();
    }

    // Start is called before the first frame update
    void Start()
    {
        //mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void Generate(List<Vector3> blob)
    {
        List<Vector3> verticeList = new List<Vector3>();
        triangleList = new List<Triangle>();

        foreach (Vector3 point in blob)
        {
            if (!verticeList.Contains(point))
                verticeList.Add(point);
        }

        List<Triangle> localTriangleList = new List<Triangle>();
        if (!Triangulator.TryTriangulateConcavePolygon(blob, out localTriangleList))
        {
            List<Vector3> reverseBlob = new List<Vector3>(blob);
            reverseBlob.Reverse();
            Triangulator.TryTriangulateConcavePolygon(reverseBlob, out localTriangleList);
        }

        triangleList.AddRange(localTriangleList);

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
    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    internal void GenerateBlobs(List<List<Vector3>> blobList)
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
                if (RemoveContainerBlobTriangles(verticeList, localTriangleList))
                    continue;

                triangleList.AddRange(localTriangleList);
            }
        }

        // szolgai convertálás
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

    private bool RemoveContainerBlobTriangles(List<Vector3> verticeList, List<Triangle> localTriangleList)
    {
        bool isAnyRemovable = false;

        // "nagy" blob kiszûrése
        // ha a talált háromszögek valamelyike tartalmaz vertice-t ami nem az egyik csúcsa
        // az annak a jele, hogy a nagy blobról van szó
        List<Triangle> removableTriangles = new List<Triangle>();
        foreach (Triangle triangle in localTriangleList)
        {
            foreach (Vector3 point in verticeList)
            {
                // valamelyik csúcsa - az rendben van
                if (point == triangle.v1.position || point == triangle.v2.position || point == triangle.v3.position)
                    continue;

                // kiszûrendõ
                if (triangle.PointInTriangle(point))
                {
                    isAnyRemovable |= true;
                    removableTriangles.Add(triangle);
                }
            }
        }
        removableTriangles.ForEach(triangle => localTriangleList.Remove(triangle));

        return isAnyRemovable;
    }

    public bool ContainsPoint(Vector3 point)
    {
        return triangleList.Any(triangle => triangle.PointInTriangle(point));
    }
}
