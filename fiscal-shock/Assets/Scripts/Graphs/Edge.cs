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
                return (p.Equals(other.p) && q.Equals(other.q)) || (p.Equals(other.q) && q.Equals(other.p));
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

        /// <summary>
        /// Angle of this edge from p to q
        /// </summary>
        /// <returns></returns>
        public float getAngle() {
            return p.getAngleOfRotationTo(q);
        }
    }
}