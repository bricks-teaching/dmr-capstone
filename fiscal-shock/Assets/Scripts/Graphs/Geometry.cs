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
                Debug.Log($"FATAL: Input array held more than two coordinates.");
                throw new ArgumentException();
            }
        }

        public Vertex(List<double> xy) : this(xy[0], xy[1]) {
            if (xy.Count > 2) {
                Debug.Log($"FATAL: Input list held more than two coordinates.");
                throw new ArgumentException();
            }
        }
        /* End overloaded constructors */

        /* Comparator functions - needed for LINQ GroupBy */
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
            return Math.Sqrt(Math.Pow(x - other.x, 2) + Math.Pow(y - other.y, 2));
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
        public static Vertex findNearestInList(Vertex origin, List<Vertex> others) {
            // Find all distances to the origin
            List<double> distances = others.Select(v => v.getDistanceTo(origin)).ToList();

            /* If origin is in the list, we want the second minimum distance.
             * This means we can't just take the minimum of this list.
             * But we can just choose to skip the first element in the sorted list instead, or skip none.
             */
            int skip = others.Contains(origin)? 1 : 0;
            double minimumDistance = distances.OrderBy(d => d).Skip(skip).First();
            int indexOfNearest = distances.IndexOf(minimumDistance);

            return others[indexOfNearest];
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
                Debug.Log($"FATAL: Wrong Vector3 list passed to Edge constructor (got {verts.Count}, not 2)");
                throw new ArgumentException();
            }
            vertices = verts;
            id = eid;
        }
        /* End overloaded constructors */

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
        /// http://www.cs.swan.ac.uk/~cssimon/line_intersection.html
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vertex findIntersection(Edge a, Edge b) {
            const float EPSILON = 1e-5f;  // Floating point correction
            Vertex one = a.vertices[0];
            Vertex two = a.vertices[1];
            Vertex three = b.vertices[0];
            Vertex four = b.vertices[1];

            // Find numerator/denominator for t_a.
            float ta_numer = ((three.y - four.y) * (one.x - three.x)) + ((four.x - three.x) * (one.y - three.y));
            float ta_denom = ((four.x - three.x) * (one.y - two.y)) - ((one.x - two.x) * (four.y - three.y));

            if (ta_denom == 0 || Math.Abs(ta_denom) < EPSILON) {  // Collinear
                return null;
            }

            float ta = ta_numer / ta_denom;

            if (ta < 0 || ta > 1) {  // Does not intersect on the segments
                return null;
            }

            // -----------------------------------

            // Find numerator/denominator for t_b.
            float tb_numer = ((one.y - two.y) * (one.x - three.x)) + ((two.x - one.x) * (one.y - three.y));
            float tb_denom = ((four.x - three.x) * (one.y - two.y)) - ((one.x - two.x) * (four.y - three.y));

            if (tb_denom == 0 || Math.Abs(tb_denom) < EPSILON) {  // Collinear
                return null;
            }

            float tb = tb_numer / tb_denom;

            if (tb < 0 || tb > 1) {  // Does not intersect on the segments
                return null;
            }

            // -----------------------------------

            // At this point, we know they intersect, so plug ta or tb into equation
            float x = one.x + (ta * (two.x - one.x));
            float y = one.y + (ta * (two.y - one.y));

            return new Vertex(x, y);
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
                Debug.Log($"FATAL: Input was not a triangle");
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
        public List<Edge> sides { get; } = new List<Edge>();
        public List<Vertex> vertices { get; } = new List<Vertex>();
        public List<Polygon> neighbors { get; set; } = new List<Polygon>();
        // Whether this polygon (as a Voronoi cell) is complete.
        public bool isClosed { get; set; }

        public Polygon(List<Edge> boundary) {
            sides = boundary;
            vertices = sides.SelectMany(e => e.vertices).ToList();  // does this strip duplicates?
        }

        public Polygon() {}

        public static List<Polygon> findChordlessCycles(List<Edge> edges) {
            List<Polygon> ccycles = new List<Polygon>();

            // Pick an edge
            Edge e = edges[0];
            // Add its endpoints to a polygon
            Polygon p = new Polygon();
            p.vertices.AddRange(e.vertices);
            p.sides.Add(e);
            // Find the next edge (how to determine next?)

            // 1. Pick a Voronoi site (Delaunay circumcenter)
            // 2. Extend a ray outwards at a 0 degree angle
            // 3. Add the first edge the ray intersects to the polygon (how do you determine "first edge that intersects"?)
            // 4. Rotate the ray clockwise until it passes one endpoint of the edge
            // 5. Repeat 3-5 until the same edge is encountered

            // Each delaunay neighbor corresponds to an edge of a cell
            // But the edge between each pair of neighbors doesn't necessarily intersect a new cell edge
            // Can we guarantee that at least one neighbor *does* provide such an intersection? This narrows down edges to check

            return ccycles;
        }
    }
}