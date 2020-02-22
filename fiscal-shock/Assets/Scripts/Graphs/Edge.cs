using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Undirected edge, defined by endpoints
    /// For only Delaunator edges, ID is relevant
    /// </summary>
    public class Edge {
        public List<Vertex> vertices { get; }
        public Vertex p => vertices[0];
        public Vertex q => vertices[1];

        public Edge(Vertex a, Vertex b) {
            vertices = new List<Vertex> { a, b };
        }

        public void connect(Vertex a, Vertex b) {
            a.neighborhood.Add(b);
            b.neighborhood.Add(a);
            a.incidentEdges.Add(this);
            b.incidentEdges.Add(this);
        }

        /* Delaunator-only helper functions */
        public static int getTriangleId(int eid) {
            return eid / 3;
        }
        /* End Delaunator helper functions */

        /* Comparator functions - needed for LINQ */
        public override bool Equals(object obj) {
            if (obj is Edge other) {
                return vertices.Intersect(other.vertices).Count() == vertices.Count;
            }
            return false;
        }

        /// <summary>
        /// Taken from https://stackoverflow.com/a/2280213
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            int hash = 23;
            hash = (hash * 31) + p.GetHashCode();
            hash = (hash * 31) + q.GetHashCode();
            return hash;
        }
        /* End comparator functions */

        /// <summary>
        /// Length of an edge in this case is the Euclidean distance
        /// between the endpoints
        /// </summary>
        /// <returns>length of this edge</returns>
        public double getLength() {
            return p.getDistanceTo(q);
        }

        public static Edge getEdge(Vertex a, Vertex b, List<Edge> existingEdges) {
            Edge tmp = new Edge(a, b);
            int idx = existingEdges.IndexOf(tmp);
            if (idx == -1) {
                tmp.connect(a, b);
                existingEdges.Add(tmp);
                return tmp;
            }
            return existingEdges[idx];
        }

        /// <summary>
        /// Find intersection point between two edges.
        /// See <see cref="Mathy.findIntersection" />.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>`Vertex` representing intersection point, or null if edges don't intersect</returns>
        public static Vertex findIntersection(Edge a, Edge b) {
            double[] intersection = Mathy.findIntersection(a.p.x, a.p.y, a.q.x, a.q.y, b.p.x, b.p.y, b.q.x, b.q.y);
            if (intersection != null) {
                return new Vertex(intersection);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Finds all edges connecting vertices in a list. Can't find edges whose endpoints aren't in the original list of vertices.
        /// </summary>
        /// <param name="listToConnect">Vertices to connect</param>
        /// <returns>List of connecting edges found. Can be empty.</returns>
        /// TODO this is part of naive algorithm, maybe delete
        public static List<Edge> findConnectingEdges(List<Vertex> listToConnect) {
            bool doneFindingPairs = false;
            List<Edge> connectors = new List<Edge>();
            while (!doneFindingPairs) {
                for (int i = 0; i < listToConnect.Count; ++i) {
                    bool foundThisPair = false;
                    foreach (Edge e in listToConnect[i].incidentEdges) {
                        /* If the size of the intersection of the edge's
                         * endpoints with the full list of vertices to connect
                         * is the same size as the edge's endpoint list (2),
                         * then that edge connects two vertices.
                         */
                        if (e.vertices.Intersect(listToConnect).Count() == e.vertices.Count) {
                            // Remove these vertices from the search list
                            listToConnect.Remove(e.p);
                            listToConnect.Remove(e.q);
                            connectors.Add(e);
                            foundThisPair = true;
                            break;
                        }
                    }
                    if (foundThisPair && listToConnect.Count > 0) {
                        // Reset the outer loop index to start the search over.
                        i = 0;
                    }
                }
                /* If we reached the end of the vertex list, no more edges can
                 * be found with the current information.
                 */
                doneFindingPairs = true;
            }

            return connectors.Distinct().ToList();
        }
    }
}