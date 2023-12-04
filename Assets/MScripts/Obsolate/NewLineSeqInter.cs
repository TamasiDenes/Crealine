using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;

namespace LinearAlgebra
{
    public class NewLineSeqInter : MonoBehaviour
    {
        public MeshGenerator meshGenerator;

        public LineRenderer lineRenderer;
        public Transform p1;
        public Transform p2;
        public Transform p3;
        public Transform p4;
        public Transform p5;
        public Transform p6;
        public Transform p7;
        public Transform p8;
        public Transform p9;
        public Transform p10;

        List<Vector2> points = new List<Vector2>();
        List<LineIntersector> pointInterList = new List<LineIntersector>();
        List<InterLine> lines = new List<InterLine>();
        List<LineIntersector> intersectList = new List<LineIntersector>();

        // Start is called before the first frame update
        void Start()
        {
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        }

        // Update is called once per frame
        void Update()
        {
            points = new List<Vector2>();
            lines = new List<InterLine>();

            AddPoint(0, p1.position);
            AddPoint(1, p2.position);
            AddPoint(2, p3.position);
            AddPoint(3, p4.position);
            AddPoint(4, p5.position);
            AddPoint(5, p6.position);
            AddPoint(6, p7.position);
            AddPoint(7, p8.position);
            AddPoint(8, p9.position);
            AddPoint(9, p10.position);


            pointInterList = CreatePointIntersections();
            lines = CreateInterLines();
            

            lineRenderer.material.color = Color.red;

            intersectList = AddIntersections(lineRenderer);

            ReconnectIntersections();

            List<List<Vector2>> blobs = new List<List<Vector2>>();

            for (int i = 0; i < intersectList.Count; i++)
            {
                List<List<Vector2>> foundedBlobs = ProcessInterPathVersion(intersectList[i]);
                blobs.AddRange(foundedBlobs);
            }


            // TODO: még nem ártana egy szûrés, hogy amiket többször megtaláltunk azokból csak egy legyen
            // plusz a "nagy" blobot ki kellene majd szûrni! 

            List<List<Vector3>> blobs3D = new List<List<Vector3>>();

            foreach (List<Vector2> blob in blobs)
            {
                List<Vector3> currentBlob = new List<Vector3>();

                foreach (Vector3 point in blob)
                {
                    currentBlob.Add(new Vector3(point.x, point.y, 0));
                }
                blobs3D.Add(currentBlob);
            }

            SingleGraphEngine engine = new SingleGraphEngine(points);
            meshGenerator.GenerateBlobs(engine.ProcessGraph());

        }

        private void ReconnectIntersections()
        {
            //foreach (InterLine InterLine in lines)

            //bool beforeFirstIntersection = true;
            //bool afterLastIntersection = false;

            for (int j = 0; j < lines.Count; j++)
            {
                InterLine interLine = lines[j];

                List<LineIntersector> relatedIntersections = new List<LineIntersector>();

                foreach(LineIntersector inter in intersectList)
                {
                    if(LineIntersector.IsPointInLine(interLine.line, inter.intersectingPoint))
                    {
                        relatedIntersections.Add(inter);
                    }
                }

                relatedIntersections.Sort((i1,i2) => ((interLine.line.startPoint - i1.intersectingPoint).magnitude > (interLine.line.startPoint - i2.intersectingPoint).magnitude) ? 1 : -1);

                if(relatedIntersections.Count > 0)
                {
                    //if(beforeFirstIntersection)
                    //{
                    //    beforeFirstIntersection = false;
                    //}

                    LineIntersector.Reconnect(interLine.startPoint, relatedIntersections.First(), interLine.endPoint, interLine.startPoint);
                    LineIntersector.Reconnect(relatedIntersections.Last(), interLine.endPoint, interLine.endPoint, interLine.startPoint);
                }

                // ha nincs legalább kettõ találat, nincs változtatás
                if (relatedIntersections.Count < 2)
                    continue;

                for(int i = 0; i < relatedIntersections.Count - 2; i++) 
                {
                    LineIntersector.Reconnect(relatedIntersections[i], relatedIntersections[i + 1], interLine.endPoint, interLine.startPoint);
                }

                LineIntersector.Reconnect(relatedIntersections[relatedIntersections.Count - 2], relatedIntersections.Last(), interLine.endPoint, interLine.startPoint);
            }
        }

