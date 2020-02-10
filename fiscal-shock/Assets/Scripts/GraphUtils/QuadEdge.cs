using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// <para>C# implementation of Guibas & Stolfi's Delaunay triangulation algorithm.</para>
/// <para>The divide-and-conquer algorithm requires all points for the Delaunay triangulation to be known. So, it only tries to make connections between those given points. The incremental algorithm allows the addition of new sites to an existing triangulation. Since this requires verifying Delaunayhood and fixing it if required, it is slower.</para>
///
///
/// <para>References:
///
/// Guibas, L., and Stolfi, J. (1985). Primitives for the Manipulation of General Subdivisions and the Computation of Voronoi Diagrams. Retrieved from https://dl-acm-org.leo.lib.unomaha.edu/doi/pdf/10.1145/282918.282923?download=true.
///     Original paper on the divide-and-conquer and incremental algorithms to compute a Delaunay triangulation, hereafter referred to as "G&S."
///
/// Heckbert, P. (2001). Quad-Edge Data Structure and Library. Retrieved from https://www.cs.cmu.edu/afs/andrew/scs/cs/15-463/2001/pub/src/a2/quadedge.html.
///     An explanation and implementation of the quad-edge data structure introduced in G&S in C++. The actual C++ code is by Andrew Bernard, based on existing code by Dani Lischinski.
///
/// Cheung, B. (2019). bennycheung/PyDelaunay: Python implementation of Delaunay and Voronoi Tessellation. Retrieved from https://github.com/bennycheung/PyDelaunay.
///     Incremental G&S algorithm in Python. MIT-licensed.</para>
/// </summary>

namespace FiscalShock.QraphUtils {
    public static class LinAlg {
        /// <summary>
        /// Determinant of a 4x4 matrix for InCircle test based on 4 points using Laplace expansion
        /// There's probably a cleaner formula out there...
        /// |.x	a .ya  .x	a^2 +.ya^2)  1 |
        /// |.x	b .yb  .x	b^2 +.yb^2)  1 |
        /// |.x	c .yc  .x	c^2 +.yc^2)  1 |
        /// |.x	d .yd  .x	d^2 +.yd^2)  1 |
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static double determinant4(Vertex a, Vertex b, Vertex c, Vertex d) {
            double m02 = (a.x*a.x) + (a.y*a.y);
            double m12 = (b.x*b.x) + (b.y*b.y);
            double m22 = (c.x*c.x) + (c.y*c.y);
            double m32 = (d.x*d.x) + (d.y*d.y);
            return  1 * m12 * c.y * d.x
                  - m02 * 1 * c.y * d.x
                  - 1 * b.y * m22 * d.x
                  + a.y * 1 * m22 * d.x
                  + m02 * b.y * 1 * d.x
                  - a.y * m12 * 1 * d.x
                  - 1 * m12 * c.x * d.y
                  + m02 * 1 * c.x * d.y
                  + 1 * b.x * m22 * d.y
                  - a.x	* 1 * m22 * d.y
                  - m02 * b.x * 1 * d.y
                  + a.x	* m12 * 1 * d.y
                  + 1 * b.y * c.x * m32
                  - a.y * 1 * c.x * m32
                  - 1 * b.x * c.y * m32
                  + a.x	* 1 * c.y * m32
                  + a.y * b.x * 1 * m32
                  - a.x * b.y * 1 * m32
                  - m02 * b.y * c.x * 1
                  + a.y * m12 * c.x * 1
                  + m02 * b.x * c.y * 1
                  - a.x * m12 * c.y * 1
                  - a.y * b.x * m22 * 1
                  + a.x * b.y * m22 * 1
            ;
        }

        /// <summary>
        /// Area of the triangle abc is the following determinant:
        /// | a.x  a.y  1 |
        /// | b.x  b.y  1 |
        /// | c.x  c.y  1 |
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double areaOfTriangle(Vertex a, Vertex b, Vertex c) {
            return (b.x	- a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }
    }

    public class Vertex {
        public double x { get; set; }
        public double y { get; set; }
        // possibly incident edges

        public Vertex(double xX, double yY) {
            x = xX;
            y = yY;
        }

        public string toString() {
            return $"({x}, {y})";
        }
    }

