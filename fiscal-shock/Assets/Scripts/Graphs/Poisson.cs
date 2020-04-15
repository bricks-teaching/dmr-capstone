using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Poisson disc sampling based on https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf.
    /// <para>Every point generated is guaranted to be at least
    /// `r` or `minDistance` units away from any other point.
    /// </para>
    /// <example>Making Delaunay and Voronoi from a Poisson distribution:
    /// <code>
    /// Poisson distribution = new Poisson(minDistanceBetweenSamples, width, height);
    /// Delaunay poissonTriangulation = new Delaunay(distribution.vertices);
    /// Voronoi poissonVoronoi = poissonTriangulation.makeVoronoi();
    /// </code>
    /// </example>
    /// </summary>
    public class Poisson {
        /// <summary>
        /// `n`, as in `R^n`. We're only working in 2D for these points, but it
        /// could be 3D, too.
        /// </summary>
        private static readonly int dimensions = 2;

        /// <summary>
        /// `k`: Maximum number of samples to attempt per iteration.
        /// </summary>
        private static readonly int sampleLimit = 30;

        /// <summary>
        /// `r`: Given any point, samples are chosen from the spherical annulus,
        /// a donut whose inner radius is r and outer radius, 2r.
        /// The scalar for the outer radius could be altered.
        /// <para>All points created by the distribution are at least
        /// this many units away from any other point.</para>
        /// </summary>
        private float minDistance;
        private float maxDistance;

        /// <summary>
        /// Grid cell is size `r/sqrt(n)`, restricting each grid cell to
        /// at most one point.
        /// </summary>
        private float cellSize;

        /// <summary>
        /// `n`-dimensional grid of points.
        /// </summary>
        private Vertex[] grid;
        private int rows;
        private int cols;

        /// <summary>
        /// A list of points that have been added to the distribution, but
        /// have not been using as the donut point (need to search for `k`
        /// samples around this one still).
        /// </summary>
        /// <typeparam name="Vertex"></typeparam>
        /// <returns></returns>
        private List<Vertex> active = new List<Vertex>();

        /// <summary>
        /// Public accessor for the list of vertices generated.
        /// </summary>
        /// <typeparam name="Vertex"></typeparam>
        /// <returns></returns>
        public List<Vertex> vertices => grid.Where(v => v != null).ToList();

        /// <summary>
        /// Formula to find index in a 1D array using 2D coordinates
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private int getArrayIndex(int col, int row) {
            return col + (row*cols);
        }

        /// <summary>
        /// Finds the row or column that a given point falls within on the grid.
        /// Pass in x for column and y for row.
        /// </summary>
        /// <param name="z">Vertex.x or Vertex.y</param>
        /// <returns></returns>
        private int getGridValueOf(float z) {
            return Mathf.FloorToInt(z / cellSize);
        }

        /// <summary>
        /// Finds the correct grid array index for a given vertex.
        /// </summary>
        /// <param name="v">vertex to locate in the grid</param>
        /// <returns></returns>
        private int getIndexOfVertex(Vertex v) {
            return getArrayIndex(getGridValueOf(v.x), getGridValueOf(v.y));
        }

        /// <summary>
        /// Grabs a random point from the active list to start sampling around.
        /// </summary>
        /// <returns></returns>
        private Vertex pickRandomActivePoint() {
            return active[Random.Range(0, active.Count)];
        }

        /// <summary>
        /// Finds a random point within the spherical annulus by drawing a
        /// vector whose magnitude is between `r` and `2r` at a random angle.
        /// </summary>
        /// <param name="origin">donut center</param>
        /// <returns>random sample</returns>
        private Vertex getRandomPointAround(Vertex origin) {
            float magnitude = Random.Range(minDistance, maxDistance);
            float angle = Random.Range(0f, 360f);
            return new Vertex(Mathy.getEndpointOfLineRotation(origin.x, origin.y, angle, magnitude));
        }

        /// <summary>
        /// Helper function to validate samples. Samples can be generated
        /// out of bounds.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool isIndexWithinGrid(int x, int y) {
            return x > -1 && y > -1 && x < cols && y < rows;
        }

        public Poisson(float minDistanceBetweenSamples, int width, int height) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            minDistance = minDistanceBetweenSamples;
            maxDistance = 2 * minDistanceBetweenSamples;
            cellSize = Mathf.Floor(minDistance/Mathf.Sqrt(dimensions));
            rows = Mathf.FloorToInt(height / cellSize);
            cols = Mathf.FloorToInt(width / cellSize);
            grid = new Vertex[rows*cols];

            // Pick a starting point. Could be random or a set point, like the center.
            Vertex v = new Vertex(Random.Range(0f, width), Random.Range(0f, height));
            // Add it to the grid and active list.
            grid[getIndexOfVertex(v)] = v;
            active.Add(v);

            // Main loop. When there are no more points to sample around, done.
            while (active.Count > 0) {
                Vertex origin = pickRandomActivePoint();

                bool shouldStayActive = false;

                for (int n = 0; n < sampleLimit; ++n) {
                    bool validPoint = true;

                    Vertex sample = getRandomPointAround(origin);
                    // Since these values could be negative and result in weird "indices," they need to be checked separately.
                    int x = getGridValueOf(sample.x);
                    int y = getGridValueOf(sample.y);
                    int index = getArrayIndex(x, y);

                    // If the point is within bounds and there is not already a point in this cell, check distance to neighbors.
                    if (isIndexWithinGrid(x, y) && grid[index] == null) {
                        for (int i = -1; i <= 1 && validPoint; ++i) {
                            for (int j = -1; j <= 1 && validPoint; ++j) {
                                if (isIndexWithinGrid(x+i, y+j)) {  // Don't sneak out of bounds on the edges here, either!
                                    Vertex neighbor = grid[getArrayIndex(x+i, y+j)];
                                    if (neighbor != null && sample.getDistanceTo(neighbor) < minDistance) {
                                        validPoint = false;
                                    }
                                }
                            }
                        }

                        // Passes all distance checks
                        if (validPoint) {
                            shouldStayActive = true;
                            grid[index] = sample;
                            active.Add(sample);
                        }
                    }
                }

                // Reached limit on this vertex, so remove it from candidates for sampling around
                if (!shouldStayActive) {
                    active.Remove(origin);
                }
            }

            sw.Stop();
            Debug.Log($"Poisson distribution generated in {sw.ElapsedMilliseconds} ms.");
        }
    }
}
