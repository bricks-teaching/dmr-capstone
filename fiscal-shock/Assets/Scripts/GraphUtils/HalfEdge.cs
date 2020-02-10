using System.Net.NetworkInformation;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

// demo triangles
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace FiscalShock.PraphUtils {
    public class Vertex {
        public int x { get; set; }
        public int y { get; set; }
        public double distanceFromSeedCenter { get; set; }
        public int origId { get; set; }

        public Vertex(int xX, int yY) {
            x = xX;
            y = yY;
        }

        public string toString() {
            return $"({x}, {y})";
        }
    }

    /// <summary>
    /// <para>
    /// An arc is a directed edge from an origin (tail) vertex to a destination
    /// (head) vertex. Delaunator and other libraries refer to arcs as "half-
    /// edges" by considering an edge bounding a geometric shape as being made
    /// of two arcs: edge E connecting vertices A and B can be split into arcs
    /// (or half-edges) H1 A->B and H2 B->A.
    ///
    /// Using arcs (half-edges) allows a single pair of connected points A and B
    /// to bound two triangles, instead of one.
    /// </para>
    ///
    /// A ======= edge E ======= B
    ///
    ///
    ///   ------- arc H1 ------>     H1 bounds triangle T1
    /// A                        B
    ///   <------ arc H2 -------     H2 bounds triangle T2
    ///
    /// </summary>
    public class Arc {
        public int id { get; set; }
        public Vertex tail { get; set; }
        public Vertex head { get; set; }
    }

    public class Triangle {
        public int id { get; set; }
        public Vertex[] vertices { get; set; }
    }

    public class Delaunay {
        public int[] vertices;

        /// <summary>
        /// Associative array of indices of vertices
        /// </summary>
        public int[] triangles;
        public int[] arcs;

        private static readonly double EPSILON = Math.Pow(2, -52);
        private int hashSize;
        private int hullStart;
        private int[] hullPrev;
        private int[] hullNext;
        private int[] hullTri;
        private int[] hullHash;
        private int[] ids;
        private int[] convexHull;
        private Vertex seedCenter;
        private int totalTris;  // number of triangles so far (times 3 since it's indices of vertices)
        private int[] edgeStack = new int[512];  // used to eliminate recursion

        /*
        /// <summary>
        /// The edges of a triangle with id n are the arcs whose ids are 3 * n, 3 * n + 1, and 3 * n + 2.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<Arc> getArcsOfTriangle(Triangle t) {
            int idx = 3 * t.id;
            return new List<Arc> {
                arcs[idx],
                arcs[idx + 1],
                arcs[idx + 2]
            };
        }

        /// <summary>
        /// The triangle bounded by arc e is the triangle whose id is equal to floor(e.id / 3).
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Triangle getTriangleOfArc(Arc e) {
            return triangles[e.id / 3];
        }

        public Arc nextArc(Arc e) {
            return arcs[
                (e.id % 3 == 2)?
                e.id - 2 : e.id + 1
            ];
        }

        public Arc prevArc(Arc e) {
            return arcs[
                (e.id % 3 == 0)?
                e.id + 2 : e.id - 1
            ];
        }
        */
        /*public void forEachTriangleEdge(Func fun) {
            for (int ti = 0; ti < triangles.Length; ++ti) {
                if (ti > arcs[ti]) {  // why comparing int to Arc???

                }
            }
        }*/

        /// <summary>
        /// Determine the hash key of a given vertex
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int getHashKey(Vertex v) {
            return (int)(
                Geometry.getPseudoAngle(v.x - seedCenter.x, v.y - seedCenter.y)
                * hashSize) % hashSize;
        }
        private int getHashKey(int vx, int vy) {
            return (int)(
                Geometry.getPseudoAngle(vx - seedCenter.x, vy - seedCenter.y)
                * hashSize) % hashSize;
        }

        private void linkArcsByIndices(int a, int b) {
            arcs[a] = b;
            if (b != -1) {
                arcs[b] = a;
            }
            Console.WriteLine($"linked {a}-{b}");
        }

        private int addTriangleByIndices(int v1, int v2, int v3, int e1, int e2, int e3) {
            int t = totalTris;
            Console.WriteLine($"adding triangle {t}");
            triangles[t] = v1;
            triangles[t + 1] = v2;
            triangles[t + 2] = v3;

            linkArcsByIndices(t, e1);
            linkArcsByIndices(t + 1, e2);
            linkArcsByIndices(t + 2, e3);

            totalTris += 3;
            return t;
        }

        private int legalizeTriangle(int a) {
            int i = 0;
            int ar = 0;

            while(true) {
                int b = arcs[a];

            /* comment stolen from original delaunator
             * if the pair of triangles doesn't satisfy the Delaunay condition
             * (p1 is inside the circumcircle of [p0, pl, pr]), flip them,
             * then do the same check/flip recursively for the new pair of triangles
             *
             *           pl                    pl
             *          /||\                  /  \
             *       al/ || \bl            al/    \a
             *        /  ||  \              /      \
             *       /  a||b  \    flip    /___ar___\
             *     p0\   ||   /p1   =>   p0\---bl---/p1
             *        \  ||  /              \      /
             *       ar\ || /br             b\    /br
             *          \||/                  \  /
             *           pr                    pr
             */
             // mod 3 to find triangle ids
                int a0 = a - a % 3;
                ar = a0 + (a + 2) % 3;

                if (b == -1) {  // b is on the convex hull
                   if (i == 0)  break;  // ... okay then
                   a = edgeStack[--i];
                   continue;
                }

                int b0 = b - b % 3;
                int al = a0 + (a + 1) % 3;
                int bl = b0 + (b + 2) % 3;

                int p0 = triangles[ar];
                int pr = triangles[a];
                int pl = triangles[al];
                int p1 = triangles[bl];

                bool illegal = Geometry.isInCircle(vertices[2*p0], vertices[2*p0+1], vertices[2*pr], vertices[2*pr+1], vertices[2*pl], vertices[2*pl+1], vertices[2*p1], vertices[2*p1+1]);

                if (illegal) {
                    triangles[a] = p1;
                    triangles[b] = p0;

                    int hbl = arcs[bl];

                    // fix arc reference if it was moved to other side of hull
                    if (hbl == -1) {
                        int e = hullStart;
                        do {
                            if (hullTri[e] == bl) {
                                hullTri[e] = a;
                                break;
                            }
                            e = hullPrev[e];
                        } while (e != hullStart);
                    }

                    linkArcsByIndices(a, hbl);
                    linkArcsByIndices(b, arcs[ar]);
                    linkArcsByIndices(ar, bl);

                    int br = b0 + (b + 1) % 3;
                    if (i < edgeStack.Length) {
                        edgeStack[i++] = br;
                    }
                } else {  // legal
                    if (i == 0) {
                        break;
                    }
                    a = edgeStack[--i];
                }
            }

            return ar;
        }

        /// <summary>
        /// Fastest way to fill an array in C#, sadly
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="value"></param>
        private void fillArray(int[] arr, int value) {
            for (int i = 0; i < arr.Length; ++i) {
                arr[i] = value;
            }
        }

        private int[] vertexToInts(Vertex[] points) {
            int[] vs = new int[points.Length*2];
            for (int i = 0, z = 0; i < points.Length; ++i, z += 2) {
                vs[z] = (int)points[i].x;
                vs[z+1] = (int)points[i].y;
            }
            return vs;
        }

        public Delaunay(Vertex[] points) {
            //vertices = points;
            vertices = vertexToInts(points);
            int numPoints = points.Length >> 1;
            int maxNumTriangles = 2 * numPoints - 5;
            triangles = new int[maxNumTriangles * 3*2];  // associated with 3 vertices here
            fillArray(triangles, 0);
            arcs = new int[maxNumTriangles * 3*2];  // 3 arcs for each triangle
            fillArray(arcs, 0);
            ids = new int[vertices.Length];
            /*for (int i = 0; i < ids.Length; ++i) {  // fill the ids array, fastest method
                ids[i] = i;
            }*/

            // convex hull stuff
            hashSize = (int)Math.Ceiling(Math.Sqrt(numPoints));
            hullPrev = new int[numPoints*2];
            hullNext = new int[numPoints*2];
            hullTri = new int[numPoints*2];
            hullHash = new int[hashSize];
            fillArray(hullHash, -1);

            // still okay because we just want the values
            List<Vertex> sortedX = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
            List<Vertex> sortedY = points.OrderBy(p => p.y).ToList();
            double minX = sortedX[0].x;
            double minY = sortedY[0].y;
            double maxX = sortedX.Last().x;
            double maxY = sortedY.Last().y;

            Vertex absoluteCenter = new Vertex(
                (int)((minX + maxX) / 2.0),
                (int)((minY + maxY) / 2.0)
            );

            double minDist = double.PositiveInfinity;
            int[] s = new int[3];
            Console.WriteLine("finding seed triangle");
            // Find seed point close to center
            for (int i = 0; i < numPoints; ++i) {
                double dist = Geometry.getDistance(absoluteCenter, points[i]);
                if (dist < minDist) {
                    s[0] = i;
                    minDist = dist;
                }
            }
            // reset
            minDist = double.PositiveInfinity;
            for (int i = 0; i < numPoints; ++i) {
                if (i == s[0])  continue;
                double dist = Geometry.getDistance(points[s[0]], points[i]);
                if (dist < minDist && dist > 0) {
                    s[1] = i;
                    minDist = dist;
                }
            }

            // given s[0] and s[1], find a third point s[2] that forms the
            // smallest circumcircle
            double minRadius = double.PositiveInfinity;
            for (int i = 0; i < numPoints; ++i) {
                if (i == s[0] || i == s[1])  continue;
                double r = Geometry.getTriangleCircumradius(points[s[0]], points[s[1]], points[i]);
                if (r < minRadius) {
                    s[2] = i;
                    minRadius = r;
                }
            }

            // there was a check for minRadius == infinity here but let's skip it for now
            // that happens when the seed points *are* the convex hull
            if (double.IsPositiveInfinity(minRadius)) {
                Console.WriteLine("warning: positive infinity minradius");
            }

            // orient s[] points counterclockwise; transposing points will change the orientation
            if (Geometry.getOrientation(points[s[0]].x, points[s[0]].y, points[s[1]].x, points[s[1]].y, points[s[2]].x, points[s[2]].y)) {
                // temp variables for swap
                int i = s[1];
                Vertex tmp = points[s[1]];

                // swap 1 <- 2
                s[1] = s[2];
                points[s[1]] = points[s[2]];

                // swap 2 <- tmp
                s[2] = i;
                points[s[2]] = tmp;
            }

            // find circumcenter and then sort points by how far they are from it
            Console.WriteLine("finding circumcenter and distances");
            seedCenter = Geometry.getTriangleCircumcenter(points[s[0]], points[s[1]], points[s[2]]);
            int ugh = 0;
            foreach (Vertex v in points) {
                v.origId = ugh;
                v.distanceFromSeedCenter = Geometry.getDistance(v, seedCenter);
                ugh++;
            }
            Vertex[] sortedDistFromCenter = points.OrderByDescending(p => p.distanceFromSeedCenter).ToArray();
            // now sort the ids based on this distance
            for (int barf = 0; barf < sortedDistFromCenter.Length; ++barf) {
                ids[barf] = sortedDistFromCenter[barf].origId;
            }

            // set the seed triangle as the convex hull
            hullStart = s[0];
            int hullSize = 3;

            hullNext[s[0]] = hullPrev[s[2]] = s[1];
            hullNext[s[1]] = hullPrev[s[0]] = s[2];
            hullNext[s[2]] = hullPrev[s[1]] = s[0];

            for (int i = 0; i < hullSize; ++i) {
                hullTri[s[i]] = i;
                hullHash[getHashKey(points[s[i]])] = s[i];
            }

            totalTris = 0;
            // add the seed triangle to the array
            addTriangleByIndices(s[0], s[1], s[2], -1, -1, -1);

            // now actually do stuff
            double xp, yp;
            xp = yp = double.NaN;
            Console.WriteLine("starting the big loop");
            for (int k = 0; k < ids.Length; ++k) {
                Console.Write($"k{k}..");
                int i = ids[k];
                Vertex v = points[i];

                // skip near-duplicate points
                if (k > 0
                    && (double.IsNaN(xp) || double.IsNaN(yp))
                    && Math.Abs(v.x - xp) <= EPSILON
                    && Math.Abs(v.y - yp) <= EPSILON) {
                        continue;
                }
                // also skip the seed triangle points
                if (i == s[0] || i == s[1] || i == s[2]) {
                    continue;
                }

                // will be used on next iteration of the loop
                xp = v.x;
                yp = v.y;

                // find a visible edge on the convex hull using hashes
                int start = 0;
                for (int j = 0, key = getHashKey(v);
                     j < this.hashSize; ++j) {
                         Console.Write($"j{j}..");
                         start = hullHash[(key + j) % hashSize];
                         if (start != -1
                             && start != hullNext[start]) {
                                 break;  // found one
                             }
                }

                start = hullPrev[start];
                int e = start;
                int q = hullNext[e];
                Console.WriteLine("checking orientation");
                while (!Geometry.getOrientation(v.x, v.y, points[e].x, points[e].y, points[q].x, points[q].y)) { // infinite loop here
                    Console.Write($"e{e}..q{q}..");
                    e = q;
                    if (e == start) {
                        e = -1;
                        break;
                    }
                    q = hullNext[e];
                }
                if (e == -1) {  // near-duplicate point, so skip
                    continue;
                }

                // add first triangle from the point
                int t = addTriangleByIndices(e, i, hullNext[e], -1, -1, hullTri[e]);

                // recursively flip triangles until they are Delaunized
                Console.WriteLine("legalizing");
                hullTri[i] = legalizeTriangle(t + 2);
                hullTri[e] = t;  // keep track of boundary triangles on hull
                hullSize++;

                // walk forward through hull and add more triangles
                Console.WriteLine("walking forward");
                int n = hullNext[e];
                q = hullNext[n];
                while (Geometry.getOrientation(v.x, v.y, vertices[2*q], vertices[2*q+1], vertices[2*e], vertices[2*e+1])) {
                    t = addTriangleByIndices(n, i, q, hullTri[i], -1, hullTri[n]);
                    hullTri[i] = legalizeTriangle(t + 2);
                    hullNext[n] = n;  // removed
                    hullSize--;
                    n = q;
                    q = hullNext[n];
                }

                // walk backward from other side
                Console.WriteLine("walking backward");
                if (e == start) {
                    q = hullPrev[e];
                    while (Geometry.getOrientation(v.x, v.y, vertices[2*q], vertices[2*q+1], vertices[2*e], vertices[2*e+1])) {
                        t = addTriangleByIndices(q, i, e, -1, hullTri[e], hullTri[q]);
                        legalizeTriangle(t + 2);
                        hullTri[q] = t;
                        hullNext[e] = e;  // removed
                        hullSize--;
                        e = q;
                        q = hullPrev[e];
                    }
                }

                // update hull indices
                hullStart = hullPrev[i] = e;
                hullNext[e] = hullPrev[n] = i;
                hullNext[i] = n;

                // save two new edges in hull hash
                hullHash[getHashKey(v)] = i;
                hullHash[getHashKey(vertices[2*e], vertices[2*e+1])] = e;
            }  // done with that really long loop

            convexHull = new int[hullSize];
            for (int i = 0, e = hullStart; i < hullSize; ++i) {
                convexHull[i] = e;
                e = hullNext[e];
            }

            // clear out temp arrays
            hullPrev = hullNext = hullTri = null;

            // trim the triangles/arcs arrays
            // -1 was used as a sentinel value and filled the array earlier
            // since these are arrays of indices, they can't be negative
           // triangles = triangles.Where(id => id != -1).ToArray();
            //arcs = arcs.Where(id => id != -1).ToArray();
        }

        public int[] getArcIdsOfTriangle(int id) {
            return new int[] { 3*id, 3*id + 1, 3*id + 2 };
        }

        public int[] getVerticesOfTriangle(int id) {
            int[] es = getArcIdsOfTriangle(id);
            int[] vs = new int[6];
            for (int i = 0; i < es.Length; ++i) {
                vs[2*i] = vertices[arcs[es[i]]];
                vs[2*i+1] = vertices[arcs[es[i]]+1];
            }
            return vs;
        }
    }

    public static class Geometry {
        /// <summary>
        /// Get distance between points a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double getDistance(Vertex a, Vertex b) {
            double[] d = {a.x - b.x, a.y - b.y};
            return (d[0] * d[0]) + (d[1] * d[1]);
        }

        /// <summary>
        /// Operations used by both circumradius and circumcenter functions
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static double[] fn(Vertex a, Vertex b, Vertex c) {
            double[] d = {b.x - a.x, b.y - a.y};
            double[] e = {c.x - a.x, c.y - a.y};

            double bl = (d[0] * d[0]) + (d[1] * d[1]);
            double cl = (e[0] * e[0]) + (e[1] * e[1]);
            double D = 0.5 / ((d[0] * e[1]) - (d[1] * e[0]));

            double X = ((e[1] * bl) - (d[1] * cl)) * D;
            double Y = ((d[0] * cl) - (e[0] * bl)) * D;

            return new double[] { X, Y, D };
        }

        public static double getTriangleCircumradius(Vertex a, Vertex b, Vertex c) {
            double[] r = fn(a, b, c);
            return (r[0] * r[0]) + (r[1] * r[1]);
        }

        public static Vertex getTriangleCircumcenter(Vertex a, Vertex b, Vertex c) {
            double[] r = fn(a, b, c);
            return new Vertex((int)(a.x + r[0]), (int)(a.y + r[1]));
        }

        /// <summary>
        /// Determine orientation of a set of vertices
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool getOrientation(int ax, int ay, int bx, int by, int cx, int cy) {
            return ((by - ay) * (cx - ax)) - ((bx - ax) * (cy - ay)) < 0;
        }

        /// <summary>
        /// what is this sorcery?
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns>something on the interval [0, 1] maybe?</returns>
        public static double getPseudoAngle(double dx, double dy) {
            double p = dx / (Math.Abs(dx) + Math.Abs(dy));
            return ((dy > 0)?
                    3 - p : 1 + p)
                    / 4;
        }

        /// <summary>
        /// Is p inside the circumcircle of abc?
        /// Uses determinants
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool isInCircle(int ax, int ay, int bx, int by, int cx, int cy, int px, int py) {
            double[] d = new double[] { ax - px, ay - py };
            double[] e = new double[] { bx - px, by - py };
            double[] f = new double[] { cx - px, cy - py };

            double ap = d[0] * d[0] + d[1] * d[1];
            double bp = e[0] * e[0] + e[1] * e[1];
            double cp = f[0] * f[0] + f[1] * f[1];

            return (d[0] * (e[1] * cp - bp * f[1])
                  - d[1] * (e[0] * cp - bp * f[0])
                  + ap * (e[0] * f[1] - e[1] * f[0]))
                  < 0;
        }
    }

    // Test method
    class DTDemo : Form {
        public Delaunay dt;

        public static void Main() {
            Console.WriteLine("starting");
            Application.Run(new DTDemo());
        }

        public DTDemo() {
            ResizeRedraw = true;
        }
        private int offsX = 50;
        private int offsY = 50;

        protected override void OnPaint(PaintEventArgs pea) {
            int min = 0;
            int max = 750;
            int n = 100;
            Console.WriteLine("making points");
            Vertex[] pts = new Vertex[n];
            long ms1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Random rand = new Random(1246565756);
            for (int i = 0; i < n; ++i) {
                pts[i] = new Vertex(rand.Next(min, max), rand.Next(min, max));
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
            for (int i = 0; i < dt.vertices.Length; i += 2) {
                pea.Graphics.DrawEllipse(redPen, new Rectangle((dt.vertices[i] + offsX), (dt.vertices[i+1] + offsY), 3, 3));
            }

            // Draw line to screen.
            for (int i = 0; i < dt.triangles.Length / 3; i += 3) {
                Console.WriteLine($"tid: {i}");
                int[] vs = dt.getVerticesOfTriangle(i);
                draw(blackPen, pea, vs[0], vs[1], vs[2], vs[3]);
                draw(blackPen, pea, vs[4], vs[5], vs[2], vs[3]);
                draw(blackPen, pea, vs[0], vs[1], vs[4], vs[5]);
                // Console.WriteLine($"edges? ({dt.arcs[dt.triangles[i]]}, {dt.arcs[dt.triangles[i+1]]}, {dt.arcs[dt.triangles[i+2]]})");
                // int tri1 = dt.triangles[i];
                // int arc1 = dt.arcs[tri1];
                // int arc12 = dt.arcs[tri1+1];
                // Console.WriteLine($"drawing1: {arc1} -> {arc12}");
                // draw(blackPen, pea, dt.vertices[arc1], dt.vertices[arc1+1], dt.vertices[arc12], dt.vertices[arc12+1]);
                // int tri2 = dt.triangles[i+1];
                // int arc2 = dt.arcs[tri2];
                // int arc22 = dt.arcs[tri2+1];
                // Console.WriteLine($"drawing2: {arc2} -> {arc22}");
                // draw(blackPen, pea, dt.vertices[arc2], dt.vertices[arc2+1], dt.vertices[arc22], dt.vertices[arc22+1]);
                // //draw(blackPen, pea, dt.vertices[arc2], dt.vertices[arc22]);
                // int tri3 = dt.triangles[i+2];
                // int arc3 = dt.arcs[tri3];
                // int arc32 = dt.arcs[tri3+1];
                // Console.WriteLine($"drawing3: {arc3} -> {arc32}");
                // draw(blackPen, pea, dt.vertices[arc3], dt.vertices[arc3+1], dt.vertices[arc32], dt.vertices[arc32+1]);
                // draw(blackPen, pea, dt.vertices[arc3], dt.vertices[arc32]);
                // pea.Graphics.DrawLine(blackPen, (int)dt.vertices[dt.triangles[i]].x + offsX, (int)dt.vertices[dt.triangles[i]].y + offsY, (int)dt.vertices[dt.triangles[i+1]].x + offsX, (int)dt.vertices[dt.triangles[i+1]].y + offsY);
                // pea.Graphics.DrawLine(blackPen, (int)dt.vertices[dt.triangles[i+2]].x + offsX, (int)dt.vertices[dt.triangles[i+2]].y + offsY, (int)dt.vertices[dt.triangles[i+1]].x + offsX, (int)dt.vertices[dt.triangles[i+1]].y + offsY);
                // pea.Graphics.DrawLine(blackPen, (int)dt.vertices[dt.triangles[i+2]].x + offsX, (int)dt.vertices[dt.triangles[i+2]].y + offsY, (int)dt.vertices[dt.triangles[i]].x + offsX, (int)dt.vertices[dt.triangles[i]].y + offsY);
            }
        }

        private void draw(Pen pen, PaintEventArgs pea, Vertex a, Vertex b) {
            pea.Graphics.DrawLine(pen, (int)a.x + offsX, (int)a.y + offsY, (int)b.x + offsX, (int)b.y + offsY);
        }
        private void draw(Pen pen, PaintEventArgs pea, int ax, int ay, int bx, int by) {
            pea.Graphics.DrawLine(pen, (ax + offsX), (ay + offsY), (bx + offsX), (by + offsY));
        }
    }
}