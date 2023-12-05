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

        // k�l�nbs�g a base-hez k�pest: csak az adott pontok k�z�tt csin�l vonalakat
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
