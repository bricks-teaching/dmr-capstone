using System.Globalization;
using System.Net.Mime;
using System.Security.Cryptography;
using System;
using System.Linq;
using System.Collections.Generic;

// demo triangles
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace FiscalShock.GraphUtils {
    public class Vertex {
        public double x { get; }
        public double y { get; }
        public List<Triangle> adjacentTris { get; set; }

        public Vertex(int xX, int yY) {
            x = xX;
            y = yY;
            adjacentTris = new List<Triangle>();
        }

        public Vertex(double xX, double yY) {
            x = xX;
            y = yY;
            adjacentTris = new List<Triangle>();
        }

        public static double getDistance(Vertex a, Vertex b) {
            return Math.Sqrt(
                       Math.Pow(a.x - b.x, 2)
                     + Math.Pow(a.y - b.y, 2));
        }

        public bool Equals(Vertex other) {
            return x == other.x && y == other.y;
        }
    }

    public class Edge {
        public Vertex tail { get; set; }
        public Vertex head { get; set; }
        public double length { get; }

        /// <summary>
        /// Connect two vertices
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        public Edge(Vertex origin, Vertex destination) {
            tail = origin;
            head = destination;
            length = Vertex.getDistance(tail, head);
        }

        // public bool Equals(Edge obj) {
        //     // See the full list of guidelines at
        //     //   http://go.microsoft.com/fwlink/?LinkID=85237
        //     // and also the guidance for operator== at
        //     //   http://go.microsoft.com/fwlink/?LinkId=85238
        //     //
        //     return (
        //         (head.Equals(obj.head) && tail.Equals(obj.tail))
        //         || (head.Equals(obj.tail) && tail.Equals(obj.head))
        //     );
        // }
        // // override object.GetHashCode
        // public override int GetHashCode()
        // {
        //     // TODO: write your implementation of GetHashCode() here
        //     throw new System.NotImplementedException();
        //     return base.GetHashCode();
        // // }
    }

    public class Triangle {
        public List<Edge> edges { get; }
        public List<Vertex> vertices { get; }
        public double circumradius { get; }
        public double circumradius2 { get; }
        public Vertex circumcenter { get; }

        /// <summary>
        /// Form a triangle from a given edge and a point
        /// Basically, connect both ends of e to v
        /// </summary>
        /// <param name="e"></param>
        /// <param name="v"></param>
        public Triangle(Edge e, Vertex v) : this() {
            if (!areVerticesCCW(e.head, e.tail, v)) {
                // transposing any 2 vertices will change to ccw
                // proof is in Guibas & Stolfi (1985)
                vertices.Add(e.tail);
                vertices.Add(e.head);
            } else {  // points were ccw, add in that order
                vertices.Add(e.head);
                vertices.Add(e.tail);
            }
            vertices.Add(v);
            e.tail.adjacentTris.Add(this);
            e.head.adjacentTris.Add(this);
            v.adjacentTris.Add(this);
            edges.Add(e);
            edges.Add(new Edge(e.head, v));
            edges.Add(new Edge(v, e.tail));
            circumradius = findCircumradius();
            circumradius2 = Math.Pow(circumradius, 2);
            circumcenter = findCircumcenter();
        }

        public Triangle() {
            edges = new List<Edge>(3);
            vertices = new List<Vertex>(3);
        }

        private bool areVerticesCCW(Vertex a, Vertex b, Vertex c) {
            return ((b.x - a.x) * (c.y - a.y)
                  - (c.x - a.x) * (b.y - a.y))
                  > 0;
        }

        /// <summary>
        /// When making the triangle, we made sure the vertices were in
        /// ccw order, so if det > 0 then d is inside
        /// https://stackoverflow.com/questions/39984709/how-can-i-check-wether-a-point-is-inside-the-circumcircle-of-3-points
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public bool isPointInCircumcircleSlow(Vertex d) {
            double _ax = vertices[0].x - d.x;
            double _ay = vertices[0].y - d.y;
            double _bx = vertices[1].x - d.x;
            double _by = vertices[1].y - d.y;
            double _cx = vertices[2].x - d.x;
            double _cy = vertices[2].y - d.y;

            return (
                  (Math.Pow(_ax, 2) + Math.Pow(_ay, 2)) + (_bx * _cy - _cx * _by)
                - (Math.Pow(_bx, 2) + Math.Pow(_by, 2)) + (_ax * _cy - _cx * _ay)
                + (Math.Pow(_cx, 2) + Math.Pow(_cy, 2)) + (_ax * _by - _bx * _ay)
            ) > 0;
        }

        /// <summary>
        /// Thanks, Pythagoras
        /// (x-center_x)^2 + (y - center_y)^2 < radius^2
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public bool isPointInCircumcircle(Vertex d) {
            return Math.Sqrt((Math.Pow(circumcenter.x - d.x, 2) + Math.Pow(circumcenter.y - d.y, 2))) < circumradius;
        }


        /// <summary>
        /// Convert barycentric to Cartesian
        /// https://en.wikipedia.org/wiki/Circumscribed_circle#Circumcenter_coordinates
        /// </summary>
        /// <returns></returns>
        private Vertex findCircumcenter() {
            // lengths = [a, b, c]
            double[] lengths = edges.Select(e => e.length).ToArray();

            // barycentric coords
            double a2 = Math.Pow(lengths[0], 2);
            double b2 = Math.Pow(lengths[1], 2);
            double c2 = Math.Pow(lengths[2], 2);
            double[] barycenter = new double[] {
                a2 * (b2 + c2 - a2),
                b2 * (c2 + a2 - b2),
                c2 * (a2 + b2 - c2)
            };

            // convert to Cartesian
            return new Vertex(
                barycenter[0] * vertices[0].x
              + barycenter[1] * vertices[1].x
              + barycenter[2] * vertices[2].x,

                barycenter[0] * vertices[0].y
              + barycenter[1] * vertices[1].y
              + barycenter[2] * vertices[2].y
            );
        }

        /// <summary>
        /// https://www.mathopenref.com/trianglecircumcircle.html
        /// </summary>
        private double findCircumradius() {
            // a*b*c
            // lengths = [a, b, c]
            double[] lengths = edges.Select(e => e.length).ToArray();
            double numerator = lengths[0] * lengths[1] * lengths[2];
            double denominator = Math.Sqrt(
                  (lengths[0] + lengths[1] + lengths[2])
                * (lengths[1] + lengths[2] - lengths[0])
                * (lengths[2] + lengths[0] - lengths[1])
                * (lengths[0] + lengths[1] - lengths[2])
            );
            return numerator / denominator;
        }
    }

    public class Delaunay {
        public List<Vertex> points { get; }
        public List<Triangle> tris { get; private set; }
        public List<Edge> tmp { get; set; }

        /// <summary>
        /// takes array of ints where each even index is an x-coordinate and
        /// the following odd index is the corresponding y-coordinate
        /// </summary>
        /// <param name="input"></param>
        public Delaunay(int[] input) {
            points = new List<Vertex>(input.Length/2);
            for (int i = 0; i < input.Length; i += 2) {
                points[i] = new Vertex(input[i], input[i+1]);
            }

            doTriangulation();
        }

        public Delaunay(List<Vertex> input) {
            points = input;
            doTriangulation();
        }

        /// <summary>
        /// Maybe someday there will be another algorithm
        /// </summary>
        public void doTriangulation() {
            triangulateBowyerWatson();
        }

        /// <summary>
        /// Naive Bowyer-Watson. O(n^2) implementation
        /// </summary>
        public void triangulateBowyerWatson() {
            // do super triangle
            tris = new List<Triangle>();
            List<Triangle> superTriangle = makeSuperTriangles();
            Console.WriteLine("supertriangle");

            tris.AddRange(superTriangle);
            int it = 0;
            foreach (Vertex v in points) {
                Console.WriteLine($"v: {it}");
                Console.WriteLine("finding bads");
                List<Triangle> badTriangles = tris.Where(t => t.isPointInCircumcircleSlow(v)).ToList();

                Console.WriteLine("removing bads");
                long ms1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                List<Edge> polygonalHole = badTriangles
                                           .SelectMany(bt => bt.edges)
                                           .GroupBy(e => e)
                                           //.Where(e => e.Count() == 1)
                                           .Select(e => e.First())
                                           .ToList();
                // List<Edge> polygonalHole = new List<Edge>();

                // unlink bad triangle from vertices
                // var tmp = points
                     //   .Where(p => p.adjacentTris
                                 //   .Any(a => !badTriangles.Contains(a)));
                // // foreach (Triangle bt in badTriangles) {
                //     foreach (Vertex p in points) {
                //         p.adjacentTris.Remove(bt);
                //         Console.Write(".");
                //     }
                //     // ...
                //     tmp = badTriangles
                //             .SelectMany(bt => bt.edges)
                //             .ToList();
                //     for (int i = 0; i < tmp.Count; ++i) {
                //         bool ok = true;
                //         for (int j = 0; j < tmp.Count; ++j) {
                //             if (i == j)  continue;  // same
                //             if (tmp[i].Equals(tmp[j])) {
                //                 ok = false;
                //             }
                //         }
                //         if (ok) {
                //             polygonalHole.Add(tmp[i]);
                //         }
                //     }
                //     // ...
                //     tris.Remove(bt);
                // // }
                //tris.RemoveAll(bt => badTriangles.Contains(bt));
                Console.WriteLine($"took {(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - ms1}ms to remove {badTriangles.Count} bad tris");
               //  foreach (Triangle bt in badTriangles) {  // find boundary of polygonal hole
               //      List<Edge> otherBadEdges = badTriangles.Where(t => t != bt).SelectMany(t => t.edges).ToList();
               //      foreach (Edge e in bt.edges) {
               //          if (!otherBadEdges.Contains(e)) {
               //              polygonalHole.Add(e);
               //          }
               //      }
               //      tris.Remove(bt);
               //      Console.Write(".");
               //  }

                Console.WriteLine($"triangulating hole on {polygonalHole.Count} edges");
                foreach (Edge e in polygonalHole) {
                    tris.Add(new Triangle(e, v));
                }
                it++;
            }

            // after points have been inserted, clear out the supertriangle vertices
            Console.WriteLine("cleaning up");
            //List<Triangle> trisToRemove = new List<Triangle>();
            //foreach (Triangle t in tris) {
            //    if (t.vertices.Any(v => superTriangle.Select(st => st.vertices.Contains(v))) {
            //        trisToRemove.Add(t);
            //    }
            //}
            //tris = tris.Where(t => !trisToRemove.Contains(t)).ToList();
        }

        private List<Triangle> makeSuperTriangles() {
            // find bounds for the supertri
            IEnumerable<double> xs = points.Select(p => p.x);
            IEnumerable<double> ys = points.Select(p => p.y);
           //  double dx = xs.Max() - xs.Min();
           //  double dy = ys.Max() - ys.Min();
           //  double dmax = (dx > dy? dx : dy) * 20;  // not sure why it's 20 but okay
           //  double xmid = (xs.Max() + xs.Min()) * 0.5;
           //  double ymid = (ys.Max() + ys.Min()) * 0.5;

           //  Vertex a = new Vertex(xmid - dmax, ymid - dmax);
           //  Vertex b = new Vertex(xmid, ymid + dmax);
           //  Vertex c = new Vertex(xmid + dmax, ymid - dmax);
           //  Edge e = new Edge(a, b);
           //  return new Triangle(e, c);
            Vertex a = new Vertex(0, 0);
            Vertex b = new Vertex(0, ys.Max());
            Vertex c = new Vertex(xs.Max(), ys.Max());
            Vertex d = new Vertex(xs.Max(), 0);
            Edge ab = new Edge(a, b);
            Edge cd = new Edge(c, d);
            return new List<Triangle> {
                new Triangle(ab, c),
                new Triangle(cd, a)
            };
        }
    }

    class DTDemo : Form {
        public Delaunay dt;

        public static void Main() {
            Application.Run(new DTDemo());
        }

        public DTDemo() {
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs pea) {
            int min = 0;
            int max = 100;
            int n = 10;
            Console.WriteLine("making points");
            List<Vertex> pts = new List<Vertex>();
            long ms1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Random rand = new Random(1246555755);
            for (int i = 0; i < n; ++i) {
                pts.Add(new Vertex(rand.Next(min, max), rand.Next(min, max)));
            }
            Console.WriteLine($"took {(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - ms1}ms");

            Console.WriteLine("starting delaunay");
            long ms2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            dt = new Delaunay(pts);

            Console.WriteLine("done!");
            Console.WriteLine($"took {(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - ms2}ms");

            // Create pen.
            Pen blackPen = new Pen(Color.Black, 1);
            Pen redPen = new Pen(Color.Red, 1);

            // Draw each point
            for (int i = 0; i < dt.points.Count; ++i) {
                pea.Graphics.DrawEllipse(redPen, new Rectangle((int)dt.points[i].x, (int)dt.points[i].y, 3, 3));
            }

            List<Edge> es = dt.tris.SelectMany(t => t.edges).ToList();
            foreach (Edge e in es) {
                pea.Graphics.DrawLine(blackPen, (int)e.head.x, (int)e.head.y, (int)e.tail.x, (int)e.tail.y);
            }
        }
    }
}