    public class Edge {
        // ID of this edge within the QuadEdge
        public int id { get; set; }

        // Next edge
        public Edge next { get; set; }

        // Endpoints of the edge
        public Vertex tail { get; set; }
        public Vertex head { get; set; }

        public QuadEdge quad { get; set; }

        public string toString() {
            return $"Edge {id} from {tail.toString()} to {head.toString()}";
        }

        /// <summary>
        /// Get the right-to-left dual of this edge
        /// </summary>
        public Edge rotate() {  // Rot
            return quad.edges[
                   id < 3?
                   id + 1 : id - 3
            ];
        }

        /// <summary>
        /// Get the left-to-right dual of this edge
        /// </summary>
        /// <returns></returns>
        public Edge inverseRotate() {  // InvRot
            return quad.edges[
                   id > 0?
                   id - 1 : id + 3
            ];
        }

        /// <summary>
        /// Get the edge with head (destination) and tail (origin) flipped
        /// </summary>
        /// <returns></returns>
        public Edge symmetric() {  // Sym
            return quad.edges[
                   id < 2 ?
                   id + 2 : id - 2
            ];
        }

        /// <summary>
        /// Get the next edge, counterclockwise, around the tail (origin) of this edge
        /// </summary>
        /// <returns></returns>
        public Edge nextFromTail() {  // Onext
            if (id == 0 || id == 2) {
                Console.WriteLine($"Onext is myself? id: {id} {this == next}");
            } else if (id == 1) {
                Console.WriteLine($"Onext is 3? id: {id} {quad.edges[3] == next}");
            } else if (id == 3) {
                Console.WriteLine($"Onext is 1? id: {id} {quad.edges[1] == next}");
            }
            return next;
        }

        /// <summary>
        /// Get the next edge, clockwise, around the tail of this edge
        /// </summary>
        /// <returns></returns>
        public Edge prevFromTail() {  // Oprev
            // Rot -> Onext -> Rot
            return rotate().nextFromTail().rotate();
        }

        /// <summary>
        /// Get the next edge, counterclockwise, around the head (destination) of this edge
        /// </summary>
        /// <returns></returns>
        public Edge nextFromHead() {  // Dnext
            // Sym -> Onext -> Sym
            return symmetric().nextFromTail().symmetric();
        }

        /// <summary>
        /// Get the next edge, clockwise, around the head of this edge
        /// </summary>
        /// <returns></returns>
        public Edge prevFromHead() {  // Dprev
            // InvRot -> Onext -> InvRot
            return inverseRotate().nextFromTail().inverseRotate();
        }

        /// <summary>
        /// Get the edge bounding the left face, counterclockwise, after this edge
        /// </summary>
        /// <returns></returns>
        public Edge nextFromLeft() {  // Lnext
            // InvRot -> Onext -> Rot
            return inverseRotate().nextFromTail().rotate();
        }

        /// <summary>
        /// Get the edge bounding the left face, counterclockwise, before this edge
        /// </summary>
        /// <returns></returns>
        public Edge prevFromLeft() {  // Lprev
            // Onext -> Sym
            return symmetric().nextFromTail();
        }

        /// <summary>
        /// Get the edge bounding the right face, counterclockwise, after this edge
        /// </summary>
        /// <returns></returns>
        public Edge nextFromRight() {  // Rnext
            // Rot -> Onext -> InvRot
            return rotate().nextFromTail().inverseRotate();
        }

        /// <summary>
        /// Get the edge bounding the right face, counterclockwise, before this edge
        /// </summary>
        /// <returns></returns>
        public Edge prevFromRight() {  // Rprev
            // Sym -> Onext
            return symmetric().nextFromTail();
        }
    }

    public class QuadEdge {
        // Edges of the QuadEdge. Different ways to represent the edge and its dual (e*).
        // 0    e
        // 1    e.rotate()          e*, rotated counterclockwise 90 degrees
        // 2    e.symmetric()       e, with head and tail flipped
        // 3    e.inverseRotate()   e*, rotated clockwise 90 degrees
        public List<Edge> edges;

