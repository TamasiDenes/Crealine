using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static UnityEngine.ParticleSystem;

namespace LinearAlgebra
{
    public abstract class GraphEngineBase
    {
        // sz�rmaztatott h�l�zat:
        // tartalmazza az eredeti h�l�zat pontjait eredeti szomsz�ds�gaikkal:
        protected List<LineIntersector> baseInterList = new List<LineIntersector>();
        // eredeti h�l�zat pontjai k�z�tti vonalakat tartalmazza - amik a v�geiken refer�lnak a pontokra is:
        protected List<InterLine> lines = new List<InterLine>();
        // a megtal�lt keresztez�d�seket is tartalmaz� pontok (minden benne van):
        protected List<LineIntersector> fullInterList = new List<LineIntersector>();

        // baseInterList -> lines -> fullInterList
        public abstract void PrepareGraph();

        // az�rt van r� sz�ks�g, mert a MeshGenerator-nak �gy kellenek a blobok
        protected List<List<Vector3>> ConvertBlobsTo3D(List<List<Vector2>> blobs2D)
        {
            List<List<Vector3>> result = new List<List<Vector3>>();

            foreach (List<Vector2> blob in blobs2D)
            {
                List<Vector3> currentBlob = new List<Vector3>();

                foreach (Vector3 point in blob)
                {
                    currentBlob.Add(new Vector3(point.x, point.y, 0));
                }
                result.Add(currentBlob);
            }

            return result;
        }

        // minden tal�lt keresztez�d�s alapb�l szomsz�dk�nt az eredeti gr�f pontjaira mutat
        // itt megkeress�k az adott egyenesen k�zvetlen szomsz�ds�gokat �s �jra huzalozzuk a h�l�zatot
        protected void ReconnectIntersections()
        {
            for (int j = 0; j < lines.Count; j++)
            {
                InterLine interLine = lines[j];

                List<LineIntersector> relatedIntersections = new List<LineIntersector>();

                // adott vonalhoz tartoz� keresztez�d�sek, pontok
                foreach (LineIntersector inter in fullInterList)
                {
                    if (LineIntersector.IsPointInLine(interLine.line, inter.intersectingPoint))
                    {
                        relatedIntersections.Add(inter);
                    }
                }

                // vonalon az egyik v�gpontt�l m�rt t�vols�gok szerint sorba rendezz�k a tal�lt pontokat
                relatedIntersections.Sort((i1, i2) => ((interLine.line.startPoint - i1.intersectingPoint).magnitude > (interLine.line.startPoint - i2.intersectingPoint).magnitude) ? 1 : -1);

                // v�geket �sszek�tj�k az els�, utols� keresztez�d�ssel
                if (relatedIntersections.Count > 0)
                {
                    LineIntersector.Reconnect(interLine.startPoint, relatedIntersections.First(), interLine.endPoint, interLine.startPoint);
                    LineIntersector.Reconnect(relatedIntersections.Last(), interLine.endPoint, interLine.endPoint, interLine.startPoint);
                }

                // ha nincs legal�bb kett� tal�lat, nincs v�ltoztat�s
                if (relatedIntersections.Count < 2)
                    continue;

                // k�ztes keresztez�d�seket sorban �sszek�tj�k
                for (int i = 0; i < relatedIntersections.Count - 1; i++)
                {
                    LineIntersector.Reconnect(relatedIntersections[i], relatedIntersections[i + 1], interLine.endPoint, interLine.startPoint);
                }
            }
        }

