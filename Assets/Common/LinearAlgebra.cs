using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LinearAlgebra
{
    //How to figure out if two lines are intersecting
    public class LineLineIntersection : MonoBehaviour
    {
        //The start end of each line
        public Transform L1_start;
        public Transform L1_end;
        public Transform L2_start;
        public Transform L2_end;

        //Just attach a line renderer to an empty game object
        public LineRenderer lineRenderer1;
        public LineRenderer lineRenderer2;

        public Vector2 intersectingPoint = Vector2.zero;

        void Start()
        {
            //Give the line renderer a material so we can change the color if they intersect
            lineRenderer1.material = new Material(Shader.Find("Unlit/Color"));
            lineRenderer2.material = new Material(Shader.Find("Unlit/Color"));
        }

        void Update()
        {
            //Connect the points with line renderers
            lineRenderer1.SetPosition(0, L1_start.position);
            lineRenderer1.SetPosition(1, L1_end.position);

            lineRenderer2.SetPosition(0, L2_start.position);
            lineRenderer2.SetPosition(1, L2_end.position);

            //Set the default color
            lineRenderer1.material.color = Color.red;
            lineRenderer2.material.color = Color.red;

            Line l1 = new Line(L1_start.position, L1_end.position);

            Line l2 = new Line(L2_start.position, L2_end.position);

            //Change color if they intersect
            if (IsIntersecting(l1, l2))
            {
                lineRenderer1.material.color = Color.blue;
                lineRenderer2.material.color = Color.blue;
            }
            else
                intersectingPoint = Vector2.zero;
        }

        //Check if the lines are interesecting in 2d space
        public bool IsIntersecting(Line l1, Line l2)
        {
            bool isIntersecting = false;


            //Direction of the lines
            Vector2 l1_dir = (l1.End - l1.Start).normalized;
            Vector2 l2_dir = (l2.End - l2.Start).normalized;

            //If we know the direction we can get the normal vector to each line
            Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
            Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);


            //Step 1: Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
            //The normal vector is the A, B
            float A = l1_normal.x;
            float B = l1_normal.y;

            float C = l2_normal.x;
            float D = l2_normal.y;

            //To get k we just use one point on the line
            float k1 = (A * l1.Start.x) + (B * l1.Start.y);
            float k2 = (C * l2.Start.x) + (D * l2.Start.y);


            //Step 2: are the lines parallel? -> no solutions
            if (IsParallel(l1_normal, l2_normal))
            {
                Debug.Log("The lines are parallel so no solutions!");

                return isIntersecting;
            }


            //Step 3: are the lines the same line? -> infinite amount of solutions
            //Pick one point on each line and test if the vector between the points is orthogonal to one of the normals
            if (IsOrthogonal(l1.Start - l2.Start, l1_normal))
            {
                Debug.Log("Same line so infinite amount of solutions!");

                //Return false anyway
                return isIntersecting;
            }


            //Step 4: calculate the intersection point -> one solution
            float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
            float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

            Vector2 intersectPoint = new Vector2(x_intersect, y_intersect);


            //Step 5: but we have line segments so we have to check if the intersection point is within the segment
            if (IsBetween(l1.Start, l1.End, intersectPoint) && IsBetween(l2.Start, l2.End, intersectPoint))
            {
                Debug.Log("We have an intersection point!");
                intersectingPoint = intersectPoint;

                isIntersecting = true;
            }

            return isIntersecting;
        }

        //Are 2 vectors parallel?
        bool IsParallel(Vector2 v1, Vector2 v2)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
            {
                return true;
            }

            return false;
        }

        //Are 2 vectors orthogonal?
        bool IsOrthogonal(Vector2 v1, Vector2 v2)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            if (Mathf.Abs(Vector2.Dot(v1, v2)) < 0.000001f)
            {
                return true;
            }

            return false;
        }

        //Is a point c between 2 other points a and b?
        bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
        {
            bool isBetween = false;

            //Entire line segment
            Vector2 ab = b - a;
            //The intersection and the first point
            Vector2 ac = c - a;

            //Need to check 2 things: 
            //1. If the vectors are pointing in the same direction = if the dot product is positive
            //2. If the length of the vector between the intersection and the first point is smaller than the entire line
            if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
            {
                isBetween = true;
            }

            return isBetween;
        }
    }

    public class Intersection
    {
        public Vector2 intersectingPoint = Vector2.zero;

        // új rendszerben minden pont is keresztezõdés, már nem indexekben gondolkodunk
        public List<Intersection> neighbourInter = new List<Intersection>();

        //Check if the lines are interesecting in 2d space
        public static (bool,Vector2) IsIntersecting(Line l1, Line l2)
        {
            //Direction of the lines
            Vector2 l1_dir = (l1.End - l1.Start).normalized;
            Vector2 l2_dir = (l2.End - l2.Start).normalized;

            //If we know the direction we can get the normal vector to each line
            Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
            Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);


            //Step 1: Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
            //The normal vector is the A, B
            float A = l1_normal.x;
            float B = l1_normal.y;

            float C = l2_normal.x;
            float D = l2_normal.y;

            //To get k we just use one point on the line
            float k1 = (A * l1.Start.x) + (B * l1.Start.y);
            float k2 = (C * l2.Start.x) + (D * l2.Start.y);


            //Step 2: are the lines parallel? -> no solutions
            if (IsParallel(l1_normal, l2_normal))
            {
                return (false,default(Vector2));
            }


            //Step 3: are the lines the same line? -> infinite amount of solutions
            //Pick one point on each line and test if the vector between the points is orthogonal to one of the normals
            if (IsOrthogonal(l1.Start - l2.Start, l1_normal))
            {
                //Return false anyway
                return (false, default(Vector2));
            }


            //Step 4: calculate the intersection point -> one solution
            float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
            float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

            Vector2 intersectPoint = new Vector2(x_intersect, y_intersect);


            //Step 5: but we have line segments so we have to check if the intersection point is within the segment
            if (IsBetween(l1.Start, l1.End, intersectPoint) && IsBetween(l2.Start, l2.End, intersectPoint))
            {
                return (true, intersectPoint);
            }

            return (false, default(Vector2));
        }

        public static bool IsPointInLine(Line line, Vector2 point)
        {
            //Direction of the lines
            Vector2 line_dir = (line.End - line.Start).normalized;

            //If we know the direction we can get the normal vector to each line
            Vector2 line_normal = new Vector2(-line_dir.y, line_dir.x);

            //Step 1: Rewrite the lines to a general form: Ax + By = k1
            float A = line_normal.x;
            float B = line_normal.y;

            //To get k we just use one point on the line
            float k1 = (A * line.Start.x) + (B * line.Start.y);

            float pointK = A * point.x + B * point.y;

            // ha az egyenes képletébe behelyettesítve nem ugyanazt a k-t kapjuk - nincs rajta
            if (Math.Abs(k1 - pointK) > 0.000001)
                return false;

            if (IsBetween(line.Start, line.End, point))
                return true;
            else
                return false;
        }

        //Are 2 vectors parallel?
        static bool IsParallel(Vector2 v1, Vector2 v2)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
            {
                return true;
            }

            return false;
        }

        //Are 2 vectors orthogonal?
        static bool IsOrthogonal(Vector2 v1, Vector2 v2)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            if (Mathf.Abs(Vector2.Dot(v1, v2)) < 0.000001f)
            {
                return true;
            }

            return false;
        }

        //Is a point c between 2 other points a and b?
        public static bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
        {
            bool isBetween = false;

            //Entire line segment
            Vector2 ab = b - a;
            //The intersection and the first point
            Vector2 ac = c - a;

            //Need to check 2 things: 
            //1. If the vectors are pointing in the same direction = if the dot product is positive
            //2. If the length of the vector between the intersection and the first point is smaller than the entire line
            if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
            {
                isBetween = true;
            }

            return isBetween;
        }

        internal void AddNeighbour(Intersection neighbour)
        {
            neighbourInter.Add(neighbour);
        }

        internal void AddNeighbour(IntersectionLine line1, IntersectionLine line2)
        {
            neighbourInter = new List<Intersection>();
            neighbourInter.Add(line1.Start);

            if (IsRight(line1.Line.Start - intersectingPoint, line2.Line.Start - intersectingPoint))
            {
                neighbourInter.Add(line2.Start);
                neighbourInter.Add(line1.End);
                neighbourInter.Add(line2.End);
            }
            else
            {
                neighbourInter.Add(line2.End);
                neighbourInter.Add(line1.End);
                neighbourInter.Add(line2.Start);
            }
        }

        internal Intersection GetLeftOuterNeighbour(Intersection innerNeighbour)
        {
            int innerIndex = neighbourInter.FindIndex(n => innerNeighbour == n);

            int outerIndex = innerIndex == (neighbourInter.Count - 1) ? 0 : (innerIndex + 1);

            return neighbourInter[outerIndex];
        }

        internal  Intersection GetOppositeNeighbour(Intersection innerNeighbour)
        {
            int innerIndex = neighbourInter.FindIndex(n => innerNeighbour == n);

            if (neighbourInter.Count == 2)
            {
                return innerIndex == 1 ? neighbourInter[0] : neighbourInter[1];
            }
            else
            {
                switch(innerIndex)
                {
                    case 0:
                        return neighbourInter[2];
                    case 1: 
                        return neighbourInter[3];
                    case 2: 
                        return neighbourInter[0];
                    case 3: 
                        return neighbourInter[1];
                    default: // nem kéne ide jutni..
                        return neighbourInter[0];        
                }
            }
        }

        bool IsRight(Vector2 youDir, Vector2 waypointDir)
        {

            //The cross product between these vectors
            Vector3 crossProduct = Vector3.Cross(youDir, waypointDir);

            //The dot product between the your up vector and the cross product
            //This can be said to be a volume that can be negative
            float dotProduct = Vector3.Dot(crossProduct, new Vector3(0, 0, 1));

            //Now we can decide if we should turn left or right
            return !(dotProduct > 0f);
        }

        public static void Reconnect(Intersection first, Intersection second, Intersection firstOrigNeighbour, Intersection secondOrigNeighbour)
        {
            int firstOrigIndex = first.neighbourInter.FindIndex(i => i == firstOrigNeighbour);
            int secondOrigIndex = second.neighbourInter.FindIndex(i => i == secondOrigNeighbour);

            first.neighbourInter[firstOrigIndex] = second;
            second.neighbourInter[secondOrigIndex] = first;
        }     
    }

    public class Line
    {
        Vector2 start;
        Vector2 end;

        public Vector2 Start => start;
        public Vector2 End => end;

        public Line(Vector2 v1, Vector2 v2)
        {
            start = v1;
            end = v2;
        }
    }

    public class IntersectionLine
    {
        public IntersectionLine(Intersection i1, Intersection i2)
        {
            line = new Line(i1.intersectingPoint, i2.intersectingPoint);
            start = i1;
            end = i2;
        }

        Line line;
        Intersection start;
        Intersection end;

        public Line Line => line;
        public Intersection Start => start;
        public Intersection End => end;
    }
}