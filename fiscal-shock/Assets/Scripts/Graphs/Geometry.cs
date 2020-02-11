using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    public class Vertex {
        public float x { get; }
        public float y { get; }
        public int id { get; }

        public Vertex(int xX, int yY, int vid) {
            x = xX;
            y = yY;
            id = vid;
        }

        public Vertex(double xX, double yY, int vid) {
            // Debug.Log($"WARNING: Converting double precision coordinates to integer. Input: ({xX}, {yY})");
            x = (float)xX;
            y = (float)yY;
            id = vid;
        }

        public Vertex(double[] xy, int id) : this(xy[0], xy[1], id) {
            if (xy.Length > 2) {
                Debug.Log($"FATAL: Input array held more than two coordinates.");
                throw new System.ArgumentException();
            }
        }

        public Vertex(List<double> xy, int id) : this(xy[0], xy[1], id) {
            if (xy.Count > 2) {
                Debug.Log($"FATAL: Input list held more than two coordinates.");
                throw new System.ArgumentException();
            }
        }

        public double getDistanceTo(Vertex other) {
            return Math.Sqrt(Math.Pow(x - other.x, 2) + Math.Pow(y - other.y, 2));
        }

        public static double getDistanceBetween(Vertex a, Vertex b) {
            return a.getDistanceTo(b);
        }

        public static Vector3 toVector3AtHeight(Vertex v, float height) {
            return new Vector3(v.x, height, v.y);
        }

        public Vector3 toVector3AtHeight(float height) {
            return toVector3AtHeight(this, height);
        }
    }

    public class Edge {
        public List<Vertex> vertices { get; }
        public int id { get; }

        public Edge(Vertex a, Vertex b, int eid) {
            vertices = new List<Vertex> { a, b };
            id = eid;
        }

        public Edge(List<Vertex> verts, int eid) {
            if (verts.Count != 2) {
                Debug.Log($"FATAL: Wrong Vector3 list passed to Edge constructor (got {verts.Count}, not 2)");
                throw new ArgumentException();
            }
            vertices = verts;
            id = eid;
        }

        public static int nextHalfedgeId(int eid) {
            return (eid % 3 == 2)? eid - 2 : eid + 1;
        }

        public int nextHalfedgeId() {
            return nextHalfedgeId(id);
        }

        public static int prevHalfedgeId(int eid) {
            return (eid % 3 == 0)? eid + 2: eid - 1;
        }

        public int prevHalfedgeId() {
            return prevHalfedgeId(id);
        }

        public static int getTriangleId(int eid) {
            return eid / 3;
        }

        public int getTriangleId() {
            return getTriangleId(id);
        }
    }

    public class Triangle {
        public List<Edge> edges { get; set; }
        public List<Vertex> vertices { get; set; }
        public int id { get; }
        public Vertex circumcenter { get; }

        public Triangle(List<Vertex> points, List<Edge> sides, int tid) {
            vertices = points;
            edges = sides;
            id = tid;
        }

        /// <summary>
        /// Area of the triangle abc is the following determinant:
        /// | a.x  a.y  1 |
        /// | b.x  b.y  1 |
        /// | c.x  c.y  1 |
        /// </summary>
        /// <returns></returns>
        public static double getArea(Vertex a, Vertex b, Vertex c) {
            return (b.x	- a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        public static double getArea(List<Vertex> tri) {
            if (tri.Count != 3) {
                Debug.Log($"FATAL: Input was not a triangle");
                throw new ArgumentException();
            }
            return getArea(tri[0], tri[1], tri[2]);
        }

        public static double getArea(Triangle t) {
            return getArea(t.vertices);
        }

        /// <summary>
        /// When the area of the triangle (as a determinant) is less than
        /// zero, the points, in the order given, form a counterclockwise
        /// triangle.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static bool isTriangleClockwise(List<Vertex> points) {
            return getArea(points) < 0;
        }

        public static bool isTriangleClockwise(Triangle t) {
            return isTriangleClockwise(t.vertices);
        }

        public static List<int> getEdgeIds(int tid) {
            return new List<int> {
                3 * tid,
                3 * tid + 1,
                3 * tid + 2
            };
        }

        public List<int> getEdgeIds() {
            return getEdgeIds(id);
        }

        public Vertex findCircumcenter() {
            double dx = vertices[1].x - vertices[0].x;
            double dy = vertices[1].y - vertices[0].y;
            double ex = vertices[2].x - vertices[0].x;
            double ey = vertices[2].y - vertices[0].y;

            double bl = dx * dx + dy * dy;
            double cl = ex * ex + ey * ey;
            double d = dx * ey - dy * ex;

            double x = vertices[0].x + (ey * bl - dy * cl) * 0.5 / d;
            double y = vertices[0].y + (dx * cl - ex * bl) * 0.5 / d;

            return new Vertex(x, y, -1);
        }
    }
}