using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Vertex (or "node") as part of a graph, defined by
    /// 2D Cartesian coordinates
    /// </summary>
    public class Vertex {
        public float x { get; }
        public float y { get; }
        public int id { get; }

        /* Spending the space to track connected components simplifies
         * any algorithms that need to traverse a graph.
         */
        // Vertices adjacent (connected by an edge)
        public List<Vertex> neighborhood { get; set; } = new List<Vertex>();
        // Edges incident (having this vertex as an endpoint)
        public List<Edge> incidentEdges { get; set; } = new List<Edge>();

        /* Begin overloaded constructors */
        public Vertex(float xX, float yY) {
            x = xX;
            y = yY;
        }

        public Vertex(float xX, float yY, int vid) : this(xX, yY) {
            id = vid;
        }

        public Vertex(double xX, double yY) : this((float)xX, (float)yY) {}

        public Vertex(double[] xy) : this(xy[0], xy[1]) {
            if (xy.Length > 2) {
                Debug.LogError($"FATAL: Input array held more than two coordinates.");
                throw new ArgumentException();
            }
        }

        public Vertex(List<double> xy) : this(xy[0], xy[1]) {
            if (xy.Count > 2) {
                Debug.LogError($"FATAL: Input list held more than two coordinates.");
                throw new ArgumentException();
            }
        }
        /* End overloaded constructors */

        /* Comparator functions - needed for LINQ */
        public override bool Equals(object obj) {
            if (obj is Vertex other) {
                return x == other.x && y == other.y;
            }
            return false;
        }

        /// <summary>
        /// Taken from https://stackoverflow.com/a/2280213
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            int hash = 23;
            hash = (hash * 31) + x.GetHashCode();
            hash = (hash * 31) + y.GetHashCode();
            return hash;
        }
        /* End comparator functions */

        /// <summary>
        /// Euclidean distance between two Cartesian coordiates
        /// </summary>
        /// <param name="other">distant vertex</param>
        /// <returns>distance</returns>
        public double getDistanceTo(Vertex other) {
            return Mathy.getDistanceBetween(x, y, other.x, other.y);
        }

        /// <summary>
        /// Calls member function
        /// </summary>
        /// <param name="a">point a</param>
        /// <param name="b">point b</param>
        /// <returns>Euclidean distance between a and b</returns>
        public static double getDistanceBetween(Vertex a, Vertex b) {
            return a.getDistanceTo(b);
        }

        /// <summary>
        /// Given a list of vertices and an origin vertex, find the nearest one (via Euclidean distance).
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static Vertex findNearestInListTo(Vertex origin, List<Vertex> others) {
            // Find all distances to the origin
            List<double> distances = others.Select(v => v.getDistanceTo(origin)).ToList();

            /* If origin is in the list, we want the second minimum distance.
             * This means we can't just take the minimum of this list.
             * But we can just choose to skip the first element in the sorted list instead, or skip na
             */
            int skip = others.Contains(origin)? 1 : 0;
            double minimumDistance = distances.OrderBy(d => d).Skip(skip).First();
            int indexOfNearest = distances.IndexOf(minimumDistance);

            return others[indexOfNearest];
        }

        /// <summary>
        /// Calls static function
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        public Vertex findNearestInList(List<Vertex> others) {
            return findNearestInListTo(this, others);
        }

        /// <summary>
        /// Find a point on the line drawn at the angle theta from site that is distance units away from site.
        /// </summary>
        /// <param name="site">origin vertex</param>
        /// <param name="theta">angle in radians</param>
        /// <param name="distance">desired length of line segment</param>
        /// <returns></returns>
        public static Vertex getEndpointOfLineRotation(Vertex site, double theta, float distance) {
            return new Vertex(Mathy.getEndpointOfLineRotation(site.x, site.y, theta, distance));
        }

        /// <summary>
        /// Convert to Unity Vector3
        /// Unity uses y-axis as height (up/down) and z-axis as depth, unlike
        /// e.g. Blender where z is up/down
        /// Since a Vertex is 2D, the third dimension must be specified
        /// </summary>
        /// <param name="v">vertex to convert</param>
        /// <param name="height">desired height of Vector3</param>
        /// <returns>3D vertex</returns>
        public static Vector3 toVector3AtHeight(Vertex v, float height) {
            return new Vector3(v.x, height, v.y);
        }

        /// <summary>
        /// Calls static function
        /// </summary>
        /// <param name="height">desired height of Vector3</param>
        /// <returns>3D vertex</returns>
        public Vector3 toVector3AtHeight(float height) {
            return toVector3AtHeight(this, height);
        }
    }

    /// <summary>
    /// Undirected edge, defined by endpoints
    /// For only Delaunator edges, ID is relevant
    /// </summary>
    public class Edge {
        public List<Vertex> vertices { get; }
        public int id { get; }  // TODO is this needed?

        /* Begin overloaded constructors */
        public Edge(Vertex a, Vertex b, bool _) {  // Hack to avoid adding to neighborhood
            vertices = new List<Vertex> { a, b };
        }

        public Edge(Vertex a, Vertex b) {
            vertices = new List<Vertex> { a, b };
            a.neighborhood.Add(b);
            b.neighborhood.Add(a);
            a.incidentEdges.Add(this);
            b.incidentEdges.Add(this);
        }

        public Edge(Vertex a, Vertex b, int eid) : this(a, b) {
            id = eid;
        }

        public Edge(List<Vertex> verts, int eid) {
            if (verts.Count != 2) {
                Debug.LogError($"FATAL: Wrong Vector3 list passed to Edge constructor (got {verts.Count}, not 2)");
                throw new ArgumentException();
            }
            vertices = verts;
            id = eid;
        }
        /* End overloaded constructors */

        /* Comparator functions - needed for LINQ */
        public override bool Equals(object obj) {
            if (obj is Edge other) {
                return vertices.Intersect(other.vertices).Count() == vertices.Count;
            }
            return false;
        }

        /// <summary>
        /// Taken from https://stackoverflow.com/a/2280213
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            int hash = 23;
            hash = (hash * 31) + vertices[0].GetHashCode();
            hash = (hash * 31) + vertices[1].GetHashCode();
            return hash;
        }
        /* End comparator functions *

        /// <summary>
        /// Length of an edge in this case is the Euclidean distance
        /// between the endpoints
        /// </summary>
        /// <returns>length of this edge</returns>
        public double getLength() {
            return Vertex.getDistanceBetween(vertices[0], vertices[1]);
        }

        /* Delaunator-only helper functions */
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
        /* End Delaunator helper functions */

        /// <summary>
        /// Find intersection point between two edges.
        /// See <see cref="Mathy.findIntersection" />.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>`Vertex` representing intersection point, or null if edges don't intersect</returns>
        public static Vertex findIntersection(Edge a, Edge b) {
            return new Vertex(Mathy.findIntersection(a.vertices[0].x, a.vertices[0].y, a.vertices[1].x, a.vertices[1].y, b.vertices[0].x, b.vertices[0].y, b.vertices[1].x, b.vertices[1].y));
        }

        /// <summary>
        /// Finds all edges connecting vertices in a list. Can't find edges whose endpoints aren't in the original list of vertices.
        /// </summary>
        /// <param name="listToConnect">Vertices to connect</param>
        /// <returns>List of connecting edges found. Can be empty.</returns>
        /// TODO this is part of naive algorithm, maybe delete
        public static List<Edge> findConnectingEdges(List<Vertex> listToConnect) {
            bool doneFindingPairs = false;
            List<Edge> connectors = new List<Edge>();
            while (!doneFindingPairs) {
                for (int i = 0; i < listToConnect.Count; ++i) {
                    bool foundThisPair = false;
                    foreach (Edge e in listToConnect[i].incidentEdges) {
                        /* If the size of the intersection of the edge's
                         * endpoints with the full list of vertices to connect
                         * is the same size as the edge's endpoint list (2),
                         * then that edge connects two vertices.
                         */
                        if (e.vertices.Intersect(listToConnect).Count() == e.vertices.Count) {
                            // Remove these vertices from the search list
                            listToConnect.Remove(e.vertices[0]);
                            listToConnect.Remove(e.vertices[1]);
                            connectors.Add(e);
                            foundThisPair = true;
                            break;
                        }
                    }
                    if (foundThisPair && listToConnect.Count > 0) {
                        // Reset the outer loop index to start the search over.
                        i = 0;
                    }
                }
                /* If we reached the end of the vertex list, no more edges can
                 * be found with the current information.
                 */
                doneFindingPairs = true;
            }

            return connectors.Distinct().ToList();
        }
    }

    /// <summary>
    /// Triangle, defined by both vertices and edges
    /// A triangle could be defined by either, but this logic simplifies
    /// the conversion from Delaunator
    /// </summary>
    public class Triangle {
        public List<Edge> edges { get; set; }
        public List<Vertex> vertices { get; set; }
        public int id { get; }
        public Vertex circumcenter { get; private set; } = null;

        public Triangle(List<Vertex> points, List<Edge> sides, int tid) {
            vertices = points;
            edges = sides;
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
        public static double getArea(Vertex a, Vertex b, Vertex c) {
            return 2 * (((b.x - a.x) * (c.y - a.y)) - ((b.y - a.y) * (c.x - a.x)));
        }

        /// <summary>
        /// Calls static function
        /// </summary>
        /// <param name="tri">list of vertices representing triangle</param>
        /// <returns>area of triangle on given list of vertices</returns>
        public static double getArea(List<Vertex> tri) {
            if (tri.Count != 3) {
                Debug.LogError($"FATAL: Input was not a triangle");
                throw new ArgumentException();
            }
            return getArea(tri[0], tri[1], tri[2]);
        }

        /// <summary>
        /// Calls static function
        /// </summary>
        /// <param name="t">Triangle object</param>
        /// <returns>area of given triangle</returns>
        public static double getArea(Triangle t) {
            return getArea(t.vertices);
        }

        /// <summary>
        /// Calls static function on this object
        /// <para>Could later be used to store area and retrieve it if already
        /// calculated, if we need to check area often</para>
        /// </summary>
        /// <returns>area of this triangle</returns>
        public double getArea() {
            return getArea(this);
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
            return getArea(points) < 0;
        }

        /// <summary>
        /// Calls static method
        /// </summary>
        /// <param name="t">Triangle object</param>
        /// <returns>true if triangle's vertices are oriented clockwise</returns>
        public static bool isTriangleClockwise(Triangle t) {
            return isTriangleClockwise(t.vertices);
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
                double dx = vertices[1].x - vertices[0].x;
                double dy = vertices[1].y - vertices[0].y;
                double ex = vertices[2].x - vertices[0].x;
                double ey = vertices[2].y - vertices[0].y;

                double bl = dx * dx + dy * dy;
                double cl = ex * ex + ey * ey;
                double d = dx * ey - dy * ex;

                double x = vertices[0].x + (ey * bl - dy * cl) * 0.5 / d;
                double y = vertices[0].y + (dx * cl - ex * bl) * 0.5 / d;

                circumcenter = new Vertex(x, y);
            }
            return circumcenter;
        }

        /* Begin Delaunator-only helper functions */
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
        /* End Delaunator helper functions */
    }

    /// <summary>
    /// 2D polygon, defined by its edges (and thereby, vertices)
    /// </summary>
    public class Polygon {
        public List<Edge> sides { get; private set; } = new List<Edge>();
        public List<Vertex> vertices { get; private set; } = new List<Vertex>();

        public Polygon() {}

        public Polygon(List<Edge> boundary) {
            setSides(boundary);
        }

        public void setSides(List<Edge> boundary) {
            sides = boundary;
            vertices = sides.SelectMany(e => e.vertices).Distinct().ToList();
        }
    }

    public class Cell : Polygon {
        public Vertex site { get; set; }
        public List<Cell> neighbors { get; set; } = new List<Cell>();
        public int id { get; }

        public Cell(Vertex delaunayVertex) {
            site = delaunayVertex;
            id = site.id;
        }

        new public void setSides(List<Edge> boundary) {
            base.setSides(boundary);

            if (sides.Count < 3) {
                Debug.LogWarning($"{id}: Not a closed polygon ({sides.Count} sides)");
            }
            if (vertices.Count <= sides.Count) {
                Debug.LogWarning($"{id}: Illogical number of vertices compared to edges ({vertices.Count} vs {sides.Count})");
            }
        }
    }

    /// <summary>
    /// Math functions (can't name it `Math` because that collides with `System.Math`)
    /// </summary>
    public static class Mathy {
        /// <summary>
        /// Find determinant of a 2x2 matrix by cross-multiplying.
        /// <para>`| a b |`</para><para/>
        /// <para>`| c d |`</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns>determinant of 2x2 matrix</returns>
        public static double determinant2(double a, double b, double c, double d) {
            return (a * d) - (b * c);
        }

        public static double determinant2(Vertex a, Vertex b) {
            return determinant2(a.x, b.x, a.y, b.y);
        }

        /// <summary>
        /// Euclidean distance between two Cartesian coordiates
        /// </summary>
        /// <returns>distance</returns>
        public static double getDistanceBetween(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        /// <summary>
        /// Find a point on the line drawn at the angle theta from site that is distance units away from site.
        /// </summary>
        /// <param name="x">x-coordinate of site</param>
        /// <param name="y">y-coordinate of site</param>
        /// <param name="theta">angle in radians</param>
        /// <param name="distance">desired length of line segment</param>
        /// <returns>2-element array representing the x- and y-coordinate of the point, respectively</returns>
        public static double[] getEndpointOfLineRotation(double x, double y, double theta, float distance) {
            double u = x + (distance * Math.Cos(theta));
            double v = y + (distance * Math.Cos(theta));
            return new double[] { u, v };
        }

        /// <summary>
        /// Find intersection between the lines ab and cd
        /// http://www.cs.swan.ac.uk/~cssimon/line_intersection.html
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns>2-element double representing x- and y-coordinates, respectively</returns>
        public static double[] findIntersection(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy) {
            const float EPSILON = 1e-5f;  // Floating point correction

            // Find numerator/denominator for t_a.
            float ta_numer = ((cy - dy) * (ax - cx)) + ((dx - cx) * (ay - cy));
            float ta_denom = ((dx - cx) * (ay - by)) - ((ax - bx) * (dy - cy));

            if (ta_denom == 0 || Math.Abs(ta_denom) < EPSILON) {  // Collinear
                return null;
            }

            float ta = ta_numer / ta_denom;

            if (ta < 0 || ta > 1) {  // Does not intersect on the segments
                return null;
            }

            // -----------------------------------
            // Find numerator/denominator for t_b.
            float tb_numer = ((ay - by) * (ax - cx)) + ((bx - ax) * (ay - cy));
            float tb_denom = ((dx - cx) * (ay - by)) - ((ax - bx) * (dy - cy));

            if (tb_denom == 0 || Math.Abs(tb_denom) < EPSILON) {  // Collinear
                return null;
            }

            float tb = tb_numer / tb_denom;

            if (tb < 0 || tb > 1) {  // Does not intersect on the segments
                return null;
            }

            // -----------------------------------
            // At this point, we know they intersect, so plug ta or tb into equation
            float x = ax + (ta * (bx - ax));
            float y = ay + (ta * (by - ay));

            return new double[] { x, y };
        }
    }
}