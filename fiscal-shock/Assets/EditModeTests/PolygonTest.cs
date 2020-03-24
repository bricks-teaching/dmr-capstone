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
            List<Vertex> vs = new List<Vertex> {
                new Vertex(1, 0),
                new Vertex(0, 1),
                new Vertex(-1, 0),
                new Vertex(0, -1)
            };
            List<Edge> es = new List<Edge> {
                new Edge(vs[0], vs[1]),
                new Edge(vs[1], vs[2]),
                new Edge(vs[2], vs[3]),
                new Edge(vs[3], vs[0]),
            };
            Polygon p = new Polygon(es);

            double area = p.getArea();
            Assert.AreEqual(2, area);
        }

        [Test]
        public void testCellGetArea() {
            List<Vertex> vs = new List<Vertex> {
                new Vertex(0, 1),
                new Vertex(1, 0),
                new Vertex(0, -1),
                new Vertex(-1, 0)
            };
            List<Edge> es = new List<Edge> {
                new Edge(vs[1], vs[0]),
                new Edge(vs[0], vs[3]),
                new Edge(vs[3], vs[2]),
                new Edge(vs[2], vs[1]),
            };
            Cell c = new Cell(new Vertex(0, 0));
            c.setSides(es);
            c.orderVertices();

            double area = c.getArea();
            Assert.AreEqual(2, area);
        }
    }
}
