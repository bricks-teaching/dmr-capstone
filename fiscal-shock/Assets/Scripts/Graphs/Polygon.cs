using System;
using System.Linq;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// 2D polygon, defined by its edges (and thereby, vertices)
    /// </summary>
    public class Polygon {
        public List<Edge> sides { get; private set; } = new List<Edge>();
        public List<Vertex> vertices { get; set; } = new List<Vertex>();
        public bool clockwiseVertices { get; private set; }
        public double signedArea { get; set; }
        public double area { get; set; }

        public Polygon() {}

        public Polygon(List<Edge> boundary) {
            setSides(boundary);
        }

        public void setSides(List<Edge> boundary) {
            sides = boundary;
            setVerticesFromSides();
        }

        public void setVerticesFromSides() {
            vertices = sides.SelectMany(e => e.vertices).Distinct().ToList();
        }

        /// <summary>
        /// Requires vertices to be ordered counter-clockwise
        /// </summary>
        /// <returns></returns>
        public double getArea() {
            double area = 0;
            for (int i = 0; i < vertices.Count; ++i) {
                if (i == vertices.Count - 1) {  // Last one "wraps around"
                    area += Mathy.determinant2(vertices[i], vertices[0]);
                } else {
                    area += Mathy.determinant2(vertices[i], vertices[i+1]);
                }
            }

            return area * 0.5;
        }
    }

    /// <summary>
    /// A three-sided polygon. What did you expect?
    /// </summary>
    public class Triangle : Polygon {
        public int id { get; }
        public Vertex circumcenter { get; private set; }
        public Vertex a => vertices[0];
        public Vertex b => vertices[1];
        public Vertex c => vertices[2];

        public Triangle(List<Edge> tSides, int tid) {
            setSides(tSides);
            id = tid;
        }

        public Triangle(List<Vertex> corners) {
            vertices = corners;
        }

        /// <summary>
        /// Area of the triangle abc is twice the following determinant:
        /// <para />
        /// | a.x  a.y  1 |
        /// | b.x  b.y  1 |
        /// | c.x  c.y  1 |
        ///
        /// <para>Formula derived from Laplacian expansion</para>
        /// </summary>
        /// <returns>area of triangle on given vertices</returns>
        new public double getArea() {
            return 2 * (((b.x - a.x) * (c.y - a.y)) - ((b.y - a.y) * (c.x - a.x)));
        }

        /// <summary>
        /// Finds the center point of the circle circumscribed by
        /// this triangle.
        /// Relatively computationally expensive, so save the result
        /// and reuse it in the future.
        /// <para>https://github.com/delfrrr/delaunator-cpp</para>
        /// </summary>
        /// <returns>center point of circumscribed circle</returns>
        public Vertex findCircumcenter() {
            if (circumcenter == null) {
                double dx = b.x - a.x;
                double dy = b.y - a.y;
                double ex = c.x - a.x;
                double ey = c.y - a.y;

                double bl = dx * dx + dy * dy;
                double cl = ex * ex + ey * ey;
                double d = dx * ey - dy * ex;

                double x = vertices[0].x + (ey * bl - dy * cl) * 0.5 / d;
                double y = vertices[0].y + (dx * cl - ex * bl) * 0.5 / d;

                circumcenter = new Vertex(x, y);
            }
            return circumcenter;
        }

        /// <summary>
        /// When the area of the triangle (as a determinant) is less than
        /// zero, the points, in the order given, form a counterclockwise
        /// triangle.
        /// <para>From Guibas &amp; Stolfi (1985)</para>
        /// </summary>
        /// <param name="points">list of vertices representing a triangle</param>
        /// <returns>true if the vertices, in list index order, are oriented clockwise</returns>
        public static bool isTriangleClockwise(List<Vertex> points) {
            return new Triangle(points).getArea() < 0;
        }
    }

    /// <summary>
    /// Voronoi cell extension of base Polygon class
    /// </summary>
    public class Cell : Polygon {
        public Vertex site { get; set; }
        public List<Cell> neighbors { get; set; } = new List<Cell>();
        public int id { get; }
        public bool incomplete { get; set; }  // Generally indicates a point that was on the Delaunay's convex hull that would have infinite edges. We don't want to use this to spawn anything, because it has no borders.

        public Cell(Vertex delaunayVertex) {
            site = delaunayVertex;
            id = site.id;
        }

        /// <summary>
        /// Order vertices counterclockwise
        /// </summary>
        public void orderVertices() {
            List<Vertex> ordered = new List<Vertex>();

            List<Tuple<Vertex, float>> centroidAngles = new List<Tuple<Vertex, float>>();
            centroidAngles = vertices.Select(
                v => new Tuple<Vertex, float>(
                        v, site.getAngleOfRotationTo(v)
                    )).ToList();
            ordered = centroidAngles.OrderByDescending(t => t.Item2).Select(t => t.Item1).ToList();
            vertices = ordered;
        }
    }
}