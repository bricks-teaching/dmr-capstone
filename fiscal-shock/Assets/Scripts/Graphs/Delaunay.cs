using System.Diagnostics;
using System.Linq;
using ThirdParty.Delaunator;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Interface and extension of the Delaunator library
    /// </summary>
    public class Delaunay : Graph {
        public Triangulation delaunator { get; }
        public List<Triangle> triangles { get; } = new List<Triangle>();

        public Voronoi dual { get; set; }

        // Track min/max values to aid in constructing the Voronoi
        public int minX { get; }
        public int maxX { get; }
        public int minY { get; }
        public int maxY { get; }
        public List<Vertex> convexHull { get; } = new List<Vertex>();

        public Delaunay(List<double> input, int minX, int maxX, int minY, int maxY) {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            delaunator = new Triangulation(input);

            // Set up data structures for use in other scripts
            setTypedGeometry();
            convexHull = findConvexHull();
        }

        /// <summary>
        /// Get Delaunator-based triangle vertices
        /// </summary>
        /// <param name="t">id of the Delaunator triangle</param>
        /// <returns>array of vertices</returns>
        public float[] getTriangleVertices(int t) {
            return new float[] {
                (float)delaunator.coords[2 * delaunator.triangles[t]],
                (float)delaunator.coords[2 * delaunator.triangles[t] + 1],

                (float)delaunator.coords[2 * delaunator.triangles[t + 1]],
                (float)delaunator.coords[2 * delaunator.triangles[t + 1] + 1],

                (float)delaunator.coords[2 * delaunator.triangles[t + 2]],
                (float)delaunator.coords[2 * delaunator.triangles[t + 2] + 1]
            };
        }

        /// <summary>
        /// Sets up all geometry from the triangulation into data structures
        /// that are easier to deal with
        /// </summary>
        public void setTypedGeometry() {
            for (int i = 0; i < delaunator.triangles.Count; i += 3) {
                float[] fVertices = getTriangleVertices(i);
                // Track id to simplify Voronoi cell finding
                Vertex a = Vertex.getVertex(fVertices[0], fVertices[1], vertices);
                Vertex b = Vertex.getVertex(fVertices[2], fVertices[3], vertices);
                Vertex c = Vertex.getVertex(fVertices[4], fVertices[5], vertices);

                Edge ab = Edge.getEdge(a, b, edges);
                Edge bc = Edge.getEdge(b, c, edges);
                Edge ca = Edge.getEdge(c, a, edges);

                // Add to the Delaunay object
                Triangle t = new Triangle(
                    new List<Edge> { ab, bc, ca },
                    i
                );
                triangles.Add(t);
            }
        }

        /// <summary>
        /// Generates the corresponding Voronoi diagram for this triangulation
        /// </summary>
        /// <returns>dual graph</returns>
        public Voronoi makeVoronoi() {
            dual = new Voronoi(this);
            return dual;
        }
    }
}