        private List<LineIntersector> AddIntersections(LineRenderer lineRenderer)
        {
            List<LineIntersector> result = new List<LineIntersector>();

            for (int i = 0; i < lines.Count - 2; i++)
            {
                for (int j = i + 2; j < lines.Count; j++)
                {
                    (bool, Vector2) interPoint = LineIntersector.IsIntersecting(lines[i].line, lines[j].line);
                    if (interPoint.Item1)
                    {
                        LineIntersector intersect = new LineIntersector() { intersectingPoint = interPoint.Item2 };

                        intersect.AddNeighbour(lines[i], lines[j]);

                        lineRenderer.material.color = Color.blue;
                        result.Add(intersect);
                    }
                }
            }

            return result;
        }


        List<List<Vector3>> ProcessIntersection(LineIntersector inter)
        {
            // TODO: le kellene hozni minden irányba és elõre - hátra
            List<List<Vector3>> blobList = new List<List<Vector3>>();

            foreach(var neighbour in inter.neighbours)
            {
                List<Vector3> blob = new List<Vector3>();

                // maga a keresztezõdés:
                blob.Add(inter.intersectingPoint);

                int currentIndex = points.FindIndex(p => p == neighbour.Item1);
                LineIntersector currentIntersect = null;

                // a 0-s index mindig start point, tehát mindig visszafele kell elindulnunk
                bool incrementalStep = !neighbour.Item2;
                while (inter != currentIntersect)
                {
                    blob.Add(points[currentIndex]);

                    currentIndex += incrementalStep ? 1 : -1;

                    // ha valahogy a végére értünk hagyjuk - lehet kelleni fog majd késõbb
                    if (currentIndex == points.Count || currentIndex < 0)
                    {
                        blob.Clear();
                        break;
                    }

                    currentIntersect = intersectList.Find(i => i.HasElement(points[currentIndex]));

                    if (currentIntersect != null && currentIntersect != inter)
                    {
                        blob.Add(currentIntersect.intersectingPoint);

                        Vector2 currentVector;
                        (currentVector, incrementalStep) = currentIntersect.GetRightOuterNeighbour(points[currentIndex]);

                        currentIndex = points.FindIndex(p => p == currentVector);
                    }
                }

                if(blob.Count != 0)
                    blobList.Add(blob);
            }

            return blobList;
        }
        List<List<Vector2>> ProcessInterPathVersion(LineIntersector inter)
        {
            // TODO: le kellene hozni minden irányba és elõre - hátra
            List<List<Vector2>> blobList = new List<List<Vector2>>();

            for (int i = 0; i < inter.neighbourInter.Count; i++)
            {
                List<Vector2> blob = ProcessPath(inter, inter.neighbourInter[i]);

                if (blob.Count != 0)
                    blobList.Add(blob);
            }


            return blobList;
        }

