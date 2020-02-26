using System.Collections.Generic;
using System.Linq;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Base class for all graphs
    /// </summary>
    public class Graph {
        public List<Vertex> vertices { get; } = new List<Vertex>();
        public List<Edge> edges { get; } = new List<Edge>();

        /// <summary>
        /// Find the points comprising the convex hull of this graph. Useful for
        /// error checking some things in the Delaunay/Voronoi.
        /// <para>https://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Convex_hull/Monotone_chain</para>
        /// </summary>
        /// <returns></returns>
        public List<Vertex> findConvexHull() {
            // Sort based on x-values and start in the lower left
            List<Vertex> sortedList = vertices.OrderBy(v => v.x).ThenBy(v => v.y).ToList();
            List<Vertex> lowerHull = new List<Vertex>();
            List<Vertex> upperHull = new List<Vertex>();

            // Find lower hull
            foreach (Vertex v in sortedList) {
                while (lowerHull.Count >= 2
                    && Triangle.isTriangleClockwise(new List<Vertex> {
                        lowerHull[lowerHull.Count - 2],
                        lowerHull[lowerHull.Count - 1],
                        v
                    })) {
                    lowerHull.RemoveAt(lowerHull.Count - 1);
                }
                lowerHull.Add(v);
            }

            // Find upper hull
            int n = lowerHull.Count;
            for (int i = sortedList.Count - 1; i >= 0; i--) {
                Vertex v = sortedList[i];
                while (upperHull.Count >= n
                    && Triangle.isTriangleClockwise(new List<Vertex> {
                        upperHull[upperHull.Count - 2],
                        upperHull[upperHull.Count - 1],
                        v
                    })) {
                    upperHull.RemoveAt(upperHull.Count - 1);
                }
                upperHull.Add(v);
            }

            return upperHull.Union(lowerHull).ToList();
        }
    }
}