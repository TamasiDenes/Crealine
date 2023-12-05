using Assets;
using LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tail : MonoBehaviour
{
    Player player;

    [SerializeField] float pointSpacing = .1f;
    [SerializeField] int numberOfPoints = 80;

    List<Vector2> points = new List<Vector2>();
    public List<Vector2> Points => points;

    Vector2 headPos => player.GetSnakePosition();

    LineRenderer line;
    EdgeCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.gameObject.GetComponent<Player>();
        line = GetComponent<LineRenderer>();
        col = GetComponent<EdgeCollider2D>();

        SetPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(points.Last(), headPos) > pointSpacing)
        {
            if (points.Count < numberOfPoints)
                SetPoint();
            else
                RedrawPoints();

            if (points.Count > 1)
                player.RefreshGraph();
        }
    }

    void SetPoint()
    {
        if (points.Count > 1)
            col.points = points.ToArray();

        points.Add(headPos);

        line.positionCount = points.Count;
        line.SetPosition(points.Count - 1, headPos);

    }

    void RedrawPoints()
    {
        col.points = points.ToArray();

        points.RemoveAt(0);

        points.Add(headPos);

        // redraw
        for (int i = 0; i < points.Count(); i++)
        {
            line.SetPosition(i, points[i]);
        }
    }
}
