using LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private List<WallPoint> wallPoints = new List<WallPoint>();
    [SerializeField] private List<Vector2> points = new List<Vector2>();
    public List<Vector2> Points => points;

    LineRenderer lineRenderer;
    EdgeCollider2D col;


    void Awake()
    {
        foreach (WallPoint item in transform.gameObject.GetComponentsInChildren<WallPoint>())
        {
            wallPoints.Add(item);
        }

        lineRenderer = GetComponent<LineRenderer>();
        col = GetComponent<EdgeCollider2D>();
        points = wallPoints.Select(p => new Vector2(p.transform.position.x, p.transform.position.y)).ToList();


        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        col.points = points.ToArray();
    }

    public bool ContainsPoint(Vector2 point)
    {
        if(points.First() == point || points.Last() == point) 
            return true;

        for (int i = 0; i < points.Count - 1; i++)
        {
            if (LineIntersector.IsPointInLine(new Line() { startPoint = points[i], endPoint = points[i + 1] }, point))
                return true;
        }

        return false;
    }
}
