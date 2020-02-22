using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// 2D polygon, defined by its edges (and thereby, vertices)
    /// </summary>
    public class Polygon {
        public List<Edge> sides { get; private set; } = new List<Edge>();
        public List<Vertex> vertices { get; private set; } = new List<Vertex>();

        public Polygon() {}

        public Polygon(List<Edge> boundary) {
            setSides(boundary);
        }

        public void setSides(List<Edge> boundary) {
            sides = boundary;
            vertices = sides.SelectMany(e => e.vertices).Distinct().ToList();
        }
    }

    /// <summary>
    /// Voronoi cell extension of base Polygon class
    /// </summary>
    public class Cell : Polygon {
        public Vertex site { get; set; }
        public List<Cell> neighbors { get; set; } = new List<Cell>();
        public int id { get; }

        public Cell(Vertex delaunayVertex) {
            site = delaunayVertex;
            id = site.id;
        }

        new public void setSides(List<Edge> boundary) {
            base.setSides(boundary);

            if (sides.Count < 3) {
                Debug.LogWarning($"{id}: Not a closed polygon ({sides.Count} sides)");
            }
            if (vertices.Count <= sides.Count) {
                Debug.LogWarning($"{id}: Illogical number of vertices compared to edges ({vertices.Count} vs {sides.Count})");
            }
        }
    }
}