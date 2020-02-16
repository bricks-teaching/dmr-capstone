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

            cells = new List<Polygon>(sites.Count);
            for (int i = 0; i < sites.Count; ++i) {
                // Draw lines to each neighbor.
                List<Edge> delrays = new List<Edge>();
                foreach (Vertex v in sites[i].neighborhood) {
                    delrays.Add(new Edge(sites[i], v, false));
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

                // If we have a cell side for each neighbor, we're done
                int delta = cellSides.Count - sites[i].neighborhood.Count;
                if (delta == 0) {
                    cells[i] = new Polygon(cellSides);
                }

                while (delta > 0) {
                    // Find the "hanging" vertices
                    List<Vertex> hanging = cellSides
                        .SelectMany(e => e.vertices)  // Flatten vertex lists
                        .GroupBy(v => v)  // Group each entry
                        .Where(g => g.Count() == 1)  // Pick only unique entries
                        .Select(v => v.First())  // Get only the objects from the grouping
                        .ToList();

                    if (delta == 1) {
                        /* Case: one missing edge
                         * Connect the two hanging vertices.
                         */
                        if (hanging.Count != 2) {
                            Debug.Log($"FATAL: Missing one edge from Voronoi cell, but didn't find exactly 2 hanging vertices. (Found {hanging.Count} instead)");
                            throw new System.Exception();
                        }
                        // Find the Voronoi edge connecting these two.
                        foreach (Edge e in hanging[0].incidentEdges) {
                            // If the endpoints are the same as the hanging vertices, this is it
                            if (e.vertices.All(hanging.Contains)) {
                                cellSides.Add(e);
                                break;  // TODO don't break, make a function that returns e
                            }
                        }
                    } else {
                        /* Case: multiple missing edges
                         * Connect the nearest vertex to one arbitrary vertex.
                         */

                        Vertex a = hanging[0];
                        // Considering the topology of a Voronoi diagram, the nearest vertex in this case should *always* be connected and not create a chord
                        Vertex b = Vertex.findNearestInList(a, hanging);
                        List<Vertex> ab = new List<Vertex> { a, b };

                        // Find the Voronoi edge that contains these two endpoints
                        // It'll be incident to them, of course

                        // TODO DRY LINE 107
                        foreach (Edge e in hanging[0].incidentEdges) {
                            // If the endpoints are the same as the hanging vertices, this is it
                            if (e.vertices.All(ab.Contains)) {
                                cellSides.Add(e);
                                break;  // TODO don't break, make a function that returns e
                            }
                        }
                    }

                    delta = sites[i].neighborhood.Count - cellSides.Count;
                } // end finding missing edges

                cells[i] = new Polygon(cellSides);
            }
        }
    }
}