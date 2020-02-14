using ThirdParty.Delaunator;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Interface and extension of the Delaunator library
    /// </summary>
    public class Delaunay {
        public Triangulation triangulation { get; }
        public List<Vertex> vertices { get; private set; }
        public List<Edge> edges { get; private set; }
        public List<Triangle> triangles { get; private set; }

        public Voronoi dual { get; set; }

        public Delaunay(List<double> input) {
            triangulation = new Triangulation(input);

            // Set up data structures for use in other scripts
            setTypedGeometry(this);
        }

        // testing
        public float[] getTriangleVertices(int t) {
            return new float[] {
                (float)triangulation.coords[2 * triangulation.triangles[t]],
                (float)triangulation.coords[2 * triangulation.triangles[t] + 1],

                (float)triangulation.coords[2 * triangulation.triangles[t + 1]],
                (float)triangulation.coords[2 * triangulation.triangles[t + 1] + 1],

                (float)triangulation.coords[2 * triangulation.triangles[t + 2]],
                (float)triangulation.coords[2 * triangulation.triangles[t + 2] + 1]
            };
        }

        /// <summary>
        /// Sets up all geometry from the triangulation into data structures
        /// that are easier to deal with
        /// </summary>
        /// <returns></returns>
        public static void setTypedGeometry(Delaunay dt) {
            dt.vertices = new List<Vertex>();
            dt.edges = new List<Edge>();
            dt.triangles = new List<Triangle>();
            for (int i = 0; i < dt.triangulation.triangles.Count; i += 3) {
                // TODO do the points need to be in clockwise order?
                float[] vertices = dt.getTriangleVertices(i);
                // TODO get vertex ids, or are they necessary? the indices of coords are sorted, I think
                Vertex a = new Vertex(vertices[0], vertices[1]);
                Vertex b = new Vertex(vertices[2], vertices[3]);
                Vertex c = new Vertex(vertices[4], vertices[5]);

                /* Link up the vertices. The ids for triangle t's edges are
                 *    3 * t, 3 * t + 1, 3 * t + 2
                 */
                Edge ab = new Edge(a, b, 3*i);
                Edge bc = new Edge(b, c, 3*i+1);
                Edge ca = new Edge(c, a, 3*i+2);

                // Add to the Delaunay object
                List<Vertex> vabc = new List<Vertex> { a, b, c };
                List<Edge> eabc = new List<Edge> { ab, bc, ca };
                dt.vertices.AddRange(vabc);
                dt.edges.AddRange(eabc);

                Triangle t = new Triangle(
                    vabc,
                    eabc,
                    i
                );
                dt.triangles.Add(t);
            }
        }

        public Voronoi makeVoronoi() {
            dual = new Voronoi(this);
            return dual;
        }
    }
}