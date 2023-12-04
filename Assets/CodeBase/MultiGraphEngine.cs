using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;

namespace LinearAlgebra
{
    // több egymástól független szekvenciális pontot képes kezelni (pl mindkét játékos + falak)
    public class MultiGraphEngine : GraphEngineBase
    {
        public MultiGraphEngine(List<List<Vector2>> pointList) 
        {
            points = pointList;
        }

        // kapott pontok:
        List<List<Vector2>> points = new List<List<Vector2>>();


        public override void PrepareGraph()
        {
            (baseInterList,lines) = CreatePointsAndLines();

            fullInterList = AddIntersections();

            ReconnectIntersections();
        }

        // igazából a baseInterList-et szükségtelen visszaadni, de így az is értelmes és használható
        (List<LineIntersector>,List<InterLine>) CreatePointsAndLines()
        {
            List<LineIntersector> baseInterList = new List<LineIntersector>();
            List<InterLine> lines = new List<InterLine>();

            for (int i = 0; i < points.Count; i++)
            {
                List<LineIntersector> currentPointsInter = new List<LineIntersector>();
                for (int j = 0; j < points[i].Count; j++)
                {
                    LineIntersector lineIntersector = new LineIntersector();
                    lineIntersector.intersectingPoint = points[i][j];
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

                baseInterList.AddRange(currentPointsInter);
            }

            return (baseInterList,lines);
        }

        // különbség a base-hez képest: csak az adott pontok között csinál vonalakat
        List<InterLine> CreateInterLines(List<LineIntersector> currentPointInterList)
        {
            List<InterLine> result = new List<InterLine>();
            for(int i = 0; i < currentPointInterList.Count - 1; i++)
            {
                result.Add( new InterLine()
                { 
                    line = new Line() { startPoint = currentPointInterList[i].intersectingPoint, endPoint = currentPointInterList[i + 1].intersectingPoint},
                    startPoint = currentPointInterList[i], 
                    endPoint = currentPointInterList[i + 1]
                });
            }

            return result;
        }
    }
}