        // v�gig megy�nk a vonalakon �s n�zz�k, hogy b�rmelyik m�sik nem k�zvetlen szomsz�dos vonallal van-e metsz�spont
        // ha igen, felvessz�k a keresztez�d�sek k�z�, �s egyel�re minden ilyen keresztez�d�snek az eredeti vonal v�gpontjait adom meg szomsz�dnak
        protected List<LineIntersector> AddIntersections()
        {
            List<LineIntersector> result = new List<LineIntersector>();

            // az�rt �gy, mert a szomsz�dosok mindig metszik egym�st de az nem jelent keresztez�d�st
            for (int i = 0; i < lines.Count - 2; i++)
            {
                for (int j = i + 2; j < lines.Count; j++)
                {
                    (bool, Vector2) interPoint = LineIntersector.IsIntersecting(lines[i].line, lines[j].line);
                    if (interPoint.Item1)
                    {
                        LineIntersector intersect = new LineIntersector() { intersectingPoint = interPoint.Item2 };

                        intersect.AddNeighbour(lines[i], lines[j]);

                        result.Add(intersect);
                    }
                }
            }

            return result;
        }

        // a gr�fot megkapva egy ir�nyhelyes keresztez�d�seket figyelembe vett blobokat tartalmaz� list�t ad vissza 3d-set hogy a triangulator is tudjon vele b�nni
        // lesz�ri, hogy minden blob csak egyszer szerepeljen benne:
        // PrepareGraph -> blobs -> uniqueBlobs -> blobs3D
        public List<List<Vector3>> ProcessGraph()
        {
            PrepareGraph();

            List<List<Vector2>> blobs = new List<List<Vector2>>();

            for (int i = 0; i < fullInterList.Count; i++)
            {
                List<List<Vector2>> foundedBlobs = ProcessInterPath(fullInterList[i]);
                blobs.AddRange(foundedBlobs);
            }


            // sz�r�s, hogy amiket t�bbsz�r megtal�ltunk azokb�l csak egy legyen
            List<List<Vector2>> uniqueBlobs = new List<List<Vector2>>();
            List<IOrderedEnumerable<Vector2>> orderedBlobs = new List<IOrderedEnumerable<Vector2>>();
            blobs.ForEach(blob => orderedBlobs.Add(blob.OrderBy(v => v.x)));

            for (int i = 0; i < orderedBlobs.Count - 1; i++)
            {
                bool isUnique = true;
                // mivel mindig csak a soron k�vetkez�kkel vetj�k �ssze, minden nem unique-b�l is a legutols� m�r az lesz
                for (int j = i + 1; j < orderedBlobs.Count && isUnique; j++)
                {
                    if (orderedBlobs[i].SequenceEqual(orderedBlobs[j]))
                        isUnique = false;
                }

                // itt megtehetj�k, hogy az eredeti t�rol�b�l c�mezz�k meg i-vel a nek�nk fontosat, mert ugyanott szereplnek benne az elemek mint az sorba rendezettben
                if (isUnique) // �s nek�nk sz�m�t az eredeti sorrend!
                    uniqueBlobs.Add(blobs[i]);
            }

            // legutols� elem term�szet�b�l fakad�an unique lesz
            if(blobs.Count > 0)
                uniqueBlobs.Add(blobs.Last());

            List <List<Vector3>> blobs3D = ConvertBlobsTo3D(uniqueBlobs);

            return blobs3D;
        }

        // adott keresztez�d�s minden szomsz�dj�b�l elindulva keres blobokat
        protected List<List<Vector2>> ProcessInterPath(LineIntersector inter)
        {
            List<List<Vector2>> blobList = new List<List<Vector2>>();

            for (int i = 0; i < inter.neighbourInter.Count; i++)
            {
                List<Vector2> blob = ProcessPath(inter, inter.neighbourInter[i]);

                if (blob.Count != 0)
                    blobList.Add(blob);
            }


            return blobList;
        }

        // az eredeti pontok k�z�tti vonalakat kinyerj�k
        protected List<InterLine> CreateInterLines()
        {
            List<InterLine> result = new List<InterLine>();
            for (int i = 0; i < baseInterList.Count - 1; i++)
            {
                result.Add(new InterLine()
                {
                    line = new Line() { startPoint = baseInterList[i].intersectingPoint, endPoint = baseInterList[i + 1].intersectingPoint },
                    startPoint = baseInterList[i],
                    endPoint = baseInterList[i + 1]
                });
            }

            return result;
        }

