using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{

    public class HalfEdge
    {
        //The vertex the edge points to
        public Vertex v;

        //The face this edge is a part of
        public Triangle t;

        //The next edge
        public HalfEdge nextEdge;
        //The previous
        public HalfEdge prevEdge;
        //The edge going in the opposite direction
        public HalfEdge oppositeEdge;

        //This structure assumes we have a vertex class with a reference to a half edge going from that vertex
        //and a face (triangle) class with a reference to a half edge which is a part of this face 
        public HalfEdge(Vertex v)
        {
            this.v = v;
        }
    }

    public class Triangle
    {
        //Corners
        public Vertex v1;
        public Vertex v2;
        public Vertex v3;

        //If we are using the half edge mesh structure, we just need one half edge
        public HalfEdge halfEdge;

        public Triangle(Vertex v1, Vertex v2, Vertex v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            this.v1 = new Vertex(v1);
            this.v2 = new Vertex(v2);
            this.v3 = new Vertex(v3);
        }

        float sign(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public bool PointInTriangle(Vector3 pt)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1.position, v2.position);
            d2 = sign(pt, v2.position, v3.position);
            d3 = sign(pt, v3.position, v1.position);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }
    }

    public class Vertex
    {
        public Vector3 position;

        //The outgoing halfedge (a halfedge that starts at this vertex)
        //Doesnt matter which edge we connect to it
        public HalfEdge halfEdge;

        //Which triangle is this vertex a part of?
        public Triangle triangle;

        //The previous and next vertex this vertex is attached to
        public Vertex prevVertex;
        public Vertex nextVertex;

        //Properties this vertex may have
        //Reflex is concave
        public bool isReflex;
        public bool isConvex;
        public bool isEar;

        public Vertex(Vector3 position)
        {
            this.position = position;
        }

        //Get 2d pos of this vertex
        public Vector2 GetPos2D_XY()
        {
            Vector2 pos_2d_xy = new Vector2(position.x, position.y);

            return pos_2d_xy;
        }
    }

    public static class MathUtility
    {
        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }
    }

    public static class Geometry
    {
        //Is a triangle in 2d space oriented clockwise or counter-clockwise
        //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        //https://en.wikipedia.org/wiki/Curve_orientation
        public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            bool isClockWise = true;

            float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }

        public static bool IsRight(Vector2 youDir, Vector2 waypointDir)
        {

            //The cross product between these vectors
            Vector3 crossProduct = Vector3.Cross(youDir, waypointDir);

            //The dot product between the your up vector and the cross product
            //This can be said to be a volume that can be negative
            float dotProduct = Vector3.Dot(crossProduct, new Vector3(0, 0, 1));

            //Now we can decide if we should turn left or right
            return !(dotProduct > 0f);
        }
        //Are 2 vectors parallel?
        public static bool IsParallel(Vector2 v1, Vector2 v2)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
            {
                return true;
            }

            return false;
        }

        //Are 2 vectors orthogonal?
        public static bool IsOrthogonal(Vector2 v1, Vector2 v2)
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
    }

    public static class Intersections
    {
        //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        //p is the testpoint, and the other points are corners in the triangle
        public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            bool isWithinTriangle = false;

            //Based on Barycentric coordinates
            float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

            float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
            float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
            float c = 1 - a - b;

            //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
            //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
            //{
            //    isWithinTriangle = true;
            //}

            //The point is within the triangle
            if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }
    }

}
