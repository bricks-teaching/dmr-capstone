using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Vertex (or "node") as part of a graph, defined by
    /// 2D Cartesian coordinates
    /// </summary>
    public class Vertex {
        public Vector2 vector { get; }
        public float x => vector.x;
        public float y => vector.y;
        public int id { get; }

        /* Spending the space to track connected components simplifies
         * any algorithms that need to traverse a graph.
         */
        // Vertices adjacent (connected by an edge)
        public List<Vertex> neighborhood { get; set; } = new List<Vertex>();
        // Edges incident (having this vertex as an endpoint)
        public List<Edge> incidentEdges { get; set; } = new List<Edge>();

        /* Begin overloaded constructors */
        public Vertex(float xX, float yY) {
            vector = new Vector2(xX, yY);
        }

        public Vertex(float xX, float yY, int vid) : this(xX, yY) {
            id = vid;
        }

        public Vertex(double xX, double yY) : this((float)xX, (float)yY) {}

        public Vertex(double[] xy) : this(xy[0], xy[1]) {
            if (xy.Length > 2) {
                Debug.LogError($"FATAL: Input array held more than two coordinates.");
                throw new ArgumentException();
            }
        }
        /* End overloaded constructors */

        /* Comparator functions - needed for LINQ */
        public override bool Equals(object obj) {
            if (obj is Vertex other) {
                return vector == other.vector;
            }
            return false;
        }

        /// <summary>
        /// Taken from https://stackoverflow.com/a/2280213
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            int hash = 23;
            hash = (hash * 31) + x.GetHashCode();
            hash = (hash * 31) + y.GetHashCode();
            return hash;
        }
        /* End comparator functions */

        /// <summary>
        /// Helper function to create a vertex only if it doesn't already
        /// exist. Delaunator output leads to duplicate data structures when
        /// this isn't used.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="existingVertices"></param>
        /// <returns></returns>
        public static Vertex getVertex(float x, float y, List<Vertex> existingVertices) {
            Vertex tmp = new Vertex(x, y, existingVertices.Count);
            int idx = existingVertices.IndexOf(tmp);
            if (idx == -1) {
                // new vertex, add it
                existingVertices.Add(tmp);
                return tmp;
            }
            return existingVertices[idx];
        }

        /// <summary>
        /// Vertex creation sometimes called from a static context
        /// </summary>
        /// <param name="v"></param>
        /// <param name="existingVertices"></param>
        /// <returns></returns>
        public static Vertex getVertex(Vertex v, List<Vertex> existingVertices) {
            return Vertex.getVertex(v.x, v.y, existingVertices);
        }

        /// <summary>
        /// Euclidean distance between two Cartesian coordiates
        /// </summary>
        /// <param name="other">distant vertex</param>
        /// <returns>distance</returns>
        public double getDistanceTo(Vertex other) {
            return Mathy.getDistanceBetween(x, y, other.x, other.y);
        }

        /// <summary>
        /// Given a list of vertices and an origin vertex, find the nearest one (via Euclidean distance).
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        public Vertex findNearestInList(List<Vertex> others) {
            // Find all distances to the origin
            List<double> distances = others.Select(v => v.getDistanceTo(this)).ToList();

            /* If origin is in the list, we want the second minimum distance.
             * This means we can't just take the minimum of this list.
             * But we can just choose to skip the first element in the sorted list instead, or skip na
             */
            int skip = others.Contains(this)? 1 : 0;
            double minimumDistance = distances.OrderBy(d => d).Skip(skip).First();
            int indexOfNearest = distances.IndexOf(minimumDistance);

            return others[indexOfNearest];
        }

        /// <summary>
        /// Find a point on the line drawn at the angle theta from site that is distance units away from site.
        /// </summary>
        /// <param name="theta">angle in radians</param>
        /// <param name="distance">desired length of line segment</param>
        /// <returns></returns>
        public Vertex getEndpointOfLineRotation(double theta, float distance) {
            return new Vertex(Mathy.getEndpointOfLineRotation(x, y, theta, distance));
        }

        /// <summary>
        /// Get the angle of rotation from this vertex to another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public float getAngleOfRotationTo(Vertex other) {
            return (float)Mathy.getAngleOfRotation(other.y, y, other.x, x);
        }

        /// <summary>
        /// Convert to Unity Vector3
        /// Unity uses y-axis as height (up/down) and z-axis as depth, unlike
        /// e.g. Blender where z is up/down
        /// Since a Vertex is 2D, the third dimension must be specified
        /// </summary>
        /// <param name="height">desired height of Vector3</param>
        /// <returns>3D vertex</returns>
        public Vector3 toVector3AtHeight(float height) {
            return new Vector3(x, height, y);
        }
    }
}