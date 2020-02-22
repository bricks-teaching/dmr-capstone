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
            //findVoronoiCellsNaive();
            findVoronoiCellsClock();
        }

        /// <summary>
        /// Algorithm taken from Delaunator guide.
        /// <para>https://mapbox.github.io/delaunator/</para>
        /// </summary>
        private void calculateVerticesAndEdgesFromDelaunator() {
            for (int e = 0; e < dual.delaunator.triangles.Count; e++) {
                if (e < dual.delaunator.halfedges[e]) {
                    // TODO clean up calls here
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

        private void findVoronoiCellsClock() {
            const float ROTATE_DELTA = (float)Math.PI/6.0f;
            // Set the initial search point
            List<float> xs = vertices.Select(v => v.x).ToList();
            float xdelta = xs.Max() - xs.Min();
            List<float> ys = vertices.Select(v => v.y).ToList();
            float ydelta = ys.Max() - ys.Min();
            Vertex reallyFarAway = new Vertex(xdelta, ydelta);

            foreach (Cell cell in cells) {
                Debug.LogError($"Starting {cell.site.id}");
                //List<Vertex> checkedEndpoints = new List<Vertex>();
                Edge farcaster;

                float theta = findFirstEdge(reallyFarAway, cell/*,checkedEndpoints*/);

                // Found one edge already, so start the loop index at 1
                for (int i = 1; i < cell.site.neighborhood.Count; ++i) {
                    // Rotate the angle and get a new point far away.
                    Vertex distant = cell.site.getEndpointOfLineRotation(theta - ROTATE_DELTA, 2000);
                    farcaster = new Edge(cell.site, distant);
                    //Vector2 nu = cell.site.vector.Rotate(theta - ROTATE_DELTA);
                    //farcaster = new Edge(cell.site, new Vertex(nu.x, nu.y));

                    // Check if it intersects neighbors of the selected site.
                    Edge intersectedEdge = findRayIntersections(cell.site, farcaster, edges);

                    float temp_delta = ROTATE_DELTA;
                    // probably also make a counter to abort infinite loops
                    int tmp = 0;
                    bool duh = false;
                    while (intersectedEdge == null || cell.sides.Contains(intersectedEdge)) {
                        //Debug.Log($"Trying again, was intersectedEdge null? {intersectedEdge == null}");
                        if (tmp > 44) {
                            Debug.LogError($"Can't find intersection for {cell.site.id} side {i}/{cell.site.neighborhood.Count}, fix your code");
                            duh = true;
                            break;
                        }
                        // try again, but reduce the rotation
                        temp_delta *= 1.1f;
                        //nu = cell.site.vector.Rotate(theta - temp_delta);
                        distant = cell.site.getEndpointOfLineRotation(theta - temp_delta, 2000);
                        farcaster = new Edge(cell.site, distant);
                        //farcaster = new Edge(cell.site, new Vertex(nu.x, nu.y));
                        intersectedEdge = findRayIntersections(cell.site, farcaster, edges);
                        ++tmp;
                    }

                    if (duh) continue;
                    cell.sides.Add(intersectedEdge);
                    // Pick a new endpoint not in the list already
                    //Vertex ie = intersectedEdge.vertices.Except(checkedEndpoints).First();
                    //checkedEndpoints.Add(ie);
                    //theta = cell.site.getAngleOfRotationTo(checkedEndpoints.Last());
                }
                Debug.LogError($"Finished {cell.site.id}");
            }
        }

        private float findFirstEdge(Vertex reallyFarAway, Cell cell/*, List<Vertex> checkedEndpoints*/) {
            // First pass for each site requires checking all Voronoi edges (expensive!)
            Edge farcaster = new Edge(reallyFarAway, cell.site);
            Edge firstEdge = findRayIntersections(cell.site, farcaster, edges);
            if (firstEdge == null) {  // This vertex might be on the convex hull
                reallyFarAway = new Vertex(-reallyFarAway.x, -reallyFarAway.y);
                farcaster = new Edge(reallyFarAway, cell.site);
                firstEdge = findRayIntersections(cell.site, farcaster, edges);
            }

            cell.sides.Add(firstEdge);

            // Find the angle of rotation between both endpoints of the first edge.
            //Vector2 s = new Vector2(cell.site.x, cell.site.y);
            //float theta_cand1 = Vector2.SignedAngle(s, new Vector2(firstEdge.vertices[0].x, firstEdge.vertices[0].y));
            //float theta_cand2 = Vector2.SignedAngle(s, new Vector2(firstEdge.vertices[1].x, firstEdge.vertices[1].y));

            if (cell.site.id == 88) {
                Debug.Log("hi");
            }
            float theta_cand1 = cell.site.getAngleOfRotationTo(firstEdge.p);
            float theta_cand2 = cell.site.getAngleOfRotationTo(firstEdge.q);

            // Select the smallest angle, since we're going to decrement the angle.
            if (theta_cand1 < theta_cand2) {
                return theta_cand1;
                //checkedEndpoints.Add(firstEdge.p);
            } else {
                return theta_cand2;
                //checkedEndpoints.Add(firstEdge.q);
            }
        }

        private Edge findRayIntersections(Vertex site, Edge ray, List<Edge> edgesToHit) {
            List<Tuple<Edge, Vertex>> hits = new List<Tuple<Edge, Vertex>>();
            foreach (Edge e in edgesToHit) {
                Vertex hit = Edge.findIntersection(ray, e);
                //Vector2 hit = new Vector2();
                //bool was_hit = Mathy.LineIntersection(e.vertices[0].vector, e.vertices[1].vector, ray.vertices[0].vector, ray.vertices[1].vector, ref hit);
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

            /* Only calculated for the first edge, otherwise returned above */
            // Calculate distance to each intersected point
            List<Tuple<Edge, double>> dists = hits.Select(v => new Tuple<Edge, double> (v.Item1, v.Item2.getDistanceTo(site))).ToList();

            // Minimum is the winner. Set the corresponding edge as a side
            return dists.OrderBy(l => l.Item2).First().Item1;
        }

        /// <summary>
        /// Main function to determine the Voronoi cells.
        /// <para>See https://docs.google.com/document/d/1sLGPW8PTkT1xbsvpPxAPN4MfuH-lIZKejbIR041zVU8/edit#bookmark=id.f8uivvb00dbh for details.</para>
        /// </summary>
        private void findVoronoiCellsNaive() {
            foreach (Cell cell in cells) {
                List<Edge> cellSides = findGuaranteedCellSidesOfSite(cell.site);

                // If we have a cell side for each neighbor, we're done
                int delta = cell.neighbors.Count - cellSides.Count;

                // If not all edges were guaranteed, we need to fall back to other methods
                int tries = 0;
                while (delta > 0) {
                    if (tries > 3) {
                        Debug.Log($"{cell.id} Delta: {delta}, giving up");
                        break;
                    }
                    // Find the "hanging" vertices
                    List<Vertex> hanging = cellSides
                        .SelectMany(e => e.vertices)  // Flatten vertex lists
                        .GroupBy(v => v)  // Group each entry
                        .Where(g => g.Count() < 2)
                        .Select(v => v.First())  // Get only the objects from the grouping
                        .ToList();

                    // ---------------------------------------------------------
                    // Connect all hanging vertices only separated by one edge.
                    List<Edge> connectors = Edge.findConnectingEdges(hanging);
                    if (connectors.Count > 0) {
                        // Restart the while-loop, in case we found all edges.
                        cellSides.AddRange(connectors);
                        delta = cell.neighbors.Count - cellSides.Count;
                        continue;
                    }
                    // ---------------------------------------------------------

                    // ---------------------------------------------------------
                    // Try to find multiple edge "segments" separating hanging vertices.
                    List<Edge> missingEdgePair = findMissingEdgePairs(hanging);

                    // Sanity check
                    if ((missingEdgePair.Count & 1) == 1) {
                        Debug.LogWarning($"{cell.id}: Found {missingEdgePair.Count} incident edges, expected multiple of 2.");
                        // Something probably went wrong here
                        // Could cause infinite loop
                        // Set a breakpoint and debug if that happens
                    } else {
                        cellSides.AddRange(missingEdgePair);
                    }
                    // ---------------------------------------------------------

                    // Update the delta for the while-loop
                    delta = cell.neighbors.Count - cellSides.Count;
                    tries++;
                } // end finding missing edges

                // TODO check if cellSides is a cycle?
                cell.setSides(cellSides);
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
                delrays.Add(new Edge(site, v));
            }

            // Find Voronoi edges intersected by each line. Warning: expensive!
            List<List<Edge>> intersectedVEdges = new List<List<Edge>>();
            foreach (Edge ray in delrays) {
                List<Edge> voronoiEdges = new List<Edge>();
                foreach (Edge voronoiEdge in edges) {  // Check every Voronoi edge
                    if (Edge.findIntersection(ray, voronoiEdge) != null) {
                        voronoiEdges.Add(voronoiEdge);
                    }
                }
                // Indices of delrays will correspond with intersections
                intersectedVEdges.Add(voronoiEdges);
            }

            // When only 1 edge exists in the edge list, it's guaranteed to be a side of the Voronoi cell
            List<Edge> cellSides = new List<Edge>();
            foreach (List<Edge> l in intersectedVEdges) {
                if (l.Count == 1) {
                    cellSides.Add(l[0]);
                } else if (l.Count == 0) {
                    Debug.LogWarning($"{site.id}: No intersections! ({site.x}, {site.y})");
                } else {
                    Debug.Log($"{site.id}: Intersects {l.Count} edges");
                }
            }

            return cellSides.Distinct().ToList();
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
            return missingEdgePairs.Distinct().ToList();
        }
    }
}
