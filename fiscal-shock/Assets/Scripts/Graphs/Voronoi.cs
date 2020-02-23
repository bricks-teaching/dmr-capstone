using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    public class Voronoi : Graph {
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
        public List<Cell> cells { get; }

        public Delaunay dual { get; }

        /// <summary>
        /// Generates a Voronoi diagram using Delaunator output
        /// </summary>
        /// <param name="del"></param>
        public Voronoi(Delaunay del)  {
            dual = del;
            sites = dual.vertices;
            cells = new List<Cell>();

            calculateVerticesAndEdgesFromDelaunator();
            createCells();
            findVoronoiCellsClock();
        }

        /// <summary>
        /// Algorithm taken from Delaunator guide.
        /// <para>https://mapbox.github.io/delaunator/</para>
        /// </summary>
        private void calculateVerticesAndEdgesFromDelaunator() {
            for (int e = 0; e < dual.delaunator.triangles.Count; e++) {
                if (e < dual.delaunator.halfedges[e]) {
                    Vertex p = Vertex.getVertex(
                        dual.triangles[Edge.getTriangleId(e)].findCircumcenter(),
                        vertices
                    );
                    Vertex q = Vertex.getVertex(
                        dual.triangles[Edge.getTriangleId(dual.delaunator.halfedges[e])].findCircumcenter(),
                        vertices
                    );

                    // getEdge adds to the list if necessary
                    Edge.getEdge(p, q, edges);
                }
            }
        }

        /// <summary>
        /// Cell neighbors are just adjacent sites in the Delaunay triangulation.
        /// This is ugly and inefficient, but C# is cranky about instantiating collections.
        /// </summary>
        private void createCells() {
            // First, construct the list completely
            foreach (Vertex site in sites) {
                Cell cell = new Cell(site);
                cells.Add(cell);
            }
            // Now we can reference other cells farther down the list
            foreach (Cell cell in cells) {
                foreach (Vertex neighbor in cell.site.neighborhood) {
                    cell.neighbors.Add(cells[neighbor.id]);
                }
            }
        }

        /// <summary>
        /// Combination of the clock algorithm and the naive algorithm to clean up edges that the sweeping clock hand misses.
        /// <para>See https://docs.google.com/document/d/1sLGPW8PTkT1xbsvpPxAPN4MfuH-lIZKejbIR041zVU8/edit#bookmark=id.f8uivvb00dbh for details.</para>
        /// </summary>
        private void findVoronoiCellsClock() {
            const float ROTATE_DELTA = (float)Math.PI/6.0f;
            // Set the initial search point far north
            float greaterDistance = (dual.maxX - dual.minX) + (dual.maxY - dual.minY);
            Vertex northMost = new Vertex(0, greaterDistance);

            foreach (Cell cell in cells) {
                List<Vertex> foundVertices = new List<Vertex>();
                Edge farcaster;

                Tuple<Vertex, float> vf = findFirstEdge(northMost, cell);
                foundVertices.Add(vf.Item1);

                // theta: Angle of the "clock hand"
                float theta = vf.Item2;

                // Found one edge already, so start the loop index at 1
                for (int i = 1; i < cell.site.neighborhood.Count; ++i) {
                    // Rotate the angle and get a new point far away.
                    Vertex distant = cell.site.getEndpointOfLineRotation(theta - ROTATE_DELTA, greaterDistance);
                    // Draw an edge between the new point and the site.
                    farcaster = new Edge(cell.site, distant);

                    // Check if the edge intersects neighbors of the selected site.
                    // TODO Milestone 2/3 find a way to not check every edge
                    Edge intersectedEdge = findClosestRayIntersection(cell.site, farcaster, edges);

                    float temp_delta = ROTATE_DELTA;
                    int infinityShield = 0;
                    bool clockFailed = false;
                    /* If this edge was already found or there was no
                     * intersection, adjust the rotation and try again.
                     */
                    while (intersectedEdge == null || cell.sides.Contains(intersectedEdge)) {
                        if (infinityShield > 12) {
                            clockFailed = true;
                            break;
                        }
                        temp_delta += ROTATE_DELTA;
                        distant = cell.site.getEndpointOfLineRotation(theta - temp_delta, greaterDistance);
                        farcaster = new Edge(cell.site, distant);
                        intersectedEdge = findClosestRayIntersection(cell.site, farcaster, edges);
                        ++infinityShield;
                    }

                    /* Very short edges will be missed by the clock hand
                     * rotating, so fall back to the naive algorithm to
                     * make a last attempt.
                     * In all tested cases, this patched up the cell when
                     * its site was not one of the Delaunay convex hull
                     * points that have infinite edges.
                     * Some libraries opt to spend time clipping the diagram
                     * to a "viewport," but we'll just mark the cell as
                     * incomplete and not use it.
                     */
                    if (clockFailed) {
                        tryFindMissingSides(cell);
                        if (cell.sides.Count != cell.neighbors.Count) {
                            cell.incomplete = true;
                        }
                        continue;
                    }

                    // Add this edge
                    cell.sides.Add(intersectedEdge);

                    // Try to make an educated guess at the next best angle
                    if (cell.sides.Count < cell.neighbors.Count) {
                        Vertex[] cap = intersectedEdge.vertices.Except(foundVertices).ToArray();
                        if (cap.Length > 0) {
                            foundVertices.Add(cap[0]);
                            theta = cell.site.getAngleOfRotationTo(cap[0]);
                        }
                    }
                }  /* end finding sides of this cell */
            }  /* end checking all cells */
        }

        /// <summary>
        /// Find the first edge for the clock algorithm and returns a best
        /// guess at an angle to start rotating from.
        /// </summary>
        /// <param name="distantVertex">arbitrarily distant vertex</param>
        /// <param name="cell"></param>
        /// <returns>vertex to start rotating from and angle to it</returns>
        private Tuple<Vertex, float> findFirstEdge(Vertex distantVertex, Cell cell) {
            // First pass for each site requires checking all Voronoi edges (expensive!)
            Edge farcaster = new Edge(distantVertex, cell.site);
            Edge firstEdge = findClosestRayIntersection(cell.site, farcaster, edges);

            if (firstEdge == null) {
                // Maybe on the convex hull; search in the other direction
                distantVertex = new Vertex(-distantVertex.x, -distantVertex.y);
                farcaster = new Edge(distantVertex, cell.site);
                firstEdge = findClosestRayIntersection(cell.site, farcaster, edges);
            }

            cell.sides.Add(firstEdge);

            // Find the angle of rotation between both endpoints of the first edge
            float theta_cand1 = cell.site.getAngleOfRotationTo(firstEdge.p);
            float theta_cand2 = cell.site.getAngleOfRotationTo(firstEdge.q);

            // Select the smallest angle, since we're going to decrement the angle
            if (theta_cand1 < theta_cand2) {
                return new Tuple<Vertex, float>(firstEdge.p, theta_cand1);
            } else {
                return new Tuple<Vertex, float>(firstEdge.q, theta_cand2);
            }
        }

        /// <summary>
        /// Find the closest edge to the given ray in the list of edges.
        /// Slow if you're iterating over every edge of a graph!
        /// </summary>
        /// <param name="site"></param>
        /// <param name="ray"></param>
        /// <param name="edgesToHit"></param>
        /// <returns></returns>
        private Edge findClosestRayIntersection(Vertex site, Edge ray, List<Edge> edgesToHit) {
            List<Tuple<Edge, Vertex>> hits = new List<Tuple<Edge, Vertex>>();
            foreach (Edge e in edgesToHit) {
                Vertex hit = Edge.findIntersection(ray, e);
                if (hit != null) {
                    hits.Add(new Tuple<Edge, Vertex> (e, new Vertex(hit.x, hit.y)));
                }
            }

            if (hits.Count < 1) {
                return null;
            }
            if (hits.Count == 1) {
                return hits[0].Item1;
            }

            // Calculate distance to each intersected point
            List<Tuple<Edge, double>> dists = hits.Select(v => new Tuple<Edge, double> (v.Item1, v.Item2.getDistanceTo(site))).ToList();

            // Minimum is the winner. Set the corresponding edge as a side
            return dists.OrderBy(l => l.Item2).First().Item1;
        }

        /// <summary>
        /// Runs parts of the original naive algorithm on a cell to try
        /// and find missing edges. Works for sites not on the Delaunay's
        /// convex hull.
        /// </summary>
        /// <param name="cell"></param>
        private void tryFindMissingSides(Cell cell) {
            int infinityShield = 0;
            while (cell.neighbors.Count > cell.sides.Count) {
                addMissingEdgesFunc(cell, getHangingVertices(cell), Edge.findConnectingEdges);
                if (infinityShield > cell.neighbors.Count) {
                    return;
                }
                int delta = cell.neighbors.Count - cell.sides.Count;
                if (delta == 0) {  // Done!
                    return;
                } else if (delta == 1) {
                    // Just need to do addConnectingEdges again (except for convex hull trolls)
                    infinityShield++;
                    continue;
                }
                addMissingEdgesFunc(cell, getHangingVertices(cell), findMissingEdgePairs);
                infinityShield++;
            }
        }

        /// <summary>
        /// Gets vertices that aren't 2-connected in the current iteration
        /// of the cell polygon. Since the 2D polygons we're making are
        /// always 2-connected (all vertices connected to at least 2 other
        /// vertices), that means we're missing an edge that should exist.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private List<Vertex> getHangingVertices(Cell cell) {
            return cell.sides
                .SelectMany(e => e.vertices)  // Flatten vertex lists
                .GroupBy(v => v)  // Group each entry
                .Where(g => g.Count() < 2)
                .Select(v => v.First())  // Get only the objects from the grouping
                .ToList();
        }

        /// <summary>
        /// Helper function to check the return values of `func` so we don't
        /// try to add empty lists or null
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="hanging"></param>
        /// <param name="func"></param>
        private void addMissingEdgesFunc(Cell cell, List<Vertex> hanging, Func<List<Vertex>, List<Edge>> func) {
            List<Edge> foundEdges = func(hanging);

            if (foundEdges?.Count > 0) {
                cell.sides.AddRange(foundEdges);
            }
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
                Vertex b = a.findNearestInList(hanging);
                List<Vertex> ab = new List<Vertex> { a, b };

                // Search their neighborhoods for a common vertex
                List<Vertex> commonNeighbors = ab
                    .SelectMany(u => u.neighborhood)  // Flatten lists
                    .GroupBy(v => v)  // Group duplicate vertices
                    .Where(g => g.Count() == 2)  // Take only groups with 2 members, implying it was in both neighborhoods
                    .Select(v => v.First())
                    .ToList();
                if (commonNeighbors.Count < 1) {
                    return null;
                }
                Vertex commonNeighbor = commonNeighbors[0];

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
            return missingEdgePairs.Distinct().ToList();
        }
    }
}
