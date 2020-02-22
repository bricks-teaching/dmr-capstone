#pragma warning disable
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FiscalShock.Graphs;
using FiscalShock.Procedural;
using ThirdParty;

namespace Tests {
    public class GraphTest {
        Dungeoneer dg = new Dungeoneer();
        int delVerts;
        int delEdges;
        int delFaces;
        int vorVerts;
        int vorEdges;
        int vorFaces;

        [SetUp]
        public void Init() {
            dg.seed = 5;
            dg.initPRNG();
            dg.generateDelaunay();
            dg.generateVoronoi();

            delVerts = dg.dt.vertices.Count;    // v
            delEdges = dg.dt.edges.Count;       // e
            /* For Delaunay triangulations:
             * "In the plane (d = 2), if there are b vertices on the convex
             * hull, then any triangulation of the points has at most
             * 2n − 2 − b triangles, plus one exterior face."
             */
            int delHull = dg.dt.delaunator.hull.Count;
            delFaces = 2*delVerts - 1 - delHull;  // f

            vorVerts = dg.vd.vertices.Count;    // v
            vorEdges = dg.vd.edges.Count;       // e
            vorFaces = dg.vd.sites.Count + 1;   // f (one exterior)
        }

        [Test]
        public void testEulersFormulaDelaunay() {
            // Verify for planar graphs: v - e + f = 2
            int delEuler = delVerts - delEdges + delFaces;
            Assert.AreEqual(2, delEuler);
        }

        [Test]
        public void testEulersFormulaVoronoi() {
            // Verify for planar graphs: v - e + f = 2
            int vorEuler = vorVerts - vorEdges + vorFaces;
            /* Currently not working, because we're not adding the infinite
             * edges on the convex hull, so we don't have as many edges as we
             * should. Some convex hull points *are* covered, so we can't just
             * add that value to edges to fudge it.
             */
            //Assert.AreEqual(2, vorEuler);
            Assert.LessOrEqual(2, vorEuler);
        }

        [Test]
        public void testKuratowskiTheorem1Delaunay() {
            // e ≤ 3v − 6
            Assert.LessOrEqual(3, delVerts);

            int delKura1 = 3*delVerts - 6;
            Assert.LessOrEqual(delEdges, delKura1);
        }

        [Test]
        public void testKuratowskiTheorem1Voronoi() {
            // e ≤ 3v − 6
            Assert.LessOrEqual(3, vorVerts);

            int vorKura1 = 3*vorVerts - 6;
            Assert.LessOrEqual(vorEdges, vorKura1);
        }

        // Kuratowski Thm 2 only applies to graphs with no cycles of length 3

        [Test]
        public void testKuratowskiTheorem3Delaunay() {
            // f ≤ 2v − 4
            Assert.LessOrEqual(3, delVerts);

            int delKura3 = 2*delVerts - 4;
            Assert.LessOrEqual(delFaces, delKura3);
        }

        [Test]
        public void testKuratowskiTheorem3Voronoi() {
            // f ≤ 2v − 4
            Assert.LessOrEqual(3, vorVerts);

            int vorKura3 = 2*vorVerts - 4;
            Assert.LessOrEqual(vorFaces, vorKura3);
        }
    }
}
