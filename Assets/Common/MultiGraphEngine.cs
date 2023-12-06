using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;

namespace LinearAlgebra
{
    // t�bb egym�st�l f�ggetlen szekvenci�lis pontot k�pes kezelni (pl mindk�t j�t�kos + falak)
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

        // igaz�b�l a baseInterList-et sz�ks�gtelen visszaadni, de �gy az is �rtelmes �s haszn�lhat�
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

        // k�l�nbs�g a base-hez k�pest: csak az adott pontok k�z�tt csin�l vonalakat
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
