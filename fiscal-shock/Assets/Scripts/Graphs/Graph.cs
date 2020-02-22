using System.Collections.Generic;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Base class for all graphs
    /// </summary>
    public class Graph {
        public List<Vertex> vertices { get; } = new List<Vertex>();
        public List<Edge> edges { get; } = new List<Edge>();
    }
}