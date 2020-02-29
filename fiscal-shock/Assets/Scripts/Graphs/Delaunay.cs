using System.Collections.Generic;
using ThirdParty.Delaunator;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Interface and extension of the Delaunator library
    /// </summary>
    public class Delaunay : Graph {
        public Triangulation delaunator { get; }
        public List<Triangle> triangles { get; } = new List<Triangle>();

        public Voronoi dual { get; set; }
        public List<Vertex> convexHull { get; } = new List<Vertex>();
        public List<Edge> convexHullEdges { get; } = new List<Edge>();

        // Track min/max values for map bounds for now
        public int minX { get; }
        public int maxX { get; }
        public int minY { get; }
        public int maxY { get; }

        public Delaunay(List<double> input, int minX, int maxX, int minY, int maxY) {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            delaunator = new Triangulation(input);

            // Set up data structures for use in other scripts
            setTypedGeometry();
            convexHull = findConvexHull();
            List<Edge> hull = new List<Edge>();
            for (int i = 0; i < convexHull.Count; ++i) {
                if (i + 1 == convexHull.Count) {  // wrap around
                    hull.Add(new Edge(convexHull[i], convexHull[0]));
                } else {
                    hull.Add(new Edge(convexHull[i], convexHull[i+1]));
                }
            }
            convexHullEdges = hull;
        }

        /// <summary>
        /// Sets up all geometry from the triangulation into data structures
        /// that are easier to deal with
        /// </summary>
        public void setTypedGeometry() {
            // Get all vertices from the Delaunator triangulation
            List<double> triCoords = delaunator.coords;
            for (int i = 0; i < triCoords.Count; i += 2) {
                vertices.Add(new Vertex((float)triCoords[i], (float)triCoords[i + 1], vertices.Count));
            }

            // Simultaneously make edges and triangles without duplication
            List<List<int>> delEdges = new List<List<int>>();
            List<int> delTriangles = delaunator.triangles;
            List<List<List<int>>> triangleEdges = new List<List<List<int>>>();
            int triangleNum = 0;
            for (int i = 0; i < delTriangles.Count; i += 3) {
                int a = delTriangles[i];
                int b = delTriangles[i + 1];
                int c = delTriangles[i + 2];

                while (delEdges.Count <= a || delEdges.Count <= b || delEdges.Count <= c) {
                    delEdges.Add(new List<int>());
                    triangleEdges.Add(new List<List<int>>());
                }

                addToEdgeList(a, b, triangleNum, delEdges, triangleEdges);
                addToEdgeList(b, c, triangleNum, delEdges, triangleEdges);
                addToEdgeList(a, c, triangleNum, delEdges, triangleEdges);

                List<Vertex> vabc = new List<Vertex> { vertices[a], vertices[b], vertices[c] };

                Triangle t = new Triangle(
                    vabc,
                    i
                );
                triangles.Add(t);
                triangleNum++;
            }

            // Associate edges, triangles, and vertices
            for (int i = 0; i < delEdges.Count; i++) {
                for (int j = 0; j < delEdges[i].Count; j++) {
                    Edge edge = new Edge(vertices[i], vertices[delEdges[i][j]]);
                    edge.connect(vertices[i], vertices[delEdges[i][j]]);
                    edges.Add(edge);
                    foreach (int t in triangleEdges[i][delEdges[i][j]]){
                        triangles[t].sides.Add(edge);
                        vertices[i].triangles.Add(triangles[t]);
                        vertices[delEdges[i][j]].triangles.Add(triangles[t]);
                    }
                }
            }
        }

        /// <summary>
        /// Helper function to create Delaunay data structures
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="triangleNum"></param>
        /// <param name="edges"></param>
        /// <param name="triangleEdges"></param>
        public static void addToEdgeList(int a, int b, int triangleNum, List<List<int>> edges, List<List<List<int>>> triangleEdges) {
            if (a < b) {
                while (triangleEdges[a].Count <= b) {
                    triangleEdges[a].Add(new List<int>());
                }
                triangleEdges[a][b].Add(triangleNum);

                if(!edges[a].Contains(b)) {
                    edges[a].Add(b);
                }
            } else {
                while (triangleEdges[b].Count <= a) {
                    triangleEdges[b].Add(new List<int>());
                }
                triangleEdges[b][a].Add(triangleNum);
                if (!edges[b].Contains(a) ){
                    edges[b].Add(a);
                }
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