        List<Vector2> ProcessPath(LineIntersector origIntersect, LineIntersector origNeighbour)
        {
            List<Vector2> resultBlob = new List<Vector2>();

            LineIntersector currentNeighbour = origNeighbour;
            LineIntersector prevNeighbour = origIntersect;

            float angleInDeg = 0;

            do
            {
                LineIntersector nextNeighbour = null;

                if (currentNeighbour.neighbourInter.Count == 1)
                {
                    // simán visszafordulunk
                    nextNeighbour = currentNeighbour.neighbourInter[0];

                    // visszafordulunk és addig megyünk amíg egy rendes keresztezõdéshez nem érünk
                    while (nextNeighbour.neighbourInter.Count != 4)
                    {
                        // minden sima elemet kiveszünk a blob-ból
                        resultBlob.Remove(nextNeighbour.intersectingPoint);
                        LineIntersector nextTemp = nextNeighbour.GetOppositeNeighbour(currentNeighbour);
                        currentNeighbour = nextNeighbour;
                        nextNeighbour = nextTemp;
                    }

                    // és a keresztezõdésnél balra kanyarodva folytathatjuk az utunk
                    LineIntersector next = nextNeighbour.GetLeftOuterNeighbour(currentNeighbour);
                    currentNeighbour = nextNeighbour;
                    nextNeighbour = next;
                }
                else
                {
                    nextNeighbour = currentNeighbour.GetLeftOuterNeighbour(prevNeighbour);
                }

                // redundáns elemek törlése
                if (resultBlob.Contains(currentNeighbour.intersectingPoint))
                {
                    // ha keresztezõdésnél találtuk meg magunkat, vissza indulva minden pontot ki kell szedni addig amíg elsõként nem találkoztunk ezzel a keresztezõdéssel
                    if (currentNeighbour.neighbourInter.Count == 4)
                    {
                        Vector2 currentPoint = resultBlob.Last();
                        while (currentPoint != currentNeighbour.intersectingPoint)
                        {
                            resultBlob.Remove(currentPoint);
                            currentPoint = resultBlob.Last();
                        }
                    } // egyébként pedig csak simán ki kell szedni mindent
                    else
                    {
                        resultBlob.Remove(currentNeighbour.intersectingPoint);
                    }
                }
                else if (currentNeighbour.neighbourInter.Count != 1) // végeket ne adjuk hozzá
                {
                    resultBlob.Add(currentNeighbour.intersectingPoint);
                }
                

                // angleInDeg += Vector2.SignedAngle(nextNeighbour.intersectingPoint - currentNeighbour.intersectingPoint, prevNeighbour.intersectingPoint - currentNeighbour.intersectingPoint);

                prevNeighbour = currentNeighbour;
                currentNeighbour = nextNeighbour;

                // ha visszaértünk az eredeti keresztezõdéshez álljunk meg
                if (prevNeighbour == origIntersect)
                {
                    // ha viszont nem ugyanarra a szomszédra kanyarodnánk rá ahonnan elindultunk - nincs talált blob
                    if (currentNeighbour != origNeighbour)
                    {
                        resultBlob.Clear();
                    }

                    break;
                }
            }
            while (true);

            if (resultBlob.Count > 2)
            {
                float angle = 0;
                for (int i = 0; i < resultBlob.Count; i++)
                {
                    if (i == 0)
                        angle = Vector2.SignedAngle(resultBlob[1] - resultBlob.First(), resultBlob.Last() - resultBlob.First());
                    else if(i == resultBlob.Count - 1)
                        angle = Vector2.SignedAngle(resultBlob.First() - resultBlob.Last(), resultBlob[i - 1] - resultBlob.Last());
                    else
                        angle = Vector2.SignedAngle(resultBlob[i + 1] - resultBlob[i], resultBlob[i - 1] - resultBlob[i]);

                    if(angle != 180 && angle != -180)
                           angleInDeg += angle;
                }
            }

            if (angleInDeg < 0)
                resultBlob.Reverse();

            return resultBlob;
        }

        void ReconnectPointGraph()
        {
            foreach (LineIntersector inter in intersectList)
            {
                foreach (LineIntersector neighbour in inter.neighbourInter)
                {
                    LineIntersector point = pointInterList.Find(p => p == neighbour);

                    if (point != null)
                    {
                        point.neighbourInter.RemoveAll(n => n == neighbour);
                        point.AddNeighbour(neighbour);
                    }
                }
            }
        }

        List<LineIntersector> CreatePointIntersections()
        {
            List<LineIntersector> pointsInter = new List<LineIntersector>();

            for (int i = 0; i < points.Count; i++)
            {
                LineIntersector lineIntersector = new LineIntersector();
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

        List<InterLine> CreateInterLines()
        {
            List<InterLine> result = new List<InterLine>();
            for(int i = 0; i < pointInterList.Count - 1; i++)
            {
                result.Add( new InterLine()
                { 
                    line = new Line() { startPoint = pointInterList[i].intersectingPoint, endPoint = pointInterList[i + 1].intersectingPoint},
                    startPoint = pointInterList[i], 
                    endPoint = pointInterList[i + 1]
                });
            }

            return result;
        }

        void AddPoint(int index, Vector2 point)
        {

            lineRenderer.SetPosition(index, point);
            points.Add(point);
        }

        public void OnDrawGizmos()
        {
            foreach(LineIntersector inter in intersectList)
            {
                Gizmos.DrawSphere(inter.intersectingPoint, 0.1f);
            }
        }
    }
}
