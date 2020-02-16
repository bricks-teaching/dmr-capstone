using System.Collections.ObjectModel;
using UnityEngine;
using System.Linq;
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
        public List<Polygon> cells { get; private set; }

        public Delaunay dual { get; }

        /// <summary>
        /// Generates a Voronoi diagram using Delaunator output
        /// </summary>
        /// <param name="del"></param>
        public Voronoi(Delaunay del)  {
            dual = del;
            sites = dual.vertices;

            calculateVerticesAndEdgesFromDelaunator();
            findVoronoiCells();
        }

        /// <summary>
        /// Algorithm taken from Delaunator guide.
        /// <para>https://mapbox.github.io/delaunator/</para>
        /// </summary>
        private void calculateVerticesAndEdgesFromDelaunator() {
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
        }

        /// <summary>
        /// Main function to determine the Voronoi cells.
        /// <para>See https://docs.google.com/document/d/1sLGPW8PTkT1xbsvpPxAPN4MfuH-lIZKejbIR041zVU8/edit#bookmark=id.f8uivvb00dbh for details.</para>
        /// </summary>
        private void findVoronoiCells() {
            cells = new List<Polygon>(sites.Count);
            for (int i = 0; i < sites.Count; ++i) {
                List<Edge> cellSides = findGuaranteedCellSidesOfSite(sites[i]);

                // If we have a cell side for each neighbor, we're done
                int delta = cellSides.Count - sites[i].neighborhood.Count;
                if (delta == 0) {
                    cells.Add(new Polygon(cellSides));
                    continue;
                }

                // If not all edges were guaranteed, we need to fall back to other methods
                while (delta > 0) {
                    // Find the "hanging" vertices
                    List<Vertex> hanging = cellSides
                        .SelectMany(e => e.vertices)  // Flatten vertex lists
                        .GroupBy(v => v)  // Group each entry
                        .Where(g => g.Count() == 1)  // Pick only unique entries
                        .Select(v => v.First())  // Get only the objects from the grouping
                        .ToList();

                    // ---------------------------------------------------------
                    // Connect all hanging vertices only separated by one edge.
                    List<Edge> connectors = Edge.findConnectingEdges(hanging);
                    if (connectors.Count > 0) {
                        // Restart the while-loop, in case we found all edges.
                        cellSides.AddRange(connectors);
                        delta = sites[i].neighborhood.Count - cellSides.Count;
                        continue;
                    } else {
                        Debug.Log("INFO: Didn't find any edges connecting hanging vertices.");
                        // That's fine, will remove this later
                    }
                    // ---------------------------------------------------------

                    // ---------------------------------------------------------
                    // Try to find multiple edge "segments" separating hanging vertices.
                    List<Edge> missingEdgePair = findMissingEdgePairs(hanging);

                    // Sanity check
                    if ((missingEdgePair.Count & 1) == 1) {
                        Debug.Log($"WARNING: Found {missingEdgePair.Count} incident edges, expected multiple of 2.");
                        // Something probably went wrong here
                        // Could cause infinite loop
                        // Set a breakpoint and debug if that happens
                    } else {
                        cellSides.AddRange(missingEdgePair);
                    }
                    // ---------------------------------------------------------

                    // Update the delta for the while-loop
                    delta = sites[i].neighborhood.Count - cellSides.Count;
                } // end finding missing edges

                // TODO check if cellSides is a cycle
                cells.Add(new Polygon(cellSides));
            }
        }

        /// <summary>
        /// Finds edges in the Voronoi diagram that are guaranteed to sides of the Voronoi cell corresponding to the given site.
        /// </summary>
        /// <param name="site">Voronoi site for which to find cell sides.</param>
        /// <returns>List of edges of the Voronoi diagram guaranteed to be sides of this cell.</returns>
        private List<Edge> findGuaranteedCellSidesOfSite(Vertex site) {
            // Draw lines to each neighbor.
            List<Edge> delrays = new List<Edge>();
            foreach (Vertex v in site.neighborhood) {
                delrays.Add(new Edge(site, v, false));
            }

            // Find Voronoi edges intersected by each line. Warning: expensive!
            List<List<Edge>> intersectedVEdges = new List<List<Edge>>();
            foreach (Edge e in delrays) {
                List<Edge> jEdges = new List<Edge>();
                foreach (Edge f in edges) {  // Check every Voronoi edge
                    if (Edge.findIntersection(e, f) != null) {
                        jEdges.Add(f);
                    }
                }
                // Indices of delrays will correspond with intersections
                intersectedVEdges.Add(jEdges);
            }

            // When only 1 edge exists in the edge list, it's guaranteed to be a side of the Voronoi cell
            List<Edge> cellSides = new List<Edge>();
            foreach (List<Edge> l in intersectedVEdges) {
                if (l.Count == 1) {
                    cellSides.Add(l[0]);
                }
            }

            return cellSides;
        }

        /// <summary>
        /// There is at least one vertex not in the list of hanging vertices that is needed to find the missing edges. If there are only two consecutive edges comprising a "hole" in the polygon's perimeter, then the third vertex should lie between the two closest hanging vertices. This missing vertex exists as an endpoint to an edge incident to one hanging vertex, and an endpoint to a separate edge that is incident to the other nearby hanging vertex.
        /// </summary>
        /// <param name="hanging">List of vertices to try finding a pair of edges connecting two of these vertices.</param>
        /// <returns>List of adjacent edges connecting two vertices in the list. Could be empty, so caller needs to check return value.</returns>
        private List<Edge> findMissingEdgePairs(List<Vertex> hanging) {
            // Select a pair of nearby hanging vertices.
            /* Any initial vertex will do, because all remaining hanging
             * vertices must be separated by multiple edges, otherwise,
             * they would have been connected up above.
             */
            List<Edge> missingEdgePairs = new List<Edge>();
            foreach (Vertex a in hanging) {
                Vertex b = Vertex.findNearestInListTo(a, hanging);
                List<Vertex> ab = new List<Vertex> { a, b };

                // Search their neighborhoods for a common vertex
                Vertex commonNeighbor = ab
                    .SelectMany(u => u.neighborhood)  // Flatten lists
                    .GroupBy(v => v)  // Group duplicate vertices
                    .Where(g => g.Count() == 2)  // Take only groups with 2 members, implying it was in both neighborhoods
                    .Select(v => v.First())
                .First();

                // Find the edges incident to commonNeighbor
                List<Edge> found = ab
                    .SelectMany(u => u.incidentEdges)
                    .Where(e => e.vertices.Contains(commonNeighbor))
                    .ToList();

                if (found.Count == 2) {
                    missingEdgePairs.AddRange(found);
                } else if (found.Count != 0) {
                    Debug.Log($"INFO: Found {found.Count} edges, expected 0 or 2.");
                }
            }
            return missingEdgePairs;
        }
    }
}