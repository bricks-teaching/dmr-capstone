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
        public double area { get; set; }
        public bool isClosed => getIsClosed();
        public float minX => vertices.Min(v => v.x);
        public float maxX => vertices.Max(v => v.x);
        public float minY => vertices.Min(v => v.y);
        public float maxY => vertices.Max(v => v.y);

        public Polygon() {}

        /// <summary>
        /// WARNING: Do not use vertex-constructed polygons if you need edges!
        /// Only useful for operations like finding bounding points of points.
        /// Without edges, it is impossible to know the correct area, as the
        /// polygon could be self-intersecting.
        /// </summary>
        /// <param name="vs"></param>
        public Polygon(List<Vertex> vs) {
            vertices = vs;
        }

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
        /// Vertices can come from many places, so just checking that each
        /// vertex has >=2 neighbors doesn't guarantee those neighbors are
        /// part of this polygon.
        /// </summary>
        public bool getIsClosed() {
            if (vertices.Count < 3 || sides.Count < 3) {  // I don't consider a line a polygon
                return false;
            }
            foreach (Vertex v in vertices) {
                // it should have two neighbors in this polygon
                if (v.neighborhood.Intersect(vertices).Count() < 2) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds area of a polygon that is not self-intersecting.
        /// Requires vertices to be ordered counter-clockwise. Calling this
        /// function on self-intersecting polygons will result in unexpected
        /// answers.
        /// </summary>
        /// <returns></returns>
        public double getArea() {
            if (sides.Count < 3) {
                return 0;
            }
            double area = 0;
            for (int i = 0; i < vertices.Count; ++i) {
                if (i == vertices.Count - 1) {  // Last one "wraps around"
                    area += Mathy.determinant2(vertices[i], vertices[0]);
                } else {
                    area += Mathy.determinant2(vertices[i], vertices[i+1]);
                }
            }

            return Math.Abs(area * 0.5);
        }

        /// <summary>
        /// Orders vertices counter-clockwise based on their angles to an
        /// interior point
        /// </summary>
        /// <param name="centroid"></param>
        public void orderVerticesCounterClockwise(Vertex centroid) {
            List<Vertex> ordered = new List<Vertex>();

            List<Tuple<Vertex, float>> centroidAngles = new List<Tuple<Vertex, float>>();
            centroidAngles = vertices.Select(
                v => new Tuple<Vertex, float>(
                        v, centroid.getAngleOfRotationTo(v)
                    )).ToList();
            ordered = centroidAngles.OrderByDescending(t => t.Item2).Select(t => t.Item1).ToList();
            this.vertices = ordered;
        }

        /// <summary>
        /// Gets the bounding box coordinates of this polygon from the minimum and maximum vertex coordinates
        /// </summary>
        /// <returns></returns>
        public Polygon getBoundingBox() {
            if (vertices.Count < 3) {
                return null;
            }
            List<Vertex> corners = new List<Vertex> {
                new Vertex(minX, minY),  // 0,0
                new Vertex(minX, maxY),  // 0,1
                new Vertex(maxX, maxY),  // 1,1
                new Vertex(maxX, minY)   // 1,0
            };

            List<Edge> boundary = new List<Edge>();
            for (int i = 0; i < corners.Count; ++i) {
                if (i != corners.Count-1) {
                    boundary.Add(new Edge(corners[i], corners[i+1]));
                } else {
                    boundary.Add(new Edge(corners[0], corners[i]));
                }
            }

            return new Polygon(boundary);
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

        public Triangle(List<Vertex> corners) {
            vertices = corners;
        }

        public Triangle(List<Vertex> corners, int tid) : this(corners) {
            id = tid;
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
        public UnityEngine.GameObject spawnedObject { get; set; }
        public bool hasPortal { get; set; }
        public VoronoiRoom room { get; set; }

        // PATHFINDING ONLY
        public bool reachable = true;

        public Cell(Vertex delaunayVertex) {
            site = delaunayVertex;
            id = site.id;
            site.cell = this;
        }

        /// <summary>
        /// Order vertices counterclockwise based on the site
        /// </summary>
        public void orderVertices() {
            orderVerticesCounterClockwise(site);
        }

        /* Comparator functions - needed for LINQ */
        // Only based on site for now, should really take into account sides
        public override bool Equals(object obj) {
            if (obj is Cell other) {
                return site == other.site;
            }
            return false;
        }

        /// <summary>
        /// Taken from https://stackoverflow.com/a/2280213
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            int hash = 23;
            hash = (hash * 31) + site.x.GetHashCode();
            hash = (hash * 31) + site.y.GetHashCode();
            return hash;
        }
        /* End comparator functions */

    }
}
