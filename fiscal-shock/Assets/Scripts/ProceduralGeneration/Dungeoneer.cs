using System.Collections.Generic;
using System;
using UnityEngine;
using FiscalShock.Graphs;
using ThirdParty;

/// <summary>
/// Generates graphs and stuff
/// </summary>
namespace FiscalShock.Procedural {
    public class Dungeoneer : MonoBehaviour {
        [Tooltip("Seed for random number generator. Uses current Unix epoch time (ms) if left at 0.")]
        public long seed;

        [Tooltip("Number of vertices to generate. A higher number will generate a finer-grained mesh.")]
        public int numberOfVertices = 100;

        [Tooltip("Unit scale. All vertex coordinates are multiplied by this number.")]
        public float unitScale = 1;

        [Tooltip("Minimum x-value of a vertex.")]
        public int minX = -100;

        [Tooltip("Maximum x-value of a vertex.")]
        public int maxX = 100;

        [Tooltip("Minimum y-value of a vertex.")]
        public int minY = -100;

        [Tooltip("Maximum y-value of a vertex.")]
        public int maxY = 100;

        public Delaunay dt { get; private set; }
        public Voronoi vd { get; private set; }
        // private Delaunay masterDt;
        // private Something spanningTree;

        private MersenneTwister mt;

        public void Start() {
            initPRNG();
            generateDelaunay();
            generateVoronoi();
        }

        public void initPRNG() {
            // Set up the PRNG
            if (seed == 0) {
                seed = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            // TODO Poisson disc sampling instead?
            mt = new MersenneTwister((int)seed);
            UnityEngine.Random.InitState((int)seed);
        }

        public List<double> makeRandomPoints() {
            List<double> vertices = new List<double>();
            for (int i = 0; i < numberOfVertices*2; i += 2) {
                vertices.Add(mt.Next(minX, maxX) * unitScale);
                vertices.Add(mt.Next(minY, maxY) * unitScale);
            }
            return vertices;
        }

        public void generateDelaunay() {
            dt = new Delaunay(makeRandomPoints());
        }

        public void generateVoronoi() {
            vd = dt.makeVoronoi();
        }
    }
}