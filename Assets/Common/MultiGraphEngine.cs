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
        (List<Intersection>,List<IntersectionLine>) CreatePointsAndLines()
        {
            List<Intersection> baseInterList = new List<Intersection>();
            List<IntersectionLine> lines = new List<IntersectionLine>();

            for (int i = 0; i < points.Count; i++)
            {
                List<Intersection> currentPointsInter = new List<Intersection>();
                for (int j = 0; j < points[i].Count; j++)
                {
                    Intersection lineIntersector = new Intersection();
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
