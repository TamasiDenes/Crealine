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
        // származtatott hálózat:
        // tartalmazza az eredeti hálózat pontjait eredeti szomszédságaikkal:
        protected List<Intersection> baseInterList = new List<Intersection>();
        // eredeti hálózat pontjai közötti vonalakat tartalmazza - amik a végeiken referálnak a pontokra is:
        protected List<IntersectionLine> lines = new List<IntersectionLine>();
        // a megtalált keresztezõdéseket is tartalmazó pontok (minden benne van):
        protected List<Intersection> fullInterList = new List<Intersection>();

        // baseInterList -> lines -> fullInterList
        public abstract void PrepareGraph();

        // azért van rá szükség, mert a MeshGenerator-nak így kellenek a blobok
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

        // minden talált keresztezõdés alapból szomszédként az eredeti gráf pontjaira mutat
        // itt megkeressük az adott egyenesen közvetlen szomszédságokat és újra huzalozzuk a hálózatot
        protected void ReconnectIntersections()
        {
            for (int j = 0; j < lines.Count; j++)
            {
                IntersectionLine interLine = lines[j];

                List<Intersection> relatedIntersections = new List<Intersection>();

                // adott vonalhoz tartozó keresztezõdések, pontok
                foreach (Intersection inter in fullInterList)
                {
                    if (Intersection.IsPointInLine(interLine.Line, inter.intersectingPoint))
                    {
                        relatedIntersections.Add(inter);
                    }
                }

                // vonalon az egyik végponttól mért távolságok szerint sorba rendezzük a talált pontokat
                relatedIntersections.Sort((i1, i2) => ((interLine.Line.Start - i1.intersectingPoint).magnitude > (interLine.Line.Start - i2.intersectingPoint).magnitude) ? 1 : -1);

                // végeket összekötjük az elsõ, utolsó keresztezõdéssel
                if (relatedIntersections.Count > 0)
                {
                    Intersection.Reconnect(interLine.Start, relatedIntersections.First(), interLine.End, interLine.Start);
                    Intersection.Reconnect(relatedIntersections.Last(), interLine.End, interLine.End, interLine.Start);
                }

                // ha nincs legalább kettõ találat, nincs változtatás
                if (relatedIntersections.Count < 2)
                    continue;

                // köztes keresztezõdéseket sorban összekötjük
                for (int i = 0; i < relatedIntersections.Count - 1; i++)
                {
                    Intersection.Reconnect(relatedIntersections[i], relatedIntersections[i + 1], interLine.End, interLine.Start);
                }
            }
        }

        // végig megyünk a vonalakon és nézzük, hogy bármelyik másik nem közvetlen szomszédos vonallal van-e metszéspont
        // ha igen, felvesszük a keresztezõdések közé, és egyelõre minden ilyen keresztezõdésnek az eredeti vonal végpontjait adom meg szomszédnak
        protected List<Intersection> AddIntersections()
        {
            List<Intersection> result = new List<Intersection>();

            // azért így, mert a szomszédosok mindig metszik egymást de az nem jelent keresztezõdést
            for (int i = 0; i < lines.Count - 2; i++)
            {
                for (int j = i + 2; j < lines.Count; j++)
                {
                    (bool, Vector2) interPoint = Intersection.IsIntersecting(lines[i].Line, lines[j].Line);
                    if (interPoint.Item1)
                    {
                        Intersection intersect = new Intersection() { intersectingPoint = interPoint.Item2 };

                        intersect.AddNeighbour(lines[i], lines[j]);

                        result.Add(intersect);
                    }
                }
            }

            return result;
        }

        // a gráfot megkapva egy irányhelyes keresztezõdéseket figyelembe vett blobokat tartalmazó listát ad vissza 3d-set hogy a triangulator is tudjon vele bánni
        // leszûri, hogy minden blob csak egyszer szerepeljen benne:
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


            // szûrés, hogy amiket többször megtaláltunk azokból csak egy legyen
            List<List<Vector2>> uniqueBlobs = new List<List<Vector2>>();
            List<IOrderedEnumerable<Vector2>> orderedBlobs = new List<IOrderedEnumerable<Vector2>>();
            blobs.ForEach(blob => orderedBlobs.Add(blob.OrderBy(v => v.x)));

            for (int i = 0; i < orderedBlobs.Count - 1; i++)
            {
                bool isUnique = true;
                // mivel mindig csak a soron következõkkel vetjük össze, minden nem unique-ból is a legutolsó már az lesz
                for (int j = i + 1; j < orderedBlobs.Count && isUnique; j++)
                {
                    if (orderedBlobs[i].SequenceEqual(orderedBlobs[j]))
                        isUnique = false;
                }

                // itt megtehetjük, hogy az eredeti tárolóból címezzük meg i-vel a nekünk fontosat, mert ugyanott szereplnek benne az elemek mint az sorba rendezettben
                if (isUnique) // és nekünk számít az eredeti sorrend!
                    uniqueBlobs.Add(blobs[i]);
            }

            // legutolsó elem természetébõl fakadóan unique lesz
            if(blobs.Count > 0)
                uniqueBlobs.Add(blobs.Last());

            List <List<Vector3>> blobs3D = ConvertBlobsTo3D(uniqueBlobs);

            return blobs3D;
        }

        // adott keresztezõdés minden szomszédjából elindulva keres blobokat
        protected List<List<Vector2>> ProcessInterPath(Intersection inter)
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

        // az eredeti pontok közötti vonalakat kinyerjük
        protected List<IntersectionLine> CreateInterLines()
        {
            List<IntersectionLine> result = new List<IntersectionLine>();
            for (int i = 0; i < baseInterList.Count - 1; i++)
            {
                result.Add(new IntersectionLine(
                    baseInterList[i],
                    baseInterList[i + 1]));
            }

            return result;
        }

        // adott keresztezõdés adott szomszédjából elindulva (mindig balra kanyarodva) keres blobot amit aztán a kívánt orientációval visszaad 
        List<Vector2> ProcessPath(Intersection origIntersect, Intersection origNeighbour)
        {
            List<Vector2> resultBlob = new List<Vector2>();

            Intersection currentNeighbour = origNeighbour;
            Intersection prevNeighbour = origIntersect;

            float angleInDeg = 0;

            do
            {
                Intersection nextNeighbour = null;

                if (currentNeighbour.neighbourInter.Count == 1)
                {
                    // simán visszafordulunk
                    nextNeighbour = currentNeighbour.neighbourInter[0];

                    // visszafordulunk és addig megyünk amíg egy rendes keresztezõdéshez nem érünk
                    while (nextNeighbour.neighbourInter.Count != 4)
                    {
                        // minden sima elemet kiveszünk a blob-ból
                        resultBlob.Remove(nextNeighbour.intersectingPoint);
                        Intersection nextTemp = nextNeighbour.GetOppositeNeighbour(currentNeighbour);
                        currentNeighbour = nextNeighbour;
                        nextNeighbour = nextTemp;
                    }

                    // és a keresztezõdésnél balra kanyarodva folytathatjuk az utunk
                    Intersection next = nextNeighbour.GetLeftOuterNeighbour(currentNeighbour);
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

            // talált blob orientációját meghatározzuk
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

            // ha kell, megfordítjuk az orientációt
            if (angleInDeg < 0)
                resultBlob.Reverse();

            return resultBlob;
        }
    }
}
