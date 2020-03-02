using System;
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

            createCells();
            calculateVerticesAndEdgesFromDelaunator();
            calculateCellAreas();
        }

        /// <summary>
        /// Algorithm taken from Delaunator guide.
        /// <para>https://mapbox.github.io/delaunator/</para>
        /// </summary>
        private void calculateVerticesAndEdgesFromDelaunator() {
            for (int e = 0; e < dual.delaunator.triangles.Count; e++) {
                if (e < dual.delaunator.halfedges[e]) {
                    Triangle triP = dual.triangles[Edge.getTriangleId(e)];
                    Triangle triQ = dual.triangles[Edge.getTriangleId(dual.delaunator.halfedges[e])];
                    Vertex p = triP.findCircumcenter();
                    Vertex q = triQ.findCircumcenter();

                    Edge pq = new Edge(p, q);

                    vertices.Add(p);
                    vertices.Add(q);
                    edges.Add(pq);

                    List<Vertex> triPVertices = triP.vertices;
                    List<Vertex> triQVertices = triQ.vertices;
                    foreach (Vertex vertex in triPVertices) {
                        if (triQVertices.Contains(vertex)) {
                            vertex.cell.sides.Add(pq);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cell neighbors are just adjacent sites in the Delaunay triangulation.
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

        /// <summary>
        /// Calculate and store all cell areas. Don't run this until cells have
        /// been found properly.
        /// </summary>
        private void calculateCellAreas() {
            foreach (Cell c in cells) {
                if (dual.convexHull.Contains(c.site)) {
                    c.area = 0;
                } else {
                    c.setVerticesFromSides();
                    c.orderVertices();
                    c.area = c.getArea();
                }
            }
        }
    }
}
