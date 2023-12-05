using Assets;
using LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tail : MonoBehaviour
{
    [SerializeField] Player _player;

    [SerializeField] float _pointSpacing = .1f;
    [SerializeField] int _numberOfPoints = 40;

    [SerializeField] List<Vector2> points = new List<Vector2>();

    public List<Vector2> Points => points;
    Vector2 headPos => _player.GetSnakePosition();

    Vector2 lastTailPosition;
    Vector2 lastHead;

    LineRenderer line;
    EdgeCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        col = GetComponent<EdgeCollider2D>();

        SetPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(points.Last(), headPos) > _pointSpacing)
        {
            if (points.Count < _numberOfPoints)
                SetPoint();
            else
                RedrawPoints();
        }
    }

    void SetPoint()
    {
        if (points.Count > 1)
            col.points = points.ToArray();

        points.Add(headPos);

        line.positionCount = points.Count;
        line.SetPosition(points.Count - 1, headPos);

        lastHead = points.Last();

        if (points.Count > 1)//&& CheckCreatedPoint())
           _player.RefreshGraph();
    }

    void RedrawPoints()
    {
        bool refreshGraph = false;
        col.points = points.ToArray();

        refreshGraph |= CheckDisapearedPoint();

        lastTailPosition = points.First();

        points.RemoveAt(0);

        lastHead = points.Last();

        points.Add(headPos);

        // redraw
        for (int i = 0; i < points.Count(); i++)
        {
            line.SetPosition(i, points[i]);
        }

        refreshGraph |= CheckCreatedPoint();

        //if (refreshGraph)
            _player.RefreshGraph();
    }

    // if the line which is disapeared now intersect any other line -> need a refresh, blobs might be changed
    bool CheckDisapearedPoint()
    {
        Line lastLine = new Line() { startPoint = points.First(), endPoint = points[1] };

        // 3: shouldn't interfere with neighbours
        for (int i = 3; i < points.Count; i++)
        {
            Line actLine = new Line() { startPoint = points[i], endPoint = points[i - 1] };
            if (LineIntersector.IsIntersecting(lastLine, actLine).Item1)
            {
                return true;
            }
        }

        return false;
    }

    // if the newly created line intersect any other -> need a refresh, new blobs might be created
    bool CheckCreatedPoint()
    {
        Line firstLine = new Line() { startPoint = points.Last(), endPoint = points[points.Count - 2] };

        // 3: shouldn't interfere with neighbours
        for (int i = points.Count - 3; i > 0; i--)
        {
            Line actLine = new Line() { startPoint = points[i], endPoint = points[i - 1] };
            if (LineIntersector.IsIntersecting(firstLine, actLine).Item1)
            {
                return true;
            }
        }

        return false;
    }
}
