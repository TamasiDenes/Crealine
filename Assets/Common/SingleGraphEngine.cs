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
    // jelenleg nem használjuk
    // azt feltételezi hogy a kapott pontok egy szekvenciát alkotnak (pl egy játékos, vagy egy egybefüggõ lineáris fal)
    public class SingleGraphEngine : GraphEngineBase
    {
        public SingleGraphEngine(List<Vector2> pointList)
        {
            points = pointList;
        }

        // kapott pontok:
        List<Vector2> points = new List<Vector2>();

        public override void PrepareGraph()
        {
            baseInterList = CreatePointIntersections();
            lines = CreateInterLines();

            fullInterList = AddIntersections();

            ReconnectIntersections();
        }

        // szekvenciálisan határozzuk meg a pontok szoszédait
        List<Intersection> CreatePointIntersections()
        {
            List<Intersection> pointsInter = new List<Intersection>();

            for (int i = 0; i < points.Count; i++)
            {
                Intersection lineIntersector = new Intersection();
                lineIntersector.intersectingPoint = points[i];
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
