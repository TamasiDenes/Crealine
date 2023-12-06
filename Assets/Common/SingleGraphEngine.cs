using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

namespace LinearAlgebra
{
    // assume that the given points are in one sequence (player's snake, or one wall)
    public class SingleGraphEngine : GraphEngineBase
    {
        public SingleGraphEngine(List<Vector2> pointList)
        {
            points = pointList;
        }

        // given points
        List<Vector2> points = new List<Vector2>();

        public override void PrepareGraph()
        {
            origIntersections = CreatePointIntersections();
            origInterLines = CreateInterLines();

            completeIntersections = AddIntersections();

            ReconnectIntersections();
        }

        // set point neighbours based on the sequence
        List<Intersection> CreatePointIntersections()
        {
            List<Intersection> pointsInter = new List<Intersection>();

            for (int i = 0; i < points.Count; i++)
            {
                Intersection lineIntersector = new Intersection(points[i]);
                pointsInter.Add(lineIntersector);
            }


            for (int i = 0; i < pointsInter.Count; i++)
            {
                if (i != 0)
                    pointsInter[i].AddNeighbour(pointsInter[i - 1]);

                if (i != points.Count - 1)
                    pointsInter[i].AddNeighbour(pointsInter[i + 1]);
            }

            return pointsInter;
        }
    }
}
