using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;

namespace LinearAlgebra
{

    // OBSOLATE
    public class LineSeqIntersection : MonoBehaviour
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
        List<Line> lines = new List<Line>();
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
            lines = new List<Line>();

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



            lineRenderer.material.color = Color.red;

            intersectList = AddIntersections(lineRenderer);

            // Reconnect(intersectList);

            List<List<Vector2>> blobs = new List<List<Vector2>>();

            foreach(LineIntersector inter in intersectList)
            {
                List<List<Vector2>> foundedBlobs = ProcessInterPathVersion(inter);
                blobs.AddRange(foundedBlobs);
            }

            // TODO: m�g nem �rtana egy sz�r�s, hogy amiket t�bbsz�r megtal�ltunk azokb�l csak egy legyen
            // plusz a "nagy" blobot ki kellene majd sz�rni! 

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

            meshGenerator.GenerateBlobs(blobs3D);

        }

        private void Reconnect(List<LineIntersector> intersectList)
        {
            foreach (Line line in lines)
            {
                List<LineIntersector> relatedIntersections = new List<LineIntersector>();

                foreach(LineIntersector inter in intersectList)
                {
                    if(LineIntersector.IsPointInLine(line, inter.intersectingPoint))
                    {
                        relatedIntersections.Add(inter); 
                    }
                }

                relatedIntersections.Sort((i1,i2) => ((line.startPoint - i1.intersectingPoint).magnitude > (line.startPoint - i2.intersectingPoint).magnitude) ? 1 : -1);

                // ha nincs legal�bb kett� tal�lat, nincs v�ltoztat�s
                if (relatedIntersections.Count < 2)
                    continue;

                for(int i = 0; i < relatedIntersections.Count - 2; i++) 
                {
                    LineIntersector.Reconnect(relatedIntersections[i], relatedIntersections[i + 1], line.endPoint, line.startPoint);
                }

                LineIntersector.Reconnect(relatedIntersections[relatedIntersections.Count - 2], relatedIntersections.Last(), line.endPoint, line.startPoint);
            }
        }

        private List<LineIntersector> AddIntersections(LineRenderer lineRenderer)
        {
            List<LineIntersector> result = new List<LineIntersector>();

            for (int i = 0; i < lines.Count - 2; i++)
            {
                for (int j = i + 2; j < lines.Count; j++)
                {
                    (bool, Vector2) interPoint = LineIntersector.IsIntersecting(lines[i], lines[j]);
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
            // TODO: le kellene hozni minden ir�nyba �s el�re - h�tra
            List<List<Vector3>> blobList = new List<List<Vector3>>();

            foreach(var neighbour in inter.neighbours)
            {
                List<Vector3> blob = new List<Vector3>();

                // maga a keresztez�d�s:
                blob.Add(inter.intersectingPoint);

                int currentIndex = points.FindIndex(p => p == neighbour.Item1);
                LineIntersector currentIntersect = null;

                // a 0-s index mindig start point, teh�t mindig visszafele kell elindulnunk
                bool incrementalStep = !neighbour.Item2;
                while (inter != currentIntersect)
                {
                    blob.Add(points[currentIndex]);

                    currentIndex += incrementalStep ? 1 : -1;

                    // ha valahogy a v�g�re �rt�nk hagyjuk - lehet kelleni fog majd k�s�bb
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
            // TODO: le kellene hozni minden ir�nyba �s el�re - h�tra
            List<List<Vector2>> blobList = new List<List<Vector2>>();

            foreach (var neighbour in inter.neighbours)
            {
                List<Vector2> blob = ProcessPath(inter, neighbour);

                if (blob.Count != 0)
                    blobList.Add(blob);
            }

            return blobList;
        }

        List<Vector2> ProcessPath(LineIntersector origIntersect, (Vector2, bool) currentNeighbour)
        {
            List<Vector2> resultBlob = new List<Vector2>();
            LineIntersector currentIntersect = null;
            LineIntersector prevIntersect = origIntersect;
            int currentIndex = points.FindIndex(p => p == currentNeighbour.Item1);
            bool incrementalStep = currentNeighbour.Item2;

            resultBlob.Add(origIntersect.intersectingPoint);

            while (true)
            {

                resultBlob.Add(points[currentIndex]);
                    // ne tal�ljuk v�letlen pont az el�z� keresztz�d�st mint ahonnan j�tt�nk
                currentIntersect = intersectList.Find(i => i != prevIntersect && i.HasElement(points[currentIndex]));


                if (currentIntersect != null)
                {
                    // egy m�sik keresztez�d�shez �rt�nk - felvessz�k �s ki�rt�kelj�k merre tov�bb
                    if (currentIntersect != origIntersect)
                    {
                        // ha m�r tal�lkoztunk a keresztez�d�shez �s felvett�k egyszer �s megint ide jutottunk akkor val�j�ban egy m�sik blobot tal�ltunk meg
                        // nem az eredeti kiindul�b�l l�trej�v�t - ezt elvetj�k - majd m�s megtal�lja
                        if (resultBlob.Contains(currentIntersect.intersectingPoint))
                        {
                            int lastOccurenceIndex = resultBlob.FindIndex(i => i == currentIntersect.intersectingPoint);
                            resultBlob.RemoveRange(lastOccurenceIndex, resultBlob.Count - lastOccurenceIndex);
                            // nem csak kivonjuk bel�le a nem oda ill�t, hanem t�nylegesen elvetj�k, mert megv�ltozhat az ir�nyults�g ami ment�n pont a kil�g� r�szeket fogja �br�zolni
                            break;
                        }

                        resultBlob.Add(currentIntersect.intersectingPoint);

                        Vector2 currentVector;
                        (currentVector, incrementalStep) = currentIntersect.GetLeftOuterNeighbour(points[currentIndex]);

                        currentIndex = points.FindIndex(p => p == currentVector);

                        prevIntersect = currentIntersect;
                    }
                    else // vissza�rt�nk az eredeti keresztez�d�shez - meg�llhatunk
                    {
                        // csak abban az esetben valid a tal�lt blob, ha a keresztez�d�sb�l tov�bb kanyarodva val�ban az eredetileg kiindul� szomsz�dot tal�ljuk meg
                        // ez kell ahhoz, hogy a h�romsz�gel�sn�l csak �ramutat� j�r�s�val ellent�tes ir�nyults�g� blobok k�pz�djenek
                        if (currentIntersect.GetLeftOuterNeighbour(points[currentIndex]) != currentNeighbour)
                            resultBlob.Clear();

                        break;
                    }
                }
                else // ha olyan pontn�l vagyunk amelyik nem keresztez�d�shez tartozik - csak sim�n megy�nk tov�bb a k�gy�n
                {
                    prevIntersect = null;
                    currentIndex += incrementalStep ? 1 : -1;
                }

                // ha valahogy a v�g�re �rt�nk hagyjuk - lehet kelleni fog majd k�s�bb
                if (currentIndex == points.Count || currentIndex < 0)
                {
                    resultBlob.Clear();
                    break;
                }
            }
            return resultBlob;
        }

        List<LineIntersector> CreatePointGraph()
        {
            List<LineIntersector> pointsInter = CreatePointIntersections();

            foreach (LineIntersector inter in intersectList)
            {
                foreach ((Vector2, bool) neighbour in inter.neighbours)
                {
                    int pointIndex = pointsInter.FindIndex(point => point.intersectingPoint == neighbour.Item1);

                    if (pointIndex != -1)
                    {
                        pointsInter[pointIndex].neighbours.RemoveAll(n => n.Item2 == neighbour.Item2);
                        pointsInter[pointIndex].AddNeighbour(neighbour);
                    }
                }
            }

            return pointsInter;
        }

        List<LineIntersector> CreatePointIntersections()
        {
            List<LineIntersector> pointsInter = new List<LineIntersector>();

            for (int i = 0; i < points.Count; i++)
            {
                LineIntersector lineIntersector = new LineIntersector();
                lineIntersector.intersectingPoint = points[i];
                if (i != 0)
                    lineIntersector.AddNeighbour((points[i--], false));

                if (i != points.Count - 1)
                    lineIntersector.AddNeighbour((points[i++], true));

                pointsInter.Add(lineIntersector);
            }

            return pointsInter;
        }

        void AddPoint(int index, Vector2 point)
        {

            lineRenderer.SetPosition(index, point);
            points.Add(point);

            if (index != 0)
            {
                lines.Add(new Line() { startPoint = points[index - 1], endPoint = points[index] });
            }
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