        public QuadEdge() {
            edges = new List<Edge> { new Edge(), new Edge(), new Edge(), new Edge() };  // TODO not so dumb

            int i = 0;
            foreach (Edge e in edges) {
                e.id = i;
                e.quad = this;

                i++;
            }
            edges[0].next = edges[0];
            edges[1].next = edges[3];
            edges[2].next = edges[2];
            edges[3].next = edges[1];
        }

        /// <summary>
        /// <para>Set the endpoints for the primal edges of the quad edge</para>
        ///
        /// <para>We can't set the endpoints of the duals (1, 3) without already knowing the triangulation. But the D&C algorithm also states that it doesn't need the endpoints of the dual edges to run, so we shouldn't encounter problems.</para>
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        public QuadEdge(Vertex origin, Vertex destination) : this() {  // also call the default constructor first
        Console.WriteLine("Calling vertex constructor");
            edges[0].tail = origin;
            edges[0].head = destination;
            edges[2].tail = destination;
            edges[2].head = origin;
            Console.WriteLine($"0: {edges[0].toString()}");
            Console.WriteLine($"2: {edges[2].toString()}");
        }

        /// <summary>
        /// Get the representative edge
        /// </summary>
        /// <returns></returns>
        public Edge getEdge() {
            return edges[0];
        }

        /// <summary>
        /// G&S, pp. 98-102
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void splice(Edge a, Edge b) {
            Edge alpha = a.nextFromTail().rotate();
            Edge beta = b.nextFromTail().rotate();

            List<Edge> tmp = new List<Edge> {
                a.nextFromTail(),
                b.nextFromTail(),
                alpha.nextFromTail(),
                beta.nextFromTail()
            };

            b.next = tmp[0];
            a.next = tmp[1];
            beta.next = tmp[2];
            alpha.next = tmp[3];
        }

        /// <summary>
        /// G&S, pp. 102-103
        /// </summary>
        public static void removeEdge(Edge e) {
            splice(e, e.prevFromTail());
            splice(e.symmetric(), e.symmetric().prevFromTail());
        }

        /// <summary>
        /// G&S, p. 103
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Edge connect(Edge a, Edge b) {
            Edge e = new QuadEdge(a.head, b.tail).getEdge();
            splice(e, a.nextFromLeft());
            splice(e.symmetric(), b);
            return e;
        }
    }

    public class Delaunay {
        public List<Vertex> delaunayPoints;

        /// <summary>
        /// G&S, p. 113
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool ccw(Vertex a, Vertex b, Vertex c) {
            return LinAlg.areaOfTriangle(a, b, c) > 0;
        }

        private bool rightOf(Vertex point, Edge e) {
            return ccw(point, e.head, e.tail);
        }

        private bool leftOf(Vertex point, Edge e) {
            // e.tail and e.head are not defined here during triangulation
            return ccw(point, e.tail, e.head);
        }

        /// <summary>
        /// G&S, p. 113
        /// Is this edge above the base line?
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool isEdgeValid(Edge e, Edge baseLine) {
            return rightOf(e.head, baseLine);
        }

        private bool inCircle(Vertex a, Vertex b, Vertex c, Vertex d) {
            return LinAlg.determinant4(a, b, c, d) > 0;
        }

