using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;

namespace LinearAlgebra
{
    // can handle more independent sequence (two player and the walls)
    public class MultiGraphEngine : GraphEngineBase
    {
        public MultiGraphEngine(List<List<Vector2>> pointList) 
        {
            points = pointList;
        }

        List<List<Vector2>> points = new List<List<Vector2>>();


        public override void PrepareGraph()
        {
            origInterLines = CreatePointsAndLines();

            completeIntersections = AddIntersections();

            ReconnectIntersections();
        }

        List<IntersectionLine> CreatePointsAndLines()
        {
            List<IntersectionLine> lines = new List<IntersectionLine>();

            for (int i = 0; i < points.Count; i++)
            {
                List<Intersection> currentPointsInter = new List<Intersection>();
                for (int j = 0; j < points[i].Count; j++)
                {
                    Intersection lineIntersector = new Intersection(points[i][j]);
                    currentPointsInter.Add(lineIntersector);
                }


                for (int j = 0; j < currentPointsInter.Count; j++)
                {
                    if (j != 0)
                        currentPointsInter[j].AddNeighbour(currentPointsInter[j - 1]);

                    if (j != points[i].Count - 1)
                        currentPointsInter[j].AddNeighbour(currentPointsInter[j + 1]);
                }

                lines.AddRange(CreateInterLines(currentPointsInter));
            }

            return lines;
        }

        List<IntersectionLine> CreateInterLines(List<Intersection> currentPointInterList)
        {
            List<IntersectionLine> result = new List<IntersectionLine>();
            for(int i = 0; i < currentPointInterList.Count - 1; i++)
            {
                result.Add( new IntersectionLine(
                    currentPointInterList[i], 
                    currentPointInterList[i + 1]));
            }

            return result;
        }
    }
}
