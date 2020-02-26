#pragma warning disable
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FiscalShock.Graphs;
using ThirdParty;

namespace Tests {
    public class PolygonTest {
        [Test]
        public void testGetArea() {
            Polygon p = new Polygon();
            p.vertices.Add(new Vertex(1, 0));
            p.vertices.Add(new Vertex(0, 1));
            p.vertices.Add(new Vertex(-1, 0));
            p.vertices.Add(new Vertex(0, -1));

            double area = p.getArea();
            Assert.AreEqual(2, area);
        }

        [Test]
        [Ignore("Doesn't seem to work with vertices out of order even though they should be ordered")]
        public void testCellGetArea() {
            Cell p = new Cell(new Vertex(0, 0));
            p.vertices.Add(new Vertex(0, 1));
            p.vertices.Add(new Vertex(1, 0));
            p.vertices.Add(new Vertex(-1, 0));
            p.vertices.Add(new Vertex(0, -1));
            p.orderVertices();

            double area = p.getArea();
            Assert.AreEqual(2, area);
        }
    }
}