        private void printPoints(List<Edge> edges) {  // temp
            foreach (Edge e in edges) {
                System.Console.WriteLine($"{e.toString()}");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="points"></param>
        public Delaunay(List<Vertex> points) {
            // Sort the input list by x, then y
            delaunayPoints = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();

            // triangulate right now just returns 2 edges, how 2 store the right ones
            List<Edge> ded = triangulate(delaunayPoints);
        }

        private List<Edge> triangulate(List<Vertex> points) {
            Edge baseLine;  // basel is updated during this function so it should be local

            if (points.Count == 2) {
                // Only two points, so make an edge from points[0] to points[1]
                Edge a = new QuadEdge(points[0], points[1]).getEdge();
                return new List<Edge> { a, a.symmetric() };
            } else if (points.Count == 3) {
                // Connect (a) points[0] to points[1] and (b) points[1] to points[2]
                Edge a = new QuadEdge().getEdge();
                Edge b = new QuadEdge().getEdge();
                QuadEdge.splice(a.symmetric(), b);
                a.tail = points[0];
                a.head = b.tail = points[1];
                b.head = points[2];
            Console.WriteLine("Spliced:");
            Console.WriteLine($"a: {a.toString()}");
            Console.WriteLine($"b: {b.toString()}");
                // Two edges, so we can make a triangle here
                if (ccw(points[0], points[1], points[2])) {
                    QuadEdge.connect(b, a);  // throw away the resulting line for some reason
                    return new List<Edge> { a, b.symmetric() };
                } else if (ccw(points[0], points[2], points[1])) {  // It's a different order in G&S than the first check
                    Edge c = QuadEdge.connect(b, a);
                    return new List<Edge> { c.symmetric(), c };
                } else {  // collinear case
                    return new List<Edge> { a, b.symmetric() };
                }
            } else {  // divide-and-conquer
                // split into L and R
                int leftHalfSize = (points.Count & 1) == 1?
                                  (points.Count/2) + 1 : (points.Count/2);
                List<Vertex> leftHalf = points.GetRange(0, leftHalfSize);
                List<Vertex> rightHalf = points.GetRange(leftHalfSize, points.Count - leftHalfSize);
                List<Edge> ld = triangulate(leftHalf);  // ld[0] = ldo, ld[1] = ldi
                List<Edge> rd = triangulate(rightHalf);  // rd[0] = rdo, rd[1] = rdi

                bool foundTangent = false;
                while (!foundTangent) {
                    if (leftOf(rd[1].tail, ld[1])) {
                        ld[1] = ld[1].nextFromLeft();
                    } else if (rightOf(ld[1].tail, rd[1])) {
                        rd[1] = rd[1].prevFromRight();
                    } else {
                        foundTangent = true;  // stop looking, lower common tangent found
                    }
                }

                // Set up the initial basel and update weird ldo/rdo
                baseLine = QuadEdge.connect(rd[1].symmetric(), ld[1]);
                if (ld[1].tail == ld[0].tail) {
                    ld[0] = baseLine.symmetric();
                }
                if (rd[1].tail == rd[0].tail) {
                    rd[0] = baseLine;
                }

                foundTangent = false;
                while (!foundTangent) {
                    Edge leftCandidate = findCandidate(true, baseLine);
                    Edge rightCandidate = findCandidate(false, baseLine);

                    if (!isEdgeValid(leftCandidate, baseLine) && !isEdgeValid(rightCandidate, baseLine)) {
                        foundTangent = true;  // upper common tangent found
                    }

                    if (!isEdgeValid(leftCandidate, baseLine)
                       || (isEdgeValid(rightCandidate, baseLine) && inCircle(leftCandidate.head, leftCandidate.tail, rightCandidate.tail, rightCandidate.head))) {
                        // Add cross edge basel from rcand.Dest to basel.Dest
                        baseLine = QuadEdge.connect(rightCandidate, baseLine.symmetric());
                    } else { // pick lcand
                        baseLine = QuadEdge.connect(baseLine.symmetric(), leftCandidate.symmetric());
                    }
                }
                return new List<Edge> { ld[0], rd[0] };
            }
        }

        // this is ugly
        private Edge findCandidate(bool isLeftCandidate, Edge baseLine) {
            Edge candidate;
            Vertex candidatePoint;
            if (isLeftCandidate) {
                candidate = baseLine.symmetric().nextFromTail();
                candidatePoint = candidate.nextFromTail().head;
            } else { // rcand
                candidate = baseLine.prevFromTail();
                candidatePoint = candidate.prevFromTail().head;
            }

            if (isEdgeValid(candidate, baseLine)) {
                while (inCircle(baseLine.head, baseLine.tail, candidate.head, candidatePoint)) {
                    Edge temp = isLeftCandidate? candidate.nextFromTail() : candidate.prevFromTail();
                    QuadEdge.removeEdge(candidate);
                    candidate = temp;
                }
            }

            return candidate;
        }
    }

    // Test method
    static class TestClass {
        public static void Main() {
            Console.WriteLine("making points");
            List<Vertex> pts = new List<Vertex> {
                new Vertex(0, 0),
                new Vertex(1, 2),
                new Vertex(3, 1),
                new Vertex(1, 3),
                new Vertex(3, 0),
                new Vertex(0, 6)
            };

            Delaunay dt = new Delaunay(pts);
        }
    }
}