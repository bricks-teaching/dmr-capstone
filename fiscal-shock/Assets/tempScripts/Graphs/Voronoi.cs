using System.Collections.Generic;

namespace FiscalShock.Graphs {
    public class Voronoi {
        public List<Vertex> vertices { get; } = new List<Vertex>();
        public List<Edge> edges { get; } = new List<Edge>();

        /// <summary>
        /// Corresponds to the vertices of the Delaunay triangulation
        /// used to create this graph.
        /// Anywhere within a given cell, there exists one site, which is
        /// the closest site, no matter where one stands within that cell's
        /// boundaries.
        ///
        /// <para>WARNING: Reference to the dual's vertices -- don't mutate!</para>
        /// </summary>
        public List<Vertex> sites { get; }

        /// <summary>
        /// Polygons representing faces of the Voronoi diagram.
        /// </summary>
        public List<Polygon> cells { get; }

        public Delaunay dual { get; }

        /// <summary>
        /// Generates a Voronoi diagram using Delaunator output
        ///
        /// <para>https://mapbox.github.io/delaunator/</para>
        /// </summary>
        /// <param name="del"></param>
        public Voronoi(Delaunay del)  {
            dual = del;
            sites = dual.vertices;

            for (int e = 0; e < dual.triangulation.triangles.Count; e++) {
                if (e < dual.triangulation.halfedges[e]) {
                    Vertex p = dual.triangles[Edge.getTriangleId(e)].findCircumcenter();
                    Vertex q = dual.triangles[Edge.getTriangleId(dual.triangulation.halfedges[e])].findCircumcenter();

                    Edge pq = new Edge(p, q);

                    vertices.Add(p);
                    vertices.Add(q);
                    edges.Add(pq);
                }
            }

            /* TODO find polygons (cells)
             * much harder than it sounds - both vertex and edge are shared
             * among many polygons
             * but any given edge should be shared among at most 2 polygons
             * => proof: an edge on a planar graph splits a face into 2
             * so, iterating over edges is the easiest
             *
             * next concern: determining how to walk along edges
             *
             * pick any edge
             *   polygon := null
             *   add the edge's endpoints to the polygon
             *   find the next edge, clockwise, from one endpoint
             *      add this next edge's opposite endpoint to polygon
             *      repeat until we land on an opposite endpoint already in
             *        the polygon's vertices
             * pick any edge that has NOT been used twice to make a polygon
             * repeat
             *
             * problems:
             * - polygons on convex hull, they're used just once
             * >>> maybe find a way to detect and do something to them
             * - probably inefficient but whatever; linq should make the syntax
             *   very easy to handle
             */
        }
    }
}