using LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Wall : MonoBehaviour
{
    List<WallPoint> wallPoints = new List<WallPoint>();
    [SerializeField] float interpolationTreshold = 1.5f;

    List<Vector2> points = new List<Vector2>();
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

        col.points = points.ToArray();

    }

    public bool ContainsPoint(Vector2 point)
    {
        if(points.First() == point || points.Last() == point) 
            return true;

        for (int i = 0; i < points.Count - 1; i++)
        {
            if (Line.IsPointInLine(new Line(points[i], points[i + 1]), point))
                return true;
        }

        return false;
    }

    public List<Vector2> Interpolate()
    {
        List<Vector2> interpolatedPoints = new List<Vector2>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            interpolatedPoints.Add(points[i]);

            // if the distance of between the to endpoint bigger than the treshold
            // insert a point and check it again as an endpoint
            Vector2 currentPoint = points[i];
            Vector2 shift = ((points[i + 1] - currentPoint) / (points[i + 1] - currentPoint).magnitude) * interpolationTreshold;


            while ((currentPoint - points[i+1]).magnitude > interpolationTreshold)
            {
                currentPoint = currentPoint + shift;
                interpolatedPoints.Add(currentPoint);
            } 
        }

        interpolatedPoints.Add(points.Last());

        return interpolatedPoints;
    }
}