        // adott keresztez�d�s adott szomsz�dj�b�l elindulva (mindig balra kanyarodva) keres blobot amit azt�n a k�v�nt orient�ci�val visszaad 
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
                    // sim�n visszafordulunk
                    nextNeighbour = currentNeighbour.neighbourInter[0];

                    // visszafordulunk �s addig megy�nk am�g egy rendes keresztez�d�shez nem �r�nk
                    while (nextNeighbour.neighbourInter.Count != 4)
                    {
                        // minden sima elemet kivesz�nk a blob-b�l
                        resultBlob.Remove(nextNeighbour.intersectingPoint);
                        LineIntersector nextTemp = nextNeighbour.GetOppositeNeighbour(currentNeighbour);
                        currentNeighbour = nextNeighbour;
                        nextNeighbour = nextTemp;
                    }

                    // �s a keresztez�d�sn�l balra kanyarodva folytathatjuk az utunk
                    LineIntersector next = nextNeighbour.GetLeftOuterNeighbour(currentNeighbour);
                    currentNeighbour = nextNeighbour;
                    nextNeighbour = next;
                }
                else
                {
                    nextNeighbour = currentNeighbour.GetLeftOuterNeighbour(prevNeighbour);
                }

                // redund�ns elemek t�rl�se
                if (resultBlob.Contains(currentNeighbour.intersectingPoint))
                {
                    // ha keresztez�d�sn�l tal�ltuk meg magunkat, vissza indulva minden pontot ki kell szedni addig am�g els�k�nt nem tal�lkoztunk ezzel a keresztez�d�ssel
                    if (currentNeighbour.neighbourInter.Count == 4)
                    {
                        Vector2 currentPoint = resultBlob.Last();
                        while (currentPoint != currentNeighbour.intersectingPoint)
                        {
                            resultBlob.Remove(currentPoint);
                            currentPoint = resultBlob.Last();
                        }
                    } // egy�bk�nt pedig csak sim�n ki kell szedni mindent
                    else
                    {
                        resultBlob.Remove(currentNeighbour.intersectingPoint);
                    }
                }
                else if (currentNeighbour.neighbourInter.Count != 1) // v�geket ne adjuk hozz�
                {
                    resultBlob.Add(currentNeighbour.intersectingPoint);
                }


                prevNeighbour = currentNeighbour;
                currentNeighbour = nextNeighbour;

                // ha vissza�rt�nk az eredeti keresztez�d�shez �lljunk meg
                if (prevNeighbour == origIntersect)
                {
                    // ha viszont nem ugyanarra a szomsz�dra kanyarodn�nk r� ahonnan elindultunk - nincs tal�lt blob
                    if (currentNeighbour != origNeighbour)
                    {
                        resultBlob.Clear();
                    }

                    break;
                }
            }
            while (true);

            // tal�lt blob orient�ci�j�t meghat�rozzuk
            if (resultBlob.Count > 2)
            {
                float angle = 0;
                for (int i = 0; i < resultBlob.Count; i++)
                {
                    if (i == 0)
                        angle = Vector2.SignedAngle(resultBlob[1] - resultBlob.First(), resultBlob.Last() - resultBlob.First());
                    else if (i == resultBlob.Count - 1)
                        angle = Vector2.SignedAngle(resultBlob.First() - resultBlob.Last(), resultBlob[i - 1] - resultBlob.Last());
                    else
                        angle = Vector2.SignedAngle(resultBlob[i + 1] - resultBlob[i], resultBlob[i - 1] - resultBlob[i]);

                    if (angle != 180 && angle != -180)
                        angleInDeg += angle;
                }
            }

            // ha kell, megford�tjuk az orient�ci�t
            if (angleInDeg < 0)
                resultBlob.Reverse();

            return resultBlob;
        }
    }
}
