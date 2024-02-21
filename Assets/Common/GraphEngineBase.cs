using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LinearAlgebra
{
    public abstract class GraphEngineBase
    {
        // derived network:
        // for the original network with the original neighbours
        protected List<Intersection> origIntersections = new List<Intersection>();
        protected List<IntersectionLine> origInterLines = new List<IntersectionLine>();
        // the complete network with the founded intersections as well
        protected List<Intersection> completeIntersections = new List<Intersection>();

        // origIntersections -> origInterLines -> completeIntersections
        public abstract void PrepareGraph();

        // Vector2 -> Vector3 (MeshGenerator requires it)
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

        // by default: every founded intersection refer to the points of the original network
        // now: find the new, direct neighbours on the given line and reconnect the network based on that
        protected void ReconnectIntersections()
        {
            for (int j = 0; j < origInterLines.Count; j++)
            {
                IntersectionLine interLine = origInterLines[j];

                List<Intersection> intersectionsOnTheLine = new List<Intersection>();

                // intersections for the given line
                foreach (Intersection inter in completeIntersections)
                {
                    if (Line.IsPointInLine(interLine.Line, inter.Point))
                    {
                        intersectionsOnTheLine.Add(inter);
                    }
                }

                // order intersections based on the distance of Start edge
                intersectionsOnTheLine.Sort((i1, i2) => ((interLine.Line.Start - i1.Point).magnitude > (interLine.Line.Start - i2.Point).magnitude) ? 1 : -1);

                // connect the start and end point with the first and last founded intersection
                if (intersectionsOnTheLine.Count > 0)
                {
                    Intersection.Reconnect(interLine.Start, intersectionsOnTheLine.First(), interLine.End, interLine.Start);
                    Intersection.Reconnect(intersectionsOnTheLine.Last(), interLine.End, interLine.End, interLine.Start);
                }

                // if we found only 1 intersection - we are done
                if (intersectionsOnTheLine.Count < 2)
                    continue;

                // reconnect every intersection in order
                for (int i = 0; i < intersectionsOnTheLine.Count - 1; i++)
                {
                    Intersection.Reconnect(intersectionsOnTheLine[i], intersectionsOnTheLine[i + 1], interLine.End, interLine.Start);
                }
            }
        }

        // search intersections - between lines which are not neighbours
        protected List<Intersection> AddIntersections()
        {
            List<Intersection> result = new List<Intersection>();

            // neighbours are always intersect eachother - but it is not a founded intersection
            for (int i = 0; i < origInterLines.Count - 2; i++)
            {
                for (int j = i + 2; j < origInterLines.Count; j++)
                {
                    (bool, Vector2) interPoint = Line.IsIntersecting(origInterLines[i].Line, origInterLines[j].Line);
                    if (interPoint.Item1)
                    {
                        Intersection intersect = new Intersection(interPoint.Item2);

                        // temporary neigbours - see: ReconnectIntersections
                        intersect.AddNeighbour(origInterLines[i], origInterLines[j]);

                        result.Add(intersect);
                    }
                }
            }

            return result;
        }

        // process the graph and create unique blobs based on founded intersections
        // PrepareGraph -> blobs -> uniqueBlobs -> blobs3D
        public List<List<Vector3>> ProcessGraph()
        {
            PrepareGraph();

            List<List<Vector2>> blobs = new List<List<Vector2>>();

            for (int i = 0; i < completeIntersections.Count; i++)
            {
                List<List<Vector2>> foundedBlobs = FindAllBlobs(completeIntersections[i]);
                blobs.AddRange(foundedBlobs);
            }


            // filter the blobs which we founded multiple times
            List<List<Vector2>> uniqueBlobs = new List<List<Vector2>>();
            List<IOrderedEnumerable<Vector2>> orderedBlobs = new List<IOrderedEnumerable<Vector2>>();
            blobs.ForEach(blob => orderedBlobs.Add(blob.OrderBy(v => v.x)));

            for (int i = 0; i < orderedBlobs.Count - 1; i++)
            {
                bool isUnique = true;
                // the last occurence will be unique because of the forward iteration
                for (int j = i + 1; j < orderedBlobs.Count && isUnique; j++)
                {
                    if (orderedBlobs[i].SequenceEqual(orderedBlobs[j]))
                        isUnique = false;
                }

                // we can index the original container because the order is the same in orderedBlobs and blobs and we want to keep the original
                // (before order the points of the current blob)
                if (isUnique)
                    uniqueBlobs.Add(blobs[i]);
            }

            // last element must be unique at this point
            if(blobs.Count > 0)
                uniqueBlobs.Add(blobs.Last());

            List <List<Vector3>> blobs3D = ConvertBlobsTo3D(uniqueBlobs);

            return blobs3D;
        }

        // search blobs from every neigbours
        protected List<List<Vector2>> FindAllBlobs(Intersection inter)
        {
            List<List<Vector2>> blobList = new List<List<Vector2>>();

            for (int i = 0; i < inter.neighbours.Count; i++)
            {
                List<Vector2> blob = ProcessPath(inter, inter.neighbours[i]);

                if (blob.Count != 0)
                    blobList.Add(blob);
            }


            return blobList;
        }

        // create lines based on the original points
        protected List<IntersectionLine> CreateInterLines()
        {
            List<IntersectionLine> result = new List<IntersectionLine>();
            for (int i = 0; i < origIntersections.Count - 1; i++)
            {
                result.Add(new IntersectionLine(
                    origIntersections[i],
                    origIntersections[i + 1]));
            }

            return result;
        }

        // start from given intersection given neighbour (turn left always) and search blob, return with proper orientation
        List<Vector2> ProcessPath(Intersection origIntersect, Intersection origNeighbour)
        {
            List<Vector2> resultBlob = new List<Vector2>();

            Intersection currentNeighbour = origNeighbour;
            Intersection prevNeighbour = origIntersect;

            float angleInDeg = 0;

            do
            {
                Intersection nextNeighbour = null;

                if (currentNeighbour.neighbours.Count == 1)
                {
                    // turn back if it is an end point
                    nextNeighbour = currentNeighbour.neighbours[0];

                    // find a proper intersection with 4 neighbours
                    while (nextNeighbour.neighbours.Count != 4)
                    {
                        // remove elements during the process
                        resultBlob.Remove(nextNeighbour.Point);
                        Intersection nextTemp = nextNeighbour.GetOppositeNeighbour(currentNeighbour);
                        currentNeighbour = nextNeighbour;
                        nextNeighbour = nextTemp;
                    }

                    // then we can continue our road at the intersection (turning left)
                    Intersection next = nextNeighbour.GetLeftOuterNeighbour(currentNeighbour);
                    currentNeighbour = nextNeighbour;
                    nextNeighbour = next;
                }
                else
                {
                    nextNeighbour = currentNeighbour.GetLeftOuterNeighbour(prevNeighbour);
                }

                // remove redundant elements
                if (resultBlob.Contains(currentNeighbour.Point))
                {
                    // if we found the original point at an intersection, turn back and remove all the elements until we find ourself again
                    if (currentNeighbour.neighbours.Count == 4)
                    {
                        Vector2 currentPoint = resultBlob.Last();
                        while (currentPoint != currentNeighbour.Point)
                        {
                            resultBlob.Remove(currentPoint);
                            currentPoint = resultBlob.Last();
                        }
                    } // otherwise just remove the current element
                    else
                    {
                        resultBlob.Remove(currentNeighbour.Point);
                    }
                }
                else if (currentNeighbour.neighbours.Count != 1) // never add the end points
                {
                    resultBlob.Add(currentNeighbour.Point);
                }

                prevNeighbour = currentNeighbour;
                currentNeighbour = nextNeighbour;

                // when we arrive back to the original intersection
                if (prevNeighbour == origIntersect)
                {
                    // if we found the original blob, but with a wrong direction - it is not a proper blob
                    if (currentNeighbour != origNeighbour)
                    {
                        resultBlob.Clear();
                    }

                    // stop anyway
                    break;
                }
            }
            while (true);

            // calculate the orientation of the founded blob
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

                    // filter straight angles
                    if (angle != 180 && angle != -180)
                        angleInDeg += angle;
                }
            }

            // reverse the orientation if it is necessary
            if (angleInDeg < 0)
                resultBlob.Reverse();

            return resultBlob;
        }
    }
}
