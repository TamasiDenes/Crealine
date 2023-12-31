using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Assets;

namespace LinearAlgebra
{
    public class Intersection
    {
        public Intersection(Vector2 p)
        {
            point = p;
        }

        Vector2 point;
        public Vector2 Point => point;

        public List<Intersection> neighbours = new List<Intersection>();

        internal void AddNeighbour(Intersection neighbour)
        {
            neighbours.Add(neighbour);
        }

        internal void AddNeighbour(IntersectionLine line1, IntersectionLine line2)
        {
            neighbours = new List<Intersection>();
            neighbours.Add(line1.Start);

            if (Geometry.IsRight(line1.Line.Start - Point, line2.Line.Start - Point))
            {
                neighbours.Add(line2.Start);
                neighbours.Add(line1.End);
                neighbours.Add(line2.End);
            }
            else
            {
                neighbours.Add(line2.End);
                neighbours.Add(line1.End);
                neighbours.Add(line2.Start);
            }
        }

        internal Intersection GetLeftOuterNeighbour(Intersection innerNeighbour)
        {
            int innerIndex = neighbours.FindIndex(n => innerNeighbour == n);

            int outerIndex = innerIndex == (neighbours.Count - 1) ? 0 : (innerIndex + 1);

            return neighbours[outerIndex];
        }

        internal  Intersection GetOppositeNeighbour(Intersection innerNeighbour)
        {
            if (neighbours.Count != 2 && neighbours.Count != 4)
                Debug.LogError("Intersection neighbour number must be 2 or 4");

            int innerIndex = neighbours.FindIndex(n => innerNeighbour == n);

            if (neighbours.Count == 2)
            {
                return innerIndex == 1 ? neighbours[0] : neighbours[1];
            }
            else
            {
                switch(innerIndex)
                {
                    case 0:
                        return neighbours[2];
                    case 1: 
                        return neighbours[3];
                    case 2: 
                        return neighbours[0];
                    case 3: 
                        return neighbours[1];
                    default:
                        return neighbours[0];        
                }
            }
        }

        public static void Reconnect(Intersection first, Intersection second, Intersection firstOrigNeighbour, Intersection secondOrigNeighbour)
        {
            int firstOrigIndex = first.neighbours.FindIndex(i => i == firstOrigNeighbour);
            int secondOrigIndex = second.neighbours.FindIndex(i => i == secondOrigNeighbour);

            first.neighbours[firstOrigIndex] = second;
            second.neighbours[secondOrigIndex] = first;
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

        //Check if the lines are interesecting in 2d space
        public static (bool, Vector2) IsIntersecting(Line l1, Line l2)
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
            if (Geometry.IsParallel(l1_normal, l2_normal))
            {
                return (false, default(Vector2));
            }


            //Step 3: are the lines the same line? -> infinite amount of solutions
            //Pick one point on each line and test if the vector between the points is orthogonal to one of the normals
            if (Geometry.IsOrthogonal(l1.Start - l2.Start, l1_normal))
            {
                //Return false anyway
                return (false, default(Vector2));
            }


            //Step 4: calculate the intersection point -> one solution
            float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
            float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

            Vector2 intersectPoint = new Vector2(x_intersect, y_intersect);


            //Step 5: but we have line segments so we have to check if the intersection point is within the segment
            if (Geometry.IsBetween(l1.Start, l1.End, intersectPoint) && Geometry.IsBetween(l2.Start, l2.End, intersectPoint))
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

            if (Math.Abs(k1 - pointK) > 0.000001)
                return false;

            if (Geometry.IsBetween(line.Start, line.End, point))
                return true;
            else
                return false;
        }
    }

    public class IntersectionLine
    {
        public IntersectionLine(Intersection i1, Intersection i2)
        {
            line = new Line(i1.Point, i2.Point);
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